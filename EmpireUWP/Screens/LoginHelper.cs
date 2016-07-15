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

        internal static void TryLogin(string name, string password)
        {
            string _name = name;
            string _password = Md5Encrypt(password);

            MySqlConnectionStringBuilder builder = MySQLConnectionStringBuilder();
            using (MySqlConnection connection = new MySqlConnection(builder.ConnectionString))
            {
                try
                {
                    connection.Open();

                    string sql = "SELECT password FROM Users WHERE username='" + name + "'";
                    MySqlCommand cmd = new MySqlCommand(sql, connection);
                    MySqlDataReader reader = cmd.ExecuteReader();

                    if (reader.Read())
                    {
                        string result = reader.GetString("password");
                        if (result != _password)
                        {
                            throw new LoginException("Incorrect password.", LoginException.LoginExceptions.IncorrectPassword);
                        }
                    }
                    else
                    {
                        throw new LoginException("Unknown username.", LoginException.LoginExceptions.UnknownUser);
                    }

                }
                catch (MySqlException ex) {
                    throw ex;
                }
                catch (Exception ex) {
                    throw ex;
                }
                finally
                {
                    connection.Close();
                }

            }
        }

        internal static void CreateUser(string name, string password1, string password2)
        {

            if(password1 != password2)
            {
                throw new LoginException("Password fields do not match.", LoginException.LoginExceptions.PasswordsDoNotMatch);
            }

            if(string.IsNullOrEmpty(name))
            {
                throw new LoginException("Please enter a valid username", LoginException.LoginExceptions.InvalidUsername);
            }

            string _name = name;
            string _password = Md5Encrypt(password1);

            MySqlConnectionStringBuilder builder = MySQLConnectionStringBuilder();
            using (MySqlConnection connection = new MySqlConnection(builder.ConnectionString))
            {
                try
                {
                    connection.Open();
                    
                    string sql = "INSERT into Users (username, password) VALUES ('" + name + "', '" + _password + "')";
                    MySqlCommand command = new MySqlCommand(sql, connection);
                    command.ExecuteNonQuery();

                }
                finally
                {
                    connection.Close();
                }
            }
        }

        private static MySqlConnectionStringBuilder MySQLConnectionStringBuilder()
        {
            // Note: we could do this using a static string instead of ConnectionStringBuilder, since
            // none of the values come from user input...
            MySqlConnectionStringBuilder builder = new MySqlConnectionStringBuilder();
            builder.Server = sqlServer;
            builder.UserID = sqlUser;
            builder.Database = sqlDatabase;
            builder.Port = sqlPort;
            builder.Password = sqlPassword;
            builder.SslMode = MySqlSslMode.None;
            return builder;
        }

        private static string Md5Encrypt(string stringToEncrypt)
        {
            IBuffer buffUtf8Msg = CryptographicBuffer.ConvertStringToBinary(stringToEncrypt, BinaryStringEncoding.Utf8);
            HashAlgorithmProvider objAlgProv = HashAlgorithmProvider.OpenAlgorithm(HashAlgorithmNames.Md5);

            IBuffer buffHash = objAlgProv.HashData(buffUtf8Msg);
            string result = CryptographicBuffer.EncodeToHexString(buffHash);

            return result;
        }

        internal static string Capitalize(string input)
        {
            return input.First().ToString().ToUpper() + input.Substring(1);
        }

    }
}
