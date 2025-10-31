#10.30 Update
1. Change The Project Output Directory To Same With Other Projects
```xml
    <PropertyGroup>
        <BaseOutputPath>$(SolutionDir)bin\$(RevitVersion)\</BaseOutputPath>
    </PropertyGroup>
```
2. Change The Muti-Version C# Using In .csproj File
 ```
 	<PropertyGroup Condition="$(Configuration.Contains('2024'))">
		<RevitVersion>2024</RevitVersion>
		<TargetFramework>net48</TargetFramework>
		<LangVersion>10.0</LangVersion>
		<DefineConstants>$(DefineConstants);R2024</DefineConstants>
	</PropertyGroup>
 ```
 3. Beacuse This Porject Base On Revit 2020 Test , So If The Debug Change To < 2025 , The VisualStudio Will Push `“System.Runtime.CompilerServices.CompilerFeatureRequiredAttribute..ctor”` Error
 So Change The Code And Remove `require` keyword ，Commands Line 20 ~ 55
 ```
  public class InsertWindowData
        {
            [JsonProperty(PropertyName = "eId", Required = Required.Always)]
            public  int ElementId { get; set; }

            [JsonProperty(PropertyName = "location", Required = Required.Always)]
            public double[] Location { get; set; } = null!;
        }

        ....
 ```
# 10.28 Update

1. Change Visual Studio Project Debug/Relase Manage , Make 'RevitTest' To Identity To 'Debug'
2. Beacuse The Revit 2026 Refernce The 'NewsoftJson' Package Version 13.0.3 , So Add New Nuget Package 'Costura.Fody' 
3. ReWrite The 'RevitTest.csproj' File , Add The 'Costura.Fody' Package Reference
4. Wirite File 'RequestHandler' To Handle The Request From Revit
5. The Process Is Local Directory Path . Make This To Adaptive
6. Add New Funtion That Read API_KEY from file [Net.Mcp.Client]-> Program.cs Line 43
	```csharp
	var filepath = @"F:\DevProjects\imkcrevit\RevitMCP_Blog\api_key.env";
if (!File.Exists(filepath))
{
    throw new ArgumentNullException($"The Target File Path Not Found , Path Address : {filepath}");
}
var api_key = File.ReadAllText(filepath).Trim();
	```

7. Add The File Utility Function And Make The Process Working Directory Adaptive [Revit.Test.FunctionUserCallWindow]-> Program.cs Line 132-146
	```csharp
	var client_path = FileUtility.GetAssemblyPath();

                    var process = new Process
                    {
                        StartInfo = new ProcessStartInfo
                        {
                            FileName = @$"{client_path??"."}\NET.Mcp.Client.exe", // 可执行文件路径（如 "cmd.exe"）
                            Arguments = TextBox.Text, // 命令行参数
                            UseShellExecute = false, // 必须为 false 才能重定向输出
                            CreateNoWindow = true, // 隐藏控制台窗口
                            RedirectStandardOutput = true, // 重定向标准输出
                            RedirectStandardError = true, // 重定向错误输出（可选）
                            WorkingDirectory = client_path
                            
                        }
                    };

	```
