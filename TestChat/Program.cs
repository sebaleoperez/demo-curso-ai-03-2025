string systemPrompt = "Sos un chat que responde con referencias a Los Simpsons.";
string userMessage = "Hola como estas ?";
string imageSystemPrompt = "Sos un asistente que describe y analiza imagenes.";
string imageMessage = "Hola, quien es la persona en la imagen ?";
string dalleInput = "La ciudad de Springfield de los simpsons.";
string ragMessage = "Cuales son los convocados por Argentina para el proximo partido ?";
string manualRAGMessage = "Listame los jugadores de Mis Amigos";

Console.WriteLine("Chat with Ollama:");
Console.WriteLine(await ChatWithOllama.ChatAsync(systemPrompt,userMessage));

Console.WriteLine("Chat with GPT4oMini:");
Console.WriteLine(await ChatWithGPT.ChatAsync(systemPrompt,userMessage));

Console.WriteLine("Chat Image with GPT4oMini:");
Console.WriteLine(await ImageChatWithGPT.ChatAsync(imageSystemPrompt,imageMessage));

Console.WriteLine("Image generation with Dalle:");
Console.WriteLine(await ImageGenerationWithDalle.ChatAsync(dalleInput));

Console.WriteLine("Chat with AzureAIFoundryRAG:");
Console.WriteLine(await AzureAIFoundryRAG.ChatAsync(systemPrompt,ragMessage));

Console.WriteLine("Chat with ManualRAG:");
Console.WriteLine(await ChatWithManualRAG.ChatAsync(manualRAGMessage));

Console.WriteLine("Averiguar info del vuelo:");
Console.WriteLine(await EjecutarFunciones.ChatAsync("Sos un asistente con proposito general.","Quiero informacion del proximo vuelo que vaya de Seattle a Miami."));

Console.WriteLine("Enviar un email:");
Console.WriteLine(await EjecutarFunciones.ChatAsync("Sos un asistente con proposito general.","Enviar una carta a xxx con contenido hola"));