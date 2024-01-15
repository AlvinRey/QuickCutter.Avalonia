using Avalonia.Platform.Storage;
using FFMpegCore;
using QuickCutter_Avalonia.Models;
using ReactiveUI;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace QuickCutter_Avalonia.Handler
{
    internal class FileHandler
    {
        #region Private Member
        static private IStorageProvider? mStorageProvider;
        #endregion

        static public void Init(IStorageProvider? sp)
        {
            mStorageProvider = sp;  
        }

        async static public Task<IReadOnlyList<VideoInfo>> ImportVideoFile()
        {
            var list = new List<VideoInfo>();
            if (mStorageProvider is null)
            {
                MessageBus.Current.SendMessage("ImportVideoFile: mStorageProvider is Null.", "LogHandler");
                return list.AsReadOnly();
            }


            var files = await mStorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions()
            {
                Title = "Open File",
                FileTypeFilter = new[] { new FilePickerFileType("VideoAll")
                                            {
                                                Patterns = new []
                                                {
                                                    "*.mp4", "*.mov", "*.mkv"
                                                }
                                            }
                },
                AllowMultiple = true,
            });

            if (files.Count <= 0)
            {
                MessageBus.Current.SendMessage("ImportVideoFile: There are no files selected.", "LogHandler");
                return list.AsReadOnly();
            }


            foreach(var file in files)
            {
                string? videoFullName = file.TryGetLocalPath();
                if (string.IsNullOrEmpty(videoFullName))
                    continue;
                IMediaAnalysis mediaInfo;
                mediaInfo = FFProbe.Analyse(videoFullName);
                list.Add(new VideoInfo() { VideoFullName = videoFullName, AnalysisResult = mediaInfo });
            }
            return list.AsReadOnly();
        }

        async static public Task<string> SelectExportFolder()
        {
            if (mStorageProvider is null)
            {
                MessageBus.Current.SendMessage("SelectExportFolder: mStorageProvider is Null.", "LogHandler");
                return string.Empty;
            }


            var folders = await mStorageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions()
            {
                Title = "Select a Export Folder",
                AllowMultiple = false
            });

            if (folders.Count <= 0)
            {
                MessageBus.Current.SendMessage("SelectExportFolder: There is no folder selected.", "LogHandler");
                return string.Empty;
            }


            string? folderFullName = folders[0].TryGetLocalPath();
            if (!string.IsNullOrEmpty(folderFullName))
                return folderFullName;
            else return string.Empty;
        }
    }
}
