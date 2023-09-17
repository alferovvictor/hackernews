using HackerNews.WebAPI.Entities;
using HackerNews.WebAPI.Repositories;
using System.Diagnostics;

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
		var sw = Stopwatch.StartNew();

		_logger.LogInformation("Init start...");

		var ids = await _httpClient.GetFromJsonAsync<ulong[]>("beststories.json") ?? Array.Empty<ulong>();

		var tasks = ids
			.AsParallel()
			.Select(id =>
			{
				return _httpClient
				.GetFromJsonAsync<HackerNewsSourceDto>($"item/{id}.json")
				.ContinueWith(res =>
				{
					var item = res.Result;

					_logger.LogDebug($"item/{id} loaded");

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
			});

		await Task.WhenAll(tasks);

		var items = tasks
			.Select(i => i.Result)
			.Where(i => i is not null)
			.ToArray();

		_logger.LogInformation($"Total news: {ids.Length}, Loaded: {items.Length}. Elapsed: {sw.Elapsed.TotalSeconds}s");

		_hackerNewsRepository.AddRange(items!);

		_logger.LogInformation("Init end");
	}
}

file sealed class HackerNewsSourceDto
{
	public string by { get; set; } = string.Empty;
	public string title { get; set; } = string.Empty;
	public string url { get; set; } = string.Empty;
	public long time { get; set; }
	public int score { get; set; }
	public int[] kids { get; set; } = Array.Empty<int>();
}
