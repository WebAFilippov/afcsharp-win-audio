using NAudio.CoreAudioApi;

namespace AudioManager.Models
{
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
} 