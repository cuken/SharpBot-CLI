using System;

namespace SharpBot_CLI.IRC
{
    class Delegates
    {
        public delegate void ServerMessageEventDelegate(string message);
        public delegate void ChannelMessageEventDelegate(string Channel, string User, string Message);
        public delegate void PrivateMessageEventDelegate(string User, string Message);
        public delegate void NoticeMessageEventDelegate(string User, string Message);
        public delegate void UserNickChangeEventDelegate(string oldUser, string newUser);
        public delegate void NickTakenEventDelegate(string nick);
        public delegate void UpdateUserListEventDelegate(string Channel, string[] userlist);
        public delegate void UserJoinedEventDelegate(string Channel, string User);
        public delegate void UserLeftEventDelegate(string Channel, string User);
        public delegate void ConnectedDeventDelegate();
        public delegate void DisconnectedEventDelegate();
        public delegate void ExceptionThrownEventDelegate(Exception ex);
    }
}
