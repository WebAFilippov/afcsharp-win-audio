import { spawn, type ChildProcess } from "child_process";
import EventEmitter from "events";
import path from "path";
import fs from "fs";

export interface AudioMonitorOptions {
  autoStart: boolean;
}

// Определяем интерфейсы для данных устройств
interface AudioDevice {
  id: string;
  name: string;
  dataFlow: string;
  isDefault: boolean;
  volume: number;
  isMuted: boolean;
  channels: number;
  bitDepth: number;
  sampleRate: number;
}

interface ActionDevice {
  id: string;
  name?: string;
  dataFlow?: string;
  isDefault?: boolean;
  volume?: number;
  isMuted?: boolean;
  channels?: number;
  bitDepth?: number;
  sampleRate?: number;
}

interface AudioAction {
  type: 'Initial' | 'Added' | 'Removed' | 'DefaultChanged' | 'VolumeChanged';
  device?: ActionDevice;
}

interface AudioEventData {
  action: AudioAction;
  devices: AudioDevice[];
}

// Добавляем типы для событий
interface AudioMonitorEvents {
  listen: (data: AudioEventData) => void;
  error: (error: string) => void;
}
/**
   * Монитор аудио устройств Windows
   * @class
   * @extends EventEmitter
   * @fires AudioMonitor#listen - Срабатывает при получении данных об аудио устройствах
   * @fires AudioMonitor#error - Срабатывает при возникновении ошибки
   */
class AudioMonitor extends EventEmitter {


  private execPath: string = ''
  private process: ChildProcess | null = null
  private options: AudioMonitorOptions

  constructor(options: AudioMonitorOptions = { autoStart: true }) {
    super()
    this.options = options

    this.execPath = path.join(__dirname, '..', 'bin', 'Release', 'net6.0', 'win-x64', 'publish', 'af-win-audio.exe');


    if (!fs.existsSync(this.execPath)) {
      throw new Error(`Исполняемый файл не найден: ${this.execPath}`);
    }

    if (this.options.autoStart) {
      this.start()
    }
  }

  // Добавляем перегрузку для emit
  emit<K extends keyof AudioMonitorEvents>(
    event: K,
    ...args: Parameters<AudioMonitorEvents[K]>
  ): boolean {
    return super.emit(event, ...args);
  }

  // Добавляем перегрузку для on
  on<K extends keyof AudioMonitorEvents>(
    event: K,
    listener: AudioMonitorEvents[K]
  ): this {
    return super.on(event, listener);
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

  /**
   * Устанавливает общую громкость системы
   * @param volume Уровень громкости (0-100)
   * @throws {Error} Если громкость вне допустимого диапазона
   */
  public setVolume(volume: number): void {
    this.validateVolume(volume);
    if (this.process?.stdin) {
      this.process.stdin.write(`setvolume ${volume}\n`);
    }
  }

  /**
   * Устанавливает громкость для указанного устройства
   * @param deviceId ID устройства
   * @param volume Уровень громкости (0-100)
   * @throws {Error} Если громкость вне допустимого диапазона или ID устройства пустов
   */
  public setVolumeById(deviceId: string, volume: number): void {
    this.validateDeviceId(deviceId);
    this.validateVolume(volume);
    if (this.process?.stdin) {
      this.process.stdin.write(`setvolumeid ${deviceId} ${volume}\n`);
    }
  }

  /**
   * Устанавливает шаг изменения громкости
   * @param value Значение шага (положительное число)
   * @throws {Error} Если значение отрицательное или не является числом
   */
  public setStepVolume(value: number): void {
    if (typeof value !== 'number' || value <= 0) {
      throw new Error('Значение шага должно быть положительным числом');
    }
    if (this.process?.stdin) {
      this.process.stdin.write(`setstepvolume ${value}\n`);
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

  /**
   * Увеличивает громкость указанного устройства
   * @param deviceId ID устройства
   * @throws {Error} Если ID устройства пустой
   */
  public incrementVolumeById(deviceId: string): void {
    this.validateDeviceId(deviceId);
    if (this.process?.stdin) {
      this.process.stdin.write(`upvolumeid ${deviceId}\n`);
    }
  }

  /**
   * Уменьшает громкость указанного устройства
   * @param deviceId ID устройства
   * @throws {Error} Если ID устройства пустой
   */
  public decrementVolumeById(deviceId: string): void {
    this.validateDeviceId(deviceId);
    if (this.process?.stdin) {
      this.process.stdin.write(`downvolumeid ${deviceId}\n`);
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

  /**
   * Включает/выключает звук для указанного устройства
   * @param deviceId ID устройства
   * @throws {Error} Если ID устройства пустой
   */
  public toggleMutedById(deviceId: string): void {
    this.validateDeviceId(deviceId);
    if (this.process?.stdin) {
      this.process.stdin.write(`togglemuteid ${deviceId}\n`);
    }
  }

  private validateDeviceId(deviceId: string): void {
    if (!deviceId?.trim()) {
      throw new Error('ID устройства не может быть пустым');
    }
  }

  private validateVolume(volume: number): void {
    if (typeof volume !== 'number') {
      throw new Error('Громкость должна быть числом');
    }
    if (volume < 0 || volume > 100) {
      throw new Error('Громкость должна быть в диапазоне 0-100');
    }
  }
}

const monitor = new AudioMonitor()

monitor.on('listen', (data) => {
  const { devices, action } = data

  console.log(devices)
  console.log(action)
})




