using FreeChatServer.Messenger.Server;

namespace FreeChatServer
{
    class Program
    {
        static void Main(string[] args)
        {
            MessageServer server = new MessageServer();
            server.listen();
        }
    }
}
