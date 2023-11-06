using System;
using System.Collections.Generic;

namespace EntityChatDB;

public partial class Image
{
    public int ImagesId { get; set; }

    public byte[]? Screen { get; set; }

    public virtual User? User { get; set; }
}
