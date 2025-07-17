// See https://aka.ms/new-console-template for more information


using Microsoft.Extensions.AI;
using ModelContextProtocol.Client;
using ModelContextProtocol.Protocol;
using OpenAI.Chat;
using OpenAI;
using System.ClientModel;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using ChatMessage = Microsoft.Extensions.AI.ChatMessage;
using Newtonsoft.Json;
using System.Windows.Forms;

var input = string.Join("",args);

//MessageBox.Show($"Input: {input}", "Input Command", MessageBoxButtons.OK, MessageBoxIcon.Information);
Debug.Print(input);
//var input = "在一个已经存在的id为333160的坐标为(0,0,0)到(10000,0,0)高度为3000 单位是mm的墙体 ， 插入一个窗户，窗户位置可以由你自行决定";
//var input = "创建一个墙体，墙体坐标为(0,0,0)->(10000,0,0)，单位是mm";
//var input =
//    "选中的墙体高度为3000 单位是mm的墙体 ， 插入一个窗户，窗户位置可以由你自行决定选中构件的数据为 ：WallId:333160 , WallData: Curve Data is : Start = X = 0, Y = 0, Z = 0 , End = X = 10000, Y = 0, Z = 0";
//"选中的墙体高度为3000 单位是mm的墙体 ， 插入一个窗户，窗户位置可以由你自行决定 , Curve Data is : Start = X = 0, Y = 0, Z = 0 , End = X = 10000, Y = 0, Z = 0";
await using var mcpClient = await McpClientFactory.CreateAsync(new StdioClientTransport(new StdioClientTransportOptions()
{
    Name = "Demo Server",
    Command = "powershell",
    Arguments = ["NET.Mcp.Server.exe"]
}));

var openAiOptions = new OpenAIClientOptions();
openAiOptions.Endpoint = new Uri("https://api.deepseek.com/v1/");


var chatClient = new ChatClient("deepseek-chat", new ApiKeyCredential("sk-xxxxxxxxxxxx"), openAiOptions);

var client = new ChatClientBuilder(chatClient.AsIChatClient()).UseFunctionInvocation().Build();

var prompts = new List<Microsoft.Extensions.AI.ChatMessage>
{
    new ChatMessage(ChatRole.System, """you are a professional enginer in BIM , so you can select the greate tool to user , also has a good develop tech in code , and generation a standard input style And Arguments to tools , also if some question need unique id you will generate a unique eId in this talk , the example :0B7FB9A8-DAD8-48CE-9D41-5EDB63832BD2"""),
    new ChatMessage(ChatRole.User, input)
};


var tools = await mcpClient.ListToolsAsync();

//foreach (var tool in tools)
//{
//    Console.WriteLine($"Tool Name: {tool.Name}");
//    Console.WriteLine($"Tool Description: {tool.Description}");
//    Console.WriteLine();

//}

var chatOptions = new ChatOptions()
{
    Tools = [.. tools]
};
var res = await client.GetResponseAsync(prompts, chatOptions);

var message = res.Messages[1].Contents[0];
var commandTools = from content in res.Messages
    where content.Role == ChatRole.Tool
    from toolContent in content.Contents
    select (toolContent as FunctionResultContent).Result;

var resultBuilder = new StringBuilder();
resultBuilder.AppendLine("[");
for (int i = 0; i < commandTools.Count(); i++)
{
    object commandTool = commandTools.ElementAt(i);
    // 反序列化
    ResponseData data = JsonConvert.DeserializeObject<ResponseData>(commandTool.ToString());

    // 访问数据
    foreach (var item in data.Content)
    {
        //Console.WriteLine($"Type: {item.Type}, Text: {item.Text}");
        var d = item.Text;

        resultBuilder.AppendLine(d);
    }
    if (i == commandTools.Count() - 1)
        continue;
    resultBuilder.AppendLine(",");
}

resultBuilder.AppendLine("]");

Console.WriteLine(resultBuilder);




public class CreateWallData
{
    [JsonProperty(PropertyName = "command")]
    public string Command { get; set; } = string.Empty;
    [JsonProperty(PropertyName = "arguments")]
    public CreateWallArguments Args { get; set; }
}

public class CreateWallArguments
{
    [JsonProperty(PropertyName = "start")]
    public double[] Start { get; set; }
    [JsonProperty(PropertyName = "end")]
    public double[] End { get; set; }
}

public class ContentItem
{
    public string Type { get; set; }
    public string Text { get; set; }
}

public class ResponseData
{
    public List<ContentItem> Content { get; set; }
    public bool IsError { get; set; }
}
