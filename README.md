[中文](https://github.com/AlvinRey/QuickCutter.Avalonia/blob/main/README_CN.md) | [English](https://github.com/AlvinRey/QuickCutter.Avalonia/blob/main/README.md)
# QuickCutter
This is a GUI client for FFmpeg, designed to provide a simple and easy-to-use editing tool for people who are not professional editors. 
In many cases, users do not need most of the features of professional editing software, but just want to quickly and simply cut videos without re-encoding. QuickCutter targets these light users and provides them with the most basic video editing functions based on FFmpeg.

## How to use
 - Bulid the project.
 - Download [FFmpeg](https://github.com/BtbN/FFmpeg-Builds/releases) if you do not hava FFmpeg
 - Add the `bin` folder of FFmpeg to environment path or copy the contents of the `bin` folder to `#ApplicationPath/bin/FFmpeg`(create one if it do not exist).
 - Run QuickCutter_Avalonia.exe.

## Requirement
 - [Microsoft .NET 8.0 Desktop Runtime](https://download.visualstudio.microsoft.com/download/pr/cb56b18a-e2a6-4f24-be1d-fc4f023c9cc8/be3822e20b990cf180bb94ea8fbc42fe/dotnet-sdk-8.0.101-win-x64.exe)
 - [FFmpeg](https://github.com/BtbN/FFmpeg-Builds/releases)

 ## TODO
 - [x] Preview output
 - [ ] Add Subitile to the video
 - [ ] Separate audio and video
 - [ ] Merge multiple videos