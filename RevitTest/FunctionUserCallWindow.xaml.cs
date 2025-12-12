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
using System.Collections.ObjectModel;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Autodesk.Revit.DB;
using TaskDialog = Autodesk.Windows.TaskDialog;
using Visibility = System.Windows.Visibility;


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
            TextBox.Text = "创建一个墙体，墙体坐标为(0, 0, 0)->(10000, 0, 0)，单位是mm";
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

                    #region 单个调用 Single Use MCP

                    //var args = string.Empty;
                    //if (selection.Value == -1)
                    //{
                    //    args = string.Empty;
                    //}
                    //else
                    //{

                    //    var ele = uiDoc.Document.GetElement(selection) as Wall;

                    //    var wallLocation = ele.Location as LocationCurve;
                    //    var wallString = ConvertToString(wallLocation.Curve);
                    //    args = $"WallId:{selection} , WallData: {wallString}";
                    //}

                    //var process = new Process
                    //{
                    //    StartInfo = new ProcessStartInfo
                    //    {
                    //        FileName = @"NET.Mcp.Client.exe",          // 可执行文件路径（如 "cmd.exe"）
                    //        Arguments = this.TextBox.Text + $"选中构件的数据为 ：{args}",       // 命令行参数
                    //        UseShellExecute = false,     // 必须为 false 才能重定向输出
                    //        CreateNoWindow = true,       // 隐藏控制台窗口
                    //        RedirectStandardOutput = true, // 重定向标准输出
                    //        RedirectStandardError = true  // 重定向错误输出（可选）
                    //    }
                    //};

                    //process.Start();

                    //// 读取所有输出（同步方式）
                    //string output = process.StandardOutput.ReadToEnd();
                    //string errors = process.StandardError.ReadToEnd(); // 如果需要错误流

                    //process.WaitForExit(); // 等待进程结束
                    //process.Close(); // 关闭进程

                    //if (string.IsNullOrEmpty(errors))
                    //{
                    //    var jsonConvertData = JsonConvert.DeserializeObject<CreateDataByAI>(output) ?? throw new InvalidOperationException("Failed to deserialize CreateDataByAI");
                    //    var methodName = jsonConvertData.Command;
                    //    // 1. 加载DLL
                    //    Assembly assembly = typeof(Command).Assembly;

                    //    // 2. 查找实现类（通过接口或命名约定）
                    //    Type commandType = assembly.GetTypes()
                    //        .FirstOrDefault(t => t.Name == methodName);

                    //    if (commandType == null)
                    //        throw new Exception($"未找到 {methodName} 的实现类");

                    //    var eCommand = (IRevitCommand)Activator.CreateInstance(commandType);
                    //    eCommand.Execute(JsonConvert.SerializeObject(jsonConvertData.Args));

                    //}

                    #endregion

                    #region 多重调用 LangChain That Can Auto Generation Wall And Then Insert Window

                    // if use the [Revit Add-iN Manage](https://github.com/chuongmep/RevitAddInManager) , use this code will cant find `location()` 
                    var client_path = @"F:\DevProjects\imkcrevit\RevitMCP_Blog\bin\2024\Debug2024";

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

                    process.Start();

                    // 读取所有输出（同步方式）
                    //string output = process.StandardOutput.ReadToEnd();
                    //string errors = process.StandardError.ReadToEnd(); // 如果需要错误流
                    var data = new List<string>();
                    using (var outputReader = process.StandardOutput)
                    {
                        using (var errorReader = process.StandardError)
                        {
                            while (!outputReader.EndOfStream || !errorReader.EndOfStream)
                                if (!outputReader.EndOfStream)
                                {
                                    var line = outputReader.ReadToEnd();
                                    data.Add(line);
                                }
                        }
                    }


                    process.WaitForExit(); // 等待进程结束
                    process.Close(); // 关闭进程
                    foreach (var item in data)
                    {
                        var jsonConvertData = JsonConvert.DeserializeObject<List<Command.CreateDataByAI>>(item) ??
                                              throw new InvalidOperationException(
                                                  "Failed to deserialize CreateDataByAI list");

                        InitWorkflows(jsonConvertData);
                        for (int i = 0; i < jsonConvertData.Count; i++)
                        {
                            var createData = jsonConvertData[i];
                            var methodName = createData.Command;
                            var assembly = typeof(ConvertRevitCommand).Assembly;
                            var commandType = assembly.GetTypes().FirstOrDefault(t => t.Name == methodName);
                            if (commandType == null)
                            {
                                UpdateWorkflow(i, methodName, false);
                                throw new Exception($"未找到 {methodName} 的实现类");
                            }

                            var eCommand = (IRevitCommand)Activator.CreateInstance(commandType);
                            try
                            {
                                var argsJson = BuildArgsJson(createData);
                                eCommand.Execute(argsJson, uiDoc.Document);
                                UpdateWorkflow(i, methodName, true);
                            }
                            catch (Exception exception)
                            {
                                UpdateWorkflow(i, methodName, false);
                                var dialog = new TaskDialog()
                                {
                                    WindowTitle = "R2026",
                                    MainInstruction = $"An error occurred while executing the command. data : {JsonConvert.SerializeObject(createData.Args)}",
                                    ContentText = exception.Message
                                };
                                dialog.Show();
                                return;
                            }
                        }
                    }

                    #endregion
                });
                _externalEvent.Raise();
            }
            UpdateOutputVisibility();
        }


        private string ConvertToString(Curve curve)
        {
            return
                $"Curve Data is : Start = {ConvertToString(curve.GetEndPoint(0))} , End = {ConvertToString(curve.GetEndPoint(1))}";
        }

        private string ConvertToString(XYZ point)
        {
            return $"X = {point.X * 304.8}, Y = {point.Y * 304.8}, Z = {point.Z * 304.8}";
        }


        private static string BuildArgsJson(Command.CreateDataByAI data)
        {
            var raw = JsonConvert.SerializeObject(data.Args);
            try
            {
                Newtonsoft.Json.Linq.JObject.Parse(raw);
                return raw;
            }
            catch
            {
                if (string.Equals(data.Command, "CreateDoor", StringComparison.OrdinalIgnoreCase))
                {
                    var s = data.Args?.ToString() ?? string.Empty;
                    var wallId = ExtractGuid(s);
                    var doorName = ExtractString(s, "doorName") ?? "Standard Door 1000x2100";
                    var width = ExtractNumber(s, "width");
                    var height = ExtractNumber(s, "height");
                    var position = ExtractNumber(s, "position");
                    var obj = new
                    {
                        wallId = wallId,
                        doorName = doorName,
                        width = width > 0 ? width : 1000,
                        height = height > 0 ? height : 2100,
                        position = position > 0 ? position : 0.5
                    };
                    return JsonConvert.SerializeObject(obj);
                }
                return raw;
            }
        }

        private static string ExtractGuid(string s)
        {
            var r = System.Text.RegularExpressions.Regex.Match(s, "[0-9a-fA-F]{8}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{12}");
            return r.Success ? r.Value : string.Empty;
        }

        private static string ExtractString(string s, string key)
        {
            var r = System.Text.RegularExpressions.Regex.Match(s, $"\\\"{key}\\\"\\s*:\\s*\\\"([^\\\"]*)\\\"");
            return r.Success ? r.Groups[1].Value : null;
        }

        private static double ExtractNumber(string s, string key)
        {
            var r = System.Text.RegularExpressions.Regex.Match(s, $"\\\"{key}\\\"\\s*:\\s*([0-9]+(?:\\\\.[0-9]+)?)");
            if (r.Success && double.TryParse(r.Groups[1].Value, out var v)) return v;
            var r2 = System.Text.RegularExpressions.Regex.Match(s, $"{key}[^0-9]*([0-9]+)");
            if (r2.Success && double.TryParse(r2.Groups[1].Value, out var v2)) return v2;
            return -1;
        }

        private void Clear(object sender, RoutedEventArgs e)
        {
            if (TextBox != null)
                TextBox.Text = string.Empty;
            var output = this.FindName("OutputBox") as System.Windows.Controls.TextBox;
            if (output != null)
                output.Text = string.Empty;
            UpdateOutputVisibility();
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            var tb = sender as System.Windows.Controls.TextBox;
            if (tb == null) return;
            var lineCount = tb.LineCount;
            if (lineCount <= 0) lineCount = 1;
            var lineHeight = tb.FontSize * 1.35;
            var padding = tb.Padding.Top + tb.Padding.Bottom;
            var desired = lineCount * lineHeight + padding;
            tb.MaxHeight = desired;
            UpdateOutputVisibility();
        }

        private void UpdateOutputVisibility()
        {
            var output = this.FindName("OutputBox") as System.Windows.Controls.TextBox;
            if (output == null) return;
            output.Visibility = string.IsNullOrWhiteSpace(output.Text) ?  Visibility.Collapsed : Visibility.Visible;
        }

        private ObservableCollection<string> _workflowItems = new ObservableCollection<string>();
        private int _workflowTotal = 0;
        private int _workflowDone = 0;
        private List<string> _workflowNames = new List<string>();

        private void InitWorkflows(List<Command.CreateDataByAI> list)
        {
            _workflowTotal = list.Count;
            _workflowDone = 0;
            _workflowNames = list.Select(x => x.Command).ToList();
            _workflowItems.Clear();
            for (int i = 0; i < _workflowTotal; i++)
            {
                _workflowItems.Add($"{i + 1}. {_workflowNames[i]} [待执行]");
            }
            var lb = this.FindName("WorkflowListBox") as System.Windows.Controls.ListBox;
            if (lb != null) lb.ItemsSource = _workflowItems;
            var summary = this.FindName("WorkflowSummaryText") as System.Windows.Controls.TextBlock;
            if (summary != null) summary.Text = $"工作流：总计 {_workflowTotal} 条，完成 {_workflowDone} 条";
        }

        private void UpdateWorkflow(int index, string name, bool ok)
        {
            if (index >= 0 && index < _workflowItems.Count)
            {
                _workflowItems[index] = $"{index + 1}. {name} [{(ok ? "已完成" : "失败")}]";
                if (ok) _workflowDone++;
                var summary = this.FindName("WorkflowSummaryText") as System.Windows.Controls.TextBlock;
                if (summary != null) summary.Text = $"工作流：总计 {_workflowTotal} 条，完成 {_workflowDone} 条";
            }
        }

    }
}
