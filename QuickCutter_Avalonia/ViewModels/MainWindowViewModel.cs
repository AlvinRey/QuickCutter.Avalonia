using Avalonia.Controls;
using QuickCutter_Avalonia.Handler;
using QuickCutter_Avalonia.Mode;
using QuickCutter_Avalonia.Models;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Linq;
using Ursa.Controls;

namespace QuickCutter_Avalonia.ViewModels
{
    public partial class MainWindowViewModel : ReactiveObject
    {
        private Config _config;
        #region Project List
        public ObservableCollection<Project> SelectedProjects { get; set; }
        public ObservableCollection<Project> Projects { get; }
        public IReactiveCommand ImportProjectFileCommand { get; }
        #endregion
         
        #region Media Player
        public VLCMediaplayer VLCMediaplayer { get; }
        public IReactiveCommand PlayOrPauseVideoCommand { get; }
        public IReactiveCommand ForwardCommand { get; }
        public IReactiveCommand BackwardCommand { get; }
        public IReactiveCommand NextFrameCommand { get; }
        #endregion

        #region Output Data Grid
        [Reactive]
        public bool IsExporting { get; set; }
        public ObservableCollection<OutputFile> SelectedOutputFiles { get; set; }
        [Reactive]
        public OutputFile SelectedSingleOutputFile { get; set; }
        public IReactiveCommand AddOutputFilesCommand { get; }
        public IReactiveCommand ExportCommand { get; }
        public IReactiveCommand CencelCommand { get; }
        #endregion

        public MainWindowViewModel()
        {
            _config = Utils.GetConfig();

            #region Init Project List
            Projects = new ObservableCollection<Project>();
            SelectedProjects = new ObservableCollection<Project>();
            ImportProjectFileCommand = ReactiveCommand.Create(
                async () =>
                {
                    var filesFullName = await FileHandler.SelectFiles(FileHandler.SelectAllVideo);
                    int startIndex = Projects.Count;
                    for (int i = 0; i < filesFullName.Count; i++)
                    {
                        Projects.Add(new Project());
                    }
                    for (int i = 0; i < filesFullName.Count; i++)
                    {
                        VideoInfo videoInfo = new VideoInfo()
                        {   
                            VideoFullName = filesFullName[i], 
                            AnalysisResult = await FFmpegHandler.AnaliysisMedia(filesFullName[i]) 
                        };
                        Projects[startIndex + i].SetVideoInfo(videoInfo); 
                    }
                });
            #endregion

            #region Init Media Player
            VLCMediaplayer = new VLCMediaplayer();
            PlayOrPauseVideoCommand = ReactiveCommand.Create(async () =>
            {
                if(SelectedProjects.Count != 1)
                {
                    var title = (string)App.Current.FindResource("Localization.WindowsTitle.Notice");
                    var message = (string)App.Current.FindResource("Localization.Message.SelectOneProject");
                    MessageBox.ShowAsync(message, title,MessageBoxIcon.Information, MessageBoxButton.OK);
                    return;
                }
                MediaPlayerHandler.TogglePlay();
            });
            ForwardCommand = ReactiveCommand.Create(
                () => MediaPlayerHandler.MoveForward(_config.moveStep * 1000));

            BackwardCommand = ReactiveCommand.Create(
                () => MediaPlayerHandler.MoveBackward(_config.moveStep * 1000));

            //NextFrameCommand = ReactiveCommand.Create(
            //    () => MediaPlayer.NextFrame());

            
            #endregion

            #region Init Data Grid
            SelectedOutputFiles = new ObservableCollection<OutputFile>();
            var selectedProjectChanged = Observable.FromEventPattern(SelectedProjects, nameof(SelectedProjects.CollectionChanged));
            var selectedOutputFilesChanged = Observable.FromEventPattern(SelectedOutputFiles, nameof(SelectedOutputFiles.CollectionChanged));

            AddOutputFilesCommand = ReactiveCommand.Create(
                () => 
                    { 
                        SelectedProjects.First().AddChild();
                        SelectedSingleOutputFile = SelectedProjects.First().GetLastChild();
                    },
                selectedProjectChanged.Select(_ => SelectedProjects.Count == 1));

            ExportCommand = ReactiveCommand.Create(
                async () =>
                {
                    string folderFullName = await FileHandler.SelectExportFolder();
                    if(string.IsNullOrEmpty(folderFullName))
                        return;
                    ExportHandler.GenerateExportInfo(folderFullName, SelectedOutputFiles.ToList());
                    IsExporting = true;
                    await ExportHandler.ExecuteFFmpeg();
                    IsExporting = false;
                }, selectedOutputFilesChanged.Select(_ => SelectedOutputFiles.Count > 0));

            CencelCommand = ReactiveCommand.Create(
                () => { ExportHandler.CencelExport(); IsExporting = false; });
            #endregion
        }

        public void AutoComplateInTime()
        {
            if (SelectedOutputFiles.Count <= 0)
                return;
            foreach (var file in SelectedOutputFiles)
            {
                file.Edit_InTime = VLCMediaplayer.CurrentTime;
            }
        }

        public void AutoComplateOutTime()
        {
            if (SelectedOutputFiles.Count <= 0)
                return;
            foreach (var file in SelectedOutputFiles)
            {
                file.Edit_OutTime = VLCMediaplayer.CurrentTime;
            }
        }
    }
}
