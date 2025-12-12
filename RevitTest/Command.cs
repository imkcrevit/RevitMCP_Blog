using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Structure;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.UI;

namespace RevitTest
{
    #region  Update Revit Command Attributes In 25.10.27
    [Transaction(TransactionMode.Manual)]
    [Journaling(JournalingMode.UsingCommandData)]
    #endregion 
    public class Command : IExternalCommand
    {
        // null! This Code Complies Ignore The Parameter Value Is Never Null Warning
        public class InsertWindowData
        {
            [JsonProperty(PropertyName = "eId", Required = Required.Always)]
            public string EId { get; set; } = string.Empty;

            [JsonProperty(PropertyName = "location", Required = Required.Always)]
            public double[] Location { get; set; } = null!;

        }

        public class PointData
        {
            [JsonProperty(Required = Required.Always)]
            public int X { get; set; }
            [JsonProperty(Required = Required.Always)]
            public int Y { get; set; }
            [JsonProperty(Required = Required.Always)]
            public int Z { get; set; }
        }

        public class CreateDataByAI
        {
            [JsonProperty(PropertyName = "command")]
            public string Command { get; set; } = string.Empty;

            [JsonProperty(PropertyName = "arguments", Required = Required.Always)]
            public object Args { get; set; } = null!;
        }

        public class CreateWallArguments
        {
            [JsonProperty(PropertyName = "start", Required = Required.Always)]
            public double[] Start { get; set; } = null!;

            [JsonProperty(PropertyName = "end", Required = Required.Always)]
            public double[] End { get; set; } = null!;

            [JsonProperty(PropertyName = "eId", Required = Required.Default)]
            public string EId { get; set; } = string.Empty;
        }

        public class ChangeWallWeightArguments
        {
            [JsonProperty(PropertyName = "weight", Required = Required.Always)]
            public double Weight { get; set; }
        }

        public class InsertWindowInWallArguments
        {
            [JsonProperty(PropertyName = "windowName", Required = Required.Always)]
            public string WindowName { get; set; } = string.Empty;

            [JsonProperty(PropertyName = "width", Required = Required.Always)]
            public double Width { get; set; }

            [JsonProperty(PropertyName = "height", Required = Required.Always)]
            public double Height { get; set; }

            [JsonProperty(PropertyName = "position", Required = Required.Always)]
            public double[] Position { get; set; } = null!;
            [JsonProperty(PropertyName = "wallId", Required = Required.Always)]
            public string WallId { get; set; } = string.Empty;
        }

        public class CreateFloorArguments
        {
            [JsonProperty(PropertyName = "boundaryPoints", Required = Required.Always)]
            public double[][] BoundaryPoints { get; set; } = null!;

            [JsonProperty(PropertyName = "level", Required = Required.Always)]
            public string Level { get; set; } = string.Empty;
        }

        public class CreateDoorArguments
        {
            [JsonProperty(PropertyName = "wallId", Required = Required.Always)]
            public string WallId { get; set; } = string.Empty;

            [JsonProperty(PropertyName = "doorName", Required = Required.Always)]
            public string DoorName { get; set; } = string.Empty;

            [JsonProperty(PropertyName = "width", Required = Required.Always)]
            public double Width { get; set; }

            [JsonProperty(PropertyName = "height", Required = Required.Always)]
            public double Height { get; set; }

            [JsonProperty(PropertyName = "position", Required = Required.Always)]
            public double Position { get; set; }
        }

        public class CreateColumnArguments
        {
            [JsonProperty(PropertyName = "columnName", Required = Required.Always)]
            public string ColumnName { get; set; } = string.Empty;

            [JsonProperty(PropertyName = "position", Required = Required.Always)]
            public double[] Position { get; set; } = null!;

            [JsonProperty(PropertyName = "bottomLevel", Required = Required.Always)]
            public string BottomLevel { get; set; } = string.Empty;

            [JsonProperty(PropertyName = "topLevel", Required = Required.Always)]
            public string TopLevel { get; set; } = string.Empty;
        }

        public class CreateBeamArguments
        {
            [JsonProperty(PropertyName = "beamName", Required = Required.Always)]
            public string BeamName { get; set; } = string.Empty;

            [JsonProperty(PropertyName = "startPoint", Required = Required.Always)]
            public double[] StartPoint { get; set; } = null!;

            [JsonProperty(PropertyName = "endPoint", Required = Required.Always)]
            public double[] EndPoint { get; set; } = null!;
        }

        public class CreateRoomArguments
        {
            [JsonProperty(PropertyName = "level", Required = Required.Always)]
            public string Level { get; set; } = string.Empty;

            [JsonProperty(PropertyName = "position", Required = Required.Always)]
            public double[] Position { get; set; } = null!;
        }

        public class CopyElementArguments
        {
            [JsonProperty(PropertyName = "elementId", Required = Required.Always)]
            public string ElementId { get; set; } = string.Empty;

            [JsonProperty(PropertyName = "offset", Required = Required.Always)]
            public double[] Offset { get; set; } = null!;
        }

        public class MoveElementArguments
        {
            [JsonProperty(PropertyName = "elementId", Required = Required.Always)]
            public string ElementId { get; set; } = string.Empty;

            [JsonProperty(PropertyName = "targetPoint", Required = Required.Always)]
            public double[] TargetPoint { get; set; } = null!;
        }

        public class RotateElementArguments
        {
            [JsonProperty(PropertyName = "elementId", Required = Required.Always)]
            public string ElementId { get; set; } = string.Empty;

            [JsonProperty(PropertyName = "center", Required = Required.Always)]
            public double[] Center { get; set; } = null!;

            [JsonProperty(PropertyName = "axis", Required = Required.Always)]
            public double[] Axis { get; set; } = null!;

            [JsonProperty(PropertyName = "angle", Required = Required.Always)]
            public double Angle { get; set; }
        }

        public class DeleteElementArguments
        {
            [JsonProperty(PropertyName = "elementId", Required = Required.Always)]
            public string ElementId { get; set; } = string.Empty;
        }

        public class CreateStairArguments
        {
            [JsonProperty(PropertyName = "bottomLevel", Required = Required.Always)]
            public string BottomLevel { get; set; } = string.Empty;

            [JsonProperty(PropertyName = "topLevel", Required = Required.Always)]
            public string TopLevel { get; set; } = string.Empty;

            [JsonProperty(PropertyName = "risersCount", Required = Required.Always)]
            public int RisersCount { get; set; }

            [JsonProperty(PropertyName = "runWidth", Required = Required.Always)]
            public double RunWidth { get; set; }
        }

        private static FunctionUserCallWindow m_modelessView;
        private static ExternalEvent m_externalEvent;
        private static ExecuteEventHandler m_executeEventHandler;

        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            if (m_modelessView == null)
            {
                m_executeEventHandler = new ExecuteEventHandler("MCP");
                m_externalEvent = ExternalEvent.Create(m_executeEventHandler);
                m_modelessView = new FunctionUserCallWindow(m_executeEventHandler, m_externalEvent);


                m_modelessView.Show();
            }
            else
            {
                m_modelessView.Activate();
            }

            return Result.Succeeded;
        }
    }


}
