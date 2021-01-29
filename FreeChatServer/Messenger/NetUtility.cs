using System;
using System.Collections.Generic;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text.RegularExpressions;

namespace FreeChatServer.Messenger
{
    class NetUtility
    {
        // 地址格式是否正确
        public static bool IsAddress(string address)
        {
            string pattern = "\\d{1,3}\\.\\d{1,3}\\.\\d{1,3}\\.\\d{1,3}";
            bool isMatch = Regex.IsMatch(address, pattern);
            if (isMatch)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        // 是否为端口号
        public static bool IsPort(string content)
        {
            string pattern = "^[1-9]\\d*$";
            bool isMatch = Regex.IsMatch(content, pattern);
            if (isMatch)
            {
                try
                {
                    int port = Convert.ToInt32(content);
                    if (port > 1023 && port < 65536)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                catch (Exception)
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }

        // 根据指定正则表达式拆分字符串
        public static string[] Split(string text, string pattern = " +")
        {
            string[] mathes = Regex.Split(text.Trim(), " +");
            return mathes;
        }

        public static List<string> GetAllIpv4Address()
        {
            List<string> ipv4List = new List<string>();
            string host = Dns.GetHostName();
            IPAddress[] addressList = Dns.GetHostAddresses(host);
            foreach (IPAddress address in addressList)
            {
                if (address.AddressFamily == AddressFamily.InterNetwork)
                {
                    ipv4List.Add(address.ToString());
                }
            }
            return ipv4List;
        }

        // 确定是否可通过网络访问远程计算机
        public static bool IsAccessible(string host)
        {
            Ping pingSender = new Ping();
            PingOptions options = new PingOptions();
            int timeout = 120;
            PingReply reply = pingSender.Send(host, timeout);
            if (reply.Status == IPStatus.Success)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        // 获取当前可用的本机IPV4地址
        public static string GetEnableIpv4Address()
        {
            foreach (string ip in GetAllIpv4Address())
            {
                if (IsAccessible(ip))
                {
                    return ip;
                }
            }
            return "";
        }

        // 确定套接字地址(协议/网络地址/端口)是否已使用
        public static bool SocketAddressDoBind(string address, int port)
        {
            try
            {
                TcpListener serverSocket = new TcpListener(IPAddress.Parse(address), port);
                serverSocket.Start();
                serverSocket.Stop();
                serverSocket = null;
                return false;
            }
            catch (SocketException)
            {
                return true;
            }
        }

        // 获得该地址的可用端口
        public int GetEnablePort(string host, int min = 8888, int max = 9000)
        {
            host = GetEnableIpv4Address();
            int port = min;
            for (int i = min; i < max; i++)
            {
                if (!SocketAddressDoBind(host, i))
                {
                    port = i;
                    break;
                }
            }
            return port;
        }
    }
}
