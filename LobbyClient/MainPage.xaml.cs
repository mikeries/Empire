using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace LobbyClient
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        LobbyService service;
        public static MainPage Current;

        public MainPage()
        {
            this.InitializeComponent();
            Current = this;
            service = new LobbyService();
        }

        public void NotifyUser(string message)
        {
            Output.Text += message + Environment.NewLine;
        }

        public void NotifyUserFromAsyncThread(string strMessage)
        {
            var ignore = Dispatcher.RunAsync(
                CoreDispatcherPriority.Normal, () => NotifyUser(strMessage));
        }

        private void sendButton_Click(object sender, RoutedEventArgs e)
        {
            service.Connect("localhost", "1944");
        }
    }
}
