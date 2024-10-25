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
      var volumeControl = currentDevice.AudioEndpointVolume;
      float newVolume = volumeControl.MasterVolumeLevelScalar + volumeStep;

      if (newVolume > 1.0f)
      {
        newVolume = 1.0f;
      }

      volumeControl.MasterVolumeLevelScalar = newVolume;
    }
  }

  // Метод для уменьшения громкости
  public void DownVolume(float volumeStep)
  {
    if (currentDevice != null)
    {
      var volumeControl = currentDevice.AudioEndpointVolume;
      float newVolume = volumeControl.MasterVolumeLevelScalar - volumeStep;

      if (newVolume < 0f)
      {
        newVolume = 0f;
      }

      volumeControl.MasterVolumeLevelScalar = newVolume;
    }
  }

  public void SetDelay()
  {
  }

  public void SetStepVolume(float stepVolume)
  {
    if (stepVolume >= 0 && stepVolume <= 100) // Проверяем, что значение в допустимом диапазоне
    {
      VolumeStep = stepVolume / 100f; // Преобразуем в десятичное значение
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
      delay = Math.Max(parsedDelay, 100); // Минимальное значение задержки
    }
    // Проверяем, есть ли второй аргумент
    if (args.Length > 1 && int.TryParse(args[1], out int parsedStep))
    {
      volumeStepPercent = Math.Max(parsedStep, 1); // Минимальное значение шага громкости
    }

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

    // Основной цикл командного интерфейса
    try
    {
      while (true)
      {
        string? command = Console.ReadLine();

        if (command != null)
        {
          var parts = command.Split(' ');
          var action = parts[0];

          if (action == "upVolume")
          {
            var value = parts.Length == 2 ? int.Parse(parts[1]) / 100f : client.VolumeStep;
            client.UpVolume(value);
          }
          else if (action == "downVolume")
          {
            var value = parts.Length == 2 ? int.Parse(parts[1]) / 100f : client.VolumeStep;
            client.DownVolume(value);
          }
          else if (action == "setDelay")
          {
          }
          else if (action == "setStepVolume")
          {
            if (parts.Length == 2 && float.TryParse(parts[1], out float stepVolume))
            {
              client.SetStepVolume(stepVolume);
            }
          }
          else if (action == "exit")
          {
            break; // Завершение потока команд
          }
        }
      }
    }
    finally
    {
      // Отключаем обратный вызов при завершении программы
      enumerator.UnregisterEndpointNotificationCallback(client);
      Console.WriteLine("Отписка от уведомлений произведена.");
    }
  }
}