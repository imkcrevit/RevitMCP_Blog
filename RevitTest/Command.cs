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
            public  int ElementId { get; set; }

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
