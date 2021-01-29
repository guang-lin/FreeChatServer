using System;
using System.Data;
using System.Data.SqlClient;

namespace FreeChatServer.Messenger.ServerEnd.Data
{
    class DatabaseHandler
    {
        private string connectionString = @"Server=localhost;DataBase=FreeChat;Integrated Security=True";
        private SqlConnection connection;
        private SqlCommand command;
        private SqlDataReader dataReader;

        public void Connect()
        {
            try
            {
                connection = new SqlConnection(connectionString);
                connection.Open();
            }
            catch (SqlException)
            {
                Console.WriteLine("数据库连接失败");
            }
        }

        public bool IsCanConnect()
        {
            try
            {
                connection = new SqlConnection(connectionString);
                connection.Open();
                return true;
            }
            catch (SqlException)
            {
                return false;
            }
            finally
            {
                if (connection.State == ConnectionState.Open)
                {
                    connection.Close();
                }
            }
        }

        public SqlConnection GetConnect()
        {
            return connection;
        }

        public void ReleaseDataReader()
        {
            if (dataReader != null)
            {
                dataReader.Close();
            }
        }

        public bool CloseDatabase()
        {
            try
            {
                if (connection.State == ConnectionState.Open)
                {
                    connection.Close();
                    return true;
                }
                else
                    return true;
            }
            catch (SqlException)
            {
                Console.WriteLine("数据库关闭失败");
                return false;
            }
        }

        public SqlDataReader Select(string selectSentence)
        {
            if (dataReader != null)
            {
                dataReader.Close();
            }
            command = new SqlCommand("", connection);
            command.CommandType = CommandType.Text;
            string commandString = selectSentence;
            command.CommandText = commandString;

            dataReader = command.ExecuteReader();
            return dataReader;
        }

        public bool IsExistColumnValue(string value, string columnName, SqlDataReader dataReader)
        {
            if (dataReader.HasRows)
            {
                while (dataReader.Read())
                {
                    if (dataReader[columnName].ToString() == value)
                    {
                        return true;
                    }
                }
                return false;
            }
            return false;
        }

        public void Delete(string tableName, string columnName, string columnValue)
        {
            if (dataReader != null)
            {
                dataReader.Close();
            }
            command = new SqlCommand("", connection)
            {
                CommandType = CommandType.Text,
                CommandText = "DELETE FROM " + tableName + " WHERE " + columnName + "=" + columnValue + ";"
            };
            command.ExecuteNonQuery();
        }
    }
}
