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
        // For development, use mock data
        // In production, you would need an API key from https://haveibeenpwned.com/API/Key
        var breaches = await GetMockBreaches();

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

    private async Task<List<Breach>> GetMockBreaches()
    {
        // Simulate API delay
        await Task.Delay(500);

        return new List<Breach>
        {
            new Breach
            {
                Title = "Adobe",
                Name = "Adobe",
                Domain = "adobe.com",
                BreachDate = new DateTime(2013, 10, 4),
                AddedDate = new DateTime(2013, 12, 4),
                ModifiedDate = new DateTime(2013, 12, 4),
                Description = "In October 2013, 153 million Adobe accounts were breached with each containing an internal ID, username, email, encrypted password and a password hint in plain text.",
                PwnCount = 153000000,
                LogoPath = "adobe.png"
            },
            new Breach
            {
                Title = "LinkedIn",
                Name = "LinkedIn",
                Domain = "linkedin.com",
                BreachDate = new DateTime(2012, 6, 5),
                AddedDate = new DateTime(2016, 5, 18),
                ModifiedDate = new DateTime(2016, 5, 18),
                Description = "In May 2016, LinkedIn had 164 million email addresses and passwords exposed. Originally hacked in 2012, the data remained out of sight until being offered for sale on a dark market site 4 years later.",
                PwnCount = 164000000,
                LogoPath = "linkedin.png"
            },
            new Breach
            {
                Title = "MySpace",
                Name = "MySpace",
                Domain = "myspace.com",
                BreachDate = new DateTime(2013, 1, 1),
                AddedDate = new DateTime(2016, 5, 31),
                ModifiedDate = new DateTime(2016, 5, 31),
                Description = "In approximately 2013, MySpace suffered a data breach that exposed 360 million accounts. The data was later traded online and included email addresses and passwords.",
                PwnCount = 360000000,
                LogoPath = "myspace.png"
            },
            new Breach
            {
                Title = "Dropbox",
                Name = "Dropbox",
                Domain = "dropbox.com",
                BreachDate = new DateTime(2012, 7, 1),
                AddedDate = new DateTime(2016, 8, 30),
                ModifiedDate = new DateTime(2016, 8, 30),
                Description = "In mid-2012, Dropbox suffered a data breach which exposed the stored credentials of tens of millions of their users. In August 2016, they forced password resets for customers they believed may be at risk.",
                PwnCount = 68700000,
                LogoPath = "dropbox.png"
            },
            new Breach
            {
                Title = "Tumblr",
                Name = "Tumblr",
                Domain = "tumblr.com",
                BreachDate = new DateTime(2013, 2, 28),
                AddedDate = new DateTime(2016, 5, 31),
                ModifiedDate = new DateTime(2016, 5, 31),
                Description = "In early 2013, Tumblr suffered a data breach which exposed over 65 million accounts. The data was later traded online and included email addresses and passwords stored as salted SHA1 hashes.",
                PwnCount = 65117000,
                LogoPath = "tumblr.png"
            }
        };
    }

    // Uncomment this method and comment out GetMockBreaches() when you have an API key
    /*
    private async Task<List<Breach>> GetRealBreaches()
    {
        try
        {
            var breaches = await ApiUrl
                .WithTimeout(30)
                .WithHeader("hibp-api-key", "YOUR_API_KEY_HERE") // Add your API key here
                .GetAsync()
                .ReceiveJson<List<Breach>>();

            return breaches;
        }
        catch (Exception ex)
        {
            // Log the error and return empty list or throw
            Console.WriteLine($"Error fetching breaches: {ex.Message}");
            return new List<Breach>();
        }
    }
    */

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
                    <td class='description'>{{description}}</td>
                </tr>
                {{/each}}
            </tbody>
        </table>
        
        <div class='footer'>
            <p>Report generated on {{generatedDate}}</p>
            <p>Data source: HaveIBeenPwned API</p>
        </div>
    </div>
</body>
</html>";

        var handlebars = Handlebars.Create();
        
        // Register helper functions
        handlebars.RegisterHelper("formatDate", (context, arguments) =>
        {
            if (arguments[0] is DateTime date)
            {
                return date.ToString("MMM dd, yyyy");
            }
            return arguments[0]?.ToString() ?? "";
        });

        handlebars.RegisterHelper("formatNumber", (context, arguments) =>
        {
            if (arguments[0] is long number)
            {
                return number.ToString("N0");
            }
            return arguments[0]?.ToString() ?? "";
        });

        var templateFunction = handlebars.Compile(template);

        var totalRecords = breaches.Sum(b => b.PwnCount);
        var averageRecords = breaches.Any() ? totalRecords / breaches.Count : 0;
        var dateRange = breaches.Any() 
            ? $"{breaches.Min(b => b.BreachDate):MMM dd, yyyy} to {breaches.Max(b => b.BreachDate):MMM dd, yyyy}"
            : "N/A";

        var data = new
        {
            breaches,
            totalBreaches = breaches.Count,
            hasBreaches = breaches.Any(),
            dateRange,
            totalRecords = totalRecords.ToString("N0"),
            averageRecords = averageRecords.ToString("N0"),
            generatedDate = DateTime.Now.ToString("MMM dd, yyyy 'at' HH:mm:ss"),
            hasDateFilter = request.From.HasValue || request.To.HasValue,
            fromDate = request.From?.ToString("MMM dd, yyyy"),
            toDate = request.To?.ToString("MMM dd, yyyy")
        };

        return templateFunction(data);
    }

    private async Task<byte[]> ConvertHtmlToPdf(string htmlContent)
    {
        // For development, return a simple HTML response instead of PDF
        // In production, you would use Puppeteer to convert HTML to PDF
        return Encoding.UTF8.GetBytes(htmlContent);
        
        // Uncomment the following code when you have Puppeteer installed
        /*
        await new BrowserFetcher().DownloadAsync();
        
        using var browser = await Puppeteer.LaunchAsync(new LaunchOptions
        {
            Headless = true
        });
        
        using var page = await browser.NewPageAsync();
        await page.SetContentAsync(htmlContent);
        
        var pdfBytes = await page.PdfAsync(new PdfOptions
        {
            Format = PaperFormat.A4,
            PrintBackground = true,
            MarginOptions = new MarginOptions
            {
                Top = "20px",
                Right = "20px",
                Bottom = "20px",
                Left = "20px"
            }
        });
        
        return pdfBytes;
        */
    }
} 