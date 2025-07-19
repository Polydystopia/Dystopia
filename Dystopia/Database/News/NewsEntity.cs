using System.ComponentModel.DataAnnotations;
using PolytopiaBackendBase.Game;

namespace Dystopia.Database.News;

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

    public static explicit operator NewsItem(NewsEntity v)
    {
        return new NewsItem
        {
            Id = v.Id,
            Date = v.GetUnixTimestamp(),
            Body = v.Body,
            Image = v.Image,
            Link = v.Link
        };
    }
}

public enum NewsType
{
    News,
    SystemMessage
}
