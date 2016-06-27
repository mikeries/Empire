using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Security.Cryptography;
using Windows.Security.Cryptography.Core;
using Windows.Storage.Streams;

namespace EmpireUWP
{
    static class LoginHelper
    {
        private const string sqlServer = "ries.asuscomm.com";
        private const int sqlPort = 1967;
        private const string sqlUser = "Mike";
        private const string sqlPassword = "peanut";
        private const string sqlDatabase = "EmpireData";

        internal static bool TryLogin(string name, string password)
        {
            string _name = name;
            string _password = Md5Encrypt(password);

            // Note: we could do this using a static string instead of ConnectionStringBuilder, since
            // none of the values come from user input...
            MySqlConnectionStringBuilder builder = new MySqlConnectionStringBuilder();
            builder.Server = sqlServer;
            builder.UserID = sqlUser;
            builder.Database = sqlDatabase;
            builder.Port = sqlPort;
            builder.Password = sqlPassword;
            builder.SslMode = MySqlSslMode.None;
            using (MySqlConnection connection = new MySqlConnection(builder.ConnectionString))
            {
                try
                {
                    connection.Open();
                    MySqlCommand getCommand = connection.CreateCommand();
                        getCommand.CommandText = "SELECT password FROM Users WHERE username='" + name + "'";
                    using (MySqlDataReader reader = getCommand.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            string result = reader.GetString("password");
                            if (result == _password)
                            {
                                return true;
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    throw ex;
                }
                return false;
            }
        }

        private static string Md5Encrypt(string stringToEncrypt)
        {
            IBuffer buffUtf8Msg = CryptographicBuffer.ConvertStringToBinary(stringToEncrypt, BinaryStringEncoding.Utf8);
            HashAlgorithmProvider objAlgProv = HashAlgorithmProvider.OpenAlgorithm(HashAlgorithmNames.Md5);

            IBuffer buffHash = objAlgProv.HashData(buffUtf8Msg);
            string result = CryptographicBuffer.EncodeToHexString(buffHash);

            return result;
        }

    }
}
