# 10.28 Update
1. Add The Read API_KEY function_
```
var filepath = @"F:\DevProjects\imkcrevit\RevitMCP_Blog\api_key.env";
if (!File.Exists(filepath))
{
    throw new ArgumentNullException($"The Target File Path Not Found , Path Address : {filepath}");
}
var api_key = File.ReadAllText(filepath).Trim();

```
2. Change The Solution Config Porject And Make THe Output Folder Was Same With Other Projects
```


	<PropertyGroup>
		<BaseOutputPath>$(SolutionDir)bin\$(RevitVersion)\</BaseOutputPath>
	</PropertyGroup>

```
3. Add New Package : Costura.Fody , Beasuce Revit 2026 Was Reference The Newtonsoft.Json Version , So We Need To Merge All Dependent DLLs Into One Single DLL
