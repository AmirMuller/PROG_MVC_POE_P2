using System;

namespace PROG_MVC_POE_P2.Data.Models;

public class ClaimReviewView
{
    public int ClaimId { get; set; }
    public int LecturerId { get; set; }
    public int PayId { get; set; }

    public DateTime ClaimTime { get; set; }
    public string Status { get; set; } = null!;
    public string? Message { get; set; }
    public string? FilePath { get; set; }

    public string LecturerName { get; set; } = null!;
    public double TotalAmount { get; set; }

    public Payment? Payment { get; set; }
    public Claim Claim { get; set; } = null!;
}
