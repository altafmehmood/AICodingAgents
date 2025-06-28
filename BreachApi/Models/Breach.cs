namespace BreachApi.Models;

public class Breach
{
    public string Title { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Domain { get; set; } = string.Empty;
    public DateTime BreachDate { get; set; }
    public DateTime AddedDate { get; set; }
    public DateTime ModifiedDate { get; set; }
    public string Description { get; set; } = string.Empty;
    public long PwnCount { get; set; }
    public string LogoPath { get; set; } = string.Empty;
    // Add other fields as needed from the API
} 