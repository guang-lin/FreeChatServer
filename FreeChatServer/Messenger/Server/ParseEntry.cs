namespace FreeChatServer.Messenger.Server
{
    class ParseEntry
    {
        // 命令解析入口类
        public string Command { get; set; } = ""; // 命令
        public string MethodName { get; set; } = ""; // 方法

        public ParseEntry(string command, string methodName)
        {
            Command = command;
            MethodName = methodName;
        }
    }
}
