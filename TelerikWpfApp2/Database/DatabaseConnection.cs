using System;
using System.IO;
using NLog;

namespace TelerikWpfApp2.Database
{
    class DatabaseConnection
    {
        private string sql_string;
        private string strCon;
        System.Data.SqlClient.SqlDataAdapter da_1;
        private Logger _logger = LogManager.GetLogger("DatabaseConnection");

        public string Sql
        {

            set { sql_string = value; }
        }

        public string connection_string
        {

            set { strCon = value; }
        }

        public void CreateConnection()
        {
            _logger.Info("start to create a connection");
            try
            {
                _logger.Info("get address of AppData folder");
                var localAppData =
                      Environment.GetFolderPath(
                      Environment.SpecialFolder.LocalApplicationData);

                var userFilePath
                  = Path.Combine(localAppData, "AsanPardakht");

                _logger.Info("create a directory with our name in AppData folder");
                if (!Directory.Exists(userFilePath))
                    Directory.CreateDirectory(userFilePath);

                var destFilePath = Path.Combine(userFilePath, "persianSwitch.db");

                if (!File.Exists(destFilePath))
                {
                    _connection = new SQLiteConnection("data source=" + destFilePath + ";Version=3");
                    _connection.SetPassword("weAreThe733WeTheChampions");
                    _connection.Close();
                }
                _connection = new SQLiteConnection("data source=" + destFilePath + ";Version=3;Password=weAreThe733WeTheChampions");

                CheckDbExistance();
            }
            catch (Exception exception)
            {
                _logger.Error("an error occured" + exception.Message);
                throw;
            }
        }


    }
}
