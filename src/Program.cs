using AudioManager.Services;
using NAudio.CoreAudioApi;
using System;

namespace AudioManager
{
    class Program
    {
        public static void Main()
        {
            using var notificationClient = new AudioDeviceNotificationClient();
            var enumerator = new MMDeviceEnumerator();
            enumerator.RegisterEndpointNotificationCallback(notificationClient);

            var commands = new AudioCommands(notificationClient);
            notificationClient.OnDeviceChanged += (json) => Console.WriteLine(json);
            notificationClient.InitializeDevices();

            while (true)
            {
                string? input = Console.ReadLine();
                if (string.IsNullOrEmpty(input)) continue;
                commands.ProcessCommand(input);
            }
        }
    }
}