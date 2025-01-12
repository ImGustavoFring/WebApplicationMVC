using System;
using System.Collections.Generic;

namespace WebApplicationMVC.Models;

public partial class View
{
    public int Articleid { get; set; }

    public int Userid { get; set; }

    public DateTime Createdat { get; set; }

    public virtual Article Article { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}
