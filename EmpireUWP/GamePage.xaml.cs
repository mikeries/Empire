using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.Security.Cryptography;
using Windows.Security.Cryptography.Core;
using Windows.Storage.Streams;
using MySql.Data.MySqlClient;
using EmpireUWP.View;
using EmpireUWP.Network;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace EmpireUWP
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class GamePage : Page
    {
        GameView _game;

        public GamePage()
        {
            this.InitializeComponent();

            var launchArguments = string.Empty;
            _game = MonoGame.Framework.XamlGame<GameView>.Create(launchArguments, Window.Current.CoreWindow, swapChainPanel);

            MyFrame.Navigate(typeof(LoginPage));
        }

    }
}
