using Microsoft.Extensions.Configuration;
using System.Net.Http.Json;
using System.Text.Json;

#region Setup and Bootstrap
IConfiguration configuration = new ConfigurationBuilder()
	.SetBasePath(Directory.GetCurrentDirectory())
	.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
	.AddEnvironmentVariables()
	.Build();

var settings = new AppSettings();
configuration.GetSection("Upstash").Bind(settings);
#endregion


#region Create client
using var http = new HttpClient();
http.BaseAddress = new Uri(settings.UpstashRestUrl);
http.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", settings.UpstashToken);
#endregion

// 1. Set a string value (use a GET request to set simple values)
await http.GetAsync("set/channel_name/deploy_and_destroy");

// 2. Get a string value (responses come in the form of an object)
var channelName = await http.GetFromJsonAsync<ResponseWrapper<string>>("get/channel_name");
Console.WriteLine(channelName?.Result);

// 3. Set a JSON value with 100s expiry
await http.PostAsJsonAsync("set/channel_parameters?EX=100", new ChannelMetadata { SubscriberCount = 33 });

// 4. Read the JSON value
var jsonResponse = await http.GetFromJsonAsync<ResponseWrapper<string>>("get/channel_parameters");
var data = jsonResponse?.Result;
if (!string.IsNullOrWhiteSpace(data))
{
	var res = JsonSerializer.Deserialize<ChannelMetadata>(data);
	Console.WriteLine(res?.SubscriberCount);
}

Console.ReadLine();