using MediatR;
using BreachApi.Models;
using Flurl.Http;
using HandlebarsDotNet;
using PuppeteerSharp;
using System.Text;

namespace BreachApi.Features.Breaches.Queries;

public class GetBreachesPdfQueryHandler : IRequestHandler<GetBreachesPdfQuery, byte[]>
{
    private const string ApiUrl = "https://haveibeenpwned.com/api/v3/breaches";

    public async Task<byte[]> Handle(GetBreachesPdfQuery request, CancellationToken cancellationToken)
    {
        // Fetch breaches from HaveIBeenPwned API
        var breaches = await ApiUrl
            .WithTimeout(30)
            .GetAsync()
            .ReceiveJson<List<Breach>>();

        // Apply date filters if provided
        if (request.From.HasValue)
        {
            breaches = breaches.Where(b => b.BreachDate >= request.From.Value).ToList();
        }

        if (request.To.HasValue)
        {
            breaches = breaches.Where(b => b.BreachDate <= request.To.Value).ToList();
        }

        // Sort by breach date descending
        breaches = breaches.OrderByDescending(b => b.BreachDate).ToList();

        // Generate HTML using Handlebars template
        var htmlContent = GenerateHtmlWithHandlebars(breaches, request);

        // Convert HTML to PDF using Puppeteer
        return await ConvertHtmlToPdf(htmlContent);
    }

    private string GenerateHtmlWithHandlebars(List<Breach> breaches, GetBreachesPdfQuery request)
    {
        var template = @"
<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'>
    <style>
        body { 
            font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif; 
            margin: 0; 
            padding: 20px; 
            background-color: #f8f9fa; 
        }
        .container { 
            max-width: 1200px; 
            margin: 0 auto; 
            background-color: white; 
            padding: 30px; 
            border-radius: 8px; 
            box-shadow: 0 2px 10px rgba(0,0,0,0.1); 
        }
        h1 { 
            color: #2c3e50; 
            text-align: center; 
            margin-bottom: 30px; 
            font-size: 2.5em; 
            border-bottom: 3px solid #3498db; 
            padding-bottom: 10px; 
        }
        .summary { 
            background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); 
            color: white; 
            padding: 25px; 
            margin: 20px 0; 
            border-radius: 10px; 
            box-shadow: 0 4px 15px rgba(0,0,0,0.1); 
        }
        .summary h3 { 
            margin-top: 0; 
            font-size: 1.5em; 
        }
        .summary p { 
            margin: 8px 0; 
            font-size: 1.1em; 
        }
        table { 
            width: 100%; 
            border-collapse: collapse; 
            margin-top: 20px; 
            background-color: white; 
            border-radius: 8px; 
            overflow: hidden; 
            box-shadow: 0 4px 15px rgba(0,0,0,0.1); 
        }
        th { 
            background: linear-gradient(135deg, #34495e 0%, #2c3e50 100%); 
            color: white; 
            padding: 15px; 
            text-align: left; 
            font-weight: 600; 
            font-size: 1.1em; 
        }
        td { 
            padding: 12px 15px; 
            border-bottom: 1px solid #ecf0f1; 
            font-size: 1em; 
        }
        tr:nth-child(even) { 
            background-color: #f8f9fa; 
        }
        tr:hover { 
            background-color: #e3f2fd; 
            transition: background-color 0.3s ease; 
        }
        .breach-date { 
            color: #e74c3c; 
            font-weight: bold; 
            font-size: 1.1em; 
        }
        .breach-name { 
            color: #2980b9; 
            font-weight: bold; 
            font-size: 1.1em; 
        }
        .pwn-count { 
            font-weight: bold; 
            color: #27ae60; 
        }
        .description { 
            font-size: 0.9em; 
            color: #7f8c8d; 
        }
        .footer { 
            margin-top: 40px; 
            text-align: center; 
            color: #7f8c8d; 
            font-size: 0.9em; 
            border-top: 1px solid #ecf0f1; 
            padding-top: 20px; 
        }
        .date-range { 
            background-color: #ecf0f1; 
            padding: 15px; 
            border-radius: 5px; 
            margin: 20px 0; 
            text-align: center; 
            font-style: italic; 
            color: #2c3e50; 
        }
    </style>
</head>
<body>
    <div class='container'>
        <h1>Data Breach Report</h1>
        
        <div class='date-range'>
            {{#if hasDateFilter}}
                {{#if fromDate}}
                    {{#if toDate}}
                        Breaches from {{fromDate}} to {{toDate}}
                    {{else}}
                        Breaches from {{fromDate}} onwards
                    {{/if}}
                {{else}}
                    {{#if toDate}}
                        Breaches up to {{toDate}}
                    {{/if}}
                {{/if}}
            {{else}}
                All Available Breaches
            {{/if}}
        </div>
        
        <div class='summary'>
            <h3>Summary</h3>
            <p><strong>Total Breaches:</strong> {{totalBreaches}}</p>
            {{#if hasBreaches}}
                <p><strong>Date Range:</strong> {{dateRange}}</p>
                <p><strong>Total Records Affected:</strong> {{totalRecords}}</p>
                <p><strong>Average Records per Breach:</strong> {{averageRecords}}</p>
            {{/if}}
        </div>
        
        <table>
            <thead>
                <tr>
                    <th>Breach Date</th>
                    <th>Name</th>
                    <th>Domain</th>
                    <th>Records Affected</th>
                    <th>Description</th>
                </tr>
            </thead>
            <tbody>
                {{#each breaches}}
                <tr>
                    <td class='breach-date'>{{formatDate breachDate}}</td>
                    <td class='breach-name'>{{name}}</td>
                    <td>{{domain}}</td>
                    <td class='pwn-count'>{{formatNumber pwnCount}}</td>
                    <td class='description'>{{formatDescription description}}</td>
                </tr>
                {{/each}}
            </tbody>
        </table>
        
        <div class='footer'>
            <p>Generated on {{generatedDate}}</p>
            <p>Data source: HaveIBeenPwned API</p>
            <p>Total processing time: {{processingTime}}ms</p>
        </div>
    </div>
</body>
</html>";

        // Register custom helpers
        Handlebars.RegisterHelper("formatDate", (context, arguments) =>
        {
            if (arguments[0] is DateTime date)
            {
                return date.ToString("yyyy-MM-dd");
            }
            return arguments[0]?.ToString() ?? "";
        });

        Handlebars.RegisterHelper("formatNumber", (context, arguments) =>
        {
            if (arguments[0] is long number)
            {
                return number.ToString("N0");
            }
            return arguments[0]?.ToString() ?? "0";
        });

        Handlebars.RegisterHelper("formatDescription", (context, arguments) =>
        {
            if (arguments[0] is string description)
            {
                if (description.Length > 100)
                {
                    return description.Substring(0, 97) + "...";
                }
                return description;
            }
            return "";
        });

        // Prepare data for template
        var templateData = new
        {
            breaches = breaches.Select(b => new
            {
                breachDate = b.BreachDate,
                name = b.Name,
                domain = b.Domain,
                pwnCount = b.PwnCount,
                description = b.Description
            }).ToList(),
            totalBreaches = breaches.Count,
            hasBreaches = breaches.Any(),
            dateRange = breaches.Any() ? $"{breaches.Min(b => b.BreachDate):yyyy-MM-dd} to {breaches.Max(b => b.BreachDate):yyyy-MM-dd}" : "N/A",
            totalRecords = breaches.Sum(b => b.PwnCount).ToString("N0"),
            averageRecords = breaches.Any() ? (breaches.Sum(b => b.PwnCount) / breaches.Count).ToString("N0") : "0",
            hasDateFilter = request.From.HasValue || request.To.HasValue,
            fromDate = request.From?.ToString("yyyy-MM-dd"),
            toDate = request.To?.ToString("yyyy-MM-dd"),
            generatedDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
            processingTime = 0 // Will be calculated if needed
        };

        // Compile and execute template
        var compiledTemplate = Handlebars.Compile(template);
        return compiledTemplate(templateData);
    }

    private async Task<byte[]> ConvertHtmlToPdf(string htmlContent)
    {
        // Download browser if not exists
        await new BrowserFetcher().DownloadAsync();

        using var browser = await Puppeteer.LaunchAsync(new LaunchOptions
        {
            Headless = true,
            Args = new[] { "--no-sandbox", "--disable-setuid-sandbox" }
        });

        using var page = await browser.NewPageAsync();
        
        // Set content and wait for network idle
        await page.SetContentAsync(htmlContent);
        await page.WaitForNetworkIdleAsync();

        // Generate PDF
        var pdfBytes = await page.PdfDataAsync(new PdfOptions
        {
            Format = PuppeteerSharp.Media.PaperFormat.A4,
            PrintBackground = true
        });

        return pdfBytes;
    }
} 