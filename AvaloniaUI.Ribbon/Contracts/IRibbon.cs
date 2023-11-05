using System;
using System.Collections.Generic;
using System.Text;

namespace AvaloniaUI.Ribbon.Contracts
{
    public interface IRibbon : IKeyTipHandler
    {
        public bool IsCollapsedPopupOpen { get; set; }

        public void Close();
    }
}