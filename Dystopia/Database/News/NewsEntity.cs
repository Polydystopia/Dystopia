using System.ComponentModel.DataAnnotations;

namespace Dystopia.Database.News;

public class NewsEntity
{
    [Key]
    public int Id { get; init; }

    public NewsType NewsType { get; init; }

    [Required]
    public string Body { get; init; }

    public string? Link { get; init; }

    public string? Image { get; init; }

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
