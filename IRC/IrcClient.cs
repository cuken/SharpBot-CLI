using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.IO;
using System.ComponentModel;
using System.Threading;
using System.Text.RegularExpressions;

namespace SharpBot_CLI.IRC
{
    class IrcClient
    {

        #region var

        private string _server = "";
        private int _port = 6667; // I think this is default IRC port. . .
        private string _serverPass = ""; // I don't think Twitch needs a pass -> But if it does!
        private string _nickname = "";
        private string _altNick = "";

        private TcpClient irc;
        private NetworkStream stream;
        private string inputLine;

        private StreamReader reader;
        private StreamWriter writer;

        //Multithreading. . . 
        private AsyncOperation ops;

        Queue<string> messageQueue = new Queue<string>();

        #endregion

        #region Constructor

        public IrcClient(string Server, int Port)
        {
            ops = AsyncOperationManager.CreateOperation(null);
            _server = Server;
            _port = Port;
        }

        public IrcClient(string Server) : this(Server, 6667)
        {
            ops = AsyncOperationManager.CreateOperation(null);
            _server = Server;
            _port = 6667;
        }

        public IrcClient(string Server, int Port, string ServerPassword)
        {
            ops = AsyncOperationManager.CreateOperation(null);
            _server = Server;
            _port = Port;
            _serverPass = ServerPassword;
        }


        #endregion

        #region Properties

        public string Server
        {
            get { return _server; }
        }

        public int Port
        {
            get { return _port; }
        }

        public string ServerPass
        {
            get { return _serverPass; }
            set { _serverPass = value; }
        }

        public string Nick
        {
            get { return _nickname; }
            set { _nickname = value; }
        }

        public string AltNick
        {
            get { return _altNick; }
            set { _altNick = value; }
        }

        public bool Connected
        {
            get
            {
                if (irc != null)
                    if (irc.Connected)
                        return true;
                    return false;
            }
        }

        #endregion

        #region Events

        public event Delegates.UpdateUserListEventDelegate Updateusers;
        public event Delegates.UserJoinedEventDelegate UserJoined;
        public event Delegates.UserLeftEventDelegate UserLeft;
        public event Delegates.UserNickChangeEventDelegate UserNickChange;
        public event Delegates.ChannelMessageEventDelegate ChannelMessage;
        public event Delegates.NoticeMessageEventDelegate NoticeMessage;
        public event Delegates.PrivateMessageEventDelegate PrivateMessage;
        public event Delegates.ServerMessageEventDelegate ServerMessage;
        public event Delegates.NickTakenEventDelegate NickTaken;
        public event Delegates.ConnectedDeventDelegate OnConnect;
        public event Delegates.ExceptionThrownEventDelegate ExceptionThrown;

        private void Fire_UpdateUsers(oUserList o)
        {
            if (Updateusers != null) Updateusers(o.Channel, o.UserList);
        }

        private void Fire_UserJoined(oUserJoined o)
        {
            if (UserJoined != null) UserJoined(o.Channel, o.User);
        }

        private void Fire_UserLeft(oUserLeft o)
        {
            if (UserLeft != null) UserLeft(o.Channel, o.User);
        }

        private void Fire_NickChanged(oUserNickChanged o)
        {
            if (UserNickChange != null) UserNickChange(o.Old, o.New);
        }

        private void Fire_ChannelMessage(oChannelMessage o)
        {
            if (ChannelMessage != null) ChannelMessage(o.Channel, o.From, o.Message);
        }

        private void Fire_NoticeMessage(oNoticeMessage o)
        {
            if (NoticeMessage != null) NoticeMessage(o.From, o.Message);
        }

        private void Fire_PrivateMessage(oPrivateMessage o)
        {
            if (PrivateMessage != null) PrivateMessage(o.From, o.Message);
        }

        private void Fire_ServerMessage(string s)
        {
            if (ServerMessage != null) ServerMessage(s);
        }

        private void Fire_NickTaken(string s)
        {
            if (NickTaken != null) NickTaken(s);
        }

        private void Fire_Connected()
        {
            if (OnConnect != null) OnConnect();
        }

        private void Fire_ExceptionThrown(Exception ex)
        {
            if (ExceptionThrown != null) ExceptionThrown(ex);
        }
        #endregion

        #region Public Methods

        public void Connect()
        {
            Thread t = new Thread(DoConnect);
            t.IsBackground = true;
            t.Start();
        }

        private void DoConnect()
        {
            try
            {
                irc = new TcpClient(_server, _port);
                stream = irc.GetStream();
                reader = new StreamReader(stream);
                writer = new StreamWriter(stream);

                if (!string.IsNullOrEmpty(_serverPass))
                    Send("PASS " + _serverPass);
                Send("NICK " + _nickname);
                Send("USER " + _nickname + " 0 * :" + _nickname);

                Listen();
            }
            catch(Exception ex)
            {
                ops.Post(x => Fire_ExceptionThrown((Exception)x),ex);
            }
        }

        public void Disconnect()
        {
            if (irc != null)
            {
                if(irc.Connected)
                {
                    Send("QUIT Client Disconnected: Sharpbot-CLI");
                }
                irc = null;
            }
        }

        public void JoinChannel(string Channel)
        {
            if (irc != null && irc.Connected)
            {
                Send("JOIN " + Channel);
            }
        }

        public void PartChannel(string Channel)
        {
            Send("PART " + Channel);
        }

        public void SendNotice(string Nick, string message)
        {
            Send("NOTICE " + Nick + " : " + message);
        }

        public void SendMessage(string Channel, string Message)
        {
            Send("PRIVMSG " + Channel + " :" + Message);
        }

        public void SendRAW(string message)
        {
            Send(message);
        }
        #endregion

        #region PrivateMethods

        private void Listen()
        {
            while ((inputLine = reader.ReadLine()) != null)
            {
                IrcMessage ircMessage = new IrcMessage(inputLine);
                ThreadPool.QueueUserWorkItem(new WaitCallback(ParseData), ircMessage);
                Thread.Sleep(100);
            }
        }

        private void ParseData(object a)
        {
            IrcMessage ircMessage = (IrcMessage)a;
            string data = ircMessage.Message;
            if(data != null)
            {
                string[] ircData = data.Split(' ');

                if (data.Length > 4)
                {
                    if (data.Substring(0, 4) == "PING")
                    {
                        Send("PONG " + ircData[1]);
                        return;
                    }
                }

                switch (ircData[1])
                {
                    case "001":
                        Send("MODE " + _nickname + " +B");
                        ops.Post((x) => Fire_Connected(), null);
                        break;
                    case "353":
                        ops.Post(x => Fire_UpdateUsers((oUserList)x), new oUserList(ircData[4], JoinArray(ircData, 5).Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries)));
                        break;
                    case "433":
                        ops.Post(x => Fire_NickTaken((string)x), ircData[3]);

                        if (ircData[3] == _altNick)
                        {
                            Random rand = new Random();
                            string randomNick = "Guest" + rand.Next(0, 9) + rand.Next(0, 9) + rand.Next(0, 9);
                            Send("NICK " + randomNick);
                            Send("USER " + randomNick + " 0 * :" + randomNick);
                            _nickname = randomNick;
                        }
                        else
                        {
                            Send("NICK " + _altNick);
                            Send("USER " + _altNick + " 0 * :" + _altNick);
                            _nickname = _altNick;
                        }
                        break;
                    case "JOIN":
                        ops.Post(x => Fire_UserJoined((oUserJoined)x), new oUserJoined(ircData[2], ircData[0].Substring(1, ircData[0].IndexOf("!") - 1)));
                        break;
                    case "NICK":
                        ops.Post(x => Fire_NickChanged((oUserNickChanged)x), new oUserNickChanged(ircData[0].Substring(1, ircData[0].IndexOf("!") - 1), JoinArray(ircData, 3)));
                        break;
                    case "NOTIC":
                        if (ircData[0].Contains("!"))
                        {
                            ops.Post(x => Fire_NoticeMessage((oNoticeMessage)x), new oNoticeMessage(ircData[0].Substring(1, ircData[0].IndexOf('!') - 1), JoinArray(ircData, 3)));
                        }
                        break;
                    case "PRIVMSG":
                        if (ircData[2].ToLower() == _nickname.ToLower())
                        {
                            ops.Post(x => Fire_PrivateMessage((oPrivateMessage)x), new oPrivateMessage(ircData[0].Substring(1, ircData[0].IndexOf('!') - 1), JoinArray(ircData, 3)));
                        }
                        else
                        {
                            ops.Post(x => Fire_ChannelMessage((oChannelMessage)x), new oChannelMessage(ircData[2], ircData[0].Substring(1, ircData[0].IndexOf('!') - 1), JoinArray(ircData, 3)));
                        }
                        break;
                    case "PART":
                    case "QUIT":
                        ops.Post(x => Fire_UserLeft((oUserLeft)x), new oUserLeft(ircData[2], ircData[0].Substring(1, data.IndexOf("!") - 1)));
                        Send("NAMES " + ircData[2]);
                        break;
                    default:
                        break;

                }
            }
            else
            {
                Console.WriteLine("... uh oh");
            }
        }

        private string StripMessage(string message)
        {
            foreach (Match m in new Regex((char)3 + @"(?:\d{1,2}(?:,\d{1,2})?)?").Matches(message))
                message = message.Replace(m.Value, "");
            if (message == "")
                return "";
            else if (message.Substring(0, 1) == ":" && message.Length > 2)
                return message.Substring(1, message.Length - 1);
            else
                return message;
        }

        private string JoinArray(string[] strArray, int startIndex)
        {
            return StripMessage(String.Join(" ", strArray, startIndex, strArray.Length - startIndex));
        }

        private void Send(string message)
        {
            writer.WriteLine(message);
            writer.Flush();
        }
        #endregion

        #region Structs

        public struct oUserList
        {
            public string Channel;
            public string[] UserList;
            public oUserList(string Channel, string[] UserList)
            {
                this.Channel = Channel;
                this.UserList = UserList;
            }
        }

        public struct oUserJoined
        {
            public string Channel;
            public string User;
            public oUserJoined(string Channel, string User)
            {
                this.Channel = Channel;
                this.User = User;
            }
        }

        public struct oUserLeft
        {
            public string Channel;
            public string User;
            public oUserLeft(string Channel, string User)
            {
                this.Channel = Channel;
                this.User = User;
            }
        }

        public struct oPrivateMessage
        {
            public string From;
            public string Message;
            public oPrivateMessage(string From, string Message)
            {
                this.From = From;
                this.Message = Message;
            }
        }

        public struct oChannelMessage
        {
            public string Channel;
            public string From;
            public string Message;
            public oChannelMessage(string Channel, string From, string Message)
            {
                this.Channel = Channel;
                this.From = From;
                this.Message = Message;
            }
        }

        public struct oNoticeMessage
        {
            public string From;
            public string Message;
            public oNoticeMessage(string From, string Message)
            {
                this.From = From;
                this.Message = Message;
            }
        }

        public struct oUserNickChanged
        {
            public string Old;
            public string New;
            public oUserNickChanged(string Old, string New)
            {
                this.Old = Old;
                this.New = New;
            }
        }

        public struct IrcMessage
        {
            public string Message;
            public IrcMessage(string message)
            {
                Message = message;
            }
        }

        #endregion

    }
}
