using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing.Imaging;

namespace EntityChatDB
{
    public class UsersMessage
    {
        public User TUser { get; private set; }
        public List<Message>? Messages { get; private set; }
        public int CountNotRead { get; private set; }
        public bool IsNotRead { get; private set; }
        public bool IsConnect { get; set; }

        public byte[]? ImageUser;
        
        public UsersMessage() { }
        public UsersMessage(User user)
        {
            TUser = user.Clone() as User;
            Messages = new List<Message>();
        }

        public void UpdateMessage(List<Message> messages, User UserTo)
        {
            Messages.Clear();
            CountNotRead = 0;
            IsNotRead = false;
            foreach (var message in messages)
            {
                if (TUser.UserId == message.UserFrom && message.UserTo == UserTo.UserId || TUser.UserId == message.UserTo && message.UserFrom == UserTo.UserId)
                {
                    if (message.UserFrom == TUser.UserId) message.UserFromNavigation = TUser;
                    else message.UserFromNavigation = UserTo;
                    Messages.Add(message);
                    if (message.UserFrom!= UserTo.UserId && !message.IsRead) CountNotRead++;
                }
            }
            Messages.Sort(delegate (Message x, Message y)
            {
                if (x.DataMessage == y.DataMessage) return 0;
                if (x.DataMessage > y.DataMessage) return 1;
                return -1;
            });
            if(CountNotRead > 0) IsNotRead=true;
        }

    }
    public record class UserConnect(User UserTo, bool IsConnect);
}
