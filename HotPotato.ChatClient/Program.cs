using HotPotato.ChatClient;
using HotPotato.ChatClient.Models;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = Host.CreateApplicationBuilder(args);

// Bind options
builder.Services.Configure<AzureOpenAIOptions>(
	builder.Configuration.GetSection("AzureOpenAI"));

// Register your custom client
builder.Services.AddHttpClient<IChatClient, AzureOpenAIChatClient>();

var app = builder.Build();

var chat = app.Services.GetRequiredService<IChatClient>();

Console.WriteLine("HotPotato Chat Client (Azure AI Foundry)");
Console.WriteLine("Type 'exit' to quit.");
Console.WriteLine();

while (true)
{
	Console.Write("You: ");
	var input = Console.ReadLine();

	if (string.Equals(input, "exit", StringComparison.OrdinalIgnoreCase))
		break;

	var messages = new List<ChatMessage>
	{
		new(ChatRole.User, input)
	};

	Console.Write("AI: ");

	await foreach (var update in chat.GetStreamingResponseAsync(messages))
	{
		if (update.Text is string text)
			Console.Write(text);
	}

	Console.WriteLine();
}
