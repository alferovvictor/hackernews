using HackerNews.WebAPI;
using HackerNews.WebAPI.Entities;
using HackerNews.WebAPI.Repositories;
using HackerNews.WebAPI.Services;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using System.Net;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddSingleton<HackerNewsRepository>();
builder.Services.AddSingleton<IHackerNewsProvider>(sp => sp.GetService<HackerNewsRepository>()!);
builder.Services
	.AddHttpClient<HackerNewsService>(client =>
	{
		var baseUrl = builder.Configuration["HackerNews:BaseUrl"];
		client.BaseAddress = new Uri(baseUrl!);
	})
	.SetHandlerLifetime(TimeSpan.FromSeconds(5))
	.SetRetryPolicyHandler();

builder.Services.AddOutputCache(options =>
{
	options.AddBasePolicy(builder => builder.Expire(TimeSpan.FromDays(1)));
});

var app = builder.Build();

// Configure the HTTP request pipeline.

app.UseHttpsRedirection();
app.UseOutputCache();

// caching news ...
await app.Services.GetService<HackerNewsService>()!.Init();

// mapping endpoints
app
	.MapGet("/stories/{count:int}", (
		int count,
		HackerNewsRepository hackerNewsRepository) =>
	{
		return count > 0 ? hackerNewsRepository.NewsByScore().Take(count) : Array.Empty<HackerNewsEntity>();
	})
	.CacheOutput();

Task.Factory.StartNew(async () =>
{
	while (true)
	{
		await Task.Delay(TimeSpan.FromSeconds(1));

		var server = app.Services.GetService<IServer>();
		var addressFeature = server.Features.Get<IServerAddressesFeature>();

		if (addressFeature.Addresses?.Count() > 0 is false)
		{
			Console.WriteLine($"wating ...");
			continue;
		}

		Console.WriteLine("");
		Console.WriteLine("Open in a web bowser:");

		foreach (var address in addressFeature.Addresses)
		{
			Console.WriteLine($"{address}/stories/50");
		}

		return;
	}	
});

// run
app.Run();
