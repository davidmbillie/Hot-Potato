using Azure;
using Azure.AI.OpenAI;
using HotPotato.ChatClient.Models;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Options;
using OpenAI.Chat;

namespace HotPotato.ChatClient;

public sealed class AzureOpenAIChatClient : IChatClient, IDisposable
{
	private readonly OpenAI.Chat.ChatClient _client;

	public AzureOpenAIChatClient(IOptions<AzureOpenAIOptions> options)
	{
		var cfg = options.Value;

		var azureClient = new AzureOpenAIClient(
			new Uri(cfg.Endpoint),
			new AzureKeyCredential(cfg.ApiKey));

		_client = azureClient.GetChatClient(cfg.Deployment);
	}

	// ------------------------------------------------------------
	// 1. Non‑streaming
	// ------------------------------------------------------------
	public async Task<ChatResponse> GetResponseAsync(
		IEnumerable<Microsoft.Extensions.AI.ChatMessage> messages,
		ChatOptions? options = null,
		CancellationToken cancellationToken = default)
	{
		var sdkMessages = ConvertMessages(messages);

		var result = await _client.CompleteChatAsync(
			sdkMessages,
			cancellationToken: cancellationToken);

		var text = result.Value.Content[0].Text;

		return new ChatResponse([
			new Microsoft.Extensions.AI.ChatMessage(
				Microsoft.Extensions.AI.ChatRole.Assistant,
				text)
		]);
	}

	// ------------------------------------------------------------
	// 2. Streaming (sync → async wrapper)
	// ------------------------------------------------------------
	public async IAsyncEnumerable<ChatResponseUpdate> GetStreamingResponseAsync(
		IEnumerable<Microsoft.Extensions.AI.ChatMessage> messages,
		ChatOptions? options = null,
		[System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken = default)
	{
		var sdkMessages = ConvertMessages(messages);

		// NEW SDK: synchronous streaming
		var updates = _client.CompleteChatStreaming(
			sdkMessages,
			cancellationToken: cancellationToken);

		foreach (var update in updates)
		{
			foreach (var part in update.ContentUpdate)
			{
				if (!string.IsNullOrEmpty(part.Text))
				{
					yield return new ChatResponseUpdate(
						Microsoft.Extensions.AI.ChatRole.Assistant,
						part.Text);
				}
			}

			// allow cancellation between chunks
			await Task.Yield();
		}
	}

	// ------------------------------------------------------------
	// Helper: Convert abstraction messages → SDK messages
	// ------------------------------------------------------------
	private static IEnumerable<OpenAI.Chat.ChatMessage> ConvertMessages(
		IEnumerable<Microsoft.Extensions.AI.ChatMessage> messages)
	{
		foreach (var m in messages)
		{
			if (m.Role == Microsoft.Extensions.AI.ChatRole.User)
				yield return new UserChatMessage(m.Text);
			else if (m.Role == Microsoft.Extensions.AI.ChatRole.System)
				yield return new SystemChatMessage(m.Text);
			else if (m.Role == Microsoft.Extensions.AI.ChatRole.Assistant)
				yield return new AssistantChatMessage(m.Text);
			else
				yield return new UserChatMessage(m.Text);
		}
	}

	public object? GetService(Type serviceType, object? serviceKey) => null;

	public void Dispose() { }
}
