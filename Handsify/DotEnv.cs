namespace SALLY_API
{
    using System;
    using System.IO;

    public static class DotEnv
    {
        public static void Load(string filePath)
        {
            if (!File.Exists(filePath))
            {
                //Console.WriteLine(("missing file");
                return;
            }

            foreach (var line in File.ReadAllLines(filePath))
            {
                var parts = line.Split(
                    '~',
                    StringSplitOptions.RemoveEmptyEntries);

                if (parts.Length != 2)
                    continue;
                //Console.WriteLine((parts[0] + "=" + parts[1]);
                Environment.SetEnvironmentVariable(parts[0],parts[1].Replace("\"", ""));
            }
        }
    }
}
