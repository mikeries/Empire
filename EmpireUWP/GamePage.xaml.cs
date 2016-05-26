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

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace EmpireUWP
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class GamePage : Page
    {
        readonly GameView _game;

        public GamePage()
        {
            this.InitializeComponent();

            // Create the game.
            var launchArguments = string.Empty;
            _game = MonoGame.Framework.XamlGame<GameView>.Create(launchArguments, Window.Current.CoreWindow, swapChainPanel);
        }

        private void submitButton_Click(object sender, RoutedEventArgs e)
        {
            string name = Username.Text;
            string password = Md5Encrypt(passwordBox.Password);

            string connectionString = "server=localhost;user=Mike;database=EmpireData;port=1967;password=peanut;SslMode=None";
            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                connection.Open();
                MySqlCommand getCommand = connection.CreateCommand();
                getCommand.CommandText = "SELECT password FROM Users WHERE username='" + name + "'";
                using (MySqlDataReader reader = getCommand.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        string result = reader.GetString("password");
                        if (result == password)
                        {
                            bool success = true;
                        }
                    }
                }
            }

        }

        private string Md5Encrypt(string stringToEncrypt)
        {
            IBuffer buffUtf8Msg = CryptographicBuffer.ConvertStringToBinary(stringToEncrypt, BinaryStringEncoding.Utf8);
            HashAlgorithmProvider objAlgProv = HashAlgorithmProvider.OpenAlgorithm(HashAlgorithmNames.Md5);

            IBuffer buffHash = objAlgProv.HashData(buffUtf8Msg);
            string result = CryptographicBuffer.EncodeToHexString(buffHash);

            return result;
        }
    }
}
