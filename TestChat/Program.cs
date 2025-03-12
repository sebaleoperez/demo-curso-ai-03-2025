// See https://aka.ms/new-console-template for more information
using System.ClientModel;
using System.Diagnostics;
using Azure;
using Azure.AI.OpenAI;
using Microsoft.Extensions.AI;
using OpenAI.Chat;
using OpenAI.Images;

/*
var deployName = "";
var endpoint = "";
var key = new ApiKeyCredential("");

IChatClient client = new AzureOpenAIClient(new Uri(endpoint), key).AsChatClient(deployName);
*/
IChatClient client = new OllamaChatClient(new Uri("http://localhost:11434/"), "llama3.2");

List<Microsoft.Extensions.AI.ChatMessage> messages = new List<Microsoft.Extensions.AI.ChatMessage>();
messages.Add(new Microsoft.Extensions.AI.ChatMessage(ChatRole.System, "Sos un asistente que saluda."));
messages.Add(new Microsoft.Extensions.AI.ChatMessage(ChatRole.User, "Quien es el de la imagen ?"));

var imagePath = "./messi.jpg";
AIContent imageContent = new DataContent(File.ReadAllBytes(imagePath), "image/jpeg");
messages.Add(new Microsoft.Extensions.AI.ChatMessage(ChatRole.User, [imageContent]));


var response = await client.GetResponseAsync(messages);

Console.WriteLine(response.Messages.First().Text);

// Ejemplo AzureOpenAI SDK

var endpoint = "";
var key = "";

AzureOpenAIClient client = new AzureOpenAIClient(new Uri(endpoint), new AzureKeyCredential(key));

List<ChatMessage> messages = new List<ChatMessage>();
messages.Add(new SystemChatMessage("Sos un asistente que sabe de cocinar, quiero que me des consejos para hacer un plato con los ingredientes que te de."));

ChatClient chatClient = client.GetChatClient("gpt-4o-mini");

ChatCompletion chatCompletion = await chatClient.CompleteChatAsync(messages);
string response = chatCompletion.Content[0].Text;

Console.WriteLine(response);

//messages.Add(new AssistantChatMessage(response));

string? request = Console.ReadLine();

messages.Add(new UserChatMessage(request));

chatCompletion = await chatClient.CompleteChatAsync(messages);
response = chatCompletion.Content[0].Text;

Console.WriteLine(response);

AzureOpenAIClient dalleClient = new AzureOpenAIClient(new Uri(""));
ImageClient imageClient = dalleClient.GetImageClient("");

ImageGenerationOptions imageGenerationOptions = new ImageGenerationOptions()
{
    Quality = GeneratedImageQuality.High,
    Size = GeneratedImageSize.W1024xH1024,
    Style = GeneratedImageStyle.Natural,
    ResponseFormat = GeneratedImageFormat.Uri
};

GeneratedImage generatedImage = await imageClient.GenerateImageAsync(response, imageGenerationOptions);

Process.Start(new ProcessStartInfo(generatedImage.ImageUri.ToString()) { UseShellExecute = true });