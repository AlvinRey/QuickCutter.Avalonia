using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuickCutter_Avalonia.Models
{

    public class ExportInfo
    {
        public string ExportDirectory { get; set; }

        public ObservableCollection<OutputFile> OutputFiles { get; set; }
    }
}
