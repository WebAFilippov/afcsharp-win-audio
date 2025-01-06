using AudioManager.Models;
using AudioManager.Interfaces;
using AudioManager.Utils;
using NAudio.CoreAudioApi;
using NAudio.CoreAudioApi.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Encodings.Web;
using System.Text.Unicode;

namespace AudioManager.Services
{
    public class AudioDeviceNotificationClient : IMMNotificationClient, IDisposable, IAudioDeviceManager
    {
        private readonly MMDeviceEnumerator _enumerator = new MMDeviceEnumerator();
        private readonly Dictionary<string, AudioDeviceInfo> _devices = new Dictionary<string, AudioDeviceInfo>();
        private readonly object _lock = new object();
        private float _stepVolume = 2f;

        public event Action<string> OnDeviceChanged = delegate { };

        public void SetStepVolume(float value)
    {
        if (value <= 0 || value > 100)
        {
            Console.Error.WriteLine("Step volume must be between 0 and 100");
            return;
        }
        _stepVolume = value;
    }

    public void SetMute()
    {
        try
        {
            var defaultDevice = _enumerator.GetDefaultAudioEndpoint(DataFlow.Render, Role.Multimedia);
            if (defaultDevice?.AudioEndpointVolume != null)
            {
                defaultDevice.AudioEndpointVolume.Mute = true;
            }
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error setting mute: {ex.Message}");
        }
    }

    public void SetMuteById(string deviceId)
    {
        lock (_lock)
        {
            if (_devices.ContainsKey(deviceId) && _devices[deviceId].Device?.AudioEndpointVolume != null)
            {
                _devices[deviceId].Device.AudioEndpointVolume.Mute = true;
            }
            else
            {
                Console.Error.WriteLine($"Device with ID {deviceId} not found");
            }
        }
    }

    public void SetUnMute()
    {
        try
        {
            var defaultDevice = _enumerator.GetDefaultAudioEndpoint(DataFlow.Render, Role.Multimedia);
            if (defaultDevice?.AudioEndpointVolume != null)
            {
                defaultDevice.AudioEndpointVolume.Mute = false;
            }
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error setting unmute: {ex.Message}");
        }
    }

    public void SetUnMuteById(string deviceId)
    {
        lock (_lock)
        {
            if (_devices.ContainsKey(deviceId) && _devices[deviceId].Device?.AudioEndpointVolume != null)
            {
                _devices[deviceId].Device.AudioEndpointVolume.Mute = false;
            }
            else
            {
                Console.Error.WriteLine($"Device with ID {deviceId} not found");
            }
        }
    }

    public void ToggleMuted()
    {
        try
        {
            var defaultDevice = _enumerator.GetDefaultAudioEndpoint(DataFlow.Render, Role.Multimedia);
            if (defaultDevice?.AudioEndpointVolume != null)
            {
                bool currentMute = defaultDevice.AudioEndpointVolume.Mute;
                defaultDevice.AudioEndpointVolume.Mute = !currentMute;
            }
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error toggling mute: {ex.Message}");
        }
    }

    public void ToggleMutedById(string deviceId)
    {
        lock (_lock)
        {
            if (_devices.TryGetValue(deviceId, out var deviceInfo) && 
                deviceInfo?.Device?.AudioEndpointVolume != null)
            {
                bool currentMute = deviceInfo.Device.AudioEndpointVolume.Mute;
                deviceInfo.Device.AudioEndpointVolume.Mute = !currentMute;
            }
            else
            {
                Console.Error.WriteLine($"Device with ID {deviceId} not found");
            }
        }
    }

    public void IncrementVolume()
    {
        try
        {
            var defaultDevice = _enumerator.GetDefaultAudioEndpoint(DataFlow.Render, Role.Multimedia);
            if (defaultDevice?.AudioEndpointVolume != null)
            {
                float currentVolume = defaultDevice.AudioEndpointVolume.MasterVolumeLevelScalar * 100;
                float newVolume = Math.Min(currentVolume + _stepVolume, 100);
                defaultDevice.AudioEndpointVolume.MasterVolumeLevelScalar = newVolume / 100f;
            }
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error incrementing volume: {ex.Message}");
        }
    }

    public void DecrementVolume()
    {
        try
        {
            var defaultDevice = _enumerator.GetDefaultAudioEndpoint(DataFlow.Render, Role.Multimedia);
            if (defaultDevice?.AudioEndpointVolume != null)
            {
                float currentVolume = defaultDevice.AudioEndpointVolume.MasterVolumeLevelScalar * 100;
                float newVolume = Math.Max(currentVolume - _stepVolume, 0);
                defaultDevice.AudioEndpointVolume.MasterVolumeLevelScalar = newVolume / 100f;
            }
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error decrementing volume: {ex.Message}");
        }
    }

    public void IncrementVolumeById(string deviceId)
    {
        lock (_lock)
        {
            if (_devices.TryGetValue(deviceId, out var deviceInfo) && 
                deviceInfo?.Device?.AudioEndpointVolume != null)
            {
                float currentVolume = deviceInfo.Device.AudioEndpointVolume.MasterVolumeLevelScalar * 100;
                float newVolume = Math.Min(currentVolume + _stepVolume, 100);
                deviceInfo.Device.AudioEndpointVolume.MasterVolumeLevelScalar = newVolume / 100f;
            }
            else
            {
                Console.Error.WriteLine($"Device with ID {deviceId} not found");
            }
        }
    }

    public void DecrementVolumeById(string deviceId)
    {
        lock (_lock)
        {
            if (_devices.TryGetValue(deviceId, out var deviceInfo) && 
                deviceInfo?.Device?.AudioEndpointVolume != null)
            {
                float currentVolume = deviceInfo.Device.AudioEndpointVolume.MasterVolumeLevelScalar * 100;
                float newVolume = Math.Max(currentVolume - _stepVolume, 0);
                deviceInfo.Device.AudioEndpointVolume.MasterVolumeLevelScalar = newVolume / 100f;
            }
            else
            {
                Console.Error.WriteLine($"Device with ID {deviceId} not found");
            }
        }
    }

    public void SetVolume(float value)
    {
        if (value < 0 || value > 100)
        {
            Console.Error.WriteLine("Volume must be between 0 and 100");
            return;
        }

        try
        {
            var defaultDevice = _enumerator.GetDefaultAudioEndpoint(DataFlow.Render, Role.Multimedia);
            if (defaultDevice?.AudioEndpointVolume != null)
            {
                defaultDevice.AudioEndpointVolume.MasterVolumeLevelScalar = value / 100f;
            }
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error setting volume: {ex.Message}");
        }
    }

    public void SetVolumeById(string deviceId, float value)
    {
        if (value < 0 || value > 100)
        {
            Console.Error.WriteLine("Volume must be between 0 and 100");
            return;
        }

        lock (_lock)
        {
            if (_devices.TryGetValue(deviceId, out var deviceInfo) && 
                deviceInfo?.Device?.AudioEndpointVolume != null)
            {
                deviceInfo.Device.AudioEndpointVolume.MasterVolumeLevelScalar = value / 100f;
            }
            else
            {
                Console.Error.WriteLine($"Device with ID {deviceId} not found");
            }
        }
    }

    private void NotifyChange(string changeType, string deviceId = "")
    {
        var options = new JsonSerializerOptions
        {
            Encoder = JavaScriptEncoder.Create(UnicodeRanges.All),
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        var deviceList = _devices.Values.Select(d => new
        {
            id = d.Id,
            name = d.Name,
            dataFlow = d.DataFlow.ToString(),
            isDefault = d.IsDefault,
            volume = d.Volume,
            isMuted = d.IsMuted,
            channels = d.Channels,
            bitDepth = d.BitDepth,
            sampleRate = d.SampleRate
        }).ToList();

        object? deviceInfo = null;
        if (changeType == "Removed")
        {
            deviceInfo = new { id = deviceId };
        }
        else
        {
            var targetDevice = deviceId != "" ? _devices.GetValueOrDefault(deviceId) : null;
            if (targetDevice != null)
            {
                deviceInfo = new
                {
                    id = targetDevice.Id,
                    name = targetDevice.Name,
                    dataFlow = targetDevice.DataFlow.ToString(),
                    isDefault = targetDevice.IsDefault,
                    volume = targetDevice.Volume,
                    isMuted = targetDevice.IsMuted,
                    channels = targetDevice.Channels,
                    bitDepth = targetDevice.BitDepth,
                    sampleRate = targetDevice.SampleRate
                };
            }
        }

        var change = new
        {
            action = new
            {
                type = changeType,
                device = deviceInfo
            },
            devices = deviceList
        };

        try 
        {
            string json = JsonSerializer.Serialize(change, options);
            OnDeviceChanged(json);
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error serializing JSON: {ex.Message}");
        }
    }

    public void OnDefaultDeviceChanged(DataFlow flow, Role role, string deviceId)
    {
        lock (_lock)
        {
            // Сначала сбрасываем флаг IsDefault для всех устройств с тем же DataFlow
            foreach (var deviceInfo in _devices.Values.Where(d => d.DataFlow == flow))
            {
                deviceInfo.IsDefault = false;
            }

            // Устанавливаем IsDefault = true только для нового устройства по умолчанию
            if (_devices.ContainsKey(deviceId))
            {
                var device = _devices[deviceId];
                if (device.DataFlow == flow)
                {
                    device.IsDefault = true;
                }
            }

            NotifyChange("DefaultChanged", deviceId);
        }
    }

    public void OnDeviceAdded(string deviceId)
    {
        var device = _enumerator.GetDevice(deviceId);
        if (device == null || device.State != DeviceState.Active)
            return;

        var deviceInfo = new AudioDeviceInfo
        {
            Id = device.ID,
            Name = device.FriendlyName,
            DataFlow = device.DataFlow,
            Device = device
        };

        // Проверяем, является ли устройство устройством по умолчанию
        try
        {
            var defaultDevice = _enumerator.GetDefaultAudioEndpoint(device.DataFlow, Role.Multimedia);
            deviceInfo.IsDefault = defaultDevice.ID == deviceId;
        }
        catch
        {
            deviceInfo.IsDefault = false;
        }

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
            NotifyChange("Added", deviceId);
        }
    }

    public void OnDeviceRemoved(string deviceId)
    {
        lock (_lock)
        {
            if (_devices.ContainsKey(deviceId))
            {
                var deviceInfo = _devices[deviceId];
                if (deviceInfo.Device?.AudioEndpointVolume != null)
                {
                    deviceInfo.Device.AudioEndpointVolume.OnVolumeNotification -= 
                        (data) => VolumeNotificationHandler(deviceId, data);
                }
                _devices.Remove(deviceId);
                NotifyChange("Removed", deviceId);
            }
        }
    }

    public void OnDeviceStateChanged(string deviceId, DeviceState newState)
    {
        if (newState == DeviceState.Active)
        {
            OnDeviceAdded(deviceId);
        }
        else if (newState == DeviceState.Disabled || newState == DeviceState.NotPresent || newState == DeviceState.Unplugged)
        {
            OnDeviceRemoved(deviceId);
        }
    }

    public void OnPropertyValueChanged(string deviceId, PropertyKey key)
    {
        // Handle property value changes if necessary
    }

    public void InitializeDevices()
    {
        var devices = _enumerator.EnumerateAudioEndPoints(DataFlow.All, DeviceState.Active).ToList();

        // Получаем устройства по умолчанию для каждого типа потока
        MMDevice? defaultRenderDevice = null;
        MMDevice? defaultCaptureDevice = null;
        try
        {
            defaultRenderDevice = _enumerator.GetDefaultAudioEndpoint(DataFlow.Render, Role.Multimedia);
        }
        catch { }
        try
        {
            defaultCaptureDevice = _enumerator.GetDefaultAudioEndpoint(DataFlow.Capture, Role.Multimedia);
        }
        catch { }

        foreach (var device in devices)
        {
            var deviceInfo = new AudioDeviceInfo
            {
                Id = device.ID,
                Name = device.FriendlyName,
                DataFlow = device.DataFlow,
                Device = device,
                IsDefault = (device.DataFlow == DataFlow.Render && device.ID == defaultRenderDevice?.ID) ||
                           (device.DataFlow == DataFlow.Capture && device.ID == defaultCaptureDevice?.ID)
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
        NotifyChange("Initial");
    }

    private void VolumeNotificationHandler(string deviceId, AudioVolumeNotificationData e)
    {
        lock (_lock)
        {
            if (_devices.ContainsKey(deviceId))
            {
                _devices[deviceId].Volume = e.MasterVolume * 100;
                _devices[deviceId].IsMuted = e.Muted;
                NotifyChange("VolumeChanged", deviceId);
            }
        }
    }

    public void Dispose()
    {
        _devices.Clear();
        _enumerator.UnregisterEndpointNotificationCallback(this);
    }
    }
} 