using System;
using System.Timers;
using NAudio.CoreAudioApi;
using NAudio.CoreAudioApi.Interfaces;
using System.Text.Json;dsadas

class AudioDeviceNotificationClient : IMMNotificationClient
{
    private readonly MMDeviceEnumerator deviceEnumerator;
    private MMDevice? currentDevice; // Сделаем поле nullable, чтобы избежать ошибки компиляции
    private float lastVolumeLevel; // Для хранения предыдущего значения громкости
    private System.Timers.Timer? volumeCheckTimer; // Сделаем поле nullable

    // Событие для уведомления об изменении громкости устройства
    public event Action<string>? OnVolumeChanged;

    public AudioDeviceNotificationClient(MMDeviceEnumerator enumerator)
    {
        deviceEnumerator = enumerator;
    }

    // Метод для обработки смены устройства по умолчанию
    public void OnDefaultDeviceChanged(DataFlow flow, Role role, string defaultDeviceId)
    {
        // Проверяем, что это устройство с ролью Multimedia и типом Render
        if (flow == DataFlow.Render && role == Role.Multimedia)
        {
            GetDeviceInfo(defaultDeviceId, flow, role);
            StartVolumeMonitoring(1000); // Начинаем мониторинг громкости нового устройства
        }
    }

    // Метод для получения информации об устройстве
    public void GetDeviceInfo(string deviceId, DataFlow flow, Role role)
    {
        try
        {
            // Получаем устройство по его ID
            currentDevice = deviceEnumerator.GetDevice(deviceId);

            if (currentDevice != null)
            {
                // Инициализируем отслеживание изменений громкости
                lastVolumeLevel = currentDevice.AudioEndpointVolume.MasterVolumeLevelScalar;

                // Уведомляем о текущем состоянии устройства
                var deviceInfo = new
                {
                    id = currentDevice.ID,
                    name = currentDevice.FriendlyName,
                    volume = lastVolumeLevel * 100, // Громкость в процентах
                    muted = currentDevice.AudioEndpointVolume.Mute
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
        volumeCheckTimer.Dispose();
    }

    // Используем переданную задержку
    volumeCheckTimer = new System.Timers.Timer(delay); 
    volumeCheckTimer.Elapsed += CheckVolumeChange;
    volumeCheckTimer.Start();
}


    // Метод для проверки изменений громкости
    private void CheckVolumeChange(object? sender, ElapsedEventArgs e)
    {
        if (currentDevice != null)
        {
            float currentVolumeLevel = currentDevice.AudioEndpointVolume.MasterVolumeLevelScalar;
            if (currentVolumeLevel != lastVolumeLevel)
            {
                // Обновляем предыдущее значение громкости
                lastVolumeLevel = currentVolumeLevel;

                // Уведомляем о новом значении громкости
                var deviceInfo = new
                {
                    id = currentDevice.ID,
                    name = currentDevice.FriendlyName,
                    volume = currentVolumeLevel * 100, // Громкость в процентах
                    muted = currentDevice.AudioEndpointVolume.Mute
                };

                OnVolumeChanged?.Invoke(JsonSerializer.Serialize(deviceInfo));
            }
        }
    }

    public void UpVolume()
{
    if (currentDevice != null)
    {
        var volumeControl = currentDevice.AudioEndpointVolume;
        float newVolume = volumeControl.MasterVolumeLevelScalar + 0.05f;

        // Убедимся, что громкость не превышает 100%
        if (newVolume > 1.0f)
        {
            newVolume = 1.0f;
        }

        // Устанавливаем новую громкость
        volumeControl.MasterVolumeLevelScalar = newVolume;

        // Уведомляем о новом значении громкости
        var deviceInfo = new
        {
            id = currentDevice.ID,
            name = currentDevice.FriendlyName,
            volume = newVolume * 100, // Громкость в процентах
            muted = volumeControl.Mute
        };

        OnVolumeChanged?.Invoke(JsonSerializer.Serialize(deviceInfo));
    }
}

public void DownVolume()
{
    if (currentDevice != null)
    {
        var volumeControl = currentDevice.AudioEndpointVolume;
        float newVolume = volumeControl.MasterVolumeLevelScalar - 0.05f;

        // Убедимся, что громкость не превышает 100%
        if (newVolume < 0f)
        {
            newVolume = 0f;
        }

        // Устанавливаем новую громкость
        volumeControl.MasterVolumeLevelScalar = newVolume;

        // Уведомляем о новом значении громкости
        var deviceInfo = new
        {
            id = currentDevice.ID,
            name = currentDevice.FriendlyName,
            volume = newVolume * 100, // Громкость в процентах
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
        // Устанавливаем кодировку консоли в UTF-8
        Console.OutputEncoding = System.Text.Encoding.UTF8;

        int delay = args.Length > 0 ? int.Parse(args[0]) : 250;

        // Создаем экземпляр DeviceEnumerator
        var enumerator = new MMDeviceEnumerator();

        // Создаем экземпляр клиента уведомлений
        var client = new AudioDeviceNotificationClient(enumerator);

        // Подписываемся на событие изменения громкости
        client.OnVolumeChanged += (deviceInfo) =>
        {
            Console.WriteLine(deviceInfo);
        };

        enumerator.RegisterEndpointNotificationCallback(client);

        // Получаем текущее устройство по умолчанию для Multimedia и Render
        MMDevice defaultDevice = enumerator.GetDefaultAudioEndpoint(DataFlow.Render, Role.Multimedia);

        // Сразу выводим информацию о текущем устройстве по умолчанию
        client.GetDeviceInfo(defaultDevice.ID, DataFlow.Render, Role.Multimedia);

        // Начинаем мониторинг изменений громкости с указанной задержкой
        client.StartVolumeMonitoring(delay);

        // Слушаем команды на входе
        if (args.Length > 1 && args[1] == "upVolume")
        {
            client.UpVolume();
        }

        if (args.Length > 1 && args[1] == "downVolume")
        {
            client.DownVolume();
        }

        // Для демонстрации программы, чтобы она продолжала работать
        Console.ReadLine();

        // Важно: Отписываемся от уведомлений перед завершением программы
        enumerator.UnregisterEndpointNotificationCallback(client);
    }
}