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
using Newtonsoft.Json.Linq;
using System.Windows.Forms;




var input = string.Join("",args);

//MessageBox.Show($"Input: {input}", "Input Command", MessageBoxButtons.OK, MessageBoxIcon.Information);
Debug.Print(input);
//var input = "在一个已经存在的id为333160的坐标为(0,0,0)到(10000,0,0)高度为3000 单位是mm的墙体 ， 插入一个窗户，窗户位置可以由你自行决定";
// input = "创建一个墙体，墙体坐标为(0,0,0)->(10000,0,0)，单位是mm";
//var input =
//    "选中的墙体高度为3000 单位是mm的墙体 ， 插入一个窗户，窗户位置可以由你自行决定选中构件的数据为 ：WallId:333160 , WallData: Curve Data is : Start = X = 0, Y = 0, Z = 0 , End = X = 10000, Y = 0, Z = 0";
//"选中的墙体高度为3000 单位是mm的墙体 ， 插入一个窗户，窗户位置可以由你自行决定 , Curve Data is : Start = X = 0, Y = 0, Z = 0 , End = X = 10000, Y = 0, Z = 0";
await using var mcpClient = await McpClientFactory.CreateAsync(new StdioClientTransport(new StdioClientTransportOptions()
{
    Name = "Demo Server",
    Command = "powershell",
    Arguments = [@".\NET.Mcp.Server.exe"]
}));

var openAiOptions = new OpenAIClientOptions();
openAiOptions.Endpoint = new Uri("https://api.deepseek.com/v1/");
// Update 2025.10.27
// Read Api Key From Local Machine User Secret Store
var filepath = @"F:\DevProjects\imkcrevit\RevitMCP_Blog\api_key.env";
if (!File.Exists(filepath))
{
    throw new ArgumentNullException($"The Target File Path Not Found , Path Address : {filepath}");
}
var api_key = File.ReadAllText(filepath).Trim();

// Input You LLM Token , DeepSeek , OpenAI etc
var chatClient = new ChatClient("deepseek-chat", new ApiKeyCredential(api_key), openAiOptions);

var client = new ChatClientBuilder(chatClient.AsIChatClient()).UseFunctionInvocation().Build();

var prompts = new List<Microsoft.Extensions.AI.ChatMessage>
{
    new ChatMessage(ChatRole.System, @"You are a professional BIM Engineer and Automation Specialist. You possess deep knowledge of Revit API logic and code development.

**Core Responsibilities:**
1. Select the most appropriate tools from the provided list to execute user requests.
2. Generate standardized, machine-readable JSON arguments for every tool call.
3. **CRITICAL:** You must generate a Unique Identifier (eId) for every single element created or modified. Format: GUID (e.g., 0B7FB9A8-DAD8-48CE-9D41-5EDB63832BD2).
4. Maintain a logical memory of the elements you create to handle dependencies (e.g., creating a Window requires the specific eId of the Wall it belongs to).

**Strict Geometric & Logic Rules (To prevent errors):**
- **NO AUTO-ALIGNMENT/SNAPPING:** Treat all user-provided coordinates (x, y, z) as ABSOLUTE and FINAL. Do not assume or apply 'auto-join', 'wall-cleanup', or 'nearest-point snapping' logic unless explicitly requested. The model must be built exactly at the coordinates given.
- **EXPLICIT HOSTING:** When creating hosted elements (Windows, Doors), you must explicitly identify the correct host element (Wall) based on the coordinate geometry and pass its `eId` into the arguments. Do not rely on automatic host detection.
- **SEQUENCE INTEGRITY:** Do not skip any steps. If a user asks to create an element and then move it, generate both the Creation command and the Move command in the correct order.

**Available Tools:**
- RevitTool: Execute Revit generic commands
- CreateWall: Create a wall (Args: start_point, end_point, thickness, level, disallow_join: bool)
- ChangeWallWeight: Change weight of walls
- InsertWindowInWall: Insert a window (Args: wall_eId, insertion_point, type)
- CreateFloor: Create a floor (Args: boundary_points, level)
- CreateDoor: Create a door (Args: wall_eId, insertion_point, type)
- CreateColumn: Create a column (Args: insertion_point, type, level)
- CreateBeam: Create a beam
- CreateRoom: Create a room (Args: boundary_eIds, level)
- CopyElement: Copy an element (Args: element_eId, source_point, destination_point)
- MoveElement: Move an element (Args: element_eId, source_point, destination_point)
- RotateElement: Rotate an element
- DeleteElement: Delete an element (Args: element_eId)
- CreateStair: Create a stair (Args: start_level, end_level, run_points, width, num_steps)

**Output Format:**
Return a valid JSON list of tool execution objects. No markdown outside the code block.
Example Structure:
[
  {
    ""tool"": ""ToolName"",
    ""eId"": ""GUID"",
    ""description"": ""Human readable explanation"",
    ""arguments"": { ... }
  }
]"),
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

// var message = res.Messages[1].Contents[0];
var commandTools = from content in res.Messages
    where content.Role == ChatRole.Tool
    from toolContent in content.Contents
    select (toolContent as FunctionResultContent).Result;

var outputs = new List<JObject>();
foreach (var tool in commandTools)
{
    var t = tool?.ToString();
    if (string.IsNullOrWhiteSpace(t)) continue;
    ResponseData data;
    try { data = JsonConvert.DeserializeObject<ResponseData>(t); }
    catch { data = null; }

    if (data?.Content != null)
    {
        foreach (var item in data.Content)
        {
            var s = item?.Text?.Trim();
            if (string.IsNullOrEmpty(s)) continue;
            JObject obj = TryParseOrNormalize(s);
            if (obj != null) outputs.Add(obj);
        }
    }
    else
    {
        var s = t.Trim();
        JObject obj = TryParseOrNormalize(s);
        if (obj != null) outputs.Add(obj);
    }
}

Console.WriteLine(JsonConvert.SerializeObject(outputs));

static JObject TryParseOrNormalize(string s)
{
    try { return JObject.Parse(s); }
    catch
    {
        var normalized = NormalizeBoundaryPoints(s);
        try { return JObject.Parse(normalized); }
        catch { return null; }
    }
}

static string NormalizeBoundaryPoints(string s)
{
    var idx = s.IndexOf("\"boundaryPoints\"", StringComparison.OrdinalIgnoreCase);
    if (idx < 0) return s;
    var levelIdx = s.IndexOf("\"level\"", StringComparison.OrdinalIgnoreCase);
    if (levelIdx < 0) return s;
    var segStart = s.IndexOf(':', idx);
    if (segStart < 0) return s;
    var commaBeforeLevel = s.LastIndexOf(',', levelIdx);
    if (commaBeforeLevel < 0) return s;
    var segment = s.Substring(segStart + 1, commaBeforeLevel - segStart - 1);
    var nums = new List<double>();
    var numBuilder = new StringBuilder();
    foreach (var ch in segment)
    {
        if (char.IsDigit(ch) || ch == '.' || ch == '-' ) numBuilder.Append(ch);
        else
        {
            if (numBuilder.Length > 0)
            {
                if (double.TryParse(numBuilder.ToString(), out var v)) nums.Add(v);
                numBuilder.Clear();
            }
        }
    }
    if (numBuilder.Length > 0)
    {
        if (double.TryParse(numBuilder.ToString(), out var v)) nums.Add(v);
        numBuilder.Clear();
    }
    var groups = new List<string>();
    for (int i = 0; i + 2 < nums.Count; i += 3)
    {
        groups.Add($"[{nums[i]}, {nums[i + 1]}, {nums[i + 2]}]");
    }
    var replacement = $": [{string.Join(", ", groups)}]";
    var builder = new StringBuilder();
    builder.Append(s.AsSpan(0, segStart));
    builder.Append(replacement);
    builder.Append(s.AsSpan(commaBeforeLevel));
    return builder.ToString();
}




// 通用命令数据结构，支持所有Revit操作命令
public class RevitCommandData
{
    [JsonProperty(PropertyName = "command")]
    public string Command { get; set; } = string.Empty;
    [JsonProperty(PropertyName = "arguments")]
    public Dictionary<string, object> Arguments { get; set; } = new Dictionary<string, object>();
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
