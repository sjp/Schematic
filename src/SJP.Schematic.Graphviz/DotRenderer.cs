using System;
using System.Diagnostics;
using System.IO;

namespace SJP.Schematic.Graphviz
{
    public class DotRenderer
    {
        public DotRenderer(string dotExecutablePath)
        {
            if (string.IsNullOrWhiteSpace(dotExecutablePath))
                throw new ArgumentNullException(nameof(dotExecutablePath));
            if (!File.Exists(dotExecutablePath))
                throw new FileNotFoundException($"Expected to find a file at: '{ dotExecutablePath }', but was not found.", dotExecutablePath);

            _dotPath = dotExecutablePath;
        }

        public string RenderToSvg(string dot)
        {
            if (string.IsNullOrWhiteSpace(dot))
                throw new ArgumentNullException(nameof(dot));

            var tmpInputFilePath = Path.GetTempFileName();
            var tmpOutputFilePath = Path.GetTempFileName();

            File.Delete(tmpInputFilePath);
            File.Delete(tmpOutputFilePath);

            try
            {
                File.WriteAllText(tmpInputFilePath, dot);

                var startInfo = new ProcessStartInfo
                {
                    FileName = _dotPath,
                    Arguments = $"-Tsvg \"{ tmpInputFilePath }\" -o \"{ tmpOutputFilePath }\"",
                    RedirectStandardError = true
                };
                using (var process = new Process { StartInfo = startInfo })
                {
                    process.Start();
                    process.WaitForExit();

                    if (process.ExitCode != ExitSuccess)
                    {
                        var stdErr = process.StandardError.ReadToEnd();
                        throw new GraphvizException(process.ExitCode, stdErr);
                    }
                }

                return File.ReadAllText(tmpOutputFilePath);
            }
            finally
            {
                if (File.Exists(tmpInputFilePath))
                    File.Delete(tmpInputFilePath);
                if (File.Exists(tmpOutputFilePath))
                    File.Delete(tmpOutputFilePath);
            }
        }

        private readonly string _dotPath;

        private const int ExitSuccess = 0;
    }
}
