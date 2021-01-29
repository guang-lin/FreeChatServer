using System.Collections.Generic;
using System.Data.SqlClient;

namespace FreeChatServer.Messenger.ServerEnd.Data
{
    class GroupManager
    {
        private List<string> number = new List<string>();

        // 添加群聊
        public static int AddGroup(string group_id, List<string> group)
        {
            //
            return 0;
        }

        // 获取指定群聊中的所有成员
        public static List<string> GetNumbers(int group_id)
        {
            group_id = 1001;
            List<string> number = new List<string>();
            SqlDataReader dataReader;
            DatabaseHandler databaseHandler = new DatabaseHandler();
            databaseHandler.Connect();
            dataReader = databaseHandler.Select("SELECT number FROM " + "group_" + group_id);
            while (dataReader.Read())
            {
                number.Add(dataReader[0].ToString());
            }
            return number;
        }
    }
}
