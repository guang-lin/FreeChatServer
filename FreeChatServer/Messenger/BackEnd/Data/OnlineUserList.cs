using System;
using System.Collections.Generic;

namespace FreeChatServer.Messenger.ServerEnd.Data
{
    // 在线用户列表类
    class OnlineUserList
    {
        private static int maxCount = 100;
        private static List<User> userList = new List<User>();

        public static int UserCount
        {
            get { return userList.Count; }
            private set { }
        }

        public static int MaxUserCount
        {
            get { return userList.Count; }
            set { maxCount = value; }
        }

        public static int AddUser(User user)
        {
            if (userList.Contains(user))
            {
                return 1;
            }
            else
            {
                if (UserCount >= maxCount)
                {
                    return -1;
                }
                else
                {
                    userList.Add(user);
                    return 0;
                }
            }
        }

        public static User GetUser(string username)
        {
            if (userList.Count > 0)
            {
                foreach (User user in userList)
                {
                    if (user.Username.Equals(username))
                    {
                        return user;
                    }
                }
                return null;
            }
            else
            {
                return null;
            }
        }

        public static void RemoveUser(string username)
        {
            User user = GetUser(username);
            if (user != null)
            {
                userList.Remove(GetUser(username));
            }
        }

        public static bool SetProperty(string username, string property, string value)
        {
            User user = GetUser(username);
            if (user != null)
            {
                switch (property)
                {
                    case "Username":
                        user.Username = value;
                        return true;
                    case "Host":
                        user.Host = value;
                        return true;
                    case "Port":
                        user.Port = Convert.ToInt32(value);
                        return true;
                    default:
                        return false;
                }
            }
            else
            {
                return false;
            }
        }

        public static bool Contains(string username)
        {
            if (GetUser(username) != null)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
