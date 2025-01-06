using System;
using System.Linq;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Encodings.Web;
using System.Text.Unicode;
using AudioManager.Models;

namespace AudioManager.Utils
{
    public static class AudioDeviceJsonSerializer
    {
        private static readonly JsonSerializerOptions Options = new()
        {
            Encoder = JavaScriptEncoder.Create(UnicodeRanges.All),
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        public static string SerializeDeviceChange(string changeType, AudioDeviceInfo? device, IEnumerable<AudioDeviceInfo> allDevices)
        {
            var change = new
            {
                action = new
                {
                    type = changeType,
                    device = device != null ? CreateDeviceDto(device) : new { id = device?.Id }
                },
                devices = allDevices.Select(CreateDeviceDto)
            };

            return JsonSerializer.Serialize(change, Options);
        }

        private static object CreateDeviceDto(AudioDeviceInfo device)
        {
            return new
            {
                id = device.Id,
                name = device.Name,
                dataFlow = device.DataFlow.ToString(),
                isDefault = device.IsDefault,
                volume = device.Volume,
                isMuted = device.IsMuted,
                channels = device.Channels,
                bitDepth = device.BitDepth,
                sampleRate = device.SampleRate
            };
        }
    }
}