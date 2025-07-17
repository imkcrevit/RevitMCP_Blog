// See https://aka.ms/new-console-template for more information


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
public class RevitTool
{
    [McpServerTool(Name = "RevitTool"), Description("Revit Execute Command , Also can execute some string output")]
    public string RevitCommandTool(string command)
    {
        Console.WriteLine(command);
        //MessageBox.Show($"Revit Command: {command}", "Revit Tool Command", MessageBoxButtons.OK, MessageBoxIcon.Information);
        return command;
    }


    [McpServerTool(Name = "CreateWall"), Description("Generation Paramaters That Can Create Wall in Revit , If User Want To Generation eId , You Need To Generation a unique id base this :0B7FB9A8-DAD8-48CE-9D41-5EDB63832BD2 ")]
    public string RevitCreateWallTool(string command, double x, double y, double z, double x1, double y2, string uniqueId)
    {
        return $@"
                {{
                    ""command"": ""CreateWall"",
                    ""arguments"": {{
                        ""start"": [{x}, {y}, {z}],
                        ""end"": [{x1}, {y2}, {z}],
                        ""eId"": ""{uniqueId}""
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


    [McpServerTool(Name = "InsertWindowInWall"), Description("Generation A Window In A Selection Wall , Define Window Size : 1500 x 1200 d, Need To Calculate The Window-Top Is Small Then Wall-Height , This Command Need Input Args : ElementId , LocationX , LocationY ,LocationZ , The ElementId Need a String Type , The Best is GUID-type ")]
    public string InsertWindowInWallTool(string command, string eId, double x, double y, double z)
    {
        return $@"
                {{
                    ""command"": ""InsertWindowInWall"",
                    ""arguments"": {{
                        ""eId"" : ""{eId}"",
                        ""location"": [{x},{y},{z}]
                    }}
                }}";
    }
}


