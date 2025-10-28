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

        public class CreateDataByAI
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
