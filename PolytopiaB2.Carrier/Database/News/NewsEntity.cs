using System.ComponentModel.DataAnnotations;

namespace PolytopiaB2.Carrier.Database.News;

public class NewsEntity
{
    [Key]
    public int Id { get; set; }

    public NewsType NewsType { get; set; }

    [Required]
    public string Body { get; set; }

    public string? Link { get; set; }

    public string? Image { get; set; }

    [Required]
    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public bool IsActive { get; set; }

    public long GetUnixTimestamp() => new DateTimeOffset(CreatedAt).ToUnixTimeSeconds();
}

public enum NewsType
{
    News,
    SystemMessage
}
