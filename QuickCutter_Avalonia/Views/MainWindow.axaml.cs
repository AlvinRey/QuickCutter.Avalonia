using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using Avalonia.Styling;
using DynamicData;
using LibVLCSharp.Shared;
using QuickCutter_Avalonia.Handler;
using QuickCutter_Avalonia.Mode;
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
        private static Config _config;
        private MainWindowViewModel? viewModel;
        private double mediaPlayerAspectRatio = 16.0 / 9.0;
        private Action? mVideoViewSizeInit;
        private Project? mSelectedEditingProject;
        private SettingWindow? mSettingWindow;
        #endregion

        public MainWindow()
        {
            InitializeComponent();
            _config = Utils.GetConfig();
            switch (_config.windowStartUpStyles)
            {
                case WindowStartUpStyles.AUTOADJUST:
                    Width = Screens.Primary.Bounds.Width * 0.66;
                    Height = Screens.Primary.Bounds.Height * 0.66;
                    break;
                case WindowStartUpStyles.ALWAYSMAXIMIZE:
                    WindowState = WindowState.Maximized;
                    break;
                case WindowStartUpStyles.HISTORY:
                    if (_config.windowHistoryWidth >= MinWidth && _config.windowHistoryHeight >= MinHeight)
                    {
                        Width = _config.windowHistoryWidth;
                        Height = _config.windowHistoryHeight;
                    }
                    else
                    {
                        Width = MinWidth;
                        Height = MinHeight;
                    }
                    break;
            }
            Loaded += MainWindow_Loaded;
            Unloaded += MainWindow_Unloaded;
            ProjectsList.SelectionChanged += ProjectsList_SelectionChanged;
            OutputFilesDataGrid.SelectionChanged += OutputFilesDataGrid_SelectionChanged;
            MediaPlayerGrid.SizeChanged += MediaPlayerGrid_SizeChanged;
            VideoView.Loaded += VideoView_Loaded;
            SettingButton.Click += SettingButton_Click;
        }
        #region ++Event Handler++

        private void MainWindow_Loaded(object? sender, RoutedEventArgs e)
        {
            viewModel = DataContext as MainWindowViewModel;

            //Core.Initialize();
            //LogHandler.Init();

            //FileHandler.Init(GetStorageProvider());
            //try
            //{
            //    FFmpegHandler.CheckFFmpegIsExist();
            //}
            //catch (Exception ex)
            //{
            //    await MessageBox.ShowAsync(this,ex.Message, "Error", MessageBoxIcon.Error, MessageBoxButton.OK);
            //    Environment.Exit(0);
            //    return;
            //}
        }

        private void MainWindow_Unloaded(object? sender, RoutedEventArgs e)
        {
            _config.windowHistoryWidth = Width;
            _config.windowHistoryHeight = Height;
            //ConfigHandler.SaveConfig(ref _config);
            //ExportHandler.CencelWithAppQuit();
            //LogHandler.Dispose();
        }

        private void ProjectsList_SelectionChanged(object? sender, SelectionChangedEventArgs e)
        {
            if (viewModel is null)
                return;

            if (viewModel.SelectedProjects.Count == 1)
            {
                if(mSelectedEditingProject == null)
                {
                    mSelectedEditingProject = viewModel.SelectedProjects[0];
                    InitEditingArea();
                }
                else if(mSelectedEditingProject == viewModel.SelectedProjects[0])
                {
                    InitEditingArea();
                }
                else
                {
                    ResetEditingArea();
                    mSelectedEditingProject = viewModel.SelectedProjects[0];
                    InitEditingArea();
                }
                
            }
            else // Select more than one Project or do not select anyone
            {
                ResetEditingArea();
                mSelectedEditingProject = null;
            }
        } 

        private void OutputFilesDataGrid_SelectionChanged(object? sender, SelectionChangedEventArgs e)
        {
            if (viewModel is null)
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
                // Because the VideoView has not been loaded at this time, adjusting its Size is meaningless.
                // Pack the current Grid's Size into mVideoViewSizeInit and wait until VideoView completes loading before executing it.
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

        private void DataGridMenu_SelectAll(object? sender, RoutedEventArgs e)
        {
            OutputFilesDataGrid.SelectAll();
        }

        private void DataGridMenu_Delete(object? sender, RoutedEventArgs e)
        {
            if (viewModel is null)
                return;

            List<OutputFile> selectedItems = OutputFilesDataGrid.SelectedItems.OfType<OutputFile>().ToList();

            // Remove the item from data source
            viewModel.SelectedProjects.First().OutputFiles.Remove(selectedItems);

            OutputFilesDataGrid.SelectedItems.Clear();
        }

        private void ProjectList_SelectAll(object? sender, RoutedEventArgs e)
        {
            ProjectsList.SelectAll();
        }

        private void ProjectList_Delete(object? sender, RoutedEventArgs e)
        {
            if (viewModel is null)
                return;

            List<Project> selectedItems = ProjectsList.SelectedItems.OfType<Project>().ToList();
            viewModel.Projects.Remove(selectedItems);
            ProjectsList.SelectedItems.Clear();
        }

        private void InButton_Click(object? sender, RoutedEventArgs e)
        {
            if (viewModel is null || viewModel.SelectedProjects.Count <= 0) return;
            if (viewModel.SelectedOutputFiles.Count == 0)
            {
                viewModel.SelectedProjects[0].AddChild();
                OutputFilesDataGrid.SelectedIndex = viewModel.SelectedProjects[0].OutputFiles.Count - 1;
            }
            // Auto Complate Selected Output Files's In Time
            viewModel.AutoComplateInTime();
        }

        private void OutButton_Click(object? sender, RoutedEventArgs e)
        {
            if (viewModel is null || viewModel.SelectedProjects.Count <= 0) return;
            if (viewModel.SelectedOutputFiles.Count == 0)
            {
                viewModel.SelectedProjects[0].AddChild();
                OutputFilesDataGrid.SelectedIndex = viewModel.SelectedProjects[0].OutputFiles.Count - 1;
            }
            // Auto Complate Selected Output Files's Out Time
            viewModel.AutoComplateOutTime();
        }

        private void SettingButton_Click(object? sender, RoutedEventArgs e)
        {
            if(mSettingWindow is null)
            {
                mSettingWindow = new SettingWindow()
                {
                    DataContext = new SettingWindowViewModel()
                };
                mSettingWindow.WindowStartupLocation = WindowStartupLocation.CenterOwner;
                mSettingWindow.Closed += (s, e) => mSettingWindow = null;
            }
            mSettingWindow.Show();
        }

        private void ToggleButton_OnIsCheckedChanged(object sender, RoutedEventArgs e)
        {
            var app = Application.Current;
            if (app is not null)
            {
                var theme = app.ActualThemeVariant;
                app.RequestedThemeVariant = theme == ThemeVariant.Dark ? ThemeVariant.Light : ThemeVariant.Dark;
            }
        }
        #endregion

        private void InitEditingArea()
        {
            if (viewModel is null || mSelectedEditingProject is null)
                return;

            // Resize Video View
            double videoHeight = mSelectedEditingProject.ImportVideoInfo.AnalysisResult.VideoStreams[0].Height;
            double videoWidth = mSelectedEditingProject.ImportVideoInfo.AnalysisResult.VideoStreams[0].Width;
            mediaPlayerAspectRatio = double.IsNaN(videoWidth / videoHeight) ? mediaPlayerAspectRatio : videoWidth / videoHeight;
            if (MediaPlayerGrid.Bounds.Width >= MediaPlayerGrid.Bounds.Height * mediaPlayerAspectRatio)
            {
                VideoView_ChangeHeight(MediaPlayerGrid.Bounds.Height - 75.0 - 1.0);
            }
            else
            {
                VideoView_ChangeWidth(MediaPlayerGrid.Bounds.Width - 1);
            }

            // Load stuff about Selection
            HeaderTitle.Text = mSelectedEditingProject.ImportVideoInfo.VideoFullName;
            OutputFilesDataGrid.ItemsSource = mSelectedEditingProject.OutputFiles;
            MediaPlayerHandler.LoadMedia(new Uri(viewModel.SelectedProjects[0].MediaFullName));
        }

        private void ResetEditingArea()
        {
            if (viewModel is null)
                return;
            MediaPlayerHandler.ResetMediaPlayer();
            HeaderTitle.Text = null;
            //AudioTrackComboBox.ItemsSource = null;
            //SubtitleTrackComboBox.ItemsSource = null;
            OutputFilesDataGrid.ItemsSource = null;

        }

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
    }
}