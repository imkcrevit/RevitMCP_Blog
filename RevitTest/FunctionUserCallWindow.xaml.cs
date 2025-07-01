using Autodesk.Revit.UI;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Autodesk.Revit.DB;


namespace RevitTest
{
    /// <summary>
    /// FunctionUserCallWindow.xaml 的交互逻辑
    /// </summary>
    public partial class FunctionUserCallWindow : Window
    {
        ExecuteEventHandler _executeEventHandler = null;
        ExternalEvent _externalEvent = null;
        public FunctionUserCallWindow(ExecuteEventHandler executeEventHandler, ExternalEvent externalEvent)
        {
            InitializeComponent();
            _executeEventHandler = executeEventHandler;
            _externalEvent = externalEvent;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {

            if (_externalEvent != null)
            {
                _executeEventHandler.ExecuteAction = new Action<UIApplication>((app) =>
                {
                    if (app.ActiveUIDocument == null || app.ActiveUIDocument.Document == null)
                        return;

                    var uiDoc = app.ActiveUIDocument;
                    var selections = uiDoc.Selection.GetElementIds();
                    var selection = ElementId.InvalidElementId;
                    if (selections.Any())
                    {
                        selection = selections.First();
                    }
                    //Document revitDoc = app.ActiveUIDocument.Document;
                    //using (Transaction transaction = new Transaction(revitDoc, "Creat Line1"))
                    //{
                    //    transaction.Start();
                    //    Autodesk.Revit.DB.Line line = Autodesk.Revit.DB.Line.CreateBound(new XYZ(0, 0, 0), new XYZ(100, 0, 0));
                    //    SketchPlane sketchPlane = SketchPlane.Create(revitDoc, Plane.CreateByNormalAndOrigin(XYZ.BasisZ, XYZ.Zero));
                    //    revitDoc.Create.NewModelCurve(line as Curve, sketchPlane);
                    //    transaction.Commit();
                    //}

                    var args = string.Empty;
                    if (selection.Value == -1)
                    {
                        args = string.Empty;
                    }
                    else
                    {

                        var ele = uiDoc.Document.GetElement(selection) as Wall;

                        var wallLocation = ele.Location as LocationCurve;
                        var wallString = ConvertToString(wallLocation.Curve);
                        args = $"WallId:{selection} , WallData: {wallString}";
                    }

                    var process = new Process
                    {
                        StartInfo = new ProcessStartInfo
                        {
                            FileName = @"NET.Mcp.Client.exe",          // 可执行文件路径（如 "cmd.exe"）
                            Arguments = this.TextBox.Text + $"选中构件的数据为 ：{args}",       // 命令行参数
                            UseShellExecute = false,     // 必须为 false 才能重定向输出
                            CreateNoWindow = true,       // 隐藏控制台窗口
                            RedirectStandardOutput = true, // 重定向标准输出
                            RedirectStandardError = true  // 重定向错误输出（可选）
                        }
                    };

                    process.Start();

                    // 读取所有输出（同步方式）
                    string output = process.StandardOutput.ReadToEnd();
                    string errors = process.StandardError.ReadToEnd(); // 如果需要错误流

                    process.WaitForExit(); // 等待进程结束
                    process.Close(); // 关闭进程

                    if (string.IsNullOrEmpty(errors))
                    {
                        var jsonConvertData = JsonConvert.DeserializeObject<CreateWallData>(output);
                        var methodName = jsonConvertData.Command;
                        // 1. 加载DLL
                        Assembly assembly = typeof(Command).Assembly;

                        // 2. 查找实现类（通过接口或命名约定）
                        Type commandType = assembly.GetTypes()
                            .FirstOrDefault(t => t.Name == methodName);

                        if (commandType == null)
                            throw new Exception($"未找到 {methodName} 的实现类");

                        var eCommand = (IRevitCommand)Activator.CreateInstance(commandType);
                        eCommand.Execute(JsonConvert.SerializeObject(jsonConvertData.Args));

                    }
                });
                _externalEvent.Raise();
            }
        }


        private string ConvertToString(Curve curve)
        {
            return $"Curve Data is : Start = {ConvertToString(curve.GetEndPoint(0))} , End = {ConvertToString(curve.GetEndPoint(1))}";
        }

        private string ConvertToString(XYZ point)
        {
            return $"X = {point.X * 304.8}, Y = {point.Y * 304.8}, Z = {point.Z * 304.8}";
        }

        public class ExecuteEventHandler : IExternalEventHandler
        {
            public string Name { get; private set; }

            public Action<UIApplication> ExecuteAction { get; set; }

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
                    catch
                    { }
                }
            }

            public string GetName()
            {
                return Name;
            }
        }
    }
}
