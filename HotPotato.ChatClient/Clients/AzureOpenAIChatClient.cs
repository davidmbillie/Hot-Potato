using HotPotato.ChatClient.Models;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Options;
using System.Net.Http.Json;
using System.Text.Json;

namespace HotPotato.ChatClient;

public sealed class AzureOpenAIChatClient : IChatClient, IDisposable
{
	private readonly HttpClient _http;
	private readonly AzureOpenAIOptions _options;

	public AzureOpenAIChatClient(HttpClient http, IOptions<AzureOpenAIOptions> options)
	{
		_http = http;
		_options = options.Value;

		_http.BaseAddress = new Uri(_options.Endpoint);
		_http.DefaultRequestHeaders.Add("api-key", _options.ApiKey);
	}

	// ---------------------------
	// 1. Non‑streaming response
	// ---------------------------
	public async Task<ChatResponse> GetResponseAsync(
		IEnumerable<ChatMessage> messages,
		ChatOptions? options = null,
		CancellationToken cancellationToken = default)
	{
		var payload = new
		{
			model = _options.Deployment,
			messages = messages.Select(m => new {
				role = m.Role.ToString().ToLower(),
				content = m.Text
			}),
			stream = false
		};

		using var response = await _http.PostAsJsonAsync(
			$"/openai/deployments/{_options.Deployment}/chat/completions?api-version=2024-02-15-preview",
			payload,
			cancellationToken);

		response.EnsureSuccessStatusCode();

		using var json = await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync(cancellationToken), cancellationToken: cancellationToken);

		var content = json.RootElement
			.GetProperty("choices")[0]
			.GetProperty("message")
			.GetProperty("content")
			.GetString() ?? string.Empty;

		return new ChatResponse([new ChatMessage(ChatRole.Assistant, content)]);
	}

	// ---------------------------
	// 2. Streaming response
	// ---------------------------
	public async IAsyncEnumerable<ChatResponseUpdate> GetStreamingResponseAsync(
		IEnumerable<ChatMessage> messages,
		ChatOptions? options = null,
		[System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken = default)
	{
		var payload = new
		{
			model = _options.Deployment,
			messages = messages.Select(m => new {
				role = m.Role.ToString().ToLower(),
				content = m.Text
			}),
			stream = true
		};

		using var response = await _http.PostAsJsonAsync(
			$"/openai/deployments/{_options.Deployment}/chat/completions?api-version=2024-02-15-preview",
			payload,
			cancellationToken);

		response.EnsureSuccessStatusCode();

		using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
		using var reader = new StreamReader(stream);

		while (true)
		{
			var line = await reader.ReadLineAsync();
			if (line is null) yield break;
			if (!line.StartsWith("data:")) continue;

			var json = line["data:".Length..].Trim();
			if (json == "[DONE]") yield break;

			using var doc = JsonDocument.Parse(json);

			var delta = doc.RootElement
				.GetProperty("choices")[0]
				.GetProperty("delta")
				.GetProperty("content")
				.GetString();

			if (!string.IsNullOrEmpty(delta))
			{
				yield return new ChatResponseUpdate(ChatRole.Assistant, delta);
			}
		}
	}

	// ---------------------------
	// 3. Required by interface
	// ---------------------------
	public object? GetService(Type serviceType, object? serviceKey) => null;

	public void Dispose() => _http.Dispose();
}
