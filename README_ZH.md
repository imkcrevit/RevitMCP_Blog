当前，各类AI相关资讯充斥着网络。由于近期开发工作涉及AI技术，因此撰写本文介绍MCP与Revit的联动方式，实现公众号中的"一句话建模"功能。
这只是其中一种实现方案，还存在其他技术路径。不过，这种方法能够快速落地，有助于大家迅速探索相关应用方向。

## Function Call
在讲MCP之前需要先讲一下`Function Call` , 这是一个LLM大模型API提供的方式，通过写入`Function Call `在LLM交互的时候可以自动选用相应的`Function`执行。
![[6ibaby6qg4ku4_bc12e0bccbf148f1a5cfd5462d2c0c66.gif]]
[**图片来源：ailydoseofds **](https://developer.aliyun.com/article/1665090)
以DeepSeek为例，在API中就介绍了`Function Call`的使用方法

``` python
tools = [  
	{  
		"type": "function",  
		"function": {  
			"name": "get_weather",  
			"description": "Get weather of an location, the user shoud supply a location first",  
			"parameters": {  
				"type": "object",  
				"properties": {  
					"location": {  
						"type": "string",  
						"description": "The city and state, e.g. San Francisco, CA",  
					}  
			},  
			"required": ["location"]  
			},  
		}  
	},  
]
```
相应的逻辑是LLM再回答的时候会
1. 通过描述选择相应的`funciton`并生成符合要求的参数
2. 执行
3. 获取结果后返回一个结果
4. 总结语言返回到客户界面

## MCP
### MCP介绍
MCP（Model Context Protocol，模型上下文协议）是Function Calling技术的扩展协议。该协议的核心价值在于：通过标准化建模，使开发者能够编写一次Function Call即可适配多个主流大语言模型（如DeepSeek、ChatGPT等）。其工作原理是生成符合多平台规范的调用指令，由LLM自主选择执行路径，实现跨模型的函数调用兼容性。
![[6ibaby6qg4ku4_90f44895b25a4caf827d7b2fa49157ee.gif]]
 [**图片来源：ailydoseofds ** ](https://developer.aliyun.com/article/1665090)
### MCP架构
MCP的架构可以分为：
- 主机
- 客户端
- 服务器
#### MCP Server 
> [# 一文了解：MCP 传输机制 Stdio、SSE 与 Streamable HTTP 的核心区别](https://zhuanlan.zhihu.com/p/1896209461112197767)
- Stdio ： 这种是目前应用比较多的方式，通过标准输入/输出的方式进行数据传递 ， 如果是在Revit的应用场景下，可以选择这种方式或是通过实现`IMcpTransport`重新定义一个Revit专用的样式
- Stream ： 流式传递，完全基于标准的HTTP协议，在HTTP的应用中较多，
- SSE ：（[Server-Sent Events](https://zhida.zhihu.com/search?content_id=256527879&content_type=Article&match_order=1&q=Server-Sent+Events&zhida_source=entity)）通过HTTP长连接实现远程通信。

### MCP流程
![[6ibaby6qg4ku4_b0cfdc19ffb7456685a72fc619c8823f-3.gif]]
 [**图片来源：ailydoseofds** ](https://developer.aliyun.com/article/1665090)
## 提示词
### 介绍
在基于API的MCP实现中，提示词工程（Prompt Engineering）具有决定性作用，其核心价值体现在：
#### **1. 工具选择准确性**
通过动态调整对话上下文（Conversational Bias），引导大模型在每轮交互中选择最合适的工具。
#### **2. 概率分布调控**
使用约束性提示词（Constrained Prompts）调整模型输出的概率分布，使回答更符合目标需求。
#### **3. API 场景下的提示词工程**
在 MCP 的 API 调用中，提示词工程至关重要，它直接影响工具（Tools）的选择准确性。每轮对话都会影响模型的决策倾向，而优化提示词可以引导模型输出更符合预期的结果。
#### **4. 示例：BIM 专家系统**
- **角色设定**：在 `system` 中定义为 **"BIM 专家"**，确保模型能精准选择设计工具。
- **额外约束**：为了更流畅的交互，需进一步优化提示词，如限制工具范围或调整匹配逻辑。
#### **5. 工具匹配机制**
LLM 根据 **Tool Description** 进行选择，并通过回复内容动态匹配最合适的工具，确保每次交互都精准高效
## .Net MCP框架
由于Revit本身对于C#的支持比较丰富，所以这里选择了MCP的Net框架：[modelcontextprotocol](https://github.com/modelcontextprotocol)

### .Net MCP 框架介绍
这是一个支持.Net的MCP开源框架，我们可以直接在github下载源码学习。在这个框架中采用的传统I/O传输，并通过控制台输出。
作为使用方我们需要了解下面几个接口即可：
1. Tools
	-  通过`[McpServerToolType]`定义Tools类
	-  通过`[McpServerTool]`定义具体的方法
	-  下面这个方法，通过可以调用LLM输出一个字符串
		```csharp
		[McpServerToolType]
		public  class RevitTool
		{
		    [McpServerTool(Name = "RevitTool"), Description("Revit Execute Command , Also can execute some string output")]
		    public string RevitCommandTool(string command)
		    {
		        Console.WriteLine(command);
		        //MessageBox.Show($"Revit Command: {command}", "Revit Tool Command", MessageBoxButtons.OK, MessageBoxIcon.Information);
		        return command;s
		    }
		```
2. Server
	- 框架提供了依赖注入与普通声明的方式
	-  将`Tools`注入并且发布
	- 可以简单理解为将自己的`Tools`全部发布，并且不用植入到`Function Call`中，只需要等待LLM自动读取里面的描述进行自动调用即可
		```csharp
		var builder = Host.CreateApplicationBuilder();
		builder.Services.AddMcpServer()
		    .WithStdioServerTransport()
		    .WithTools<RevitTool>();
		
		await builder.Build().RunAsync();
		```
3. Client
	-   通过路径唤醒服务
 ```csharp
		 await using var mcpClient = await McpClientFactory.CreateAsync(new StdioClientTransport(new StdioClientTransportOptions()
		{
		    Name = "Demo Server",
		    Command = "powershell",
		    Arguments = ["D:\\NET.Mcp.Server.exe"]
		}));
 ```

-  实现LLM对话并将Server中的Tools数据传入到LLM中，我这里使用的是DeepSeek的API，通过对API的调用实现LLM对话
```csharp
var openAiOptions = new OpenAIClientOptions();
openAiOptions.Endpoint = new Uri("https://api.deepseek.com/v1/");

var chatClient = new ChatClient("deepseek-chat", new ApiKeyCredential("******"), openAiOptions);

var client = new ChatClientBuilder(chatClient.AsIChatClient()).UseFunctionInvocation().Build();

var prompts = new List<Microsoft.Extensions.AI.ChatMessage>
{
    new ChatMessage(ChatRole.System, """you are a professional enginer in BIM , so you can select the greate tool to user , and generation a standard input style And Arguments to tools"""),
    new ChatMessage(ChatRole.User, input)
};

var tools = await mcpClient.ListToolsAsync();

var chatOptions = new ChatOptions()
{
    Tools = [.. tools]
};
```
- 获取数据
```csharp
var res = await client.GetResponseAsync(prompts, chatOptions);

var message = res.Messages[1].Contents[0];
var value = ((Microsoft.Extensions.AI.FunctionResultContent)message).Result;
var convert = JsonConvert.DeserializeObject(value.ToString());

// 反序列化
ResponseData data = JsonConvert.DeserializeObject<ResponseData>(value.ToString());

// 访问数据
foreach (var item in data.Content)
{
    //Console.WriteLine($"Type: {item.Type}, Text: {item.Text}");
    var d = item.Text;
    
    Console.WriteLine(d);
}
```

## 与Revit绑定
### 实现效果
[[Revit & MCP]]
### 介绍
在Revit中使用MCP或FunctionCall时，结合代码示例可以看出，LLM会根据输入自动选择相应工具运行，从而实现AI建模或审查操作。
因此我认为，这种方式更适合已具备插件集开发能力或拥有成熟设计流程的团队集成。而对于开发资源较少的团队来说，使用MCP的时间效率可能低于直接点击操作或编写固定逻辑的插件。
### 绑定
1. 前面介绍了，stdio使用的是标准的输入输出，但是这与Revit本身的机制冲突，导致直接在Revit内唤醒server获取数据会导致Revit卡顿，整个进程会被堵塞，所以在我的案例中，我是通过process单独唤醒了一个控制台从而获取到控制台输入在连接Revit。
		- 有问题的地方应该是在`Transprot`位置使用了：`TextReader/TextWriter` ， 在源码中的位置是`ModelContextProtocol.Protocol.Transport\StreamClientSessionTransport`
	![[Snipaste_2025-06-12_14-02-24.png]]
2.  第二种则是可以通过idling连接mcp服务 ，通过闲时事件 + WCF也可以实现MCP自动运行的逻辑，有兴趣的可以自己实现一下。但是需要对idling event中的线程进行约束，否则也会导致一直在读取api的反馈从而导致Revit卡死的情况
> [# Revit中实现WCF客户端部署](https://blog.csdn.net/qq_41059339/article/details/109577195)
> [https://thebuildingcoder.typepad.com/blog/2012/11/drive-revit-through-a-wcf-service.html](https://thebuildingcoder.typepad.com/blog/2012/11/drive-revit-through-a-wcf-service.html)
3. 则是直接修改源码实现`TransportBase`/ `ITransport` ， 单独为客户端系列完成请求
4. 最后一种则是更加直接的方式，在我看来也比较有效，因为客户端如果不商用面对的LLM则为单一的厂家，比如：` ChatGPT/DeepSeek`所以直接通过`Function Call `也可以，这样也不会出现线程堵塞等情况可以直接调用。

### Revit

如果已经实现了Server与Client ， Revit这一步需要做的更改就是增加新的输入参数获取，由于AI的输入输出全部为文本输入，所以如果通过LLM进行自动运行，则需要将方法这里统一参数输入口。
所以我在这个项目中使用了json格式作为转换格式，通过对参数的转换实现方法运行。这种也是对于以后方法最小的修改方式。

#### 准备工作
##### 标准的输出格式
1. 上面说到是通过json格式作为输入输出，那么在tools中做了一下的更改，这样能够将希望得到的方法名和参数一起输出。以创建墙体为例：得到定位线的起点和终点，如果我们的参数比较复杂，则需要在描述这里就把内容标记出来防止出现错误数据或者参数
Description示例：
```general
"Generation A Window In A Selection Wall , Define Window Size : 1500 x 1200 d, Need To Calculate The Window-Top Is Small Then Wall-Height , This Command Need Input Args : ElementId , LocationX , LocationY ,LocationZ")
```
```
Tool示例：
```csharp
[McpServerTool(Name = "CreateWall"), Description("Generation Paramaters That Can Create Wall in Revit")]
public string RevitCreateWallTool(string command, double x, double y, double z, double x1, double y2)
{
    return $@"
            {{
                ""command"": ""CreateWall"",
                ""arguments"": {{
                    ""start"": [{x}, {y}, {z}],
                    ""end"": [{x1}, {y2}, {z}]
                }}
            }}"; 
}
```
2. 在revit中创建数据结构接受数据并转换成我们能够使用的数据
```csharp
 public class CreateWallData
 {
     [JsonProperty(PropertyName = "command")]
     public string Command { get; set; } = string.Empty;
     [JsonProperty(PropertyName = "arguments")]
     public object Args { get; set; }
 }
```
3. 创建一个接口，能够规范化我们的插件命令，我这里简单创建了一个希望所有的入口都接受json字符，数据将会在方法内部单独解析
```csharp
public interface IRevitCommand
{
    void Execute(string jsonArgs);
}
```
3. 对于执行函数，实现接口，将原有的坐标点修改为json字符串
``` csharp
 /// <summary>
 /// This A MCP Test
 /// </summary>
 /// <param name="x"></param>
 /// <param name="y"></param>
 /// <param name="x1"></param>
 /// <param name="y1"></param>
 /// <param name="z"></param>
 private class CreateWall : IRevitCommand
 {
     public void Execute(string jsonArgs)
     {
         var args = JsonConvert.DeserializeObject<CreateWallArguments>(jsonArgs);
         var x = args.Start[0];
         var y = args.Start[1];
         var z = args.Start[2];
         var x1 = args.End[0];
         var y1 = args.End[1];
         var z1 = args.End[2];

         TransactionUtils.Execute(RevitCommandData.Document, (nx) =>
         {
             var start = new XYZ(x/304.8, y/304.8, z);
             var end = new XYZ(x1/304.8, y1/304.8, z1);
             var line = Line.CreateBound(start, end);
             Wall.Create(RevitCommandData.Document, line ,RevitCommandData.ActiveView.GenLevel.Id, false);
         }, "CreateWall");

     }
 }
```
5. 则是通过反射可以将我们实现`IRevitCommand`的接口方法找到并新建方法进行执行
```csharp
// 1. 加载DLL
Assembly assembly = typeof(Command).Assembly;
// 2. 查找实现类（通过接口或命名约定）
Type commandType = assembly.GetTypes()
    .FirstOrDefault(t => t.Name == methodName);

if (commandType == null)
    throw new Exception($"未找到 {methodName} 的实现类");

var eCommand = (IRevitCommand)Activator.CreateInstance(commandType);
eCommand.Execute(JsonConvert.SerializeObject(jsonConvertData.Args));
```
6.  如果需要和Revit交互的话只需要在arguments中增加参数即可完成Revit-LLM的交互，相较于上面增加一部分代码(选择强并将墙的ID与数据传入LLM作为插入门窗功能的参照)
```csharp
var args = string.Empty;
if (selection.IntegerValue == -1)
{
    args = string.Empty;
}
else
{

    var ele = RevitCommandData.Document.GetElement(selection) as Wall;

    var wallLocation = ele.Location as LocationCurve;
    var wallString = ConvertToString(wallLocation.Curve);
    args = $"WallId:{selection} , WallData: {wallString}";
}
```
7.  通过process调用client并读取输出
```csharp
 var process = new Process
 {
     StartInfo = new ProcessStartInfo
     {
         FileName = @"D:\NET.Mcp.Client.exe",          // 可执行文件路径（如 "cmd.exe"）
         Arguments = this.TextBox.Text + $"选中构件的数据为 ：{args}",       // 命令行参数
         UseShellExecute = false,     // 必须为 false 才能重定向输出
         CreateNoWindow = true,       // 隐藏控制台窗口
         RedirectStandardOutput = true, // 重定向标准输出
         RedirectStandardError = true  // 重定向错误输出（可选）
     }
 };

 process.Start();

 // 读取所有输出（同步方式）
 string output = process.StandardOutput.ReadToEnd();
 string errors = process.StandardError.ReadToEnd(); // 如果需要错误流

 process.WaitForExit(); // 等待进程结束
 process.Close(); // 关闭进程
```

#### 实际运行

##### 运行内容
这个项目，我会调用LLM创建一个墙体，并且选择墙体后继续调用LLM帮我在任意位置插入一个窗体，从而实现多轮对话与Revit-LLM的双向交互。

##### MCP-Server
```csharp
using System.ComponentModel;
using System.Windows.Forms;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ModelContextProtocol.Server;

var builder = Host.CreateApplicationBuilder();
builder.Services.AddMcpServer()
    .WithStdioServerTransport()
    .WithTools<RevitTool>();

await builder.Build().RunAsync();


[McpServerToolType]
public  class RevitTool
{
    [McpServerTool(Name = "RevitTool"), Description("Revit Execute Command , Also can execute some string output")]
    public string RevitCommandTool(string command)
    {
        Console.WriteLine(command);
        //MessageBox.Show($"Revit Command: {command}", "Revit Tool Command", MessageBoxButtons.OK, MessageBoxIcon.Information);
        return command;
    }


    [McpServerTool(Name = "CreateWall"), Description("Generation Paramaters That Can Create Wall in Revit")]
    public string RevitCreateWallTool(string command, double x, double y, double z, double x1, double y2)
    {
        return $@"
                {{
                    ""command"": ""CreateWall"",
                    ""arguments"": {{
                        ""start"": [{x}, {y}, {z}],
                        ""end"": [{x1}, {y2}, {z}]
                    }}
                }}"; 
    }


    [McpServerTool(Name = "ChangeWallWeight"), Description("Change All Wall's Weight")]
    public string ChangeWallWeightTool(string command, double weight)
    {
        return $@"
                {{
                    ""command"": ""ChangeWallWeight"",
                    ""arguments"": {{
                        ""weight"": {weight}
                    }}
                }}";
    }


    [McpServerTool(Name = "InsertWindowInWall"), Description("Generation A Window In A Selection Wall , Define Window Size : 1500 x 1200 d, Need To Calculate The Window-Top Is Small Then Wall-Height , This Command Need Input Args : ElementId , LocationX , LocationY ,LocationZ")]
    public string InsertWindowInWallTool(string command , int eId , double x , double y , double z)
    {
        return $@"
                {{
                    ""command"": ""InsertWindowInWall"",
                    ""arguments"": {{
                        ""eId"" : {eId} ,
                        ""location"": [{x},{y},{z}]
                    }}
                }}"; 
    }
}
```

##### MCP-Client
```csharp
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
    Arguments = ["D:\\NET.Mcp.Server.exe"]
}));

var openAiOptions = new OpenAIClientOptions();
openAiOptions.Endpoint = new Uri("https://api.deepseek.com/v1/");

var chatClient = new ChatClient("deepseek-chat", new ApiKeyCredential("sk-0000000000000"), openAiOptions);

var client = new ChatClientBuilder(chatClient.AsIChatClient()).UseFunctionInvocation().Build();

var prompts = new List<Microsoft.Extensions.AI.ChatMessage>
{
    new ChatMessage(ChatRole.System, """you are a professional enginer in BIM , so you can select the greate tool to user , and generation a standard input style And Arguments to tools"""),
    new ChatMessage(ChatRole.User, input)
};


var tools = await mcpClient.ListToolsAsync();

var chatOptions = new ChatOptions()
{
    Tools = [.. tools]
};
var res = await client.GetResponseAsync(prompts, chatOptions);

var message = res.Messages[1].Contents[0];
var value = ((Microsoft.Extensions.AI.FunctionResultContent)message).Result;
var convert = JsonConvert.DeserializeObject(value.ToString());

// 反序列化
ResponseData data = JsonConvert.DeserializeObject<ResponseData>(value.ToString());

// 访问数据
foreach (var item in data.Content)
{
    //Console.WriteLine($"Type: {item.Type}, Text: {item.Text}");
    var d = item.Text;
    
    Console.WriteLine(d);
}

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

```

##### Revit

```csharp

                    var selections = RevitCommandData.UiDocument.Selection.GetElementIds();
                    var selection = ElementId.InvalidElementId;
                    if (selections.Any())
                    {
                        selection = selections.First();
                    }
                    var args = string.Empty;
                    if (selection.IntegerValue == -1)
                    {
                        args = string.Empty;
                    }
                    else
                    {
                        var ele = RevitCommandData.Document.GetElement(selection) as Wall;
                        var wallLocation = ele.Location as LocationCurve;
                        var wallString = ConvertToString(wallLocation.Curve);
                        args = $"WallId:{selection} , WallData: {wallString}";
                    }

                    var process = new Process
                    {
                        StartInfo = new ProcessStartInfo
                        {
                            FileName = @"D:\NET.Mcp.Client.exe",          // 可执行文件路径（如 "cmd.exe"）
                            Arguments = this.TextBox.Text + $"选中构件的数据为 ：{args}",       // 命令行参数
                            UseShellExecute = false,     // 必须为 false 才能重定向输出
                            CreateNoWindow = true,       // 隐藏控制台窗口
                            RedirectStandardOutput = true, // 重定向标准输出
                            RedirectStandardError = true  // 重定向错误输出（可选）
                        }
                    };

                    process.Start();

                    // 读取所有输出（同步方式）
                    string output = process.StandardOutput.ReadToEnd();
                    string errors = process.StandardError.ReadToEnd(); // 如果需要错误流

                    process.WaitForExit(); // 等待进程结束
                    process.Close(); // 关闭进程

                    if (string.IsNullOrEmpty(errors))
                    {
                        var jsonConvertData = JsonConvert.DeserializeObject<CreateWallData>(output);
                        var methodName = jsonConvertData.Command;
                        // 1. 加载DLL
                        Assembly assembly = typeof(Command).Assembly;
                        // 2. 查找实现类（通过接口或命名约定）
                        Type commandType = assembly.GetTypes()
                            .FirstOrDefault(t => t.Name == methodName);

                        if (commandType == null)
                            throw new Exception($"未找到 {methodName} 的实现类");

                        var eCommand = (IRevitCommand)Activator.CreateInstance(commandType);
                        eCommand.Execute(JsonConvert.SerializeObject(jsonConvertData.Args));

                    }
```
### 总结

从实际案例可以看出，AI在建筑行业的技术应用路径已较为清晰，并非如某些文章或视频渲染的那般神秘夸张。作为使用者，我们更应关注AI在具体场景中的提效能力，例如规范审查、批量建模、图纸处理等实际应用。

撰写本文的初衷，正是希望通过一个简单案例帮助读者建立对AI技术的基本认知。当前公众号和网络文章鱼龙混杂，容易造成误解。通过实际演示案例，可以让从业者更准确地评估AI的应用场景和深度，从而有效辨别和过滤无效信息。