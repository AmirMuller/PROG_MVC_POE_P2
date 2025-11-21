using System;
using System.Collections.Generic;

namespace PROG_MVC_POE_P2.Data.Models;

public partial class Payment
{
    public int PayId { get; set; }

    public int NumHours { get; set; }

    public double Rate { get; set; }

    public virtual ICollection<Claim> Claims { get; set; } = new List<Claim>();
}
