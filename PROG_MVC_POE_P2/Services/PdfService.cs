using PROG_MVC_POE_P2.Data.Models;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace PROG_MVC_POE_P2.Services;
public class PdfService : IPdfService
{
    public byte[] GenerateInvoicePdf(Lecturer lecturer, IEnumerable<Claim> claims)
    {
        // Make sure license is set once in Program.cs as well.
        QuestPDF.Settings.License = LicenseType.Community;

        var claimList = claims?.ToList() ?? new List<Claim>();

        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(30);
                page.DefaultTextStyle(x => x.FontSize(12));
                page.PageColor(Colors.White);

                // Header
                page.Header().Text($"Invoice for {lecturer?.Name ?? "Unknown"}")
                    .SemiBold()
                    .FontSize(18)
                    .FontColor(Colors.Blue.Medium);

                // Content
                page.Content().Column(col =>
                {
                    col.Item().Text($"Date: {DateTime.UtcNow:yyyy-MM-dd}");
                    col.Item().Text($"Email: {lecturer?.Email ?? "N/A"}");
                    col.Item().LineHorizontal(1).LineColor(Colors.Grey.Lighten2);

                    // Table of claims
                    col.Item().Table(table =>
                    {
                        // Columns
                        table.ColumnsDefinition(columns =>
                        {
                            columns.ConstantColumn(60);     // Claim ID
                            columns.RelativeColumn();       // Message/Date
                            columns.ConstantColumn(60);     // Hours
                            columns.ConstantColumn(80);     // Amount
                        });

                        // Header
                        table.Header(header =>
                        {
                            header.Cell().Element(CellStyle).Text("Claim ID").SemiBold();
                            header.Cell().Element(CellStyle).Text("Date / Message").SemiBold();
                            header.Cell().Element(CellStyle).Text("Hours").SemiBold();
                            header.Cell().Element(CellStyle).Text("Amount").SemiBold();
                        });

                        // Rows: Use table.Cell() in a loop for each claim
                        foreach (var claim in claimList)
                        {
                            table.Cell().Element(CellStyle).Text(claim.ClaimId.ToString());
                            table.Cell().Element(CellStyle)
                                .Text($"{claim.ClaimTime:yyyy-MM-dd} - {claim.Message}");
                            table.Cell().Element(CellStyle).Text((claim.Pay?.NumHours ?? 0).ToString());
                            var total = (claim.Pay?.NumHours ?? 0) * (claim.Pay?.Rate ?? 0);
                            table.Cell().Element(CellStyle).Text($"R {total:N2}");
                        }
                    });

                    // Total
                    var totalAmount = claimList.Sum(c => (c.Pay?.NumHours ?? 0) * (c.Pay?.Rate ?? 0));
                    col.Item().AlignRight().Text($"Total: R {totalAmount:N2}").Bold();
                });
            });
        });

        return document.GeneratePdf();
    }

    private static IContainer CellStyle(IContainer container)
    {
        return container.Padding(5)
                        .Border(1)
                        .BorderColor(Colors.Grey.Lighten2);
    }
}