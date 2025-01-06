# Audio Device Manager

Консольное приложение для управления аудио устройствами в Windows с помощью .NET.

## Возможности

- Управление громкостью устройств
- Включение/выключение звука
- Мониторинг изменений аудио устройств
- JSON-вывод информации об устройствах

## Команды

| Команда | Описание | Пример |
|---------|----------|--------|
| `setvolume` | Установить громкость по умолчанию | `setvolume 50` |
| `setvolumeid` | Установить громкость для конкретного устройства | `setvolumeid {deviceId} 50` |
| `upvolume` | Увеличить громкость | `upvolume` |
| `downvolume` | Уменьшить громкость | `downvolume` |
| `upvolumeid` | Увеличить громкость для устройства | `upvolumeid {deviceId}` |
| `downvolumeid` | Уменьшить громкость для устройства | `downvolumeid {deviceId}` |
| `setstepvolume` | Установить шаг изменения громкости | `setstepvolume 5` |
| `setmute` | Выключить звук | `setmute` |
| `setmuteid` | Выключить звук для устройства | `setmuteid {deviceId}` |
| `setunmute` | Включить звук | `setunmute` |
| `setunmuteid` | Включить звук для устройства | `setunmuteid {deviceId}` |
| `togglemute` | Переключить состояние звука | `togglemute` |
| `togglemuteid` | Переключить состояние звука для устройства | `togglemuteid {deviceId}` |

## Установка

1. Убедитесь, что у вас установлен [.NET 6.0](https://dotnet.microsoft.com/download) или выше
2. Клонируйте репозиторий: 
``` bash
git clone https://github.com/WebAFilippov/afcsharp-win-audio.git
```
3. Перейдите в директорию проекта: 
``` bash
  cd afcsharp-win-audio
```
4. Соберите проект: 
``` bash
dotnet build
```
5. Запустите проект: 
``` bash
dotnet run
```

## Публикация

Доступны два варианта публикации:

### Зависимая публикация (требует установленный .NET Runtime)

``` bash
dotnet publish -c Release -r win-x64 --self-contained
```
Создает минимальную версию приложения (~1.5MB), но требует установленный .NET Runtime на целевой машине.

### Автономная публикация (полностью независимая)

``` bash
dotnet publish -c Release -r win-x64
```
Создает более крупную версию приложения (~60MB), но не требует установленный .NET Runtime на целевой машине.

Опубликованные файлы можно найти в папке:
``` bash
bin/Release/net6.0/win-x64/publish/
```

### Поддерживаемые платформы

- win-x64 (Windows x64)
- win-x86 (Windows x86)
- win-arm64 (Windows ARM64)

## Зависимости

- [NAudio](https://github.com/naudio/NAudio)
- System.Text.Json

## Лицензия

MIT

## Автор

Alexey Filippov



