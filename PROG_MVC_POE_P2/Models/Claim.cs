using System;

namespace PROG_MVC_POE_P2.Models;

public class Claim
{
    public int ClaimId { get; set; }
    public int LecturerId { get; set; }
    public int PayId { get; set; }
    public DateTime ClaimTime { get; set; }
    public string Status { get; set; }
    public string? FilePath { get; set; }
}
