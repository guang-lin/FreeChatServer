namespace FreeChatServer.Messenger.Server
{
    class MessageServer
    {
        private RequestListener listener = null;
        private string host = string.Empty;
        private int port = 0;

        public MessageServer()
        {
            host = NetUtility.GetEnableIpv4Address();
            port = 8888;
            for (int i = port; i < 9000; i++)
            {
                if (!NetUtility.SocketAddressDoBind(host, i))
                {
                    port = i;
                    break;
                }
            }
            listener = new RequestListener(host, port);
        }

        public MessageServer(string host, int port)
        {
            this.host = host;
            this.port = port;
            listener = new RequestListener(host, port);
        }

        public void listen()
        {
            listener.Listen();
        }

        public string GetHost()
        {
            return host;
        }

        public int GetPort()
        {
            return port;
        }
    }
}
