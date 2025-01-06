namespace AudioManager.Interfaces
{
    public interface IAudioDeviceManager
    {
        void SetStepVolume(float value);
        void SetVolume(float value);
        void SetVolumeById(string deviceId, float value);
        void IncrementVolume();
        void DecrementVolume();
        void IncrementVolumeById(string deviceId);
        void DecrementVolumeById(string deviceId);
        void SetMute();
        void SetMuteById(string deviceId);
        void SetUnMute();
        void SetUnMuteById(string deviceId);
        void ToggleMuted();
        void ToggleMutedById(string deviceId);
        void InitializeDevices();
    }
} 