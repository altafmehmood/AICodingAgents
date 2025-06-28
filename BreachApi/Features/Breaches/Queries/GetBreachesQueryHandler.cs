using MediatR;
using BreachApi.Models;
using Flurl.Http;

namespace BreachApi.Features.Breaches.Queries;

public class GetBreachesQueryHandler : IRequestHandler<GetBreachesQuery, List<Breach>>
{
    private const string ApiUrl = "https://haveibeenpwned.com/api/v2/breaches";

    public async Task<List<Breach>> Handle(GetBreachesQuery request, CancellationToken cancellationToken)
    {
        // For development, use mock data
        // In production, you would need an API key from https://haveibeenpwned.com/API/Key
        var breaches = await GetMockBreaches();

        if (request.From.HasValue)
        {
            breaches = breaches.Where(b => b.BreachDate >= request.From.Value).ToList();
        }
        if (request.To.HasValue)
        {
            breaches = breaches.Where(b => b.BreachDate <= request.To.Value).ToList();
        }

        return breaches.OrderByDescending(b => b.BreachDate).ToList();
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
} 