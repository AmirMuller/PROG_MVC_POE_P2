using PROG_MVC_POE_P2.Data.Models;

namespace PROG_MVC_POE_P2.Services;

public interface IPdfService
{
    byte[] GenerateInvoicePdf(Lecturer lecturer, IEnumerable<Claim> claims);
}