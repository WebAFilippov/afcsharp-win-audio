import { ChildProcess, spawn } from 'node:child_process';
import { EventEmitter } from 'node:events';
import path from 'path';


interface AudioMonitorOptions {
  delay?: number;
  step?: number;
}

export interface IDevice {
  id: string;
  name: string;
  volume: number;
  muted: boolean;
}

export interface IChange {
  id: boolean;
  name: boolean;
  volume: boolean;
  muted: boolean;
}



class AudioDeviceMonitor extends EventEmitter {
  // Определение процесса
  private audioDeviceProcess: ChildProcess | null = null;
  private exePath = path.join('bin', 'Release', 'net6.0', 'win-x64', 'afc#-win-audio.exe');
  // Аргументы инициализации
  private delay: number;
  private stepVolume: number;
  // Парсинг информации
  private parsedInfo: IDevice = { id: '', name: '', volume: 0, muted: false };
  private change: IChange = {
    "id": false,
    "name": false,
    "volume": false,
    "muted": false
  }

  constructor(options?: AudioMonitorOptions) {
    super();

    this.delay = options?.delay !== undefined ? Math.max(options.delay, 100) : 250;
    this.stepVolume = options?.step || 5;

    this.start();
  }

  private start(): void {
    this.audioDeviceProcess = spawn(this.exePath, [this.delay.toString(), this.stepVolume.toString()]);

    if (this.audioDeviceProcess && this.audioDeviceProcess.stdout) {
      this.audioDeviceProcess.stdout.on('data', (data: Buffer) => {
        try {
          const parsedData = JSON.parse(data.toString());
          this.checkChange(parsedData); // Проверка изменения
          this.parsedInfo = parsedData;
          this.emit('change', this.parsedInfo as IDevice, this.change as IChange);
          this.defaultChange();
        } catch (e) {
          this.emit('error', `Failed to parse data: ${e}`);
        }
      });
    } else {
      this.emit('error', 'stdout not available.');
    }

    // Обработка ошибок процесса C#
    this.audioDeviceProcess.stderr?.on('data', (data: Buffer): void => {
      this.emit('C# Error', `Error: ${data.toString()}`);
    });

    this.audioDeviceProcess.on('close', (code: number): void => {
      this.emit('exit', code);
    });

    // Обработка завершения основного процесса Node.js
    process.on('SIGINT', () => {
      console.log('Received SIGINT. Terminating child process...');
      if (this.audioDeviceProcess) {
        this.audioDeviceProcess.kill('SIGTERM'); // Отправка SIGTERM дочернему процессу
      }
      process.exit(); // Завершение основного процесса
    });

    process.on('SIGTERM', () => {
      console.log('Received SIGTERM. Terminating child process...');
      if (this.audioDeviceProcess) {
        this.audioDeviceProcess.kill('SIGTERM'); // Отправка SIGTERM дочернему процессу
      }
      process.exit(); // Завершение основного процесса
    });
  }

  public upVolume(step?: number): void {
    if (this.audioDeviceProcess) {
      if (step) {
        spawn(this.exePath, [step.toString(), "upVolume"]);
      } else {
        spawn(this.exePath, ["upVolume"]);
      }
    } else {
      this.emit('error', 'Process not started.');
    }
  }

  public downVolume(step?: number): void {
    if (this.audioDeviceProcess) {
      if (step) {
        spawn(this.exePath, [step.toString(), 'downVolume']);
      } else {
        spawn(this.exePath, ['downVolume']);
      }
    } else {
      this.emit('error', 'Process not started.');
    }
  }

  public stop(): void {
    if (this.audioDeviceProcess) {
      if (!this.audioDeviceProcess.killed) {
        this.audioDeviceProcess.kill('SIGTERM');
        setTimeout(() => {
          if (!this.audioDeviceProcess?.killed) {
            this.audioDeviceProcess?.kill('SIGKILL');
            this.emit('forceExit', 'Process forcibly terminated.');
          }
        }, 3000);
      } else {
        this.emit('error', 'Process already terminated.');
      }
    } else {
      this.emit('error', 'Process not started.');
    }
  }

  private checkChange(data: IDevice): void {
    for (const key in data) {
      if (data[key as keyof IDevice] !== this.parsedInfo[key as keyof IDevice]) {
        this.change[key as keyof IChange] = true;
      }
    }
  }

  private defaultChange(): void {
    this.change.id = false;
    this.change.name = false;
    this.change.volume = false;
    this.change.muted = false;
  }
}

export default AudioDeviceMonitor




// ===================================================================================================
// ===================================================================================================
// ===================================================================================================
// ===================================================================================================
// ===================================================================================================
// ===================================================================================================
// ===================================================================================================
// ===================================================================================================
// ===================================================================================================
// ===================================================================================================
// ===================================================================================================
// ===================================================================================================



import { createServer } from 'net'

const options = {
  delay: 0,
  step: 0
}
const AudioMonitor = new AudioDeviceMonitor(
  options
);

AudioMonitor.on('change',(deviceInfo, change) => {
  if (change.id) {
    console.log(deviceInfo.id)
  }
  if (change.name) {
    console.log(deviceInfo.name)
  }
  if (change.volume) {
    console.log(deviceInfo.volume)
  }
  if (change.muted) {
    console.log(deviceInfo.muted)
  }
});

const PORT = 1883;
const server = createServer()

server.listen(PORT, function () {
  console.log(`Server listening on port ${PORT}`);
  console.log();
})

