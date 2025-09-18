using System;
using System.IO;
using System.Text;
using System.Collections.Generic;
using System.Security;
using Xunit;

namespace ApiTests.Setup
{
    public static class ReportManager
    {
        private static readonly object _lock = new object();
        private static string? _reportsDir;
        private static readonly List<(string Name, string Status, string Info)> _results = new List<(string, string, string)>();

        static ReportManager()
        {
            try
            {
                var repoRoot = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", ".."));
                _reportsDir = Path.Combine(repoRoot, "reports");
                Directory.CreateDirectory(_reportsDir);
            
                AppDomain.CurrentDomain.ProcessExit += (s, e) => Shutdown();
                System.Runtime.Loader.AssemblyLoadContext.Default.Unloading += ctx => Shutdown();
            }
            catch(Exception ex) { throw new Exception($"Could not create report: {ex.Message}");}
        }

        public static void AddTest(string name, string status, string info)
        {
            lock (_lock)
            {
                _results.Add((name, status ?? string.Empty, info ?? string.Empty));
            }
        }

        public static void Shutdown()
        {
            lock (_lock)
            {
                try
                {
                    if (string.IsNullOrEmpty(_reportsDir)) return;

                    var timestamp = DateTime.UtcNow.ToString("yyyyMMdd_HHmmss");
                    var htmlPath = Path.Combine(_reportsDir, $"TestReport_{timestamp}.html");
                    var xmlPath = Path.Combine(_reportsDir, $"TestResults_{timestamp}.xml");

                    // Build HTML
                    var sbHtml = new StringBuilder();
                    sbHtml.AppendLine("<html><head><meta charset=\"utf-8\"/><title>Test Report</title>");
                    sbHtml.AppendLine("<style>body{font-family:Segoe UI,Arial;}table{border-collapse:collapse;width:100%}td,th{border:1px solid #ddd;padding:8px}th{background:#f4f4f4}</style>");
                    sbHtml.AppendLine("</head><body>");
                    sbHtml.AppendLine($"<h2>Test Report - {timestamp}</h2>");
                    sbHtml.AppendLine("<table><tr><th>Test</th><th>Status</th><th>Info</th></tr>");

                    foreach (var r in _results)
                    {
                        var escName = SecurityElement.Escape(r.Name);
                        var escInfo = SecurityElement.Escape(r.Info);
                        var color = (r.Status != null && r.Status.StartsWith("Failed", StringComparison.OrdinalIgnoreCase)) ? "#fdd" : "#dfd";
                        sbHtml.AppendLine($"<tr style=\"background:{color}\"><td>{escName}</td><td>{SecurityElement.Escape(r.Status)}</td><td><pre>{escInfo}</pre></td></tr>");
                    }

                    sbHtml.AppendLine("</table></body></html>");
                    File.WriteAllText(htmlPath, sbHtml.ToString(), Encoding.UTF8);

                    // Build XML
                    var total = _results.Count;
                    var failures = 0;
                    foreach (var r in _results) if (r.Status != null && r.Status.StartsWith("Failed", StringComparison.OrdinalIgnoreCase)) failures++;

                    var sb = new StringBuilder();
                    sb.AppendLine("<?xml version=\"1.0\" encoding=\"utf-8\"?>");
                    sb.AppendLine("<assemblies>");
                    sb.AppendLine($"  <assembly name=\"ApiTests\" test-framework=\"xUnit.net\">\n");
                    sb.AppendLine($"    <collection name=\"ApiTests Collection\" total=\"{total}\" passed=\"{Math.Max(0, total - failures)}\" failed=\"{failures}\">\n");

                    foreach (var r in _results)
                    {
                        var result = (r.Status != null && r.Status.StartsWith("Failed", StringComparison.OrdinalIgnoreCase)) ? "Fail" : "Pass";
                        sb.AppendLine($"      <test name=\"{SecurityElement.Escape(r.Name)}\" result=\"{result}\">\n");
                        if (result == "Fail")
                        {
                            sb.AppendLine($"        <failure exception-type=\"AssertionFailure\"><message>{SecurityElement.Escape(r.Status)}</message><stack-trace><![CDATA[{r.Info}]]></stack-trace></failure>");
                        }
                        sb.AppendLine("      </test>");
                    }

                    sb.AppendLine("    </collection>");
                    sb.AppendLine("  </assembly>");
                    sb.AppendLine("</assemblies>");
                    File.WriteAllText(xmlPath, sb.ToString(), Encoding.UTF8);
                }
                catch(Exception ex) { throw new Exception($"Could not create report: {ex.Message}");}
            }
        }
    }
}
