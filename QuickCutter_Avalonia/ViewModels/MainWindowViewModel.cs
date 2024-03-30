using Avalonia.Controls;
using QuickCutter_Avalonia.Handler;
using QuickCutter_Avalonia.Models;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using Ursa.Controls;

namespace QuickCutter_Avalonia.ViewModels
{
    public partial class MainWindowViewModel : ReactiveObject
    {
        #region Project List
        public ObservableCollection<InputMieda> SelectedProjects { get; set; }
        public ObservableCollection<InputMieda> Projects { get; }
        public ReactiveCommand<Unit, Unit> ImportProjectFileCommand { get; }
        #endregion

        #region Media Player
        public VlcMediaplayerViewModel VlcMediaplayerViewModel { get; }
        public ReactiveCommand<Unit, Unit> PlayOrPauseVideoCommand { get; }

        #endregion

        #region Output Data Grid
        public ObservableCollection<OutputFile> SelectedOutputFiles { get; set; }
        [Reactive]
        public OutputFile SelectedSingleOutputFile { get; set; }
        #endregion

        #region Output Setting
        public OutputSettingViewModel OutputSettingVm { get; set; }
        #endregion

        #region Buttom Tool Bar
        public ReactiveCommand<Unit, Unit> AddOutputFilesCommand { get; }

        [Reactive] public bool IsExporting { get; set; }

        [Reactive] public string FileNameProcessing { get; set; } = string.Empty;
        [Reactive] public double ProcessingPercent { get; set; }

        public ReactiveCommand<Unit, Unit> ExportCommand { get; }
        public ReactiveCommand<Unit, Unit> CencelCommand { get; }


        #endregion

        public MainWindowViewModel()
        {
            var config = Utils.GetConfig();

            #region Init Project List
            Projects = new ObservableCollection<InputMieda>();
            SelectedProjects = new ObservableCollection<InputMieda>();
            ImportProjectFileCommand = ReactiveCommand.Create(() =>
                {
                    ImportViedoAsync().Await();
                });
            #endregion

            #region Init Media Player
            VlcMediaplayerViewModel = new VlcMediaplayerViewModel();
            PlayOrPauseVideoCommand = ReactiveCommand.Create(() =>
            {
                if (SelectedProjects.Count != 1)
                {
                    var title = (string)App.Current.FindResource("Localization.WindowsTitle.Notice");
                    var message = (string)App.Current.FindResource("Localization.Message.SelectOneProject");
                    _ = MessageBox.ShowAsync(message, title, MessageBoxIcon.Information, MessageBoxButton.OK);
                    return;
                }
                MediaPlayerHandler.TogglePlay();
            });

            #endregion

            #region Init Data Grid
            SelectedOutputFiles = new ObservableCollection<OutputFile>();
            var selectedProjectChanged = Observable.FromEventPattern(SelectedProjects, nameof(SelectedProjects.CollectionChanged));
            var selectedOutputFilesChanged = Observable.FromEventPattern(SelectedOutputFiles, nameof(SelectedOutputFiles.CollectionChanged));

            AddOutputFilesCommand = ReactiveCommand.Create(
                () =>
                    {
                        SelectedProjects.First().AddChild();
                        //Auto select the newest one
                        SelectedSingleOutputFile = SelectedProjects.First().GetLastChild();
                    },
                selectedProjectChanged.Select(_ => SelectedProjects.Count == 1));

            ExportCommand = ReactiveCommand.Create(
                ()=> ExportOutputFilesAsync().Await(), 
            selectedOutputFilesChanged.Select(_ => SelectedOutputFiles.Count > 0));

            CencelCommand = ReactiveCommand.Create(ExportHandler.CencelExport);
            #endregion

            #region Output Setting
            OutputSettingVm = new OutputSettingViewModel();
            SelectedOutputFiles.CollectionChanged += (sender, args) =>
            {
                if (SelectedOutputFiles.Count == 1)
                {
                    Debug.WriteLine($"Load OutputSetting @ {SelectedOutputFiles.First().OutputSetting.GetHashCode()}");
                    OutputSettingVm.LoadDisplayOutputSetting(SelectedOutputFiles.First().OutputSetting);
                }
                else
                    OutputSettingVm.UnLoadDisplayOutputSetting();
            };
            #endregion
        }

        public void AutoComplateInTime()
        {
            if (SelectedOutputFiles.Count <= 0)
                return;
            foreach (var file in SelectedOutputFiles)
            {
                file.EditInTime = VlcMediaplayerViewModel.CurrentTime;
            }
        }

        public void AutoComplateOutTime()
        {
            if (SelectedOutputFiles.Count <= 0)
                return;
            foreach (var file in SelectedOutputFiles)
            {
                file.EditOutTime = VlcMediaplayerViewModel.CurrentTime;
            }
        }

        async Task ImportViedoAsync()
        {
            var filesFullNames = await FileHandler.SelectFiles(FileHandler.SelectAllVideo);
            var tasks = new List<Task>();
            foreach (var fileFullName in filesFullNames) 
            {
                tasks.Add(AnaliysisMediaAndImportAsync(fileFullName));
            }
            await Task.WhenAll(tasks);
        }

        async Task AnaliysisMediaAndImportAsync(string fileFullName)
        {
            var project = new InputMieda();
            Projects.Add(project);
            var result = await FFmpegHandler.AnaliysisMedia(fileFullName);
            project.SetVideoInfo(new VideoInfo() { VideoFullName = fileFullName, AnalysisResult = result });
        }

        async Task ExportOutputFilesAsync()
        {
            Debug.WriteLine($"ExportOutputFilesAsync() runs on Thread {Environment.CurrentManagedThreadId}");
            string folderFullName = await FileHandler.SelectExportFolder();
            if (string.IsNullOrEmpty(folderFullName))
                return;
            IsExporting = true;
            var outputList = SelectedOutputFiles.ToList();
            ExportHandler.ExecuteFFmpeg(folderFullName, outputList,
                fileName => FileNameProcessing = fileName,
                percent => ProcessingPercent = percent)
                .Await(() => IsExporting = false, e => 
                { 
                    IsExporting = false; 
                    Debug.WriteLine(e.Message); 
                    Utils.SaveLog(e.Message); 
                    foreach (var output in outputList)
                    {
                        Utils.DeleteFile(Path.Combine(folderFullName, output.OutputFileName));
                    }
                });
        }
    }
}
