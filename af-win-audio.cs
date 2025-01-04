// using System;
// using System.Linq;
// using System.Text.Json;
// using System.Text.Encodings.Web;
// using System.Text.Unicode;
// using System.Threading;
// using System.Threading.Tasks;
// using NAudio.CoreAudioApi;
// using NAudio.CoreAudioApi.Interfaces;
// using System.Diagnostics;
// using System.Collections.Generic;

// class AudioDeviceInfo
// {
//     public string Id { get; set; } = string.Empty;
//     public string Name { get; set; } = string.Empty;
//     public string DataFlow { get; set; } = string.Empty;
//     public bool IsDefault { get; set; }
//     public float Volume { get; set; }
//     public bool IsMuted { get; set; }
//     public int Channels { get; set; }
//     public int BitDepth { get; set; }
//     public int SampleRate { get; set; }
// }

// class AudioDeviceNotificationClient : IMMNotificationClient
// {
//     private static MMDeviceEnumerator enumerator = new MMDeviceEnumerator();
//     public event Action<AudioVolumeNotificationData> OnVolumeNotification = delegate { };

//     private static List<AudioDeviceInfo> previousDevices = new List<AudioDeviceInfo>();
//     private Dictionary<string, AudioEndpointVolume> deviceVolumeListeners = new Dictionary<string, AudioEndpointVolume>();

//     public void OnDefaultDeviceChanged(DataFlow flow, Role role, string deviceId)
//     {
//         // Console.WriteLine($"[{DateTime.Now}] OnDefaultDeviceChanged - Flow: {flow}, Role: {role}, DeviceId: {deviceId}");
//         // CheckDevices(new[] { "default" });
//     }

//     public void OnDeviceAdded(string deviceId)
//     {
//         // Console.WriteLine($"[{DateTime.Now}] OnDeviceAdded - DeviceId: {deviceId}");
//         // CheckDevices(new[] { "add" });
//     }

//     public void OnDeviceRemoved(string deviceId)
//     {
//         // Console.WriteLine($"[{DateTime.Now}] OnDeviceRemoved - DeviceId: {deviceId}");
//         // CheckDevices(new[] { "remove" });
//     }

//     public void OnDeviceStateChanged(string deviceId, DeviceState newState)
//     {
//         Console.WriteLine($"[{DateTime.Now}] OnDeviceStateChanged - DeviceId: {deviceId}, NewState: {newState}");
//         try
//         {
//             if ((newState & DeviceState.Active) == DeviceState.Active)
//             {
//                 CheckDevices(new[] { "add" });

//                 var device = enumerator.GetDevice(deviceId);
//                 if (device?.AudioEndpointVolume != null)
//                 {
//                     deviceVolumeListeners[deviceId] = device.AudioEndpointVolume;
//                     device.AudioEndpointVolume.OnVolumeNotification += VolumeNotificationHandler;
//                 }
//             }

//             if ((newState & (DeviceState.Disabled | DeviceState.NotPresent | DeviceState.Unplugged)) != 0)
//             {
//                 if (deviceVolumeListeners.ContainsKey(deviceId))
//                 {
//                     var audioEndpointVolume = deviceVolumeListeners[deviceId];
//                     audioEndpointVolume.OnVolumeNotification -= VolumeNotificationHandler;
//                     deviceVolumeListeners.Remove(deviceId);
//                 }

//                 CheckDevices(new[] { "remove" });
//             }
//         }
//         catch (Exception ex)
//         {
//             Console.WriteLine($"[{DateTime.Now}] ERROR in OnDeviceStateChanged: {ex.Message}");
//             Console.WriteLine(ex.StackTrace);
//         }
//     }

//     public void OnPropertyValueChanged(string deviceId, PropertyKey key)
//     {
//         Console.WriteLine($"[{DateTime.Now}] OnPropertyValueChanged - DeviceId: {deviceId}, PropertyKey: {key}");
//     }

//     public void InitializeDevices()
//     {
//         Console.WriteLine($"[{DateTime.Now}] Initializing devices...");
//         try
//         {
//             var devices = enumerator.EnumerateAudioEndPoints(DataFlow.All, DeviceState.Active).ToList();
//             previousDevices = devices.Select(device => GetDeviceInfo(device)).ToList();
//             PrintDevices(previousDevices, new[] { "initial" });

//             foreach (var device in devices)
//             {
//                 if (device.AudioEndpointVolume != null)
//                 {
//                     deviceVolumeListeners[device.ID] = device.AudioEndpointVolume;
//                     device.AudioEndpointVolume.OnVolumeNotification += VolumeNotificationHandler;
//                 }
//             }
//         }
//         catch (Exception ex)
//         {
//             Console.WriteLine($"[{DateTime.Now}] ERROR in InitializeDevices: {ex.Message}");
//             Console.WriteLine(ex.StackTrace);
//         }
//     }

//     private static void CheckDevices(string[] action)
//     {
//         Console.WriteLine($"[{DateTime.Now}] Checking devices - Action: {string.Join(", ", action)}");
//         try
//         {
//             var devices = enumerator.EnumerateAudioEndPoints(DataFlow.All, DeviceState.Active).ToList();
//             var currentDevices = devices.Select(device => GetDeviceInfo(device)).ToList();

//             if (previousDevices.SequenceEqual(currentDevices)) return;

//             PrintDevices(currentDevices, action);
//             previousDevices = currentDevices;
//         }
//         catch (Exception ex)
//         {
//             Console.WriteLine($"[{DateTime.Now}] ERROR in CheckDevices: {ex.Message}");
//             Console.WriteLine(ex.StackTrace);
//         }
//     }


//     private static AudioDeviceInfo GetDeviceInfo(MMDevice device)
//     {

//             var deviceInfo = new AudioDeviceInfo
//             {
//                 Id = device.ID,
//                 Name = device.FriendlyName,
//                 DataFlow = device.DataFlow.ToString()
//             };

//             var defaultDevice = enumerator.GetDefaultAudioEndpoint(device.DataFlow, Role.Multimedia);
//             deviceInfo.IsDefault = defaultDevice != null && defaultDevice.ID == device.ID;

//             if (device.State == DeviceState.Active && device.AudioEndpointVolume != null)
//             {
//                 deviceInfo.Volume = device.AudioEndpointVolume.MasterVolumeLevelScalar * 100;
//                 deviceInfo.IsMuted = device.AudioEndpointVolume.Mute;

//                 if (device.AudioClient?.MixFormat != null)
//                 {
//                     deviceInfo.Channels = device.AudioClient.MixFormat.Channels;
//                     deviceInfo.BitDepth = device.AudioClient.MixFormat.BitsPerSample;
//                     deviceInfo.SampleRate = device.AudioClient.MixFormat.SampleRate;
//                 }
//             }
//             return deviceInfo;
//     }

//     private static void PrintDevices(List<AudioDeviceInfo> devices, string[] action)
//     {
//         var options = new JsonSerializerOptions
//         {
//             Encoder = JavaScriptEncoder.Create(UnicodeRanges.All),
//             WriteIndented = true
//         };

//         var response = new
//         {
//             data = devices,
//             action = action
//         };

//         Console.WriteLine(JsonSerializer.Serialize(response, options));
//     }

//     private void VolumeNotificationHandler(AudioVolumeNotificationData notificationData)
//     {
//         Console.WriteLine($"[{DateTime.Now}] Volume notification received - Volume: {notificationData.MasterVolume}, Muted: {notificationData.Muted}");
//         OnVolumeNotification?.Invoke(notificationData);
//         CheckDevices(new[] { "volume" });
//     }
// }

// class Program
// {
//     public static async Task Main(string[] args)
//     {
//         try
//         {
//             var notificationClient = new AudioDeviceNotificationClient();
//             MMDeviceEnumerator enumerator = new MMDeviceEnumerator();
//             enumerator.RegisterEndpointNotificationCallback(notificationClient);

//             notificationClient.InitializeDevices();

//             await Task.Delay(Timeout.Infinite);
//         }
//         catch (Exception ex)
//         {
//             Console.WriteLine($"[{DateTime.Now}] FATAL ERROR: {ex.Message}");
//             Console.WriteLine(ex.StackTrace);
//         }
//         finally
//         {
//             Console.WriteLine("Exiting application.");
//         }
//     }
// }


using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Encodings.Web;
using System.Text.Unicode;
using System.Threading;
using NAudio.CoreAudioApi;
using NAudio.CoreAudioApi.Interfaces;

public class AudioDeviceInfo
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public DataFlow DataFlow { get; set; }
    public bool IsDefault { get; set; }
    public float Volume { get; set; }
    public bool IsMuted { get; set; }
    public int Channels { get; set; }
    public int BitDepth { get; set; }
    public int SampleRate { get; set; }
    public MMDevice? Device { get; set; }
}

class AudioDeviceNotificationClient : IMMNotificationClient, IDisposable
{
    private readonly MMDeviceEnumerator _enumerator = new MMDeviceEnumerator();
    public event Action<AudioVolumeNotificationData> OnVolumeNotification = delegate { };

    private readonly Dictionary<string, AudioDeviceInfo> _devices = new Dictionary<string, AudioDeviceInfo>();
    private readonly object _lock = new object();

    public void OnDefaultDeviceChanged(DataFlow flow, Role role, string deviceId)
    {
        lock (_lock)
        {
            foreach (var deviceInfo in _devices.Values)
            {
                deviceInfo.IsDefault = deviceInfo.DataFlow == flow && deviceInfo.Id == deviceId;
            }
        }
    }

    public void OnDeviceAdded(string deviceId)
    {
        // var device = _enumerator.GetDevice(deviceId);
        // if (device == null || device.State != DeviceState.Active)
        //     return;

        // var deviceInfo = new AudioDeviceInfo
        // {
        //     Id = device.ID,
        //     Name = device.FriendlyName,
        //     DataFlow = device.DataFlow,
        //     Device = device
        // };

        // if (device.AudioEndpointVolume != null)
        // {
        //     deviceInfo.Volume = device.AudioEndpointVolume.MasterVolumeLevelScalar * 100;
        //     deviceInfo.IsMuted = device.AudioEndpointVolume.Mute;
        //     device.AudioEndpointVolume.OnVolumeNotification += VolumeNotificationHandler;

        //     _volumeToDeviceId[device.AudioEndpointVolume] = device.ID;
        // }

        // if (device.AudioClient != null && device.AudioClient.MixFormat != null)
        // {
        //     deviceInfo.Channels = device.AudioClient.MixFormat.Channels;
        //     deviceInfo.BitDepth = device.AudioClient.MixFormat.BitsPerSample;
        //     deviceInfo.SampleRate = device.AudioClient.MixFormat.SampleRate;
        // }

        // lock (_lock)
        // {
        //     _devices[device.ID] = deviceInfo;
        // }
    }

    public void OnDeviceRemoved(string deviceId)
    {
        // lock (_lock)
        // {
        //     if (_devices.ContainsKey(deviceId))
        //     {
        //         var deviceInfo = _devices[deviceId];
        //         if (deviceInfo.Device != null && deviceInfo.Device.AudioEndpointVolume != null)
        //         {
        //             deviceInfo.Device.AudioEndpointVolume.OnVolumeNotification -= VolumeNotificationHandler;
        //             _volumeToDeviceId.Remove(deviceInfo.Device.AudioEndpointVolume);
        //         }
        //         _devices.Remove(deviceId);
        //     }
        // }
    }

    public void OnDeviceStateChanged(string deviceId, DeviceState newState)
    {
        // var device = _enumerator.GetDevice(deviceId);
        // if (device == null)
        //     return;

        // if (newState == DeviceState.Active)
        // {
        //     OnDeviceAdded(deviceId);
        // }
        // else if (newState == DeviceState.Disabled || newState == DeviceState.NotPresent || newState == DeviceState.Unplugged)
        // {
        //     OnDeviceRemoved(deviceId);
        // }
    }

    public void OnPropertyValueChanged(string deviceId, PropertyKey key)
    {
        // Handle property value changes if necessary
    }

    public void InitializeDevices()
    {
        var devices = _enumerator.EnumerateAudioEndPoints(DataFlow.All, DeviceState.Active).ToList();

        foreach (var device in devices)
        {
            var deviceInfo = new AudioDeviceInfo
            {
                Id = device.ID,
                Name = device.FriendlyName,
                DataFlow = device.DataFlow,
                Device = device
            };

            if (device.AudioEndpointVolume != null)
            {
                deviceInfo.Volume = device.AudioEndpointVolume.MasterVolumeLevelScalar * 100;
                deviceInfo.IsMuted = device.AudioEndpointVolume.Mute;
                device.AudioEndpointVolume.OnVolumeNotification += (data) => VolumeNotificationHandler(device.ID, data);
            }

            if (device.AudioClient != null && device.AudioClient.MixFormat != null)
            {
                deviceInfo.Channels = device.AudioClient.MixFormat.Channels;
                deviceInfo.BitDepth = device.AudioClient.MixFormat.BitsPerSample;
                deviceInfo.SampleRate = device.AudioClient.MixFormat.SampleRate;
            }

            lock (_lock)
            {
                _devices[device.ID] = deviceInfo;
            }
        }
    }

    private void VolumeNotificationHandler(string deviceID, AudioVolumeNotificationData e)
    {
        _devices[deviceID].Volume = e.MasterVolume * 100;
        _devices[deviceID].IsMuted = e.Muted;
        Console.WriteLine(_devices[deviceID].Name);
        Console.WriteLine(_devices[deviceID].Volume);
        Console.WriteLine(_devices[deviceID].IsMuted);
    }

    public void Dispose()
    {
        _devices.Clear();
        _enumerator.UnregisterEndpointNotificationCallback(this);
    }
}

class Program
{
    public static void Main()
    {
        using (var notificationClient = new AudioDeviceNotificationClient())
        {
            var enumerator = new MMDeviceEnumerator();
            enumerator.RegisterEndpointNotificationCallback(notificationClient);

            notificationClient.InitializeDevices();

            Console.WriteLine("Press any key to exit.");
            Console.ReadKey();
        }
    }
}