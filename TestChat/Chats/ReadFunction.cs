using System.ClientModel;
using Azure.AI.OpenAI;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Configuration;
using Azure.AI.Inference;
using System.Text.Json;
using Azure;

public static class EjecutarFunciones {
    public static async Task<string> ChatAsync(string systemPrompt, string userMessage) {
        IConfigurationRoot configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json")
            .Build();

        string? azureFoundryGPTEndpoint = configuration["AzureAIInferenceEndpoint"];
        string? azureFoundryGPTModel = configuration["GPTModel"];
        string? azureFoundryGPTKey = configuration["GPTKey"];

        var functionDefinition = new FunctionDefinition("getCharacterInfoAsync")
        {
            Description = "Returns the height, mass, hair color and eyes color of a character.",
            Parameters = BinaryData.FromObjectAsJson(new
            {
                Type = "object",
                Properties = new
                {
                    characterName = new
                    {
                        Type = "string",
                        Description = "The name of the character.",
                    }
                }
            },
            new JsonSerializerOptions(){ PropertyNamingPolicy = JsonNamingPolicy.CamelCase })
        };
        ChatCompletionsToolDefinition getCharacter = new ChatCompletionsToolDefinition(functionDefinition);

        var functionDefinitionEmail = new FunctionDefinition("SendEmail")
        {
            Description = "Sends an email to a specified receiver, with a subject and body. This is not a letter or a card." +
                        "Reject if the user asks for letter or card. The receiver should be an email address." +
                        "The response is a boolean indicating whether the email was sent successfully.",
            Parameters = BinaryData.FromObjectAsJson(new
            {
                Type = "object",
                Properties = new
                {
                    to = new
                    {
                        Type = "string",
                        Description = "The email address of the receiver",
                    },
                    subject = new
                    {
                        Type = "string",
                        Description = "The subject of the email",
                    },
                    body = new
                    {
                        Type = "string",
                        Description = "The body of the email",
                    }
                }
            },
            new JsonSerializerOptions(){ PropertyNamingPolicy = JsonNamingPolicy.CamelCase })
        };
        ChatCompletionsToolDefinition sendEmail = new ChatCompletionsToolDefinition(functionDefinitionEmail);

        var flightFunctionDefinition = new FunctionDefinition("getFlightInfo")
        {
            Description = "Returns information about the next flight between two cities." +
                  "This includes the name of the airline, flight number and the date and time" +
                  "of the next flight",
            Parameters = BinaryData.FromObjectAsJson(new
            {
                Type = "object",
                Properties = new
                {
                    originCity = new
                    {
                        Type = "string",
                        Description = "The name of the city where the flight originates",
                    },
                    destinationCity = new
                    {
                        Type = "string",
                        Description = "The flight destination city",
                    }
                }
            },
            new JsonSerializerOptions(){ PropertyNamingPolicy = JsonNamingPolicy.CamelCase })
        };
        ChatCompletionsToolDefinition getFlightInfo = new ChatCompletionsToolDefinition(flightFunctionDefinition);

        var requestOptions = new ChatCompletionsOptions()
        {
            Messages =
            {
                new ChatRequestSystemMessage(systemPrompt),
                new ChatRequestUserMessage(userMessage),
            },
            Tools = { getCharacter, sendEmail, getFlightInfo },
            Model = azureFoundryGPTModel,
        };

        var oai_client = new ChatCompletionsClient(
            new Uri(azureFoundryGPTEndpoint ?? ""),
            new AzureKeyCredential(azureFoundryGPTKey ?? ""));

        Response<ChatCompletions> tool_response = await oai_client.CompleteAsync(requestOptions);

        if (tool_response.GetRawResponse().IsError)
        {
            throw new Exception(tool_response.GetRawResponse().ToString());
        }

        if (tool_response.Value.FinishReason == "tool_calls")
        {
            // Append the model response to the chat history
            requestOptions.Messages.Add(new ChatRequestAssistantMessage(tool_response.Value));

            // We expect a single tool call
            if (tool_response.Value.ToolCalls.Count == 1)
            {
                ChatCompletionsToolCall toolCall = tool_response.Value.ToolCalls[0];
                var functionArgs = JsonSerializer.Deserialize<Dictionary<string, string>>(toolCall.Arguments);
                var externalToolsType = Type.GetType("ExternalTools");
                if (externalToolsType == null)
                {
                    throw new InvalidOperationException("The type 'ExternalTools' could not be found.");
                }
                var callableFunc = externalToolsType.GetMethod(toolCall.Name);
                if (callableFunc == null)
                {
                    throw new InvalidOperationException($"The method '{toolCall.Name}' could not be found in 'ExternalTools'.");
                }
                var requiredParams = callableFunc.GetParameters();
                object[] parsedArgs = new object[requiredParams.Length];
                for(int i=0; i < requiredParams.Length; i++)
                {
                    parsedArgs[i] = (functionArgs as IReadOnlyDictionary<string?, string>)?.GetValueOrDefault(requiredParams[i].Name, "") ?? "";
                }
                var functionReturn = callableFunc.Invoke(null, parsedArgs);

                Console.WriteLine($"Function {toolCall.Name} returned: {functionReturn}");

                requestOptions.Messages.Add(new ChatRequestToolMessage(
                    toolCallId: toolCall.Id,
                    content: functionReturn?.ToString() ?? string.Empty
                ));
                tool_response = await oai_client.CompleteAsync(requestOptions);
                return tool_response.Value.Content;
            }
            return "No tool call was made.";
        }
        else {
            return tool_response.Value.Content;
        }
    }
}