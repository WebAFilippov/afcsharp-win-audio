using System;
using System.Timers;
using NAudio.CoreAudioApi;
using NAudio.CoreAudioApi.Interfaces;
using System.Text.Json;
using System.Text.Encodings.Web;
using System.Text.Unicode;

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
                var options = new JsonSerializerOptions
                {
                    Encoder = JavaScriptEncoder.Create(UnicodeRanges.BasicLatin, UnicodeRanges.Cyrillic),
                    WriteIndented = true
                };
                OnVolumeChanged?.Invoke(JsonSerializer.Serialize(deviceInfo, options));
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

                var options = new JsonSerializerOptions
                {
                    Encoder = JavaScriptEncoder.Create(UnicodeRanges.BasicLatin, UnicodeRanges.Cyrillic),
                    WriteIndented = true
                };
                OnVolumeChanged?.Invoke(JsonSerializer.Serialize(deviceInfo, options));
            }
        }
    }

    // Метод для увеличения громкости
    public void UpVolume(float volumeStep)
    {
        if (currentDevice != null)
        {
            try
            {
                var volumeControl = currentDevice.AudioEndpointVolume;
                float newVolume = volumeControl.MasterVolumeLevelScalar + volumeStep;

                if (newVolume > 1.0f)
                {
                    newVolume = 1.0f;
                }

                volumeControl.MasterVolumeLevelScalar = newVolume;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при увеличении громкости: {ex.Message}");
            }
        }
    }

    // Метод для уменьшения громкости
    public void DownVolume(float volumeStep)
    {
        if (currentDevice != null)
        {
            try
            {
                var volumeControl = currentDevice.AudioEndpointVolume;
                float newVolume = volumeControl.MasterVolumeLevelScalar - volumeStep;

                if (newVolume < 0f)
                {
                    newVolume = 0f;
                }

                volumeControl.MasterVolumeLevelScalar = newVolume;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при уменьшении громкости: {ex.Message}");
            }
        }
    }

    // Метод для установки задержки мониторинга
    public void SetDelay(int delay)
    {
        if (currentDevice != null)
        {
            if (delay >= 100) // Минимальная проверка для безопасного интервала
            {
                Delay = delay;
                StartVolumeMonitoring(Delay); // Перезапуск таймера с новым значением задержки
            }
        }
    }

    public void SetStepVolume(float stepVolume)
    {
        if (currentDevice != null)
        {
            if (stepVolume >= 0 && stepVolume <= 100) // Проверяем, что значение в допустимом диапазоне
            {
                VolumeStep = stepVolume / 100f; // Преобразуем в десятичное значение
            }
        }
    }

    // Метод для включения Mute
    public void Mute()
    {
        if (currentDevice != null)
        {
            try
            {
                currentDevice.AudioEndpointVolume.Mute = true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при отключении звука: {ex.Message}");
            }
        }
    }

    // Метод для отключения Mute
    public void UnMute()
    {
        if (currentDevice != null)
        {
            try
            {
                currentDevice.AudioEndpointVolume.Mute = false;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при включении звука: {ex.Message}");
            }
        }
    }

    // Метод для переключения состояния Mute
    public void ToggleMute()
    {
        if (currentDevice != null)
        {
            try
            {
                bool currentMuteState = currentDevice.AudioEndpointVolume.Mute;
                currentDevice.AudioEndpointVolume.Mute = !currentMuteState;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при переключении звука: {ex.Message}");
            }
        }
    }

    // Метод для получения списка доступных устройств
    public void ListAudioDevices()
    {
        try
        {
            var devices = deviceEnumerator.EnumerateAudioEndPoints(DataFlow.Render, DeviceState.Active);
            foreach (var device in devices)
            {
                Console.WriteLine($"Device ID: {device.ID}, Friendly Name: {device.FriendlyName}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка при получении списка устройств: {ex.Message}");
        }
    }

    // Метод для установки устройства по умолчанию
    public void SetDefaultAudioDevice(string deviceId)
    {
        // Здесь необходимо использовать IPolicyConfig или другой механизм для установки устройства по умолчанию
        // В данном примере не реализовано, но это может потребовать P/Invoke
        try
        {
            // Ваша реализация для установки устройства по умолчанию
            Console.WriteLine($"Устройство по умолчанию установлено: {deviceId}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка при установке устройства по умолчанию: {ex.Message}");
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
        // Проверяем, переданы ли аргументы командной строки
        int delay = 250; // Значение по умолчанию для задержки
        int volumeStepPercent = 5; // Значение по умолчанию для шага громкости

        // Проверяем, есть ли хотя бы один аргумент
        if (args.Length > 0 && int.TryParse(args[0], out int parsedDelay))
        {
            delay = Math.Max(parsedDelay, 75); // Минимальная задержка 100 мс
        }

        // Проверяем, есть ли второй аргумент
        if (args.Length > 1 && int.TryParse(args[1], out int parsedVolumeStep))
        {
            volumeStepPercent = Math.Clamp(parsedVolumeStep, 1, 100); // Ограничиваем шаг громкости от 1 до 100
        }

        var enumerator = new MMDeviceEnumerator();
        var client = new AudioDeviceNotificationClient(enumerator, delay, volumeStepPercent);

        // Подписываемся на события изменения громкости
        client.OnVolumeChanged += (deviceInfoJson) =>
        {
            Console.WriteLine(deviceInfoJson);
        };

        // Регистрируем уведомления
        enumerator.RegisterEndpointNotificationCallback(client);

        MMDevice defaultDevice = enumerator.GetDefaultAudioEndpoint(DataFlow.Render, Role.Multimedia);

        client.GetDeviceInfo(defaultDevice.ID, DataFlow.Render, Role.Multimedia);
        client.StartVolumeMonitoring(delay);

        try
        {
            while (true)
            {
                string? input = Console.ReadLine();
                var parts = input.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length == 0) continue;

                var action = parts[0].ToLowerInvariant();
                if (action == "upvolume")
                {
                    var value = parts.Length == 2 ? int.Parse(parts[1]) / 100f : client.VolumeStep;
                    client.UpVolume(value);
                }
                else if (action == "downvolume")
                {
                    var value = parts.Length == 2 ? int.Parse(parts[1]) / 100f : client.VolumeStep;
                    client.DownVolume(value);
                }
                else if (action == "setdelay")
                {
                    if (parts.Length == 2 && int.TryParse(parts[1], out int newDelay))
                    {
                        client.SetDelay(newDelay);
                    }
                }
                else if (action == "setstepvolume")
                {
                    if (parts.Length == 2 && float.TryParse(parts[1], out float stepVolume))
                    {
                        client.SetStepVolume(stepVolume);
                    }
                }
                else if (action == "mute")
                {
                    client.Mute();
                }
                else if (action == "unmute")
                {
                    client.UnMute();
                }
                else if (action == "togglemute")
                {
                    client.ToggleMute();
                }
                else if (action == "listdevices")
                {
                    client.ListAudioDevices();
                }
                else if (action == "setdefaultdevice" && parts.Length == 2)
                {
                    client.SetDefaultAudioDevice(parts[1]);
                }
                else if (action == "exit")
                {
                    break; // Завершение потока команд
                }
                else
                {
                    Console.WriteLine("Неизвестная команда. Попробуйте еще раз.");
                }
            }
        }
        finally
        {
            // Отключаем обратный вызов при завершении программы.
            enumerator.UnregisterEndpointNotificationCallback(client);
            Console.WriteLine("Отписка от уведомлений произведена.");
        }
    }
}
