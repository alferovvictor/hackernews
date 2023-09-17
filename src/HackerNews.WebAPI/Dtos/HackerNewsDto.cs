using HackerNews.WebAPI.Entities;

namespace HackerNews.WebAPI.Dtos;

public class HackerNewsDto
{
	public string Title { get; set; } = string.Empty;
	public string Uri { get; set; } = string.Empty;
	public string PostedBy { get; set; } = string.Empty;
	public DateTime Time { get; set; }
	public int Score { get; set; }
	public int CommentCount { get; set; }

	public static explicit operator HackerNewsDto(HackerNewsEntity e)
	{
		return new HackerNewsDto
		{
			CommentCount = e.CommentCount,
			PostedBy = e.PostedBy,
			Score = e.Score,
			Time = e.Time,
			Title = e.Title,
			Uri = e.Uri,
		};
	}
}
