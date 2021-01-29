using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace FreeChatServer.Messenger.Server
{
    class RequestListener
    {
        private TcpListener serverSocket;

        public RequestListener(string host, int port)
        {
            serverSocket = new TcpListener(IPAddress.Parse(host), port);
            int workerThreads = Environment.ProcessorCount; // 线程池中辅助线程的最大数目
            int completionPortThreads = Environment.ProcessorCount; // 线程池中异步 I/O 线程的最大数目
            ThreadPool.SetMaxThreads(workerThreads, completionPortThreads);
            Console.WriteLine("服务器已启动");
            Console.WriteLine("主机地址：" + host);
            Console.WriteLine("端口号：" + port + "\n");
        }

        public void Listen()
        {
            // 开始监听服务器请求
            serverSocket.Start();
            // 进入监听闭环
            while (true)
            {
                TcpClient client = serverSocket.AcceptTcpClient();
                // 将方法排入队列以执行。该方法在线程池中有可用线程时执行。
                ThreadPool.QueueUserWorkItem(arg => Executor(client), null);
            }
        }

        private void Executor(TcpClient clientSocket)
        {
            new RequestHandler(clientSocket);
        }

        // 停止监听连接请求
        public void StopReceive()
        {
            serverSocket.Stop();
        }
    }
}
