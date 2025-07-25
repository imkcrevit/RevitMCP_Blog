using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Structure;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.UI;

namespace RevitTest
{
    public class Command : IExternalCommand
    {
        public class InsertWindowData
        {
            [JsonProperty(PropertyName = "eId")]
            public required int ElementId { get; set; }
            [JsonProperty(PropertyName = "location")]
            public required double[] Location { get; set; }
        }

        public class PointData
        {
            public required int X { get; set; }
            public required int Y { get; set; }
            public required int Z { get; set; }
        }

        public class CreateWallData
        {
            [JsonProperty(PropertyName = "command")]
            public string Command { get; set; } = string.Empty;
            [JsonProperty(PropertyName = "arguments")]
            public required object Args { get; set; }
        }

        public class CreateWallArguments
        {
            [JsonProperty(PropertyName = "start")]
            public required double[] Start { get; set; }
            [JsonProperty(PropertyName = "end")]
            public required double[] End { get; set; }
        }


        private class InsertWindowInWall : IRevitCommand
        {
            public void Execute(string jsonArgs, Document document)
            {
                

                using (Transaction trans = new Transaction(document , nameof(InsertWindowInWall)))
                {
                    var filter = new FilteredElementCollector(document);
                    var windowType = filter
                        .OfClass(typeof(FamilySymbol))
                        .OfCategory(BuiltInCategory.OST_Windows)
                        .Cast<FamilySymbol>()
                        .FirstOrDefault(x => x.Name == "1500x1200");

                    if (windowType == null)
                        throw new InvalidOperationException("Window type '1500x1200' not found");

                    var data = JsonConvert.DeserializeObject<InsertWindowData>(jsonArgs);

                    var locationPoint = new XYZ(data.Location[0] / 304.8, data.Location[1] / 304.8, data.Location[2] / 304.8);

                    var window = document.Create.NewFamilyInstance(
                        locationPoint,
                        windowType,
                        document.GetElement(new ElementId(data.ElementId)) as Wall,
                        document.ActiveView.GenLevel,
                        StructuralType.NonStructural);
                }
            }
        }

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
            public void Execute(string jsonArgs, Document document)
            {
                var args = JsonConvert.DeserializeObject<CreateWallArguments>(jsonArgs);
                var x = args.Start[0];
                var y = args.Start[1];
                var z = args.Start[2];
                var x1 = args.End[0];
                var y1 = args.End[1];
                var z1 = args.End[2];

                using (Transaction trans = new Transaction(document , nameof(CreateWall)))
                {
                    var start = new XYZ(x / 304.8, y / 304.8, z);
                    var end = new XYZ(x1 / 304.8, y1 / 304.8, z1);
                    var line = Line.CreateBound(start, end);
                    Wall.Create(document, line, document.ActiveView.GenLevel.Id, false);
                }
            }
        }

        private class ChangeWallWeight : IRevitCommand
        {
            public void Execute(string jsonArgs, Document document)
            {
                var weight = JsonConvert.DeserializeObject<int>(jsonArgs);

                var filter = new FilteredElementCollector(document);
                var walls = filter
                    .OfClass(typeof(Wall))
                    .WhereElementIsNotElementType()
                    .Cast<Wall>()
                    .ToList();

                var wallTypeFilter = new FilteredElementCollector(document);
                var targetWallType = wallTypeFilter
                    .OfClass(typeof(WallType))
                    .WhereElementIsElementType()
                    .FirstOrDefault(x => x.Name.Contains($"{weight}")) as WallType;
                using (Transaction trans = new Transaction(document , nameof(ChangeWallWeight)))
                {
                    walls.ForEach(w =>
                    {
                        w.WallType = targetWallType;
                    });
                }
                
            }
        }

        private class InsertWindowOrDoor : IRevitCommand
        {
            public void Execute(string jsonArgs , Document document)
            {
                throw new NotImplementedException();
            }
        }

        // 在共享库（如 ICommandPlugin.dll）中定义
        public interface IRevitCommand
        {
            void Execute(string jsonArgs, Document document);
        }

        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            var executeEventHandler = new FunctionUserCallWindow.ExecuteEventHandler("MCP");
            var externalEvent = ExternalEvent.Create(executeEventHandler);
            // show UI  
            var modelessView = new FunctionUserCallWindow(executeEventHandler, externalEvent);

            //窗口一直显示在主程序之前  
            System.Windows.Interop.WindowInteropHelper mainUI = new System.Windows.Interop.WindowInteropHelper(modelessView);
            mainUI.Owner = System.Diagnostics.Process.GetCurrentProcess().MainWindowHandle;
            modelessView.Show();


            return Result.Succeeded;
        }
    }


}
