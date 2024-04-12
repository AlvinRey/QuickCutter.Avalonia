[English](https://github.com/AlvinRey/QuickCutter.Avalonia/blob/main/README.md) | 中文
# QuickCutter
QuickCutter 是一款基于FFmpeg的GUI工具箱，旨在为非专业剪辑用户提供一个简单易用的剪切视频工具。
在很多情况下，用户并不需要专业剪辑软件中的大部分功能，只想要快速地将视频中的一些部分剪切出来，不想要大量的时间对视频进行重新编码。比如，直播切片，游戏素材的筛选等。

## 如何构建并使用（若不想构建，则可以下载Release中的安装包）
 - 打开项目并构建
 - 如果你电脑中没有 [FFmpeg](https://github.com/BtbN/FFmpeg-Builds/releases)，则需要先下载并解压（64位Windows用户推荐下载`win64-gpl-shared`版本）
 - 将FFmpeg中的`bin`文件夹添加到系统环境变量path中，或将`bin`文件夹内的内容复制到`#ApplicationPath/bin/FFmpeg`(如果没有该文件夹，则自己创建一个).
 - 运行 QuickCutter_Avalonia.exe.

## 依赖
 - [Microsoft .NET 8.0 Desktop Runtime](https://download.visualstudio.microsoft.com/download/pr/cb56b18a-e2a6-4f24-be1d-fc4f023c9cc8/be3822e20b990cf180bb94ea8fbc42fe/dotnet-sdk-8.0.101-win-x64.exe)
 - [FFmpeg](https://github.com/BtbN/FFmpeg-Builds/releases)

 ## TODO
 - [x] 预览输出
 - [ ] 为视频添加字幕
 - [ ] 分离音视频
 - [ ] 合并多个视频