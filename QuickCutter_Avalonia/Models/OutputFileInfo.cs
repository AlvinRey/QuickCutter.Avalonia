using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuickCutter_Avalonia.Models
{
    internal class OutputFileInfo
    {
        public string FileName { get; set; }

        public bool IsTrans {  get; set; }

        public TimeSpan ExportInTime { get; set; }

        public TimeSpan ExportOutTime { get; set; }
    }
}
