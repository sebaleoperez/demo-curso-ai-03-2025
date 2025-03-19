
using System.Net.Mail;
using System.Text.Json;
using Microsoft.Extensions.Configuration;

class ExternalTools {
    public static bool SendEmail(string to, string subject, string body)
    {
    IConfigurationRoot configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json")
            .Build();

    string? smtpClient = configuration["SmtpClient"];
    string? smtpPort = configuration["SmtpPort"];
    string? smtpLogin = configuration["SmtpLogin"];
    string? smtpPassword = configuration["SmtpPassword"];
    string? emailFrom = configuration["EmailFrom"];
            
    Console.WriteLine("Sending email to {0} with subject {1} and body {2}",
        to, subject, body);

    MailMessage message = new MailMessage(emailFrom ?? "", to);
    message.Subject = subject;
    message.Body = body;
    message.IsBodyHtml=false;
    SmtpClient client = new SmtpClient(smtpClient, int.Parse(smtpPort ?? "0"));
    client.EnableSsl = false;
    client.Credentials = new System.Net.NetworkCredential(smtpLogin, smtpPassword);

    try
    {
        client.Send(message);
    }
    catch (Exception ex)
    {
        Console.WriteLine("Exception caught in CreateTestMessage2(): {0}",
            ex.ToString());
        return false;
    }
    return true;
    }

    public static string getFlightInfo(string originCity, string destinationCity)
    {
        if (originCity == "Seattle" && destinationCity == "Miami")
        {
            return JsonSerializer.Serialize(
                new Dictionary<string, string>
                {
                    { "airline", "Delta" },
                    { "flight_number", "DL123" },
                    { "flight_date", "May 7th, 2024" },
                    { "flight_time", "10:00AM" }
                }
            );
        }
        return JsonSerializer.Serialize(
            new Dictionary<string, string>
            {
                { "error", "No flights found between the cities" }
            }
        );
    }
}

