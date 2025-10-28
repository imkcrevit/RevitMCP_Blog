using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.DB;

namespace RevitTest
{
    // 在共享库（如 ICommandPlugin.dll）中定义
    public interface IRevitCommand
    {
        void Execute(string jsonArgs, Document document);
    }

}
