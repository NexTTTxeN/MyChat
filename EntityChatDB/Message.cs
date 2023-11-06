using System;
using System.Collections.Generic;

namespace EntityChatDB;

public partial class Message : ICloneable
{
    public int MessageId { get; set; }

    public string Message1 { get; set; } = null!;

    public DateTime DataMessage { get; set; }

    public int UserFrom { get; set; }

    public int UserTo { get; set; }

    public bool IsRead { get; set; }

    public virtual User UserFromNavigation { get; set; } = null!;

    public virtual User UserToNavigation { get; set; } = null!;
    public object Clone()
    {
        return MemberwiseClone();
    }
}
