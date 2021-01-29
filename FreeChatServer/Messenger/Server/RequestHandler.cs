using System;
using System.Net.Sockets;
using System.Text;

namespace FreeChatServer.Messenger.Server
{
    class RequestHandler
    {
        private TcpClient socket; // Client Socket
        private RequestParse parse;

        public RequestHandler(TcpClient socket)
        {
            this.socket = socket;
            Run();

        }

        public void Run()
        {
            string request;
            request = GetRequest();
            parse = new RequestParse(this);
            parse.Parse(request);

        }

        public string GetRequest()
        {
            StringBuilder request = new StringBuilder();
            if (socket != null)
            {
                NetworkStream stream = socket.GetStream();
                // 读入所有数据
                if (stream.CanRead)
                {
                    byte[] bytes = new byte[256];
                    int count = 0;
                    do
                    {
                        count = stream.Read(bytes, 0, bytes.Length);
                        // 将数据转换为UFT-8格式字符串
                        request.Append(Encoding.UTF8.GetString(bytes, 0, count));
                    }
                    while (stream.DataAvailable);

                    return request.ToString();
                }
                else
                {
                    return null;
                }
            }
            else
            {
                return null;
            }
        }

        // 返回响应消息
        public void ReturnResponse(string response)
        {
            NetworkStream stream = socket.GetStream();
            byte[] msg = Encoding.UTF8.GetBytes(response);
            stream.Write(msg, 0, msg.Length);
            Console.WriteLine("响应: {0}", response);
        }
    }
}
