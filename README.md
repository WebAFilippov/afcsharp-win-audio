# afc#-win-audio

Это приложение на C#, которое отслеживает изменения аудиоустройств и регулирует громкость на системах Windows. Оно использует библиотеку NAudio для отслеживания устройства по умолчанию и уведомления об изменениях громкости. Также программа позволяет увеличить или уменьшить громкость через командную строку.

## Возможности

- Отслеживает изменения основного мультимедийного аудиоустройства.
- Обеспечивает уведомления в реальном времени при изменении громкости.
- Может увеличивать или уменьшать громкость через командные аргументы.
- Выводит информацию об устройстве и изменения громкости в формате JSON.

## Требования

## Требования

- [.NET 6.0 или выше](https://dotnet.microsoft.com/download/dotnet/6.0).
- Библиотека NAudio.

## Установка

1. Установите необходимую версию .NET.
  ```bash
  `dotnet build`
  `dotnet publish -c Release -r win-x64 --self-contained false`


- Это создаст .exe файл, который можно будет найти по следующему пути: `./bin/Debug/net6.0/win-x64/publish/afc#-win-audio.exe` (или `./bin/Release/net6.0/win-x64/publish/afc#-win-audio.exe` в зависимости от конфигурации сборки).

## Вывод

- Программа выводит информацию об устройстве и изменениях громкости в формате JSON, который включает следующие данные:

  id: ID устройства.
  name: Название устройства.
  volume: Текущий уровень громкости в процентах.
  muted: Указывает, отключен ли звук (true/false).

## Пример JSON вывода

```json
{
  "id": "ID устройства.",
  "name": "Название устройства.",
  "volume": "Текущий уровень громкости в процентах.",
  "muted": "Указывает, отключен ли звук (true/false)."
}
```