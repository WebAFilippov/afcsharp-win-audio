using System;
using AudioManager.Interfaces;
using AudioManager.Models;

namespace AudioManager.Services
{
    public class AudioCommands
    {
        private readonly IAudioDeviceManager _audioManager;

        public AudioCommands(IAudioDeviceManager audioManager)
        {
            _audioManager = audioManager;
        }

        public void ProcessCommand(string input)
        {
            var parts = input.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 0) return;

            var command = parts[0].ToLowerInvariant();

            switch (command)
            {
                case "setvolume":
                    if (parts.Length == 2 && float.TryParse(parts[1], out float volume))
                    {
                        _audioManager.SetVolume(volume);
                    }
                    break;

                case "setvolumeid":
                    if (parts.Length == 3 && float.TryParse(parts[2], out float volumeById))
                    {
                        _audioManager.SetVolumeById(parts[1], volumeById);
                    }
                    break;

                case "upvolume":
                    _audioManager.IncrementVolume();
                    break;

                case "downvolume":
                    _audioManager.DecrementVolume();
                    break;

                case "upvolumeid":
                    if (parts.Length == 2)
                    {
                        _audioManager.IncrementVolumeById(parts[1]);
                    }
                    break;

                case "downvolumeid":
                    if (parts.Length == 2)
                    {
                        _audioManager.DecrementVolumeById(parts[1]);
                    }
                    break;

                case "setstepvolume":
                    if (parts.Length == 2 && float.TryParse(parts[1], out float step))
                    {
                        _audioManager.SetStepVolume(step);
                    }
                    break;

                case "setmute":
                    _audioManager.SetMute();
                    break;

                case "setmuteid":
                    if (parts.Length == 2)
                    {
                        _audioManager.SetMuteById(parts[1]);
                    }
                    break;

                case "setunmute":
                    _audioManager.SetUnMute();
                    break;

                case "setunmuteid":
                    if (parts.Length == 2)
                    {
                        _audioManager.SetUnMuteById(parts[1]);
                    }
                    break;

                case "togglemute":
                    _audioManager.ToggleMuted();
                    break;

                case "togglemuteid":
                    if (parts.Length == 2)
                    {
                        _audioManager.ToggleMutedById(parts[1]);
                    }
                    break;

                default:
                    Console.WriteLine("Unknown command. Available commands: setvolume <value>, setvolumeid <deviceId> <value>, upvolume, downvolume, upvolumeid <deviceId>, downvolumeid <deviceId>, setstep <value>, setmute, setmuteid <deviceId>, setunmute, setunmuteid <deviceId>, togglemute, togglemuteid <deviceId>");
                    break;
            }
        }
    }
}