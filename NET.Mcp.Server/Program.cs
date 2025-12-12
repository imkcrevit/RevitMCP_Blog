// See https://aka.ms/new-console-template for more information

using System.ComponentModel;
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
    [McpServerTool(Name = "RevitTool"), Description("Revit Execute Command, Also can execute some string output")]
    public string RevitCommandTool(string command)
    {
        return command;
    }

    [McpServerTool(Name = "CreateWall"), Description("Generation Paramaters That Can Create Wall in Revit, If User Want To Generation eId, You Need To Generation a unique id base this :0B7FB9A8-DAD8-48CE-9D41-5EDB63832BD2")]
    public string RevitCreateWallTool(string command, double x, double y, double z, double x1, double y2, string uniqueId)
    {
        return string.Format("{{\"command\": \"CreateWall\", \"arguments\": {{\"start\": [{0}, {1}, {2}], \"end\": [{3}, {4}, {5}], \"eId\": \"{6}\"}}}}", x, y, z, x1, y2, z, uniqueId);
    }

    [McpServerTool(Name = "ChangeWallWeight"), Description("Change All Wall's Weight")]
    public string ChangeWallWeightTool(string command, double weight)
    {
        return string.Format("{{\"command\": \"ChangeWallWeight\", \"arguments\": {{\"weight\": {0}}}}}", weight);
    }

    [McpServerTool(Name = "InsertWindowInWall"), Description("Insert Window In Wall, Need Window Family Name, Width, Height, Position X, Y, Z, wallId")]
    public string InsertWindowInWallTool(string command, string windowName, double width, double height, double x, double y, double z , string wallId)
    {
        return string.Format("{{\"command\": \"InsertWindowInWall\", \"arguments\": {{\"windowName\": \"{0}\", \"width\": {1}, \"height\": {2}, \"position\": [{3}, {4}, {5}], \"wallId\": \"{6}\"}}}}", windowName, width, height, x, y, z, wallId);
    }

    [McpServerTool(Name = "CreateFloor"), Description("Generation Parameters That Can Create Floor in Revit, Need Input Args: List of Points (x,y,z) for Floor Boundary")]
    public string RevitCreateFloorTool(string command, string boundaryPoints, string level)
    {
        return string.Format("{{\"command\": \"CreateFloor\", \"arguments\": {{\"boundaryPoints\": {0}, \"level\": \"{1}\"}}}}", boundaryPoints, level);
    }

    [McpServerTool(Name = "CreateDoor"), Description("Generation Parameters That Can Create Door in Revit, Need Input Args: WallId, Door Family Name, Width, Height and Position")]
    public string RevitCreateDoorTool(string command, string wallId, string doorName, double width, double height, double position)
    {
        return string.Format("{{\"command\": \"CreateDoor\", \"arguments\": {{\"wallId\": \"{0}\", \"doorName\": \"{1}\", \"width\": {2}, \"height\": {3}, \"position\": {4}}}}}", wallId, doorName, width, height, position);
    }

    [McpServerTool(Name = "CreateColumn"), Description("Generation Parameters That Can Create Column in Revit, Need Input Args: Column Family Name, Position (x,y,z), Bottom Level and Top Level")]
    public string RevitCreateColumnTool(string command, string columnName, double x, double y, double z, string bottomLevel, string topLevel)
    {
        return string.Format("{{\"command\": \"CreateColumn\", \"arguments\": {{\"columnName\": \"{0}\", \"position\": [{1}, {2}, {3}], \"bottomLevel\": \"{4}\", \"topLevel\": \"{5}\"}}}}", columnName, x, y, z, bottomLevel, topLevel);
    }

    [McpServerTool(Name = "CreateBeam"), Description("Generation Parameters That Can Create Beam in Revit, Need Input Args: Beam Family Name, Start Point (x,y,z) and End Point (x,y,z)")]
    public string RevitCreateBeamTool(string command, string beamName, double startX, double startY, double startZ, double endX, double endY, double endZ)
    {
        return string.Format("{{\"command\": \"CreateBeam\", \"arguments\": {{\"beamName\": \"{0}\", \"startPoint\": [{1}, {2}, {3}], \"endPoint\": [{4}, {5}, {6}]}}}}", beamName, startX, startY, startZ, endX, endY, endZ);
    }

    [McpServerTool(Name = "CreateRoom"), Description("Generation Parameters That Can Create Room in Revit, Need Input Args: Boundary Level and Position (x,y,z)")]
    public string RevitCreateRoomTool(string command, string level, double x, double y, double z)
    {
        return string.Format("{{\"command\": \"CreateRoom\", \"arguments\": {{\"level\": \"{0}\", \"position\": [{1}, {2}, {3}]}}}}", level, x, y, z);
    }

    [McpServerTool(Name = "CopyElement"), Description("Generation Parameters That Can Copy Element in Revit, Need Input Args: ElementId and Offset (dx,dy,dz)")]
    public string RevitCopyElementTool(string command, string elementId, double dx, double dy, double dz)
    {
        return string.Format("{{\"command\": \"CopyElement\", \"arguments\": {{\"elementId\": \"{0}\", \"offset\": [{1}, {2}, {3}]}}}}", elementId, dx, dy, dz);
    }

    [McpServerTool(Name = "MoveElement"), Description("Generation Parameters That Can Move Element in Revit, Need Input Args: ElementId and Target Point (x,y,z)")]
    public string RevitMoveElementTool(string command, string elementId, double targetX, double targetY, double targetZ)
    {
        return string.Format("{{\"command\": \"MoveElement\", \"arguments\": {{\"elementId\": \"{0}\", \"targetPoint\": [{1}, {2}, {3}]}}}}", elementId, targetX, targetY, targetZ);
    }

    [McpServerTool(Name = "RotateElement"), Description("Generation Parameters That Can Rotate Element in Revit, Need Input Args: ElementId, Rotation Center (x,y,z), Axis (x,y,z) and Angle (degrees)")]
    public string RevitRotateElementTool(string command, string elementId, double centerX, double centerY, double centerZ, double axisX, double axisY, double axisZ, double angle)
    {
        return string.Format("{{\"command\": \"RotateElement\", \"arguments\": {{\"elementId\": \"{0}\", \"center\": [{1}, {2}, {3}], \"axis\": [{4}, {5}, {6}], \"angle\": {7}}}}}", elementId, centerX, centerY, centerZ, axisX, axisY, axisZ, angle);
    }

    [McpServerTool(Name = "DeleteElement"), Description("Generation Parameters That Can Delete Element in Revit, Need Input Args: ElementId")]
    public string RevitDeleteElementTool(string command, string elementId)
    {
        return string.Format("{{\"command\": \"DeleteElement\", \"arguments\": {{\"elementId\": \"{0}\"}}}}", elementId);
    }

    [McpServerTool(Name = "CreateStair"), Description("Generation Parameters That Can Create Stair in Revit, Need Input Args: Bottom Level, Top Level, Number of Risers and Run Width")]
    public string RevitCreateStairTool(string command, string bottomLevel, string topLevel, int risersCount, double runWidth)
    {
        return string.Format("{{\"command\": \"CreateStair\", \"arguments\": {{\"bottomLevel\": \"{0}\", \"topLevel\": \"{1}\", \"risersCount\": {2}, \"runWidth\": {3}}}}}", bottomLevel, topLevel, risersCount, runWidth);
    }
}


