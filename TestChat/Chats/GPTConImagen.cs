using System.ClientModel;
using Azure.AI.OpenAI;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Configuration;

public static class ImageChatWithGPT {
    public static async Task<string> ChatAsync(string systemPrompt, string userMessage) {
        IConfigurationRoot configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json")
            .Build();
    
        string? azureFoundryGPTEndpoint = configuration["GPTEndpoint"];
        string? azureFoundryGPTModel = configuration["GPTModel"];
        string? azureFoundryGPTKey = configuration["GPTKey"];

        IChatClient client = new AzureOpenAIClient(new Uri(azureFoundryGPTEndpoint ?? ""), new ApiKeyCredential(azureFoundryGPTKey ?? "")).AsChatClient(azureFoundryGPTModel ?? "");

        var imagePath = Path.Combine(Directory.GetCurrentDirectory(),"messi.jpg");
        AIContent imageContent = new DataContent(File.ReadAllBytes(imagePath), "image/jpeg");        

        List<ChatMessage> messages = new List<ChatMessage>();
        messages.Add(new ChatMessage(ChatRole.System, systemPrompt));
        messages.Add(new ChatMessage(ChatRole.User, userMessage));
        messages.Add(new ChatMessage(ChatRole.User, [imageContent]));
        
        var response = await client.GetResponseAsync(messages);

        return response.Messages.First().Text;
    }
}