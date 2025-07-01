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

