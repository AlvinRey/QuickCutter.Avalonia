using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Styling;
using System;

namespace QuickCutter_Avalonia.Styles
{
    internal class GridSplitterWithoutKeyDown : GridSplitter
    {
        protected override Type StyleKeyOverride => typeof(GridSplitter);

        protected override void OnKeyDown(KeyEventArgs e) 
        {
            e.Handled = true;
        }
    }
}
