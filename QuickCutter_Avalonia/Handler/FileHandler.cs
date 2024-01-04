using QuickCutter_Avalonia.Models;
using System.IO;
using FFMpegCore;
using Avalonia.Controls;
using Avalonia.Platform.Storage;
using System.Diagnostics;
using System.Threading.Tasks;

namespace QuickCutter_Avalonia.Handler
{
    internal class FileHandler
    {
        static public TopLevel? TopLevel;
        static async public Task<VideoInfo?> ImportVideoFile()
        {
            if (TopLevel == null)
                throw new System.Exception("TopLevel is Null");
            // 启动异步操作以打开对话框。
            var files = await TopLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
            {
                Title = "Open Text File",
                FileTypeFilter = new[] { new FilePickerFileType("VideoAll")
                                            {
                                                Patterns = new []
                                                {
                                                    "*.mp4", "*.mov", "*.mkv"
                                                }
                                            }
                },
                AllowMultiple = false
            });
            if(files.Count<=0 )
                return null;
            string? videoFullName = files[0].TryGetLocalPath();
            if (string.IsNullOrEmpty(videoFullName))
                return null;

            IMediaAnalysis mediaInfo;
            mediaInfo = FFProbe.Analyse(videoFullName!);
            return new VideoInfo() { VideoFullName = videoFullName, AnalysisResult = mediaInfo };
        }

        static public string SelectSaveFolder()
        {
            //// Configure open folder dialog box
            //Microsoft.Win32.OpenFolderDialog dialog = new();

            //dialog.Multiselect = false;
            //dialog.Title = "Select a folder";
            //dialog.DefaultDirectory = historyExportDirectory;
            //// Show open folder dialog box
            //bool? result = dialog.ShowDialog();

            //// Process open folder dialog box results
            //if (result == true)
            //{
            //    historyExportDirectory = dialog.FolderName;
            //    // Get the selected folder
            //    return dialog.FolderName;
            //    //string folderNameOnly = dialog.SafeFolderName;
            //}

            return string.Empty;
        }
    }
}
