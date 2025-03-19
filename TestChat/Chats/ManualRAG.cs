using System.ClientModel;
using System.Text;
using Azure.AI.OpenAI;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.VectorData;
using Microsoft.SemanticKernel.Connectors.InMemory;

public static class ChatWithManualRAG {
    public static async Task<string> ChatAsync(string userMessage) {
        IConfigurationRoot configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json")
            .Build();
    
        string? azureFoundryGPTEndpoint = configuration["GPTEndpoint"];
        string? azureFoundryGPTModel = configuration["GPTModel"];
        string? azureFoundryGPTKey = configuration["GPTKey"];
        string? ollamaEndpoint = configuration["OllamaEndpoint"];
        string? ollamaModel = configuration["OllamaModel"];

        IChatClient client = new AzureOpenAIClient(new Uri(azureFoundryGPTEndpoint ?? ""), new ApiKeyCredential(azureFoundryGPTKey ?? "")).AsChatClient(azureFoundryGPTModel ?? "");

        var playersData = new List<Player>
        {
            new Player { Key = 1, Name = "Martin Martinez", Position = "GK", Team="Mis Amigos" },
            new Player { Key = 2, Name = "Gonzalo Gonzalez", Position = "DF", Team="Mis Amigos" },
            new Player { Key = 3, Name = "Rodrigo Rodriguez", Position = "FW", Team="Mis Amigos" },
            new Player { Key = 4, Name = "Juan Juarez", Position = "MF", Team="Otro Equipo" },
        };

        var vectorStore = new InMemoryVectorStore();
        var players = vectorStore.GetCollection<int, Player>("players");
        await players.CreateCollectionIfNotExistsAsync();

        IEmbeddingGenerator<string?, Embedding<float>> generator =
            new OllamaEmbeddingGenerator(new Uri(ollamaEndpoint ?? ""), ollamaModel ?? "") as IEmbeddingGenerator<string?, Embedding<float>>;

        foreach (var player in playersData)
        {
            player.Vector = await generator.GenerateEmbeddingVectorAsync(player.Team);
            await players.UpsertAsync(player);
        }

        var query = userMessage;
        var queryEmbedding = await generator.GenerateEmbeddingVectorAsync(query);
        var searchOptions = new VectorSearchOptions<Player>
        {
            VectorProperty = player => player.Vector
        };

        var results = await players.VectorizedSearchAsync(queryEmbedding, searchOptions);

        var resultado = new StringBuilder();
        await foreach (var result in results.Results)
        {
            resultado.AppendLine($"Nombre: {result.Record.Name} - Posicion: {result.Record.Position} - Equipo: {result.Record.Team}");
        }

        List<ChatMessage> messages = new List<ChatMessage>();
        messages.Add(new ChatMessage(ChatRole.System, resultado.ToString()));
        messages.Add(new ChatMessage(ChatRole.User, userMessage));

        var response = await client.GetResponseAsync(messages);

        return response.Messages.First().Text;
    }
}