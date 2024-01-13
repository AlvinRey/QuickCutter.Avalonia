using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using DynamicData;
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
            FileHandler.Init(GetStorageProvider());

            Loaded += MainWindow_Loaded;
            ProjectsList.SelectionChanged += ProjectsList_SelectionChanged;
            AudioTrackComboBox.PropertyChanged += AudioTrackComboBox_PropertyChanged;
            SubtitleTrackComboBox.PropertyChanged += SubtitleTrackComboBox_PropertyChanged;
            OutputFilesDataGrid.SelectionChanged += OutputFilesDataGrid_SelectionChanged;
        }

        private void AudioTrackComboBox_PropertyChanged(object? sender, AvaloniaPropertyChangedEventArgs e)
        {
            if ("ItemsSource" == e.Property.Name)
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

            // Must remove first. Inorder to make sure viewModel.SelectedProject.First() is the Select One
            if (e.RemovedItems.Count > 0)
            {
                foreach (Project item in e.RemovedItems)
                {
                    viewModel.SelectedProject.Remove(item);
                }
                viewModel.ResetMediaPlayer();
                this.HeaderTitle.Text = null;
                AudioTrackComboBox.ItemsSource = null;
                SubtitleTrackComboBox.ItemsSource = null;
                this.OutputFilesDataGrid.ItemsSource = null;
            }

            if (e.AddedItems.Count > 0)
            {
                // Set Selection
                foreach (Project item in e.AddedItems)
                {
                    viewModel.SelectedProject.Add(item);
                }

                // Load stuff about Selection
                this.HeaderTitle.Text = viewModel.SelectedProject.First().ImportVideoInfo.VideoFullName;
                viewModel.LoadMedia();
                this.OutputFilesDataGrid.ItemsSource = viewModel.SelectedProject.First().OutputFiles;

                // Resize Video View
                double videoHeight = viewModel.SelectedProject.First().ImportVideoInfo.AnalysisResult.VideoStreams[0].Height;
                double videoWidth = viewModel.SelectedProject.First().ImportVideoInfo.AnalysisResult.VideoStreams[0].Width;
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
        }

        private void DeleteProjectButton_Click(object? sender, RoutedEventArgs e)
        {
            if (viewModel is null || ProjectsList.SelectedItems is null)
                return;

            //viewModel.ResetMediaPlayer();


            List<Project> selectedItems = ProjectsList.SelectedItems.OfType<Project>().ToList();
            viewModel.Projects.Remove(selectedItems);
            ProjectsList.SelectedItems.Clear();
        }

        private void OutputFilesDataGrid_SelectionChanged(object? sender, SelectionChangedEventArgs e)
        {
            if (viewModel == null)
                return;

            if (e.RemovedItems.Count > 0)
            {
                foreach (OutputFile item in e.RemovedItems)
                {
                    viewModel.SelectedOutputFiles.Remove(item);
                }
            }

            if (e.AddedItems.Count > 0)
            {
                foreach (OutputFile item in e.AddedItems)
                {
                    viewModel.SelectedOutputFiles.Add(item);
                }
            }
        }

        private void MenuItem_Delete(object? sender, RoutedEventArgs e)
        {
            if (viewModel is null)
                return;

            List<OutputFile> selectedItems = OutputFilesDataGrid.SelectedItems.OfType<OutputFile>().ToList();

            // Remove the item from data source
            viewModel.SelectedProject.First().OutputFiles.Remove(selectedItems);

            OutputFilesDataGrid.SelectedItems.Clear();
        }

        private void MenuItem_SelectAll(object? sender, RoutedEventArgs e)
        {
            OutputFilesDataGrid.SelectAll();
        }
        
        private void InButton_Click(object? sender, RoutedEventArgs e)
        {
            if(viewModel is null || viewModel.SelectedProject.Count <= 0) return;
            if (viewModel.SelectedOutputFiles.Count == 0)
            {
                viewModel.SelectedProject[0].AddChild();
                OutputFilesDataGrid.SelectedIndex = viewModel.SelectedProject[0].OutputFiles.Count - 1;   
            }
            // Auto Complate Selected Output Files's In Time
            viewModel.AutoComplateInTime();
        }

        private void OutButton_Click(object? sender, RoutedEventArgs e)
        {
            if (viewModel is null || viewModel.SelectedProject.Count <= 0) return;
            if (viewModel.SelectedOutputFiles.Count == 0)
            {
                viewModel.SelectedProject[0].AddChild();
                OutputFilesDataGrid.SelectedIndex = viewModel.SelectedProject[0].OutputFiles.Count - 1;
            }
            // Auto Complate Selected Output Files's Out Time
            viewModel.AutoComplateOutTime();
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

        private IStorageProvider? GetStorageProvider()
        {
            var topLevel = TopLevel.GetTopLevel(this);
            return topLevel?.StorageProvider;
        }

        private void Button_Click(object? sender, RoutedEventArgs e)
        {
            Debug.WriteLine(viewModel?.SelectedOutputFiles.Count());
        }
    }
}