namespace FreeChatServer.Messenger.Client
{
    // 该类用于主动向用户发送请求
    class MessageClient
    {
        private RequestSender requestSender;

        public MessageClient()
        {
            requestSender = new RequestSender();
        }

        public MessageClient(string host, int port)
        {
            requestSender = new RequestSender(host, port);
        }

        public RequestSender GetRequestSender()
        {
            return requestSender;
        }
    }
}
