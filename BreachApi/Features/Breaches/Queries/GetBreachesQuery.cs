using MediatR;
using BreachApi.Models;

namespace BreachApi.Features.Breaches.Queries;

public class GetBreachesQuery : IRequest<List<Breach>>
{
    public DateTime? From { get; set; }
    public DateTime? To { get; set; }
    public GetBreachesQuery(DateTime? from = null, DateTime? to = null)
    {
        From = from;
        To = to;
    }
} 