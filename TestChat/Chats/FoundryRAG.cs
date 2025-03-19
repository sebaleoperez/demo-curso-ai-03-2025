using System.ClientModel;
using Azure;  
using Azure.AI.OpenAI;
using Azure.AI.OpenAI.Chat;
using Azure.Identity;
using Microsoft.Extensions.Configuration;
using OpenAI.Chat;
public static class AzureAIFoundryRAG {
    
    public static async Task<string> ChatAsync(string systemPrompt, string userMessage) {
        IConfigurationRoot configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json")
            .Build();
    
        string? azureFoundryRAGEndpoint = configuration["GPTEndpoint"];
        string? azureFoundryRAGModel = configuration["GPTModel"];
        string? azureFoundryRAGKey = configuration["GPTKey"];
        string? IndexName = configuration["IndexName"];
        string? AzureAISearchEndpoint = configuration["AzureAISearchEndpoint"];
        string? AzureAISearchKey = configuration["AzureAISearchKey"];

        AzureOpenAIClient openAIClient = new(
                    new Uri(azureFoundryRAGEndpoint ?? ""),
                    new ApiKeyCredential(azureFoundryRAGKey ?? ""));
        ChatClient chatClient = openAIClient.GetChatClient(azureFoundryRAGModel); 

        ChatCompletionOptions options = new();
#pragma warning disable AOAI001 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
        options.AddDataSource(new AzureSearchChatDataSource()
        {
            Endpoint = new Uri(AzureAISearchEndpoint ?? ""),
            IndexName = IndexName,
            Authentication = DataSourceAuthentication.FromApiKey(AzureAISearchKey),
        });
#pragma warning restore AOAI001 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.

        List<ChatMessage> messages = new List<ChatMessage>();
        messages.Add(new SystemChatMessage(systemPrompt));
        messages.Add(new UserChatMessage(userMessage));

        ChatCompletion completion = await chatClient.CompleteChatAsync(messages, options);

        return completion.Content.Last().Text;
    }
    
}