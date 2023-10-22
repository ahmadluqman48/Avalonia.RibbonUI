using CommunityToolkit.Mvvm.ComponentModel;

using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace AvaloniaUI.Ribbon.Samples.ViewModels
{
    public partial class MainWindowViewModel : ViewModelBase
    {
        [ObservableProperty]
        private string _help = "Help requested!";
        [ObservableProperty]
        private string _lastActionText = "none";
        [ObservableProperty]
        private bool _showContextualGroup1 = true;
        [ObservableProperty]
        private bool _showContextualGroup2 = false;
        [ObservableProperty]
        private bool _showContextualGroup3 = false;


      
        public void HelpCommand(object parameter)
        {
            Console.WriteLine(Help);
            LastActionText = Help;
        }

        public void OnClickCommand(object parameter)
        {
            string paramString = "[NO CONTENT]";

            if (parameter != null)
            {
                if (parameter is string str)
                    paramString = str;
                else
                    paramString = parameter.ToString();
            }

            Console.WriteLine("OnClickCommand invoked: " + paramString);
            LastActionText = paramString;
        }
    }
}