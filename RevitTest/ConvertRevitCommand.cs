using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Structure;
using Autodesk.Revit.DB.Architecture;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static RevitTest.Command;

namespace RevitTest
{
    public class ConvertRevitCommand
    {
        public class InsertWindowInWall : IRevitCommand
        {
            public void Execute(string jsonArgs, Document document)
            {


                using (Transaction trans = new Transaction(document, nameof(InsertWindowInWall)))
                {
                    trans.Start();
                    var filter = new FilteredElementCollector(document);
                    var windowType = filter
                        .OfClass(typeof(FamilySymbol))
                        .OfCategory(BuiltInCategory.OST_Windows)
                        .Cast<FamilySymbol>()
                        .FirstOrDefault(x => x.Name.Contains("1500"));

                    if (windowType == null)
                        throw new InvalidOperationException("Window type '1500x1200' not found");

                    var data = JsonConvert.DeserializeObject<InsertWindowInWallArguments>(jsonArgs);

                    var locationPoint = new XYZ(data.Position[0] / 304.8, data.Position[1] / 304.8, data.Position[2] / 304.8);

                    var hostWall = FindWallByEId(document, data.WallId);
                    if (hostWall == null)
                        throw new InvalidOperationException($"未找到匹配eId的墙体: {data.WallId}");

                    document.Create.NewFamilyInstance(
                        locationPoint,
                        windowType,
                        hostWall,
                        document.ActiveView.GenLevel,
                        StructuralType.NonStructural);

                    trans.Commit();
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
        public class CreateWall : IRevitCommand
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
                var commentId = args.EId;

                using (Transaction trans = new Transaction(document, nameof(CreateWall)))
                {
                    trans.Start();
                    var start = new XYZ(x / 304.8, y / 304.8, z);
                    var end = new XYZ(x1 / 304.8, y1 / 304.8, z1);
                    var line = Line.CreateBound(start, end);
                    var wall =  Wall.Create(document, line, document.ActiveView.GenLevel.Id, false);
                    wall.get_Parameter(BuiltInParameter.ALL_MODEL_MARK).Set(commentId);
                    trans.Commit();
                }
            }
        }

        public class ChangeWallWeight : IRevitCommand
        {
            public void Execute(string jsonArgs, Document document)
            {
                var args = JsonConvert.DeserializeObject<ChangeWallWeightArguments>(jsonArgs);
                if (args == null) throw new InvalidOperationException("ChangeWallWeight 参数无效");

                var filter = new FilteredElementCollector(document);
                var walls = filter
                    .OfClass(typeof(Wall))
                    .WhereElementIsNotElementType()
                    .Cast<Wall>()
                    .ToList();

                var wallTypes = new FilteredElementCollector(document)
                    .OfClass(typeof(WallType))
                    .WhereElementIsElementType()
                    .Cast<WallType>()
                    .ToList();

                var weightInt = (int)Math.Round(args.Weight);
                var candidates = new[] { $"{weightInt}", $"{args.Weight}", $"{weightInt}mm", $"{args.Weight}mm" };
                WallType targetWallType = wallTypes.FirstOrDefault(x =>
                    candidates.Any(c => (x.Name ?? string.Empty).IndexOf(c, StringComparison.OrdinalIgnoreCase) >= 0));

                if (targetWallType == null)
                {
                    int ExtractNumber(string s)
                    {
                        var digits = new string((s ?? string.Empty).Where(char.IsDigit).ToArray());
                        if (int.TryParse(digits, out var n)) return n;
                        return -1;
                    }
                    targetWallType = wallTypes.FirstOrDefault(x => ExtractNumber(x.Name) == weightInt);
                }

                if (targetWallType == null) throw new InvalidOperationException($"未找到包含厚度 {args.Weight} 的墙类型");

                using (Transaction trans = new Transaction(document, nameof(ChangeWallWeight)))
                {
                    trans.Start();
                    foreach (var w in walls)
                    {
                        w.WallType = targetWallType;
                    }
                    trans.Commit();
                }
            }
        }

        public class InsertWindowOrDoor : IRevitCommand
        {
            public void Execute(string jsonArgs, Document document)
            {
                throw new NotImplementedException();
            }
        }

        public class CreateDoor : IRevitCommand
        {
            public void Execute(string jsonArgs, Document document)
            {
                var args = JsonConvert.DeserializeObject<CreateDoorArguments>(jsonArgs);
                using (Transaction trans = new Transaction(document, nameof(CreateDoor)))
                {
                    trans.Start();
                    Wall wall = FindWallByEId(document, args.WallId);
                    //if (wall == null && int.TryParse(args.WallId, out var idInt))
                    //{
                    //    wall = document.GetElement(new ElementId(idInt)) as Wall;
                    //}
                    //if (wall == null)
                    //{
                    //    wall = new FilteredElementCollector(document)
                    //        .OfClass(typeof(Wall))
                    //        .WhereElementIsNotElementType()
                    //        .Cast<Wall>()
                    //        .FirstOrDefault(w => w.UniqueId == args.WallId || (w.Name != null && w.Name.Contains(args.WallId)));
                    //}
                    if (wall == null)
                        throw new InvalidOperationException($"未找到墙体: {args.WallId}");

                    var symbol = new FilteredElementCollector(document)
                        .OfClass(typeof(FamilySymbol))
                        .OfCategory(BuiltInCategory.OST_Doors)
                        .Cast<FamilySymbol>()
                        .FirstOrDefault(x => x.Name == args.DoorName) ?? new FilteredElementCollector(document)
                        .OfClass(typeof(FamilySymbol))
                        .OfCategory(BuiltInCategory.OST_Doors)
                        .Cast<FamilySymbol>()
                        .FirstOrDefault();
                    if (symbol == null)
                        throw new InvalidOperationException("未找到门族类型");
                    if (!symbol.IsActive)
                        symbol.Activate();

                    var lc = wall.Location as LocationCurve;
                    var curve = lc?.Curve;
                    double t = 0.5;
                    if (curve != null)
                    {
                        if (args.Position > 1)
                        {
                            var distanceFeet = args.Position / 304.8;
                            var lengthFeet = curve.Length;
                            t = lengthFeet <= 0 ? 0.5 : Math.Min(1.0, Math.Max(0.0, distanceFeet / lengthFeet));
                        }
                        else if (args.Position >= 0)
                        {
                            t = args.Position;
                        }
                    }
                    var p = curve.Evaluate(t, true);
                    var location = new XYZ(p.X, p.Y, p.Z);

                    document.Create.NewFamilyInstance(location, symbol, wall, document.ActiveView.GenLevel, StructuralType.NonStructural);
                    trans.Commit();
                }
            }
        }

        public class CreateFloor : IRevitCommand
        {
            public void Execute(string jsonArgs, Document document)
            {
                var args = JsonConvert.DeserializeObject<CreateFloorArguments>(jsonArgs);
                using (Transaction trans = new Transaction(document, nameof(CreateFloor)))
                {
                    trans.Start();
                    var ca = new List<Curve>();
                    for (int i = 0; i < args.BoundaryPoints.Length; i++)
                    {
                        var a = args.BoundaryPoints[i];
                        var b = args.BoundaryPoints[(i + 1) % args.BoundaryPoints.Length];
                        var pa = new XYZ(a[0] / 304.8, a[1] / 304.8, a[2] / 304.8);
                        var pb = new XYZ(b[0] / 304.8, b[1] / 304.8, b[2] / 304.8);
                        ca.Add(Line.CreateBound(pa, pb));
                    }

                    var loop = CurveLoop.Create(ca);

                    var floorType = new FilteredElementCollector(document)
                        .OfClass(typeof(FloorType))
                        .Cast<FloorType>()
                        .FirstOrDefault();
                    if (floorType == null) throw new InvalidOperationException("未找到楼板类型");
                    var level = new FilteredElementCollector(document)
                        .OfClass(typeof(Level))
                        .Cast<Level>()
                        .FirstOrDefault(l => l.Name == args.Level) ?? document.ActiveView.GenLevel;
                    var floor = Floor.Create(document, new List<CurveLoop>(){ loop }, floorType.Id, level.Id);
                    trans.Commit();
                }
            }
        }

        public class CreateColumn : IRevitCommand
        {
            public void Execute(string jsonArgs, Document document)
            {
                var args = JsonConvert.DeserializeObject<CreateColumnArguments>(jsonArgs);
                using (Transaction trans = new Transaction(document, nameof(CreateColumn)))
                {
                    trans.Start();
                    var symbol = new FilteredElementCollector(document)
                        .OfClass(typeof(FamilySymbol))
                        .OfCategory(BuiltInCategory.OST_StructuralColumns)
                        .Cast<FamilySymbol>()
                        .FirstOrDefault(x => x.Name == args.ColumnName);
                    if (symbol == null)
                        symbol = new FilteredElementCollector(document)
                            .OfClass(typeof(FamilySymbol))
                            .OfCategory(BuiltInCategory.OST_StructuralColumns)
                            .Cast<FamilySymbol>()
                            .FirstOrDefault();
                    if (symbol == null)
                        symbol = new FilteredElementCollector(document)
                            .OfClass(typeof(FamilySymbol))
                            .OfCategory(BuiltInCategory.OST_Columns)
                            .Cast<FamilySymbol>()
                            .FirstOrDefault(x => x.Name == args.ColumnName);
                    if (symbol == null)
                        symbol = new FilteredElementCollector(document)
                            .OfClass(typeof(FamilySymbol))
                            .OfCategory(BuiltInCategory.OST_Columns)
                            .Cast<FamilySymbol>()
                            .FirstOrDefault();
                    if (symbol == null)
                    {
                        trans.RollBack();
                        return;
                    }
                    if (!symbol.IsActive) symbol.Activate();

                    var p = new XYZ(args.Position[0] / 304.8, args.Position[1] / 304.8, args.Position[2] / 304.8);
                    var bottom = new FilteredElementCollector(document).OfClass(typeof(Level)).Cast<Level>().FirstOrDefault(l => l.Name == args.BottomLevel);
                    var top = new FilteredElementCollector(document).OfClass(typeof(Level)).Cast<Level>().FirstOrDefault(l => l.Name == args.TopLevel);
                    var inst = document.Create.NewFamilyInstance(p, symbol, bottom, StructuralType.Column);
                    if (top != null)
                    {
                        var topParam = inst.get_Parameter(BuiltInParameter.FAMILY_TOP_LEVEL_PARAM);
                        if (topParam != null && !topParam.IsReadOnly)
                            topParam.Set(top.Id);
                    }
                    trans.Commit();
                }
            }
        }

        public class MoveElement : IRevitCommand
        {
            public void Execute(string jsonArgs, Document document)
            {
                var args = JsonConvert.DeserializeObject<MoveElementArguments>(jsonArgs);
                using (Transaction trans = new Transaction(document, nameof(MoveElement)))
                {
                    trans.Start();
                    Element target = null;
                    if (int.TryParse(args.ElementId, out var idInt))
                        target = document.GetElement(new ElementId(idInt));
                    if (target == null)
                        target = document.GetElement(args.ElementId);
                    if (target == null)
                        throw new InvalidOperationException($"未找到构件: {args.ElementId}");

                    var loc = target.Location;
                    XYZ current = null;
                    if (loc is LocationPoint lp) current = lp.Point;
                    else if (loc is LocationCurve lc) current = lc.Curve.GetEndPoint(0);
                    if (current == null) throw new InvalidOperationException("该构件不支持移动定位");

                    var desired = new XYZ(args.TargetPoint[0] / 304.8, args.TargetPoint[1] / 304.8, args.TargetPoint[2] / 304.8);
                    var delta = desired - current;
                    ElementTransformUtils.MoveElement(document, target.Id, delta);
                    trans.Commit();
                }
            }
        }

        public class CreateStair : IRevitCommand
        {
            public void Execute(string jsonArgs, Document document)
            {
                var args = JsonConvert.DeserializeObject<CreateStairArguments>(jsonArgs);
                var bottom = new FilteredElementCollector(document)
                    .OfClass(typeof(Level)).Cast<Level>()
                    .FirstOrDefault(l => l.Name == args.BottomLevel) ?? document.ActiveView.GenLevel;
                var top = new FilteredElementCollector(document)
                    .OfClass(typeof(Level)).Cast<Level>()
                    .FirstOrDefault(l => l.Name == args.TopLevel) ?? bottom;

                using (var scope = new StairsEditScope(document, nameof(CreateStair)))
                {
                    var stairsId = scope.Start(bottom.Id, top.Id);
                    using (var t = new Transaction(document, "CreateStairRun"))
                    {
                        t.Start();
                        var width = Math.Max(args.RunWidth / 304.8, 1.0);
                        var length = Math.Max(args.RisersCount * (300.0 / 304.8), 3.0);
                        var z = bottom.Elevation;
                        var p1 = new XYZ(0, -width / 2, z);
                        var p2 = new XYZ(length, -width / 2, z);
                        var p3 = new XYZ(0, width / 2, z);
                        var p4 = new XYZ(length, width / 2, z);

                        IList<Curve> bdry = new List<Curve> { Line.CreateBound(p1, p2), Line.CreateBound(p3, p4) };
                        IList<Curve> risers = new List<Curve>();
                        int n = Math.Max(args.RisersCount, 1);
                        for (int i = 0; i <= n; i++)
                        {
                            double f = i / (double)n;
                            var e0 = p1 + (p2 - p1) * f;
                            var e1 = p3 + (p4 - p3) * f;
                            risers.Add(Line.CreateBound(e0, e1));
                        }
                        IList<Curve> path = new List<Curve> { Line.CreateBound((p1 + p3) / 2.0, (p2 + p4) / 2.0) };

                        var run = StairsRun.CreateSketchedRun(document, stairsId, z, bdry, risers, path);
                        run.ActualRunWidth = width;
                        t.Commit();
                    }
                    scope.Commit(new StairsFailurePreprocessor());
                }
            }
        }

        class StairsFailurePreprocessor : IFailuresPreprocessor
        {
            public FailureProcessingResult PreprocessFailures(FailuresAccessor failuresAccessor)
            {
                return FailureProcessingResult.ProceedWithCommit;
            }
        }

        private static Wall FindWallByEId(Document document, string eId)
        {
            if (string.IsNullOrWhiteSpace(eId)) return null;
            return new FilteredElementCollector(document)
                .OfClass(typeof(Wall))
                .WhereElementIsNotElementType()
                .Cast<Wall>()
                .FirstOrDefault(w =>
                {
                    var p = w.get_Parameter(BuiltInParameter.ALL_MODEL_MARK);
                    var s1 = p?.AsString();
                    var s2 = p?.AsValueString();
                    return string.Equals(s1, eId, StringComparison.OrdinalIgnoreCase) || string.Equals(s2, eId, StringComparison.OrdinalIgnoreCase);
                });
        }

    }
}
