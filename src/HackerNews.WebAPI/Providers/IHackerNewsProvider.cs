using HackerNews.WebAPI.Entities;

namespace HackerNews.WebAPI.Providers;

using TNewsEntity = HackerNewsEntity;

public interface IHackerNewsProvider
{
    IEnumerable<TNewsEntity> NewsByScore(int count);
}
