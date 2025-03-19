

using Azure;
using Azure.AI.OpenAI;
using Microsoft.Extensions.Configuration;
using OpenAI.Images;

public static class ImageGenerationWithDalle {
    public static async Task<string> ChatAsync(string userMessage) {
        IConfigurationRoot configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json")
            .Build();
    
        string? azureFoundryDalleEndpoint = configuration["DalleEndpoint"];
        string? azureFoundryDalleModel = configuration["DalleModel"];
        string? azureFoundryDalleKey = configuration["DalleKey"];

        AzureOpenAIClient client = new AzureOpenAIClient(new Uri(azureFoundryDalleEndpoint ?? ""), new AzureKeyCredential(azureFoundryDalleKey ?? ""));
        ImageClient imageClient = client.GetImageClient(azureFoundryDalleModel);

        ImageGenerationOptions imageGenerationOptions = new ImageGenerationOptions()
        {
            Quality = GeneratedImageQuality.High,
            Size = GeneratedImageSize.W1024xH1024,
            Style = GeneratedImageStyle.Natural,
            ResponseFormat = GeneratedImageFormat.Uri
        };

        GeneratedImage generatedImage = await imageClient.GenerateImageAsync(userMessage, imageGenerationOptions);
        return generatedImage.ImageUri.ToString();
    }
}