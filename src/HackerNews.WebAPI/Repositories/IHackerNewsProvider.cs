using HackerNews.WebAPI.Entities;

namespace HackerNews.WebAPI.Repositories;

using TNewsEntity = HackerNewsEntity;

public interface IHackerNewsProvider
{
    IEnumerable<TNewsEntity> NewsByScore();
}
