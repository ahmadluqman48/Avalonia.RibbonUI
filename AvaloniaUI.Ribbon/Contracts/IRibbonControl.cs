using Avalonia;
using Avalonia.Input;
using Avalonia.Interactivity;
using AvaloniaUI.Ribbon.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace AvaloniaUI.Ribbon.Contracts
{

    public interface IRibbonControl
    {
        RibbonControlSize Size
        {
            get;
            set;
        }

        RibbonControlSize MinSize
        {
            get;
            set;
        }

        RibbonControlSize MaxSize
        {
            get;
            set;
        }
    }
}