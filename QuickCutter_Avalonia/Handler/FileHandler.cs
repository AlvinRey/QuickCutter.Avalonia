using Avalonia.Platform.Storage;
using FFMpegCore;
using QuickCutter_Avalonia.Models;
using System;
using System.Collections.Generic;

namespace QuickCutter_Avalonia.Handler
{
    internal class FileHandler
    {
        static public void ImportVideoFile(IReadOnlyList<IStorageFile> files, Action<VideoInfo> importFileAction)
        {
            if (files.Count <= 0 && importFileAction != null)
                return;

            foreach(var file in files)
            {
                string? videoFullName = file.TryGetLocalPath();
                if (string.IsNullOrEmpty(videoFullName))
                    continue;
                IMediaAnalysis mediaInfo;
                mediaInfo = FFProbe.Analyse(videoFullName);
                importFileAction(new VideoInfo() { VideoFullName = videoFullName, AnalysisResult = mediaInfo });
            }
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
