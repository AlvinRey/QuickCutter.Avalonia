using Avalonia.Platform.Storage;
using FFMpegCore;
using QuickCutter_Avalonia.Mode;
using ReactiveUI;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace QuickCutter_Avalonia.Handler
{
    internal class FileHandler
    {
        #region Static Member
        static public FilePickerOpenOptions SelectAllVideo = new FilePickerOpenOptions()
        {
            Title = "Open File",
            FileTypeFilter = new[] { new FilePickerFileType("VideoAll")
                                                    {
                                                        Patterns = new []
                                                        {
                                                            "*.mp4", "*.mov", "*.mkv", "*.ts"
                                                        }
                                                    }
                                                },
            AllowMultiple = true
        };

        #endregion

        #region Private Member
        static private IStorageProvider? mStorageProvider;
        #endregion

        static public void Init(IStorageProvider? sp)
        {
            mStorageProvider = sp;
        }

        async static public Task<IReadOnlyList<string>> SelectFiles(FilePickerOpenOptions openOptions)
        {
            List<string> filesFullName = new List<string>();
            if (mStorageProvider is null)
            {
                Utils.SaveLog("ImportVideoFile: mStorageProvider is Null.");
                return filesFullName.AsReadOnly();
            }

            var files = await mStorageProvider.OpenFilePickerAsync(openOptions);

            if (files.Count <= 0)
            {
                return filesFullName.AsReadOnly();
            }

            foreach (var file in files)
            {
                string? videoFullName = file.TryGetLocalPath();
                if (string.IsNullOrEmpty(videoFullName))
                    continue;
                filesFullName.Add(videoFullName);
            }

            return filesFullName.AsReadOnly();
        }

        async static public Task<string> SelectExportFolder()
        {
            if (mStorageProvider is null)
            {
                Utils.SaveLog("SelectExportFolder: mStorageProvider is Null.");
                return string.Empty;
            }
            var folders = await mStorageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions()
            {
                Title = "Select a Export Folder",
                AllowMultiple = false
            });

            if (folders.Count <= 0)
            {
                return string.Empty;
            }


            string? folderFullName = folders[0].TryGetLocalPath();
            if (!string.IsNullOrEmpty(folderFullName))
                return folderFullName;
            else return string.Empty;
        }
    }
}
