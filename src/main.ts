import AudioMonitor from './app'

const af1 = new AudioMonitor({ autoStart: true });
const af2 = new AudioMonitor({ autoStart: true });

setInterval(() => {
af1.setVolume(Math.round(Math.random() * 100));
af2.setVolume(Math.round(Math.random() * 100));
}, 200)