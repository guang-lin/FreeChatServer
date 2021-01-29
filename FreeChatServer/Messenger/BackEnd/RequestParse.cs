using FreeChatServer.Messenger.Client;
using FreeChatServer.Messenger.ServerEnd.Data;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Text;
using System.Text.RegularExpressions;

namespace FreeChatServer.Messenger.Server
{
    class RequestParse
    {
        private RequestHandler handler;
        private string request;
        private string[] para;
        private DatabaseHandler databaseHandler;
        private MessageClient client;

        // 建立方法入口表
        private ParseEntry[] entrys = {
            new ParseEntry("USER", "Login"),
            new ParseEntry("TYPE", "TransferMessage"),
            new ParseEntry("PORT", "SendPort"),
            new ParseEntry("USERINFO", "UserInfo"),
            new ParseEntry("LOGOUT", "Logout")
            };

        public RequestParse(RequestHandler handler)
        {
            this.handler = handler;
            databaseHandler = new DatabaseHandler();
            client = new MessageClient();
        }

        // 解析客户端请求并调用相应的方法进行处理
        public void Parse(string request)
        {
            Console.WriteLine("请求：" + request);
            if (IsMatch(request))
            {
                this.request = request;
                para = NetUtility.Split(request);
                /*
                Console.WriteLine("拆分结果：");
                foreach (string s in para)
                {
                    Console.WriteLine(s);
                }
                */
            }
            else
            {
                return;
            }
            // 在此根据入口命令决定调用哪个方法
            int i = 0;
            for (; i < entrys.Length; i++)
            {
                if (entrys[i].Command.Equals(para[0]))
                {
                    // 调用
                    GetType().GetMethod(entrys[i].MethodName).Invoke(this, null);
                    break;
                }
            }
            if (i == entrys.Length)
            {
                handler.ReturnResponse("501");
                Console.WriteLine("{0} 没有找到合适的入口", para[0]);
            }
        }

        // 确定请求内容是否符合正确格式
        private bool IsMatch(string request)
        {
            string pattern1 = "[A-Za-z]+";
            string pattern2 = "[A-Za-z]+ +.*";
            bool match1 = Regex.IsMatch(request, pattern1);
            bool match2 = Regex.IsMatch(request, pattern2);
            if (match1 || match2)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        // 获取联系人基本信息
        public void UserInfo()
        {
            // USERINFO requester username
            // USERINFO requester ALL
            StringBuilder info = new StringBuilder();
            if (para.Length == 3)
            {
                string sql = "";
                if (para[2].Equals("ALL"))
                {
                    sql = "SELECT * FROM contacts_" + para[1];
                }
                else if(para[2].Equals("SELF"))
                {
                    sql = "SELECT username,nickname,header FROM account" + " WHERE username=" + para[1];
                }
                else
                {
                    sql = "SELECT * FROM contacts_" + para[1] + " WHERE username=" + para[2];
                }
                SqlDataReader dataReader;
                databaseHandler.Connect();
                dataReader = databaseHandler.Select(sql);
                while (dataReader.Read())
                {
                    info.Append(dataReader["username"].ToString());
                    info.Append(" ");
                    info.Append(dataReader["nickname"].ToString());
                    info.Append(" ");
                    if (!para[2].Equals("SELF"))
                    {
                        info.Append(dataReader["remark"].ToString());
                        info.Append(" ");
                    }
                    info.Append(dataReader["header"].ToString());
                    info.Append("\n");
                }
                handler.ReturnResponse("220 " + info); // 返回用户基本信息
            }
            else
            {
                handler.ReturnResponse("500"); // 语法错误
            }
        }

        // 登录
        public void Login()
        {
            // USER
            SqlDataReader dataReader;
            databaseHandler.Connect();
            dataReader = databaseHandler.Select("SELECT password FROM account WHERE username=" + para[1]);
            if (dataReader.HasRows) // 如果该用户名存在
            {
                User user = new User();
                handler.ReturnResponse("330"); // 需要密码
                request = handler.GetRequest();
                para = NetUtility.Split(request);
                dataReader.Read();
                if (para[0].Equals("PASS") && para[1].Equals(dataReader["password"].ToString()))
                {
                    user.Username = para[1];
                    OnlineUserList.AddUser(user); // 用户上线，添加用户到列表
                    Console.WriteLine("用户：{0} 已上线", user.Username);
                    handler.ReturnResponse("220"); // 登录成功，响应 220
                    Console.WriteLine("登录请求成功");
                }
                else
                {
                    handler.ReturnResponse("532"); // 密码错误
                }
            }
            else
            {
                handler.ReturnResponse("531"); // 用户名不存在
            }
        }

        // 注销
        public void Logout()
        {
            OnlineUserList.RemoveUser(para[1]);
            handler.ReturnResponse("220");
        }

        // 提交地址和端口
        public void SendPort()
        {
            if (para.Length != 4)
            {
                handler.ReturnResponse("510"); // 参数错误
                return;
            }
            if (OnlineUserList.Contains(para[1])) // 如果用户已登录
            {
                if (NetUtility.IsAddress(para[2]) && NetUtility.IsPort(para[3]))
                {
                    Console.WriteLine("获取用户端监听端口成功");

                    OnlineUserList.SetProperty(para[1], "Host", para[2]);
                    OnlineUserList.SetProperty(para[1], "Port", para[3]);

                    Console.WriteLine("主机地址：" + OnlineUserList.GetUser(para[1]).Host);
                    Console.WriteLine("端口号：" + OnlineUserList.GetUser(para[1]).Port);
                    handler.ReturnResponse("220");
                }
                else
                {
                    handler.ReturnResponse("520");
                }
            }
            else
            {
                handler.ReturnResponse("530"); // 用户未登录
            }
        }

        // 转发消息
        public void TransferMessage()
        {
            if (para[0].Equals("TYPE"))
            {
                if (para[1].Equals("0"))
                {
                    // 准备转发命令参数
                    string[] sendPara = new string[3];
                    sendPara[0] = "0"; // 消息类型
                    sendPara[1] = para[2]; // 发送者
                    sendPara[2] = para[3]; // 接收者

                    handler.ReturnResponse("331");
                    request = handler.GetRequest();
                    para = NetUtility.Split(request); // 获得新的请求命令参数
                    if (para[0].Equals("MSG") && para.Length == 2)
                    {
                        // 将消息转发到指定主机
                        Console.WriteLine("消息：" + para[1]);
                        handler.ReturnResponse("220");

                        User receiver = OnlineUserList.GetUser(sendPara[2]);
                        if (receiver == null)
                        {
                            handler.ReturnResponse("640"); // 消息接收者离线
                            return; // 不再进行转发，当然这应当把消息暂存到数据库中
                        }
                        else
                        {
                            string recvAddress = receiver.Host;
                            int port = receiver.Port;
                            client.GetRequestSender().SetClient(recvAddress, port);
                            client.GetRequestSender().TransferMessage(para[1], sendPara);
                            // 消息转发成功
                        }
                    }
                    else
                    {
                        handler.ReturnResponse("420");
                    }
                }
                else if (para[1].Equals("11"))
                {
                    // 群发消息
                    string[] sendPara = new string[3];
                    sendPara[0] = "11";
                    sendPara[1] = para[2]; // 发送者
                    int group_id = Convert.ToInt32(para[3]); // 群号

                    // 返回 331，表示需要消息内容
                    handler.ReturnResponse("331");
                    request = handler.GetRequest();
                    para = NetUtility.Split(request); // 获得新的请求命令参数
                    if (para[0].Equals("MSG") && para.Length == 2)
                    {
                        Console.WriteLine("将消息转发到多个主机");
                        Console.WriteLine("消息内容：" + para[1]);
                        handler.ReturnResponse("220");
                        // 获取群成员列表
                        List<string> group_numbers = GroupManager.GetNumbers(group_id);
                        if (group_numbers is null)
                        {
                            handler.ReturnResponse("420");
                        }
                        else
                        {
                            // 向除自己之外的每个群成员发送消息
                            foreach (string number in group_numbers)
                            {
                                User receiver = OnlineUserList.GetUser(number);
                                if (receiver != null)
                                {
                                    if (!receiver.Username.Equals(sendPara[1]))
                                    {
                                        Console.WriteLine("接收者" + receiver.Username);
                                        string recvAddress = receiver.Host;
                                        int port = receiver.Port;
                                        client.GetRequestSender().SetClient(recvAddress, port);
                                        sendPara[2] = receiver.Username; // 接收者
                                        client.GetRequestSender().TransferMessage(para[1], sendPara);
                                        // 消息转发成功
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        handler.ReturnResponse("420");
                    }
                }
            }
        }
    }
}
