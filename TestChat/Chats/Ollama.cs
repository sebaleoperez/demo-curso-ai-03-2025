using Microsoft.Extensions.AI;
using Microsoft.Extensions.Configuration;

public static class ChatWithOllama {
    public static async Task<string> ChatAsync(string systemPrompt, string userMessage) {
        IConfigurationRoot configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json")
            .Build();
    
        string? ollamaEndpoint = configuration["OllamaEndpoint"];
        string? ollamaModel = configuration["OllamaModel"];

        IChatClient client = new OllamaChatClient(new Uri(ollamaEndpoint ?? "http://localhost:11434"), ollamaModel);

        List<Microsoft.Extensions.AI.ChatMessage> messages = new List<Microsoft.Extensions.AI.ChatMessage>();
        messages.Add(new Microsoft.Extensions.AI.ChatMessage(ChatRole.System, systemPrompt));
        messages.Add(new Microsoft.Extensions.AI.ChatMessage(ChatRole.User, userMessage));

        var response = await client.GetResponseAsync(messages);

        return response.Messages.First().Text;
    }
}

