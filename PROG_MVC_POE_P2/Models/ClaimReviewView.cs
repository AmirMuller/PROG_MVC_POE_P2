using System;
using System.Collections.Generic;

namespace PROG_MVC_POE_P2.Models;

public partial class ClaimReviewView
{
    public int ClaimId { get; set; }

    public int LecturerId { get; set; }

    public string LecturerName { get; set; } = null!;

    public int PayId { get; set; }

    public int NumHours { get; set; }

    public double Rate { get; set; }

    public double TotalAmount { get; set; }

    public DateTime ClaimTime { get; set; }

    public string Status { get; set; } = null!;

    public string? Message { get; set; }

    public string? FilePath { get; set; }

    public int? AdminComment { get; set; }
}
