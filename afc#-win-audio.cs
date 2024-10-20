using System;
using System.Timers;
using NAudio.CoreAudioApi;
using NAudio.CoreAudioApi.Interfaces;
using System.Text.Json;

class AudioDeviceNotificationClient : IMMNotificationClient
{
    private readonly MMDeviceEnumerator deviceEnumerator;
    private MMDevice? currentDevice;
    private float lastVolumeLevel;
    private bool lastMutedStatus;
    private System.Timers.Timer? volumeCheckTimer;
    public int Delay { get; private set; }
    public float VolumeStep { get; private set; } // Свойство для шага изменения громкости

    // Событие для уведомления об изменении громкости устройства
    public event Action<string>? OnVolumeChanged;

    public AudioDeviceNotificationClient(MMDeviceEnumerator enumerator, int delay, int volumeStepPercent)
    {
        deviceEnumerator = enumerator;
        Delay = delay;
        VolumeStep = volumeStepPercent / 100f; // Преобразование процента в десятичную дробь
    }

    // Метод для обработки смены устройства по умолчанию
    public void OnDefaultDeviceChanged(DataFlow flow, Role role, string defaultDeviceId)
    {
        if (flow == DataFlow.Render && role == Role.Multimedia)
        {
            GetDeviceInfo(defaultDeviceId, flow, role);
            StartVolumeMonitoring(Delay);
        }
    }

    // Метод для получения информации об устройстве
    public void GetDeviceInfo(string deviceId, DataFlow flow, Role role)
    {
        try
        {
            currentDevice = deviceEnumerator.GetDevice(deviceId);

            if (currentDevice != null)
            {
                lastVolumeLevel = currentDevice.AudioEndpointVolume.MasterVolumeLevelScalar;
                lastMutedStatus = currentDevice.AudioEndpointVolume.Mute;

                var deviceInfo = new
                {
                    id = currentDevice.ID,
                    name = currentDevice.FriendlyName,
                    volume = Math.Round(lastVolumeLevel * 100), // Округление до целого
                    muted = lastMutedStatus
                };
                OnVolumeChanged?.Invoke(JsonSerializer.Serialize(deviceInfo));
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка при получении информации об устройстве: {ex.Message}");
        }
    }

    // Метод для отслеживания изменений громкости
public void StartVolumeMonitoring(int delay)
{
    if (volumeCheckTimer != null)
    {
        volumeCheckTimer.Stop();
        volumeCheckTimer.Elapsed -= CheckVolumeChangeAndMutedStatus; // Удалите обработчик события
        volumeCheckTimer.Dispose(); // Освободите ресурсы таймера
        volumeCheckTimer = null; // Установите в null для предотвращения утечек памяти
    }

    volumeCheckTimer = new System.Timers.Timer(delay);
    volumeCheckTimer.Elapsed += CheckVolumeChangeAndMutedStatus;
    volumeCheckTimer.Start();
}

    // Метод для проверки изменений громкости
    private void CheckVolumeChangeAndMutedStatus(object? sender, ElapsedEventArgs e)
    {
        if (currentDevice != null)
        {
            float currentVolumeLevel = currentDevice.AudioEndpointVolume.MasterVolumeLevelScalar;
            bool currentMuted = currentDevice.AudioEndpointVolume.Mute;
        
            if (currentVolumeLevel != lastVolumeLevel || currentMuted != lastMutedStatus)
            {
                lastVolumeLevel = currentVolumeLevel;
                lastMutedStatus = currentMuted;

                var deviceInfo = new
                {
                    id = currentDevice.ID,
                    name = currentDevice.FriendlyName,
                    volume = Math.Round(currentVolumeLevel * 100), // Округление до целого
                    muted = currentMuted
                };

                OnVolumeChanged?.Invoke(JsonSerializer.Serialize(deviceInfo));
            }
        }
    }

    // Метод для увеличения громкости
    public void UpVolume()
    {
        if (currentDevice != null)
        {
            var volumeControl = currentDevice.AudioEndpointVolume;
            float newVolume = volumeControl.MasterVolumeLevelScalar + VolumeStep;

            if (newVolume > 1.0f)
            {
                newVolume = 1.0f;
            }

            volumeControl.MasterVolumeLevelScalar = newVolume;

            var deviceInfo = new
            {
                id = currentDevice.ID,
                name = currentDevice.FriendlyName,
                volume = Math.Round(newVolume * 100), // Округление до целого
                muted = volumeControl.Mute
            };

            OnVolumeChanged?.Invoke(JsonSerializer.Serialize(deviceInfo));
        }
    }

    // Метод для уменьшения громкости
    public void DownVolume()
    {
        if (currentDevice != null)
        {
            var volumeControl = currentDevice.AudioEndpointVolume;
            float newVolume = volumeControl.MasterVolumeLevelScalar - VolumeStep;

            if (newVolume < 0f)
            {
                newVolume = 0f;
            }

            volumeControl.MasterVolumeLevelScalar = newVolume;

            var deviceInfo = new
            {
                id = currentDevice.ID,
                name = currentDevice.FriendlyName,
                volume = Math.Round(newVolume * 100), // Округление до целого
                muted = volumeControl.Mute
            };

            OnVolumeChanged?.Invoke(JsonSerializer.Serialize(deviceInfo));
        }
    }

    // Пустые реализации для ненужных методов
    public void OnDeviceStateChanged(string deviceId, DeviceState newState) { }
    public void OnDeviceAdded(string pwstrDeviceId) { }
    public void OnDeviceRemoved(string deviceId) { }
    public void OnPropertyValueChanged(string pwstrDeviceId, PropertyKey key) { }
}

class Program
{
    static void Main(string[] args)
    {        
        int delay = Math.Max(int.TryParse(args[0], out int parsedDelay) ? parsedDelay : 250, 100);
        int volumeStepPercent = int.TryParse(args[1], out int parsedStep) ? parsedStep < 0 ? 0 : parsedStep : 5;       

        var enumerator = new MMDeviceEnumerator();
        var client = new AudioDeviceNotificationClient(enumerator, delay, volumeStepPercent);

        client.OnVolumeChanged += (deviceInfo) =>
        {
            Console.WriteLine(deviceInfo);
        };

        enumerator.RegisterEndpointNotificationCallback(client);

        MMDevice defaultDevice = enumerator.GetDefaultAudioEndpoint(DataFlow.Render, Role.Multimedia);

        client.GetDeviceInfo(defaultDevice.ID, DataFlow.Render, Role.Multimedia);
        client.StartVolumeMonitoring(delay);

        // Обработка сигналов завершения
        Console.CancelKeyPress += (sender, e) =>
        {
            e.Cancel = true; // Отменяем завершение, чтобы выполнить код очистки
            Console.WriteLine("Получен сигнал завершения. Завершение процесса...");
            enumerator.UnregisterEndpointNotificationCallback(client);
            Environment.Exit(0);
        };

        AppDomain.CurrentDomain.ProcessExit += (sender, e) =>
        {
            Console.WriteLine("Процесс завершается...");
            enumerator.UnregisterEndpointNotificationCallback(client);
        };

        // Слушаем команды на входе
        if (args[1] == "upVolume")
        {
            client.UpVolume();
        }
        if (args[0] == "upVolume")
        {
            client.DownVolume();
        }     

        Console.ReadLine();
        enumerator.UnregisterEndpointNotificationCallback(client);
    }
}