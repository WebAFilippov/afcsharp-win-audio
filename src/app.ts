import { spawn, type ChildProcess } from "child_process";
import EventEmitter from "events";
import path from "path";

export interface AudioMonitorOptions {
  autoStart: boolean;
}

class AudioMonitor extends EventEmitter {
  private execPath: string = ''
  private process: ChildProcess | null = null
  private options: AudioMonitorOptions

  constructor(options: AudioMonitorOptions = { autoStart: true }) {
    super()
    this.options = options

    this.execPath = path.join(__dirname, '..', 'bin', 'Release', 'net6.0', 'win-x64', 'publish', 'af-win-audio.exe');

    if (this.options.autoStart) {
      this.start()
    }
  }

  public start() {
    if (this.process) {
      return; // Процесс уже запущен
    }

    this.process = spawn(this.execPath)

    try {
      // Обработка stdout
      if (this.process && this.process.stdout) {
        this.process.stdout.on('data', (dataBuffer: Buffer) => {
          try {
            const data = JSON.parse(dataBuffer.toString())

            this.emit('listen', {
              action: data.action,
              devices: data.devices
            })
          } catch (error) {
            this.emit('error', `Ошибка парсинга JSON: ${error}`)
          }
        })
      }

      // Обработка stderr
      if (this.process && this.process.stderr) {
        this.process.stderr.on('data', (data: Buffer) => {
          this.emit('error', `Ошибка процесса: ${data.toString()}`)
        })
      }

      // Обработка закрытия процесса
      this.process.on('close', (code) => {
        if (code !== 0) {
          this.emit('error', `Процесс завершился с кодом ${code}`)
        }
        this.process = null
      })

    } catch (error) {
      this.emit('error', `Ошибка запуска процесса: ${error}`)
    }
  }

  public stop() {
    if (this.process) {
      this.process.kill()
      this.process = null
    }
  }

  public setVolume(volume: number) {
    if (this.process && this.process.stdin) {
      console.log(volume)
      this.process.stdin.write(`setvolume ${volume}\n`)
    }
  }

  public setVolumeById(deviceId: string, volume: number) {
    if (this.process && this.process.stdin) {
      this.process.stdin.write(`setvolumeid ${deviceId} ${volume}\n`)
    }
  }

  public setStepVolume(value: number) {
    if (this.process && this.process.stdin) {
      this.process.stdin.write(`setstepvolume ${value}\n`)
    }
  }

  public incrementVolume() {
    if (this.process && this.process.stdin) {
      this.process.stdin.write('upvolume\n')
    }
  }

  public decrementVolume() {
    if (this.process && this.process.stdin) {
      this.process.stdin.write('downvolume\n')
    }
  }

  public incrementVolumeById(deviceId: string) {
    if (this.process && this.process.stdin) {
      this.process.stdin.write(`upvolumeid ${deviceId}\n`)
    }
  }

  public decrementVolumeById(deviceId: string) {
    if (this.process && this.process.stdin) {
      this.process.stdin.write(`downvolumeid ${deviceId}\n`)
    }
  }

  public setMute() {
    if (this.process && this.process.stdin) {
      this.process.stdin.write('setmute\n')
    }
  }

  public setMuteById(deviceId: string) {
    if (this.process && this.process.stdin) {
      this.process.stdin.write(`setmuteid ${deviceId}\n`)
    }
  }

  public setUnMute() {
    if (this.process && this.process.stdin) {
      this.process.stdin.write('setunmute\n')
    }
  }

  public setUnMuteById(deviceId: string) {
    if (this.process && this.process.stdin) {
      this.process.stdin.write(`setunmuteid ${deviceId}\n`)
    }
  }

  public toggleMuted() {
    if (this.process && this.process.stdin) {
      this.process.stdin.write('togglemute\n')
    }
  }

  public toggleMutedById(deviceId: string) {
    if (this.process && this.process.stdin) {
      this.process.stdin.write(`togglemuteid ${deviceId}\n`)
    }
  }
}

const monitor = new AudioMonitor()

monitor.on('listen', (data) => {
  const { devices, action } = data
  console.log('devices: ', devices)
  console.log('action: ', action)
})

// Слушаем ошибки
monitor.on('error', (error) => {
  console.error('Ошибка:', error)
})