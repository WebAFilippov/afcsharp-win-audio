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
            _audioManager = audioManager ?? throw new ArgumentNullException(nameof(audioManager));
        }

        public void ProcessCommand(string input)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(input))
                {
                    throw new ArgumentException("Command input cannot be empty", nameof(input));
                }

                var parts = input.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length == 0) return;

                var command = parts[0].ToLowerInvariant();

                switch (command)
                {
                    case "setvolume":
                        try
                        {
                            if (parts.Length == 2 && float.TryParse(parts[1], out float volume))
                            {
                                _audioManager.SetVolume(volume);
                            }
                            else
                            {
                                throw new ArgumentException("Invalid volume value format");
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Error setting volume: {ex.Message}");
                        }
                        break;

                    case "setvolumeid":
                        try
                        {
                            if (parts.Length == 3 && float.TryParse(parts[2], out float volumeById))
                            {
                                _audioManager.SetVolumeById(parts[1], volumeById);
                            }
                            else
                            {
                                throw new ArgumentException("Invalid device ID or volume value format");
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Error setting volume by ID: {ex.Message}");
                        }
                        break;

                    case "upvolume":
                        try
                        {
                            _audioManager.IncrementVolume();
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Error incrementing volume: {ex.Message}");
                        }
                        break;

                    case "downvolume":
                        try
                        {
                            _audioManager.DecrementVolume();
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Error decrementing volume: {ex.Message}");
                        }
                        break;

                    case "upvolumeid":
                        try
                        {
                            if (parts.Length == 2)
                            {
                                _audioManager.IncrementVolumeById(parts[1]);
                            }
                            else
                            {
                                throw new ArgumentException("Device ID is required");
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Error incrementing volume by ID: {ex.Message}");
                        }
                        break;

                    case "downvolumeid":
                        try
                        {
                            if (parts.Length == 2)
                            {
                                _audioManager.DecrementVolumeById(parts[1]);
                            }
                            else
                            {
                                throw new ArgumentException("Device ID is required");
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Error decrementing volume by ID: {ex.Message}");
                        }
                        break;

                    case "setstepvolume":
                        try
                        {
                            if (parts.Length == 2 && float.TryParse(parts[1], out float step))
                            {
                                _audioManager.SetStepVolume(step);
                            }
                            else
                            {
                                throw new ArgumentException("Invalid step value format");
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Error setting step volume: {ex.Message}");
                        }
                        break;

                    case "setmute":
                        try
                        {
                            _audioManager.SetMute();
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Error setting mute: {ex.Message}");
                        }
                        break;

                    case "setmuteid":
                        try
                        {
                            if (parts.Length == 2)
                            {
                                _audioManager.SetMuteById(parts[1]);
                            }
                            else
                            {
                                throw new ArgumentException("Device ID is required");
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Error setting mute by ID: {ex.Message}");
                        }
                        break;

                    case "setunmute":
                        try
                        {
                            _audioManager.SetUnMute();
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Error setting unmute: {ex.Message}");
                        }
                        break;

                    case "setunmuteid":
                        try
                        {
                            if (parts.Length == 2)
                            {
                                _audioManager.SetUnMuteById(parts[1]);
                            }
                            else
                            {
                                throw new ArgumentException("Device ID is required");
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Error setting unmute by ID: {ex.Message}");
                        }
                        break;

                    case "togglemute":
                        try
                        {
                            _audioManager.ToggleMuted();
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Error toggling mute: {ex.Message}");
                        }
                        break;

                    case "togglemuteid":
                        try
                        {
                            if (parts.Length == 2)
                            {
                                _audioManager.ToggleMutedById(parts[1]);
                            }
                            else
                            {
                                throw new ArgumentException("Device ID is required");
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Error toggling mute by ID: {ex.Message}");
                        }
                        break;

                    default:
                        Console.WriteLine("Unknown command. Available commands: setvolume <value>, setvolumeid <deviceId> <value>, upvolume, downvolume, upvolumeid <deviceId>, downvolumeid <deviceId>, setstep <value>, setmute, setmuteid <deviceId>, setunmute, setunmuteid <deviceId>, togglemute, togglemuteid <deviceId>");
                        break;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Fatal error processing command: {ex.Message}");
            }
        }
    }
}