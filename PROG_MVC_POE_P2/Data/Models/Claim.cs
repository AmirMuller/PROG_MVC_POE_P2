using System;
using System.Collections.Generic;

namespace PROG_MVC_POE_P2.Data.Models;

public partial class Claim
{
    public int ClaimId { get; set; }

    public int LecturerId { get; set; }

    public int PayId { get; set; }

    public DateTime ClaimTime { get; set; }

    public string Status { get; set; } = null!;

    public string? Message { get; set; }

    public string? FilePath { get; set; }

    public virtual Lecturer Lecturer { get; set; } = null!;

    public virtual Payment Pay { get; set; } = null!;
}
