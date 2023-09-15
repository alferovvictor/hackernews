using Polly;
using Polly.Extensions.Http;

namespace HackerNews.WebAPI;

public static class HttpExtensions
{
	public static IHttpClientBuilder SetRetryPolicyHandler(this IHttpClientBuilder builder)
	{
		return builder.AddPolicyHandler(HttpPolicyExtensions
			.HandleTransientHttpError()
			.OrResult(msg => !msg.IsSuccessStatusCode)
			.WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt))));
	}
}
