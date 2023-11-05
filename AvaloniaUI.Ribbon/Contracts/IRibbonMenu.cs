using Avalonia.Controls.Primitives;
using System;
using System.Collections.Generic;
using System.Text;

namespace AvaloniaUI.Ribbon.Contracts
{
    public interface IRibbonMenu
    {
        bool IsMenuOpen
        {
            get;
            set;
        }
    }
}
