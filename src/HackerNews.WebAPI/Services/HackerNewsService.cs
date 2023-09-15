using HackerNews.WebAPI.Entities;
using HackerNews.WebAPI.Repositories;

namespace HackerNews.WebAPI.Services;

public class HackerNewsService
{
    private HttpClient _httpClient { get; }
    private HackerNewsRepository _hackerNewsRepository { get; }
    private ILogger _logger { get; }

    public HackerNewsService(
        HttpClient httpClient,
        HackerNewsRepository hackerNewsRepository,
        ILogger<HackerNewsService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
        _hackerNewsRepository = hackerNewsRepository;
    }

    public async Task Init()
    {
        _logger.LogInformation("Init start...");

        var ids = await _httpClient.GetFromJsonAsync<ulong[]>("beststories.json") ?? Array.Empty<ulong>();

        var tasks = ids.AsParallel()
            .Select(async id =>
            {
                var item = await _httpClient.GetFromJsonAsync<HackerNewsDto>($"item/{id}.json");

                if (item is null)
                {
                    return null;
                }

                return new HackerNewsEntity
                {
                    ID = id,
                    CommentCount = item!.kids.Length,
                    PostedBy = item.by,
                    Score = item.score,
                    Time = DateTimeOffset.FromUnixTimeSeconds(item.time).DateTime,
                    Title = item.title,
                    Uri = item.url,
                };
            });

        await Task.WhenAll(tasks);


        var items = tasks
			.Select(i => i.Result)
            .Where(i => i is not null)
            .ToArray();

        _hackerNewsRepository.AddRange(items!);

        _logger.LogInformation("Init end");
    }
}

file sealed class HackerNewsDto
{
    public string by { get; set; }
    public string title { get; set; }
    public string url { get; set; }
    public long time { get; set; }
    public int score { get; set; }
    public int[] kids { get; set; }
}
