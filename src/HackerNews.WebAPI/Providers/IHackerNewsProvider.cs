using HackerNews.WebAPI.Dtos;
using HackerNews.WebAPI.Entities;

namespace HackerNews.WebAPI.Providers;

public interface IHackerNewsProvider
{
    IEnumerable<HackerNewsEntity> NewsByScore(int count);
}
