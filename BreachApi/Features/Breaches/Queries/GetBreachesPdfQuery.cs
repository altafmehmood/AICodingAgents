using MediatR;

namespace BreachApi.Features.Breaches.Queries;

public class GetBreachesPdfQuery : IRequest<byte[]>
{
    public DateTime? From { get; set; }
    public DateTime? To { get; set; }
    
    public GetBreachesPdfQuery(DateTime? from = null, DateTime? to = null)
    {
        From = from;
        To = to;
    }
} 