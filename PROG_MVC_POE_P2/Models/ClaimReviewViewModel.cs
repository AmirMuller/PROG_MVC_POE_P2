namespace PROG_MVC_POE_P2.Models;

public class ClaimReviewViewModel
{
    public Claim Claim { get; set; }
    public Payment? Payment { get; set; }
    public string LecturerName { get; set; }
    public double TotalAmount { get; set; }
    public string? AdminComment { get; set; }
}