using System;
using System.Collections.Generic;

namespace PROG_MVC_POE_P2.Models;

public partial class Lecturer
{
    public int LecturerId { get; set; }

    public string Name { get; set; } = null!;

    public string Faculty { get; set; } = null!;

    public string Position { get; set; } = null!;

    public virtual ICollection<Claim> Claim { get; set; } = new List<Claim>();
}
