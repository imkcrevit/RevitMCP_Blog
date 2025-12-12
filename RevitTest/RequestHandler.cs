using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RevitTest
{
    public class ExecuteEventHandler : IExternalEventHandler
    {
        public string Name { get; private set; }

        public Action<UIApplication>? ExecuteAction { get; set; }

        public ExecuteEventHandler(string name)
        {
            Name = name;
        }

        public void Execute(UIApplication app)
        {

            if (ExecuteAction != null)
            {
                try
                {
                    
                    ExecuteAction(app);
                }
                catch (Exception e)
                {
                    var msg = e.Message ?? string.Empty;
                    var hintNeeded = msg.IndexOf("Invalid JavaScript property identifier character", StringComparison.OrdinalIgnoreCase) >= 0
                                     && msg.IndexOf("boundaryPoints", StringComparison.OrdinalIgnoreCase) >= 0;
                    var content = hintNeeded
                        ? $"JSON格式错误：'boundaryPoints' 应为 [[x,y,z],[x,y,z],...] 的数组。请不要使用圆括号或键名形式，仅使用数值数组。\n详情：{msg}"
                        : msg;
                    var dialog = new TaskDialog("R2026");
                    dialog.MainContent = content;
                    dialog.Show();
                }
            }
        }

        public string GetName()
        {
            return Name;
        }
    }

}
