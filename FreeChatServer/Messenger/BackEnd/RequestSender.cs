using FreeChatServer.Messenger.ServerEnd.Data;
using System;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;

namespace FreeChatServer.Messenger.Client
{
    // 该类用于实现具体的请求发送及响应解析功能
    class RequestSender
    {
        private string host = string.Empty;
        private int port = 0;
        private TcpClient client;
        private NetworkStream stream;
        private string extras = string.Empty;

        public RequestSender()
        {
        }

        public RequestSender(string host, int port)
        {
            this.host = host;
            this.port = port;
        }

        // 设置将要请求的主机地址和端口号
        public void SetClient(string host, int port)
        {
            this.host = host;
            this.port = port;
        }

        public bool CreateConnect()
        {
            try
            {
                // 创建 TCP 客户端套接字
                client = new TcpClient(host, port);
                return true;
            }
            catch (SocketException e)
            {
                Console.WriteLine(e.Message);
                return false;
            }
        }

        public void CloseConnect()
        {
            if (client != null)
            {
                client.Close();
            }
        }

        // 发送请求,返回响应码
        public int SendCommand(string command)
        {
            if (host.Equals(string.Empty) || port == 0)
            {
                return -2; // 未设置主机地址或端口号
            }
            try
            {
                if (client == null)
                {
                    return 420;
                }
                else
                {
                    // 将待发送字符串转换为 UTF8 编码并存储到字节数组
                    byte[] data = Encoding.UTF8.GetBytes(command);
                    // 从客户端套接字获取数据流，用来读写数据
                    stream = client.GetStream();
                    // 向已连接的 TCP 服务器发送数据
                    stream.Write(data, 0, data.Length);
                    Console.WriteLine("请求：" + command);

                    // 等待服务器响应，读取响应数据
                    StringBuilder completeData = new StringBuilder(); // 要读入的完整数据
                    if (stream.CanRead)
                    {
                        byte[] readBuffer = new byte[256]; // 准备字节数组用于缓存从服务器接收到的数据
                        int count = 0; // 每次读入缓存数组的字节数
                        // 服务器发送的数据大小可能会大于缓存大小
                        // 这时需要多次从缓存中读取数据
                        do
                        {
                            count = stream.Read(readBuffer, 0, readBuffer.Length);
                            // 将读入缓存数组中数据以 UTF-8 编码存储为字符串,并添加到已读取到的字符串中
                            completeData.Append(Encoding.UTF8.GetString(readBuffer, 0, count));
                        }
                        while (stream.DataAvailable);
                    }
                    else
                    {
                        Console.WriteLine("无法从该网络流中读取数据");
                        return -1;
                    }

                    string response = completeData.ToString();
                    if (response.Length == 0)
                    {
                        Console.WriteLine("未收到消息");
                        return -1; // 没有任何响应
                    }
                    else
                    {
                        // 处理,响应解析
                        string pattern1 = "\\d\\d\\d";
                        string pattern2 = "\\d\\d\\d +.*";
                        bool match1 = Regex.IsMatch(response, pattern1);
                        bool match2 = Regex.IsMatch(response, pattern2);
                        if (match1 || match2) // 如果字符串符合请求格式，则返回响应码
                        {
                            int code = Convert.ToInt32(response.Substring(0, 3));
                            Console.WriteLine("响应：" + code);
                            // 如果字符串中除响应码中还有其它内容，则将其它内容保存在成员变量 extras 中
                            if (match2)
                            {
                                extras = response.Substring(3);
                            }
                            return code; // 返回响应码
                        }
                    }
                }
            }
            catch (ArgumentNullException e)
            {
                Console.WriteLine(e.Message);
                return -1;
            }
            catch (SocketException e)
            {
                Console.WriteLine(e.Message);
                return -1;
            }
            return 0;
        }

        // 读取响应附加内容
        public string ReadExtras()
        {
            return extras;
        }

        // 确定客户端是否可响应
        public int Hello(string address, int port)
        {
            CreateConnect();
            int code = SendCommand("HELLO " + address + " " + port);
            CloseConnect();
            if (code == 220)
            {
                Console.WriteLine("服务可连接");
            }
            else
            {
                Console.WriteLine("服务不可连接");
            }
            return code;
        }

        // 转发消息
        public int TransferMessage(string message, string[] parameter)
        {
            Console.WriteLine("开始转发消息");
            string recvAddress;
            int port;
            // 命令如：TYPE 0 sender receiver
            if (parameter.Length == 3)
            {
                User receiver = OnlineUserList.GetUser(parameter[2]);
                if (receiver != null)
                {
                    recvAddress = receiver.Host;
                    port = receiver.Port;
                }
                else
                {
                    return -1;
                }
            }
            else
            {
                Console.WriteLine("转发失败");
                return -1;
            }

            SetClient(recvAddress, port); // 设置目的用户IP地址
            CreateConnect(); // 与用户建立连接
            StringBuilder command = new StringBuilder();
            command.Append("TYPE ");
            for (int i = 0; i < parameter.Length; i++)
            {
                command.Append(parameter[i]);
                command.Append(" ");
            }
            int code = SendCommand(command.ToString());
            if (code == 331)
            {
                code = SendCommand("MSG " + message);
                CloseConnect();
                if (code == 220)
                {
                    Console.WriteLine("转发成功");
                }
                else
                {
                    Console.WriteLine("转发失败");
                }
                return code;
            }
            else
            {
                CloseConnect();
                Console.WriteLine("请求错误");
                return code;
            }
        }
    }
}
