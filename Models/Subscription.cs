using System;
using System.Collections.Generic;

namespace WebApplicationMVC.Models;

public partial class Subscription
{
    public int Subscriberid { get; set; }

    public int Authorid { get; set; }

    public DateTime Createdat { get; set; }

    public virtual User Author { get; set; } = null!;

    public virtual User Subscriber { get; set; } = null!;
}
