using FFMpegCore;
using Microsoft.CodeAnalysis.Diagnostics;
using QuickCutter_Avalonia.Handler;
using QuickCutter_Avalonia.Models;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;


namespace QuickCutter_Avalonia.Mode
{
    public partial class InputMieda : ReactiveObject
    {
        public IMediaAnalysis InputMediaAnalysisResult { get; private set; }
        [Reactive]
        public string MediaFullName { get; private set; }

        public TimeSpan Duration { get; private set; }
        public int Width { get; private set; }
        public int Height { get; private set; }



        [Reactive]
        public bool Loading { get; private set; }

        public ObservableCollection<OutputFile> OutputFiles { get; set; }

        public InputMieda()
        {
            MediaFullName = string.Empty;
            OutputFiles = new ObservableCollection<OutputFile>();
            OutputFiles.CollectionChanged += OutputFiles_CollectionChanged;
            Loading = true;
        }

        private void OutputFiles_CollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            for (var i = 0; i < OutputFiles.Count; i++)
            {
                OutputFiles[i].RowIndex = i + 1;
            }
        }

        public void SetVideoInfo(VideoInfo videoInfo)
        {
            InputMediaAnalysisResult = videoInfo.AnalysisResult;
            MediaFullName = videoInfo.VideoFullName;
            Duration = videoInfo.AnalysisResult.Duration;
            if (videoInfo.AnalysisResult.PrimaryVideoStream != null)
            {
                Width = videoInfo.AnalysisResult.PrimaryVideoStream.Width;
                Height = videoInfo.AnalysisResult.PrimaryVideoStream.Height;
            }

            Task.Run(() =>
            {
                if (!OutputSettingHandler.AudioStreamDictonary.ContainsKey(MediaFullName))
                {
                    List<AudioStreamOriginalInfo> audioStreams = new List<AudioStreamOriginalInfo>();
                    int index = 0;
                    foreach (var audio in InputMediaAnalysisResult.AudioStreams)
                    {
                        audioStreams.Add(new AudioStreamOriginalInfo()
                        {
                            name = FFmpegHandler.GetStreamName(audio, index),
                            absoluteStreamIndex = audio.Index,
                            relativeStreamIndex = index
                        });
                        index++;
                    }
                    OutputSettingHandler.AudioStreamDictonary.Add(MediaFullName, audioStreams);
                }

            }).Await();

            Task.Run(() =>
            {
                if (!OutputSettingHandler.SubtitleStreamDictonary.ContainsKey(MediaFullName))
                {
                    List<SubtitleStreamOriginalInfo> subtitleStreams = new List<SubtitleStreamOriginalInfo>();
                    int index = 0;
                    foreach (var subtitle in InputMediaAnalysisResult.SubtitleStreams)
                    {
                        if (Utils.SubtitleTypeConverter.ContainsKey(subtitle.CodecName))
                        {
                            subtitleStreams.Add(new SubtitleStreamOriginalInfo()
                            {
                                name = FFmpegHandler.GetStreamName(subtitle, index),
                                absoluteStreamIndex = subtitle.Index,
                                relativeStreamIndex = index,
                                isTextType = Utils.SubtitleTypeConverter[subtitle.CodecName],
                            });
                        }
                        index++;
                    }
                    OutputSettingHandler.SubtitleStreamDictonary.Add(MediaFullName, subtitleStreams);
                }
            }).Await();

            Loading = false;
        }

        public void AddChild()
        {
            OutputFiles.Add(new OutputFile(this));
        }

        public OutputFile GetLastChild()
        {
            return OutputFiles.Last();
        }

        public bool HasChild(ref OutputFile item)
        {
            return OutputFiles.Contains(item);
        }
    }
}
