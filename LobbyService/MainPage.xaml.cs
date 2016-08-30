using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;
using System.Threading.Tasks;
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

namespace LobbyService
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {

        public MainPage()
        {
            this.InitializeComponent();
        }

        private async void startButton_Click(object sender, RoutedEventArgs e)
        {
            await server.Initialize();
        }

        public void NotifyUserFromAsyncThread(string strMessage)
        {
            var ignore = Dispatcher.RunAsync(
                CoreDispatcherPriority.Normal, () => DisplayUpdate(strMessage));
        }

        private void DisplayUpdate(string update)
        {
            textBox.Content += Environment.NewLine + update;
        }

    }
}
