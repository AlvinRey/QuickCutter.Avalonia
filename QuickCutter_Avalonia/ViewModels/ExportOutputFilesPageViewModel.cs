using CommunityToolkit.Mvvm.ComponentModel;
using QuickCutter_Avalonia.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuickCutter_Avalonia.ViewModels
{
    public partial class ExportOutputFilesPageViewModel : ViewModelBase
    {
        [ObservableProperty]
        private string importVideoFileFullName;


        public ExportOutputFilesPageViewModel(Project project) 
        {
            importVideoFileFullName = project.ImportVideoInfo.VideoFullName!;
        }
    }
}
