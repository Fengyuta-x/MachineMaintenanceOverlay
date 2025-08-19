using System;
using System.Collections.Generic;
using System.IO;

namespace PrintReport
{
    internal sealed class Options
    {
        public string ReportPath { get; private set; }
        public string PrinterName { get; private set; }
        public int Copies { get; private set; } = 1;
        public string ExportPdfPath { get; private set; }

        // Optional DB
        public string DbServer { get; private set; }

        public string DbName { get; private set; }
        public string DbUser { get; private set; }
        public string DbPass { get; private set; }

        // Params: Name -> Value
        public Dictionary<string, string> Parameters { get; } = new Dictionary<string, string>(StringComparer.Ordinal);

        public static Options Parse(string[] args)
        {
            var o = new Options();

            string readValue(string key, int i)
            {
                if (i + 1 >= args.Length) throw new ArgumentException($"Missing value for {key}");
                return args[i + 1];
            }

            for (int i = 0; i < args.Length; i++)
            {
                var a = args[i];
                switch (a.ToLowerInvariant())
                {
                    case "--rpt":
                        o.ReportPath = readValue(a, i); i++; break;
                    case "--printer":
                        o.PrinterName = readValue(a, i); i++; break;
                    case "--copies":
                        if (!int.TryParse(readValue(a, i), out int c) || c < 1) throw new ArgumentException("--copies must be >= 1");
                        o.Copies = c; i++; break;
                    case "--exportpdf":
                        o.ExportPdfPath = readValue(a, i); i++; break;

                    case "--dbserver":
                        o.DbServer = readValue(a, i); i++; break;
                    case "--dbname":
                        o.DbName = readValue(a, i); i++; break;
                    case "--dbuser":
                        o.DbUser = readValue(a, i); i++; break;
                    case "--dbpass":
                        o.DbPass = readValue(a, i); i++; break;

                    case "--param":
                        {
                            var p = readValue(a, i); i++;
                            // Expect "Name=Value" (Name may contain spaces; whole pair should be quoted in the shell)
                            int eq = p.IndexOf('=');
                            if (eq <= 0) throw new ArgumentException("Invalid --param. Use --param \"Name=Value\"");
                            var name = p.Substring(0, eq).Trim();
                            var value = p.Substring(eq + 1);
                            o.Parameters[name] = value;
                        }
                        break;

                    case "--help":
                    case "-h":
                    case "/?":
                        PrintUsage();
                        Environment.Exit(0);
                        break;

                    default:
                        // allow passing the .rpt as bare path
                        if (File.Exists(a))
                            o.ReportPath = a;
                        else
                            throw new ArgumentException("Unknown argument: " + a);
                        break;
                }
            }

            return o;
        }

        public bool IsValid(out string error)
        {
            if (string.IsNullOrWhiteSpace(ReportPath))
            {
                error = "Missing --rpt <path-to-report.rpt>";
                return false;
            }
            error = null;
            return true;
        }

        public static void PrintUsage()
        {
            Console.WriteLine(
@"Usage:
  PrintReport.exe --rpt ""C:\path\MachineProtocol.rpt""
                  [--printer ""Printer Name""] [--copies 1]
                  [--param ""Nr Ewidencyjny=ABC123""]
                  [--param ""Nazwa=XYZ""]
                  [--param ""Rok produkcji=2020""]
                  [--param ""Typ maszyny=Koparka""]
                  [--exportPdf ""C:\out\protocol.pdf""]
                  [--dbServer SERVER] [--dbName DB] [--dbUser USER] [--dbPass PASS]

Notes:
- Quote any parameter that contains spaces:  --param ""Nr Ewidencyjny=123""
- Omit --printer to use the default system printer.
- Use --exportPdf instead of printing.");
        }
    }
}