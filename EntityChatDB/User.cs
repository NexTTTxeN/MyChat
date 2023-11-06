using Azure;
using System;
using System.Collections.Generic;

namespace EntityChatDB;

public partial class User : ICloneable
{
    public int UserId { get; set; }

    public string UserName { get; set; } = null!;

    public string UserPassword { get; set; } = null!;

    public int? ScreenId { get; set; }


    public virtual ICollection<Message> MessageUserFromNavigations { get; set; } = new List<Message>();

    public virtual ICollection<Message> MessageUserToNavigations { get; set; } = new List<Message>();

    public virtual Image? Screen { get; set; }
    public object Clone()
    {
        return MemberwiseClone();
    }
}
