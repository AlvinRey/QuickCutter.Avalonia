using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using LibVLCSharp.Shared;
using QuickCutter_Avalonia.Handler;
using QuickCutter_Avalonia.Models;
using QuickCutter_Avalonia.ViewModels;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;



namespace QuickCutter_Avalonia.Views
{
    public partial class MainWindow : Window
    {
        #region Private field
        private MainWindowViewModel? viewModel;
        private double mediaPlayerAspectRatio;
        #endregion


        public MainWindow()
        {
            InitializeComponent();
            Loaded += MainWindow_Loaded;
            AddItemButton.Click += OpenFileDialog;
            ProjectsList.SelectionChanged += ProjectsList_SelectionChanged;
            AudioTrackComboBox.PropertyChanged += AudioTrackComboBox_PropertyChanged;
            SubtitleTrackComboBox.PropertyChanged += SubtitleTrackComboBox_PropertyChanged;
            OutputFilesDataGrid.SelectionChanged += OutputFilesDataGrid_SelectionChanged;
        }

        private void AudioTrackComboBox_PropertyChanged(object? sender, AvaloniaPropertyChangedEventArgs e)
        {
            if("ItemsSource" == e.Property.Name)
            {
                viewModel?.SelectCurrentAudioTrack();
            }
        }

        private void SubtitleTrackComboBox_PropertyChanged(object? sender, AvaloniaPropertyChangedEventArgs e)
        {
            if ("ItemsSource" == e.Property.Name)
            {
                viewModel?.SelectCurrentSubtitleTrack();
            }
        }

        private void MainWindow_Loaded(object? sender, RoutedEventArgs e)
        {
            viewModel = DataContext as MainWindowViewModel;
            Core.Initialize();
        }

        private void ProjectsList_SelectionChanged(object? sender, SelectionChangedEventArgs e)
        {
            if (viewModel == null)
                return;
            if (e.AddedItems.Count > 0)
            {
                // Set Selection
                viewModel.SelectedProject = (Project)e.AddedItems[0]!;

                // Load stuff about Selection
                this.HeaderTitle.Text = viewModel.SelectedProject.ImportVideoInfo.VideoFullName;
                viewModel.LoadMedia();
                this.OutputFilesDataGrid.ItemsSource = viewModel.SelectedProject.OutputFiles;

                // Resize Video View
                double videoHeight = viewModel.SelectedProject.ImportVideoInfo.AnalysisResult.VideoStreams[0].Height;
                double videoWidth = viewModel.SelectedProject.ImportVideoInfo.AnalysisResult.VideoStreams[0].Width;
                mediaPlayerAspectRatio = double.IsNaN(videoWidth / videoHeight) ? 16.0 / 9.0 : videoWidth / videoHeight;
                if (this.VideoGrid.Bounds.Width > this.VideoView.Height * mediaPlayerAspectRatio)
                {
                    this.VideoView.Height = this.VideoGrid.Bounds.Height;
                    this.VideoView.Width = this.VideoView.Height * mediaPlayerAspectRatio;
                }
                else
                {
                    this.VideoView.Width = this.VideoGrid.Bounds.Width;
                    this.VideoView.Height = this.VideoView.Width / mediaPlayerAspectRatio;
                }
            }
            else
            {
                viewModel.SelectedProject = null;
            }
        }   

        private void VideoGrid_SizeChanged(object? sender, SizeChangedEventArgs e)
        {
            if (viewModel == null)
                return;
            if (e.WidthChanged)
            {
                Debug.WriteLine("WidthChanged, CurWidth: {0} | Grid_SizeChanged", e.NewSize.Width);
                if (e.NewSize.Width <= e.NewSize.Height * mediaPlayerAspectRatio)
                {
                    this.VideoView.Width = e.NewSize.Width;
                    this.VideoView.Height = this.VideoView.Width / mediaPlayerAspectRatio;
                }
            }
            if (e.HeightChanged)
            {
                Debug.WriteLine("HeightChanged, CurHeight: {0} | Grid_SizeChanged", e.NewSize.Height);

                if (e.NewSize.Width >= e.NewSize.Height * mediaPlayerAspectRatio)
                {
                    this.VideoView.Height = e.NewSize.Height;
                    this.VideoView.Width = e.NewSize.Height * mediaPlayerAspectRatio;
                }
            }
        }

        private void DeleteProjectButton_Click(object? sender, RoutedEventArgs e)
        {
            if (viewModel == null || viewModel.SelectedProject == null)
                return;
            if (viewModel.Projects.Count > 1)
            {
                viewModel.Projects.Remove(viewModel.SelectedProject!);
                ProjectsList.SelectedIndex = 0;
            }
            else
            {
                viewModel.ResetMediaPlayer();
                this.HeaderTitle.Text = null;
                AudioTrackComboBox.ItemsSource = null;
                SubtitleTrackComboBox.ItemsSource = null;
                this.OutputFilesDataGrid.ItemsSource = null;
                viewModel.Projects.Remove(viewModel.SelectedProject!);
            }
        }
        private void OutputFilesDataGrid_SelectionChanged(object? sender, SelectionChangedEventArgs e)
        {
            if (viewModel == null)
                return;
            if (e.AddedItems.Count > 0)
            {
                //viewModel.SelectedOutputFiles = e.AddedItems.OfType<OutputFile>().ToList();
                //viewModel.SelectedOutputFiles = e.AddedItems.Cast<OutputFile>().ToList();
                foreach (OutputFile item in e.AddedItems)
                {
                    viewModel.SelectedOutputFiles.Add(item);
                }
            }
            if(e.RemovedItems.Count > 0)
            {
                foreach (OutputFile item in e.RemovedItems)
                {
                    viewModel.SelectedOutputFiles.Remove(item);
                }
            }
        }

        private async void OpenFileDialog(object? sender, RoutedEventArgs args)
        {
            IStorageProvider? sp = GetStorageProvider();
            if (sp is null) return;
            var result = await sp.OpenFilePickerAsync(new FilePickerOpenOptions()
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
            if (viewModel == null)
                return;
            FileHandler.ImportVideoFile(result, viewModel.ImportProjectFile);
        }

        private IStorageProvider? GetStorageProvider()
        {
            var topLevel = TopLevel.GetTopLevel(this);
            return topLevel?.StorageProvider;
        }

        private void Button_Click(object? sender, RoutedEventArgs e)
        {
            Debug.WriteLine(viewModel?.SelectedOutputFiles.Count);
        }

        private void MenuItem_Click(object? sender, RoutedEventArgs e)
        {
            if (viewModel == null || viewModel.SelectedOutputFiles.Count > 0)
                return;
            foreach(OutputFile file in viewModel.SelectedOutputFiles)
            {
                if(viewModel.SelectedProject!.OutputFiles.Contains(file))
                {
                    viewModel.SelectedProject!.OutputFiles.Remove(file);
                }
            }
        }
    }
}