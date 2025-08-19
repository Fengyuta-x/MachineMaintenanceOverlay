using CrystalDecisions.CrystalReports.Engine;
using CrystalDecisions.Shared;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace PrintReport
{
    internal static class Program
    {
        private static int Main(string[] args)
        {
            try
            {
                var opts = Options.Parse(args);
                if (!opts.IsValid(out string err))
                {
                    Console.Error.WriteLine(err);
                    Options.PrintUsage();
                    return 2;
                }

                if (!File.Exists(opts.ReportPath))
                {
                    Console.Error.WriteLine("Report file not found: " + opts.ReportPath);
                    return 3;
                }

                using (var report = new ReportDocument())
                {
                    report.Load(opts.ReportPath);

                    // If DB credentials were provided, apply to main report + subreports
                    if (!string.IsNullOrWhiteSpace(opts.DbUser))
                    {
                        ApplyDbLogin(report, opts.DbServer, opts.DbName, opts.DbUser, opts.DbPass);
                    }

                    // Apply parameters to main and subreports (if names match)
                    ApplyParameters(report, opts.Parameters);

                    if (!string.IsNullOrWhiteSpace(opts.ExportPdfPath))
                    {
                        ExportToPdf(report, opts.ExportPdfPath);
                        Console.WriteLine("Exported PDF: " + opts.ExportPdfPath);
                    }
                    else
                    {
                        if (!string.IsNullOrWhiteSpace(opts.PrinterName))
                            report.PrintOptions.PrinterName = opts.PrinterName;

                        report.PrintToPrinter(opts.Copies, false, 0, 0);
                        Console.WriteLine($"Sent to printer '{(string.IsNullOrWhiteSpace(opts.PrinterName) ? "<default>" : opts.PrinterName)}' (copies: {opts.Copies}).");
                    }
                }

                return 0;
            }
            catch (ParameterFieldCurrentValueException pex)
            {
                Console.Error.WriteLine("Parameter error: " + pex.Message);
                return 10;
            }
            catch (LoadSaveReportException lse)
            {
                Console.Error.WriteLine("Failed to load report: " + lse.Message);
                return 11;
            }
            catch (EngineException ee)
            {
                Console.Error.WriteLine("Crystal engine error: " + ee.Message);
                return 12;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine("Unexpected error: " + ex);
                return 1;
            }
        }

        private static void ApplyDbLogin(ReportDocument report, string server, string database, string user, string pass)
        {
            var ci = new ConnectionInfo
            {
                ServerName = server,
                DatabaseName = database,
                UserID = user,
                Password = pass,
                IntegratedSecurity = false
            };

            // Main report tables
            foreach (Table table in report.Database.Tables)
            {
                var logonInfo = table.LogOnInfo;
                logonInfo.ConnectionInfo = ci;
                table.ApplyLogOnInfo(logonInfo);
            }

            // Subreport tables
            foreach (CrystalDecisions.CrystalReports.Engine.Section sec in report.ReportDefinition.Sections)
            {
                foreach (ReportObject obj in sec.ReportObjects)
                {
                    if (obj.Kind == ReportObjectKind.SubreportObject)
                    {
                        var sro = (SubreportObject)obj;
                        using (var sub = report.OpenSubreport(sro.SubreportName))
                        {
                            foreach (Table table in sub.Database.Tables)
                            {
                                var logonInfo = table.LogOnInfo;
                                logonInfo.ConnectionInfo = ci;
                                table.ApplyLogOnInfo(logonInfo);
                            }
                        }
                    }
                }
            }
        }

        private static void ApplyParameters(ReportDocument report, Dictionary<string, string> parameters)
        {
            foreach (var kv in parameters)
            {
                // Main report param
                if (HasParameter(report, kv.Key))
                    report.SetParameterValue(kv.Key, kv.Value);

                // Subreports param with same name
                foreach (ReportDocument sub in report.Subreports)
                {
                    if (HasParameter(sub, kv.Key))
                        sub.SetParameterValue(kv.Key, kv.Value);
                }
            }
        }

        private static bool HasParameter(ReportDocument doc, string paramName)
        {
            return doc.DataDefinition.ParameterFields
                      .Cast<ParameterFieldDefinition>()
                      .Any(p => string.Equals(p.Name, paramName, StringComparison.Ordinal));
        }

        private static void ExportToPdf(ReportDocument report, string path)
        {
            var dir = Path.GetDirectoryName(path);
            if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            report.ExportToDisk(ExportFormatType.PortableDocFormat, path);
        }
    }
}