using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Drawing;
using System.Windows.Forms;
using System.Reflection;
using System.Drawing.Text;
using ScottPlot;
using SharedLib.Services;

namespace ConsoleApp
{
    public partial class MainForm : Form
    {
        // jsonPath will be determined at runtime from several candidate locations
        private string jsonPath = string.Empty;
        private List<string> availableYears = new List<string>();
        private List<System.Text.Json.JsonElement> dataArray = new List<System.Text.Json.JsonElement>();

        public MainForm()
        {
            InitializeComponent();
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            // Candidate locations to search for the data file (helps when running from VS vs published EXE)
            string baseDir = AppContext.BaseDirectory; // use AppContext.BaseDirectory for robustness
            string startup = Application.StartupPath;
            var filenames = new[] { "Visitor.json", "Visitor.js" };
            var candidates = new List<string>();

            // existing likely locations
            foreach (var f in filenames)
            {
                candidates.Add(Path.Combine(baseDir, "App_Data", f));
                candidates.Add(Path.Combine(baseDir, f));
                candidates.Add(Path.Combine(startup, "App_Data", f));
                candidates.Add(Path.Combine(startup, f));
                candidates.Add(Path.Combine(startup, "..", "App_Data", f));
                candidates.Add(Path.Combine(Environment.CurrentDirectory, "App_Data", f));
            }

            // walk up parent directories from baseDir to try to find project-level App_Data (useful when running from bin/...)
            try
            {
                var dirInfo = new DirectoryInfo(baseDir);
                for (int depth =0; depth <8 && dirInfo != null; depth++)
                {
                    foreach (var f in filenames)
                    {
                        candidates.Add(Path.Combine(dirInfo.FullName, "App_Data", f));
                        candidates.Add(Path.Combine(dirInfo.FullName, f));
                    }
                    dirInfo = dirInfo.Parent;
                }
            }
            catch
            {
                // ignore any IO issues during candidate generation
            }

            // dedupe candidates but keep order
            var orderedCandidates = candidates.Where(s => !string.IsNullOrWhiteSpace(s)).Select(p => Path.GetFullPath(p)).Distinct().ToList();

            string found = orderedCandidates.FirstOrDefault(File.Exists);
            if (found == null)
            {
                // show full paths checked so user can fix copy-to-output or path issues
                labelStatus.Text = "找不到資料檔案。已檢查以下路徑：" + Environment.NewLine + string.Join(Environment.NewLine, orderedCandidates);
                return;
            }

            jsonPath = Path.GetFullPath(found);

            string json;
            try
            {
                json = File.ReadAllText(jsonPath);
            }
            catch (Exception ex)
            {
                labelStatus.Text = $"讀取檔案失敗：{jsonPath}，例外：{ex.Message}";
                return;
            }

            System.Text.Json.JsonDocument doc;
            try
            {
                doc = System.Text.Json.JsonDocument.Parse(json);
            }
            catch (Exception ex)
            {
                labelStatus.Text = $"解析 JSON失敗：{ex.Message}；檔案：{jsonPath}";
                return;
            }

            // safe-guard: check for 'data' property
            if (!doc.RootElement.TryGetProperty("data", out var dataElem) || dataElem.ValueKind != System.Text.Json.JsonValueKind.Array)
            {
                labelStatus.Text = $"JSON 格式不正確，找不到 'data' 陣列：{jsonPath}";
                return;
            }

            dataArray = dataElem.EnumerateArray().ToList();

            //取得所有年份（防禦式取值）
            availableYears = dataArray
                .Select(x => x.ValueKind == System.Text.Json.JsonValueKind.Object && x.TryGetProperty("年度", out var y) ? y.GetString() : null)
                .Where(s => !string.IsNullOrEmpty(s))
                .Distinct()
                .ToList();

            comboYear.Items.Clear();
            comboYear.Items.AddRange(availableYears.ToArray());
            if (comboYear.Items.Count >0)
                comboYear.SelectedIndex =0;

            if (availableYears.Count >0)
                DrawChartForYear(availableYears[0]);
            else
                labelStatus.Text = "資料檔案中未包含任何年度資料";
        }

        private void comboYear_SelectedIndexChanged(object sender, EventArgs e)
        {
            string selectedYear = comboYear.SelectedItem?.ToString() ?? "";
            DrawChartForYear(selectedYear);
        }

        private void DrawChartForYear(string year)
        {
            var yearObj = dataArray.FirstOrDefault(x => x.ValueKind == System.Text.Json.JsonValueKind.Object && x.TryGetProperty("年度", out var y) && y.GetString() == year);
            if (yearObj.ValueKind != System.Text.Json.JsonValueKind.Object)
            {
                labelStatus.Text = $"找不到 {year} 年資料";
                formsPlot1.Plot.Clear();
                formsPlot1.Refresh();
                return;
            }

            var labels = new List<string>();
            var values = new List<double>();
            foreach (var prop in yearObj.EnumerateObject())
            {
                if (prop.Name == "年度") continue;
                labels.Add(prop.Name);
                if (prop.Value.ValueKind == System.Text.Json.JsonValueKind.String && double.TryParse(prop.Value.GetString(), out double count))
                    values.Add(count);
                else if (prop.Value.ValueKind == System.Text.Json.JsonValueKind.Number && prop.Value.TryGetDouble(out double d))
                    values.Add(d);
                else
                    values.Add(0);
            }

            if (values.Count ==0)
            {
                labelStatus.Text = $"{year} 年找不到有效數值資料";
                formsPlot1.Plot.Clear();
                formsPlot1.Refresh();
                return;
            }

            formsPlot1.Plot.Clear();

            double maxY = values.Count > 0 ? values.Max() : 1;
            
            int n = values.Count;
            double barWidth =1; // 固定寬度
            double gap =2; // 調整間距
            double[] positions = Enumerable.Range(0, n).Select(i => i * gap).ToArray();

            // choose a preferred Chinese font name and sizes
            string chineseFontName = GetPreferredChineseFont() ?? "Microsoft JhengHei";
            double categoryFontSize =15.0;
            double valueFontSize =12.0;

            // offset for value labels above bars (relative to maxY)
            double valueOffset = Math.Max(maxY *0.03,1.0);

            // add bars and value labels on top
            for (int i =0; i < n; i++)
            {
                var barItem = formsPlot1.Plot.Add.Bar(positions[i], values[i]);

                // add value label above each bar
                double x = positions[i];
                double yTop = values[i] + valueOffset;
                string valText = values[i].ToString("N0");
                object valTxt = formsPlot1.Plot.Add.Text(valText, x-1, yTop);
                var vtType = valTxt.GetType();
                var pFontName = vtType.GetProperty("FontName");
                if (pFontName != null) SafeSetProperty(valTxt, pFontName, chineseFontName);
                var pFontSize = vtType.GetProperty("FontSize");
                if (pFontSize != null) SafeSetProperty(valTxt, pFontSize, valueFontSize);
                var pFontStyle = vtType.GetProperty("FontStyle") ?? vtType.GetProperty("Style");
                if (pFontStyle != null) SafeSetProperty(valTxt, pFontStyle, System.Drawing.FontStyle.Regular);
                // try set color to black
                var pColor = vtType.GetProperty("Color") ?? vtType.GetProperty("FontColor") ?? vtType.GetProperty("TextColor");
                if (pColor != null) SafeSetProperty(valTxt, pColor, System.Drawing.Color.Black);

                // center horizontally: try several possible property names
                var hAlignProps = new[] { "HAlign", "HAlignment", "HorizontalAlignment", "Alignment", "Anchor" };
                foreach (var hn in hAlignProps)
                {
                    var p = vtType.GetProperty(hn);
                    if (p != null)
                    {
                        TrySetEnumOrString(p, valTxt, "Center");
                        break;
                    }
                }

                // vertical alignment: align label's bottom to the y coordinate so it sits above the bar
                var vAlignProps = new[] { "VAlign", "VAlignment", "VerticalAlignment", "AnchorY", "AlignmentY" };
                foreach (var vn in vAlignProps)
                {
                    var p = vtType.GetProperty(vn);
                    if (p != null)
                    {
                        // try common enum/string values for bottom alignment
                        TrySetEnumOrString(p, valTxt, "Bottom");
                        TrySetEnumOrString(p, valTxt, "Lower");
                        TrySetEnumOrString(p, valTxt, "South");
                        break;
                    }
                }

                // ensure no rotation for value labels
                var rotProps = new[] { "Rotation", "RotationDegrees", "Angle", "RotationDeg", "RotationAngle" };
                foreach (var rp in rotProps)
                {
                    var p = vtType.GetProperty(rp);
                    if (p != null)
                    {
                        SafeSetProperty(valTxt, p,0.0);
                        break;
                    }
                }
            }

            // try to hide the numeric X-axis tick labels using several strategies to support multiple ScottPlot versions
            try
            {
                var plotObj = formsPlot1.Plot;
                bool succeeded = false;

                // Strategy A: call XTicks(double[], string[])
                try
                {
                    var pt = plotObj.GetType();
                    var m = pt.GetMethod("XTicks", new Type[] { typeof(double[]), typeof(string[]) });
                    if (m != null)
                    {
                        var blankLabels = Enumerable.Repeat("", positions.Length).ToArray();
                        m.Invoke(plotObj, new object[] { positions, blankLabels });
                        succeeded = true;
                    }
                }
                catch { }

                // Strategy B: locate an X axis object and try to set tick label color/style to transparent
                if (!succeeded)
                {
                    var axisNames = new[] { "XAxis", "AxisX", "XAxes", "X", "XAxis1" };
                    var pt = plotObj.GetType();
                    foreach (var name in axisNames)
                    {
                        try
                        {
                            var p = pt.GetProperty(name);
                            if (p == null) continue;
                            var axisObj = p.GetValue(plotObj);
                            if (axisObj == null) continue;

                            var at = axisObj.GetType();

                            // try method TickLabelStyle(Color)
                            var mStyle = at.GetMethod("TickLabelStyle", new Type[] { typeof(System.Drawing.Color) });
                            if (mStyle != null)
                            {
                                try { mStyle.Invoke(axisObj, new object[] { System.Drawing.Color.Transparent }); succeeded = true; break; } catch { }
                            }

                            // try property names commonly used for tick label color
                            var colorProps = new[] { "TickLabelColor", "TickColor", "LabelColor", "FontColor", "Color" };
                            foreach (var cp in colorProps)
                            {
                                var colP = at.GetProperty(cp);
                                if (colP != null && colP.PropertyType == typeof(System.Drawing.Color))
                                {
                                    try { colP.SetValue(axisObj, System.Drawing.Color.Transparent); succeeded = true; break; } catch { }
                                }
                            }
                            if (succeeded) break;

                            // try method TickLabelStyle() without parameters
                            var mStyleNoArgs = at.GetMethod("TickLabelStyle", Type.EmptyTypes);
                            if (mStyleNoArgs != null)
                            {
                                try { mStyleNoArgs.Invoke(axisObj, null); succeeded = true; break; } catch { }
                            }
                        }
                        catch { }
                    }
                }

                // Strategy C: if nothing worked, attempt to set X axis tick labels via a generic 'XTicks' method signature with objects
                if (!succeeded)
                {
                    try
                    {
                        var pt = plotObj.GetType();
                        var methods = pt.GetMethods().Where(m => m.Name == "XTicks").ToArray();
                        foreach (var m in methods)
                        {
                            var pars = m.GetParameters();
                            if (pars.Length ==2)
                            {
                                try
                                {
                                    var blankLabels = Enumerable.Repeat("", positions.Length).ToArray();
                                    m.Invoke(plotObj, new object[] { positions, blankLabels });
                                    succeeded = true;
                                    break;
                                }
                                catch { }
                            }
                        }
                    }
                    catch { }
                }
            }
            catch { }

            // continue with category labels (rotated) below bars
            for (int i =0; i < n; i++)
            {
                double x = positions[i];
                double y =1; // keep a small fixed offset from axis
                object txt = formsPlot1.Plot.Add.Text(labels[i], x, y);
                var tType = txt.GetType();
                var pFontName2 = tType.GetProperty("FontName");
                if (pFontName2 != null) SafeSetProperty(txt, pFontName2, chineseFontName);
                var pFontSize2 = tType.GetProperty("FontSize");
                if (pFontSize2 != null) SafeSetProperty(txt, pFontSize2, categoryFontSize);
                var pFontStyle2 = tType.GetProperty("FontStyle") ?? tType.GetProperty("Style");
                if (pFontStyle2 != null) SafeSetProperty(txt, pFontStyle2, System.Drawing.FontStyle.Bold);

                // center horizontally for category labels as well
                var hAlignProps2 = new[] { "HAlign", "HAlignment", "HorizontalAlignment", "Alignment", "Anchor" };
                foreach (var hn in hAlignProps2)
                {
                    var p = tType.GetProperty(hn);
                    if (p != null)
                    {
                        TrySetEnumOrString(p, txt, "Center");
                        break;
                    }
                }

                // 嘗試設定旋轉角度（向下旋轉35 度 -> 使用正值表示逆時針）
                double rotationDegrees =35.0;
                string[] rotationPropNames = new[] { "Rotation", "RotationDegrees", "Angle", "RotationDeg", "RotationAngle", "RotationDegree" };
                foreach (var propName in rotationPropNames)
                {
                    var p = tType.GetProperty(propName);
                    if (p != null)
                    {
                        SafeSetProperty(txt, p, rotationDegrees);
                        break;
                    }
                }
            }

            // add title as a text plottable so we can set font
            object titleObj = formsPlot1.Plot.Add.Text($"高雄市觀光景點到訪人數 ({year})", n /2.0 -0.5, maxY *1.1);
            var titleType = titleObj.GetType();
            var tfn = titleType.GetProperty("FontName");
            if (tfn != null)
                SafeSetProperty(titleObj, tfn, chineseFontName);
            var tfs = titleType.GetProperty("FontSize");
            if (tfs != null)
                SafeSetProperty(titleObj, tfs,14.0);

            formsPlot1.Refresh();
            labelStatus.Text = $"已載入 {year} 年資料（{jsonPath}）";
        }

        private string GetPreferredChineseFont()
        {
            try
            {
                var candidates = new[] { "Microsoft JhengHei", "Microsoft JhengHei UI", "Microsoft YaHei", "Noto Sans CJK TC", "PMingLiU", "SimSun", "Arial Unicode MS" };
                using (InstalledFontCollection ifc = new InstalledFontCollection())
                {
                    var names = ifc.Families.Select(f => f.Name).ToHashSet(StringComparer.OrdinalIgnoreCase);
                    foreach (var c in candidates)
                    {
                        if (names.Contains(c))
                            return c;
                    }

                    // fallback: try to find any family whose name contains common CJK substring
                    foreach (var name in names)
                    {
                        if (name.IndexOf("Microsoft", StringComparison.OrdinalIgnoreCase) >=0 || name.IndexOf("Jheng", StringComparison.OrdinalIgnoreCase) >=0 || name.IndexOf("YaHei", StringComparison.OrdinalIgnoreCase) >=0 || name.IndexOf("Noto", StringComparison.OrdinalIgnoreCase) >=0 || name.IndexOf("Ming", StringComparison.OrdinalIgnoreCase) >=0)
                            return name;
                    }
                }
            }
  
            catch
            {
                // ignore and fall through
            }
            return null;
        }

        private void SafeSetProperty(object target, PropertyInfo prop, object value)
        {
            if (target == null || prop == null) return;
            var targetType = prop.PropertyType;
            try
            {
                object converted = null;
                if (value == null)
                {
                    converted = null;
                }
                else if (targetType.IsEnum && value is string s)
                {
                    // try parsing string to enum
                    try
                    {
                        converted = Enum.Parse(targetType, s, true);
                    }
                    catch
                    {
                        converted = null;
                    }
                }
                else if (targetType == typeof(float) || targetType == typeof(Single))
                {
                    converted = Convert.ToSingle(value);
                }
                else if (targetType == typeof(double))
                {
                    converted = Convert.ToDouble(value);
                }
                else if (targetType == typeof(int))
                {
                    converted = Convert.ToInt32(value);
                }
                else if (targetType.IsEnum)
                {
                    converted = Enum.ToObject(targetType, value);
                }
                else
                {
                    converted = Convert.ChangeType(value, targetType);
                }

                prop.SetValue(target, converted);
            }
            catch
            {
                // ignore failures to set property
            }
        }

        private void TrySetEnumOrString(PropertyInfo p, object target, string value)
        {
            if (p == null || target == null) return;
            var t = p.PropertyType;
            try
            {
                if (t.IsEnum)
                {
                    // try parse enum by name
                    try
                    {
                        var e = Enum.Parse(t, value, true);
                        p.SetValue(target, e);
                        return;
                    }
                    catch { }
                }

                // fallback to string assignment if property accepts string
                if (t == typeof(string))
                {
                    p.SetValue(target, value);
                    return;
                }
            }
            catch
            {
                // ignore
            }
        }

        private void buttonSave_Click(object sender, EventArgs e)
        {
            using var dialog = new SaveFileDialog
            {
                Title = "儲存圖表",
                Filter = "PNG 圖檔 (*.png)|*.png",
                FileName = "chart.png",
                OverwritePrompt = true
            };

            if (dialog.ShowDialog(this) == DialogResult.OK)
            {
                formsPlot1.Plot.Save(dialog.FileName, formsPlot1.Width, formsPlot1.Height);
                labelStatus.Text = $"圖表已儲存至：{dialog.FileName}";
            }
        }
    }
}