using Avalonia.Controls;
using QuickCutter_Avalonia.ViewModels;
using System.Collections.Generic;

using QuickCutter_Avalonia.Models;
using QuickCutter_Avalonia.Handler;
using Avalonia.Platform.Storage;
using System.Diagnostics;
using Avalonia.Markup.Xaml;
using Avalonia;
using System;
using LibVLCSharp.Shared;
using System.Data;
using System.Threading;
using Avalonia.Media;
using Avalonia.Data;

namespace QuickCutter_Avalonia.Views
{
    public partial class MainWindow : Window
    {
        #region Private field
        private MainWindowViewModel? viewModel;
        private double mediaPlayerAspectRatio = 16.0 / 9.0;

        //private Control mVolumeSliderPopup;
        //private Control mVolumeButton;
        //private Control mMediaPlayerGrid;
        #endregion


        public MainWindow()
        {
            InitializeComponent();

            //mVolumeSliderPopup = this.FindControl<Control>("VolumeSliderPopup") ?? throw new Exception("Cannot not find VolumSliderPopup by name");
            //mVolumeButton = this.FindControl<Control>("VolumeButton") ?? throw new Exception("Can't not find VolumeButton by name");
            //mMediaPlayerGrid = this.FindControl<Control>("MediaPlayerGrid") ?? throw new Exception("Can't not find MediaPlayerGrid by name");

            FileHandler.TopLevel = GetTopLevel(this);

            this.ProjectsList.SelectionChanged += ProjectsList_SelectionChanged;
            this.Loaded += MainWindow_Loaded;

        }

        private void MainWindow_Loaded(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
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
                // unload stuff about last Selection
                viewModel.UnLoadMdeia();

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

        //private void AdjustPopup()
        //{
        //    var position = mVolumeButton.TranslatePoint(new Point(), mMediaPlayerGrid) ?? throw new Exception("Cannot get TranslatePoint from VolumButton");
        //    mVolumeSliderPopup.Margin = new Thickness(position.X, 0, 0, mMediaPlayerGrid.Bounds.Height - position.Y);
        //}
    }
}