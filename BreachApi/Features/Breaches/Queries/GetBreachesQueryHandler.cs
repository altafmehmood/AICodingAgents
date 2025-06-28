using MediatR;
using BreachApi.Models;
using Flurl.Http;

namespace BreachApi.Features.Breaches.Queries;

public class GetBreachesQueryHandler : IRequestHandler<GetBreachesQuery, List<Breach>>
{
    private const string ApiUrl = "https://haveibeenpwned.com/api/v2/breaches";

    public async Task<List<Breach>> Handle(GetBreachesQuery request, CancellationToken cancellationToken)
    {
        var breaches = await ApiUrl
            .WithTimeout(30)
            .GetAsync()
            .ReceiveJson<List<Breach>>();

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
} 