using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using Avalonia.Styling;
using DynamicData;
using LibVLCSharp.Shared;
using QuickCutter_Avalonia.Handler;
using QuickCutter_Avalonia.Models;
using QuickCutter_Avalonia.ViewModels;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Ursa.Controls;


namespace QuickCutter_Avalonia.Views
{
    public partial class MainWindow : Window
    {
        #region Private field
        private MainWindowViewModel? viewModel;
        private double mediaPlayerAspectRatio = 16.0 / 9.0;
        private Action? mVideoViewSizeInit;
        #endregion

        public MainWindow()
        {
            InitializeComponent();

            Width = Screens.Primary.Bounds.Width * 0.66;
            Height = Screens.Primary.Bounds.Height * 0.66;


            Loaded += MainWindow_Loaded;
            Unloaded += MainWindow_Unloaded;
            ProjectsList.SelectionChanged += ProjectsList_SelectionChanged;
            AudioTrackComboBox.PropertyChanged += AudioTrackComboBox_PropertyChanged;
            SubtitleTrackComboBox.PropertyChanged += SubtitleTrackComboBox_PropertyChanged;
            OutputFilesDataGrid.SelectionChanged += OutputFilesDataGrid_SelectionChanged;
            MediaPlayerGrid.SizeChanged += MediaPlayerGrid_SizeChanged;
            VideoView.Loaded += VideoView_Loaded;

        }

        #region ++Event Handler++

        private void MainWindow_Loaded(object? sender, RoutedEventArgs e)
        {
            viewModel = DataContext as MainWindowViewModel;

            Core.Initialize();
            FileHandler.Init(GetStorageProvider());
            ExportHandler.Setup();
            LogHandler.Init();
        }

        private void MainWindow_Unloaded(object? sender, RoutedEventArgs e)
        {
            ExportHandler.CencelWithAppQuit();
            LogHandler.Dispose();
            viewModel?.Dispose();
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
                HeaderTitle.Text = null;
                AudioTrackComboBox.ItemsSource = null;
                SubtitleTrackComboBox.ItemsSource = null;
                OutputFilesDataGrid.ItemsSource = null;
            }

            if (e.AddedItems.Count > 0)
            {
                // Set Selection
                foreach (Project item in e.AddedItems)
                {
                    viewModel.SelectedProject.Add(item);
                }

                // Load stuff about Selection
                HeaderTitle.Text = viewModel.SelectedProject.First().ImportVideoInfo.VideoFullName;
                viewModel.LoadMedia();
                OutputFilesDataGrid.ItemsSource = viewModel.SelectedProject.First().OutputFiles;

                // Resize Video View
                double videoHeight = viewModel.SelectedProject.First().ImportVideoInfo.AnalysisResult.VideoStreams[0].Height;
                double videoWidth = viewModel.SelectedProject.First().ImportVideoInfo.AnalysisResult.VideoStreams[0].Width;
                mediaPlayerAspectRatio = double.IsNaN(videoWidth / videoHeight) ? mediaPlayerAspectRatio : videoWidth / videoHeight;
                if (MediaPlayerGrid.Bounds.Width >= MediaPlayerGrid.Bounds.Height * mediaPlayerAspectRatio)
                {
                    VideoView_ChangeHeight(MediaPlayerGrid.Bounds.Height - 75.0 - 1.0);
                }
                else
                {
                    VideoView_ChangeWidth(MediaPlayerGrid.Bounds.Width - 1);
                }
            }
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

        private void MediaPlayerGrid_SizeChanged(object? sender, SizeChangedEventArgs e)
        {
            if(!VideoView.IsLoaded)
            {
                mVideoViewSizeInit = () =>
                {
                    if (e.HeightChanged)
                    {
                        VideoView_ChangeHeight(e.NewSize.Height - 75.0 - 1);
                    }
                    if (e.WidthChanged)
                    {
                        VideoView_ChangeWidth(e.NewSize.Width - 1);
                    }
                };
                return;
            }

            if (e.HeightChanged)
            {
                VideoView_ChangeHeight(e.NewSize.Height - 75.0 - 1);
            }
            if (e.WidthChanged)
            {
                VideoView_ChangeWidth(e.NewSize.Width - 1);
            }
        }

        private void VideoView_Loaded(object? sender, RoutedEventArgs e)
        {
            mVideoViewSizeInit?.Invoke();
        }

        private void DeleteProjectButton_Click(object? sender, RoutedEventArgs e)
        {
            if (viewModel is null || ProjectsList.SelectedItems is null || viewModel.SelectedProject.Count <= 0)
                return;

            MessageBoxResult result = MessageBoxResult.Yes;
            //if (viewModel.SelectedProject[0].OutputFiles.Count > 0)
            //{
            //    result = await MessageBox.ShowAsync("This Project has Output Files£¬Are you sure to delete this Project?", "Warning", MessageBoxIcon.Warning, MessageBoxButton.YesNo);
            //}

            if (result == MessageBoxResult.Yes)
            {
                Debug.WriteLine("Deleting project");
                List<Project> selectedItems = ProjectsList.SelectedItems.OfType<Project>().ToList();
                viewModel.Projects.Remove(selectedItems);
                ProjectsList.SelectedItems.Clear();
                Debug.WriteLine("Deleted project");
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
            if (viewModel is null || viewModel.SelectedProject.Count <= 0) return;
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
        #endregion

        private void VideoView_ChangeHeight(double height)
        {
            if (MediaPlayerGrid.Bounds.Width >= height * mediaPlayerAspectRatio)
            {
                VideoView.Height = height;
                VideoView.Width = height * mediaPlayerAspectRatio;
            }
        }

        private void VideoView_ChangeWidth(double width)
        {
            if (MediaPlayerGrid.Bounds.Height > width / mediaPlayerAspectRatio)
            {
                VideoView.Width = width;
                VideoView.Height = VideoView.Width / mediaPlayerAspectRatio;
            }
        }

        private IStorageProvider? GetStorageProvider()
        {
            var topLevel = TopLevel.GetTopLevel(this);
            return topLevel?.StorageProvider;
        }

        private void Button_Click(object? sender, RoutedEventArgs e)
        {
            VideoView.Width += 10;

        }
    }
}