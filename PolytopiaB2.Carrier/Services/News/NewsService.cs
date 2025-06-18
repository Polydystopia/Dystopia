using PolytopiaBackendBase.Game;

namespace PolytopiaB2.Carrier.Services.News;

public class NewsService : INewsService
{
    public List<NewsItem> GetNews()
    {
        var news = new List<NewsItem>();

        var item = new NewsItem();
        item.Id = 2;
        var specificDate2 = new DateTime(2025, 6, 8, 0, 0, 0, DateTimeKind.Utc);
        item.Date = new DateTimeOffset(specificDate2).ToUnixTimeSeconds();

        item.Body = "Custom server WIP\nPlease report any bugs to the developers at\nhttps://github.com/Polydystopia/curly-octo-waffle\n\ndiscord: juli.gg\nmail: polydystopia@juli.gg\n\nHave fun!";
        item.Link = "https://github.com/Polydystopia/curly-octo-waffle";

        item.Image = @"https://avatars.githubusercontent.com/u/120461041";

        news.Add(item);

        return news;
    }

    public string GetSystemMessage()
    {
        return "Custom server WIP\nPlease report any bugs to the developers at\nhttps://github.com/Polydystopia/curly-octo-waffle\n\ndiscord: juli.gg\nmail: polydystopia@juli.gg\n\nHave fun!\n" + Guid.NewGuid();
    }
}