using MediatR;
using BreachApi.Models;
using Flurl.Http;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using QuestPDF.Drawing;
using QuestPDF.Elements;
using System.Globalization;

namespace BreachApi.Features.Breaches.Queries;

public class GetBreachesPdfQueryHandler : IRequestHandler<GetBreachesPdfQuery, byte[]>
{
    private const string ApiUrl = "https://haveibeenpwned.com/api/v2/breaches";

    public async Task<byte[]> Handle(GetBreachesPdfQuery request, CancellationToken cancellationToken)
    {
        // Get breaches data
        var breaches = await ApiUrl
            .WithTimeout(30)
            .GetAsync()
            .ReceiveJson<List<Breach>>();

        // Apply date filters
        if (request.From.HasValue)
            breaches = breaches.Where(b => b.BreachDate >= request.From.Value).ToList();
        if (request.To.HasValue)
            breaches = breaches.Where(b => b.BreachDate <= request.To.Value).ToList();

        // Sort by date descending
        breaches = breaches.OrderByDescending(b => b.BreachDate).ToList();

        // Generate PDF with QuestPDF
        var pdfBytes = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Margin(30);
                page.Size(PageSizes.A4);
                page.DefaultTextStyle(x => x.FontSize(10));

                page.Header().Element(ComposeHeader);
                page.Content().Element(c => ComposeTable(c, breaches, request));
                page.Footer().AlignCenter().Text($"Generated on {DateTime.Now:yyyy-MM-dd HH:mm:ss}").FontSize(8);
            });
        }).GeneratePdf();

        return pdfBytes;
    }

    private void ComposeHeader(IContainer container)
    {
        container.Column(col =>
        {
            col.Item().AlignCenter().Text("Data Breach Report").FontSize(20).Bold();
        });
    }

    private void ComposeTable(IContainer container, List<Breach> breaches, GetBreachesPdfQuery request)
    {
        var dateRange = "All Breaches";
        if (request.From.HasValue && request.To.HasValue)
            dateRange = $"Breaches from {request.From.Value:yyyy-MM-dd} to {request.To.Value:yyyy-MM-dd}";
        else if (request.From.HasValue)
            dateRange = $"Breaches from {request.From.Value:yyyy-MM-dd} onwards";
        else if (request.To.HasValue)
            dateRange = $"Breaches up to {request.To.Value:yyyy-MM-dd}";

        container.Column(col =>
        {
            col.Item().AlignCenter().Text(dateRange).FontSize(12).Italic();
            col.Item().AlignLeft().Text($"Total Breaches: {breaches.Count}").FontSize(10).Bold();
            col.Item().PaddingTop(10).Element(e =>
            {
                e.Table(table =>
                {
                    table.ColumnsDefinition(columns =>
                    {
                        columns.RelativeColumn(2); // Title
                        columns.RelativeColumn(2); // Domain
                        columns.RelativeColumn(1); // Breach Date
                        columns.RelativeColumn(1); // Pwn Count
                        columns.RelativeColumn(4); // Description
                    });

                    // Header
                    table.Header(header =>
                    {
                        header.Cell().Element(CellStyle).Text("Title").Bold();
                        header.Cell().Element(CellStyle).Text("Domain").Bold();
                        header.Cell().Element(CellStyle).Text("Breach Date").Bold();
                        header.Cell().Element(CellStyle).Text("Pwn Count").Bold();
                        header.Cell().Element(CellStyle).Text("Description").Bold();
                    });

                    // Data
                    foreach (var breach in breaches)
                    {
                        table.Cell().Element(CellStyle).Text(breach.Title);
                        table.Cell().Element(CellStyle).Text(breach.Domain);
                        table.Cell().Element(CellStyle).Text(breach.BreachDate.ToString("yyyy-MM-dd"));
                        table.Cell().Element(CellStyle).Text(breach.PwnCount.ToString("N0", CultureInfo.InvariantCulture));
                        var description = breach.Description.Length > 100 ? breach.Description.Substring(0, 97) + "..." : breach.Description;
                        table.Cell().Element(CellStyle).Text(description);
                    }
                });
            });
        });
    }

    private IContainer CellStyle(IContainer container)
    {
        return container.PaddingVertical(2).PaddingHorizontal(4).BorderBottom(1).BorderColor(Colors.Grey.Lighten2);
    }
} 