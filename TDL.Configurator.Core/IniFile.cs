using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace TDL.Configurator.Core;

public static class IniFile
{
    private static readonly Encoding Utf8NoBom = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false);

    public static List<string> LoadLines(string path)
        => File.Exists(path)
            ? File.ReadAllLines(path, Utf8NoBom).ToList()
            : new List<string>();

    public static void SaveLines(string path, List<string> lines)
        => File.WriteAllLines(path, lines, Utf8NoBom);

    public static Dictionary<string, string> ReadSection(string path, string sectionName)
    {
        var result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        if (!File.Exists(path))
            return result;

        var lines = File.ReadAllLines(path, Utf8NoBom);
        var inSection = false;

        foreach (var raw in lines)
        {
            var line = raw.Trim();
            if (line.Length == 0) continue;
            if (line.StartsWith(";") || line.StartsWith("#")) continue;

            if (line.StartsWith("[") && line.EndsWith("]"))
            {
                inSection = string.Equals(line.Trim('[', ']'), sectionName, StringComparison.OrdinalIgnoreCase);
                continue;
            }

            if (!inSection) continue;

            var eq = line.IndexOf('=');
            if (eq <= 0) continue;

            var key = line[..eq].Trim();
            var val = line[(eq + 1)..].Trim();
            if (key.Length == 0) continue;

            result[key] = val;
        }

        return result;
    }

    public static void UpsertSection(List<string> lines, string sectionName, IDictionary<string, string> values)
    {
        var header = $"[{sectionName}]";
        var start = lines.FindIndex(l => string.Equals(l.Trim(), header, StringComparison.OrdinalIgnoreCase));

        var newSection = new List<string> { header };
        foreach (var kv in values)
            newSection.Add($"{kv.Key}={kv.Value}");

        if (start < 0)
        {
            if (lines.Count > 0 && lines[^1].Trim().Length != 0)
                lines.Add("");
            lines.AddRange(newSection);
            return;
        }

        var end = start + 1;
        while (end < lines.Count)
        {
            var t = lines[end].Trim();
            if (t.StartsWith("[") && t.EndsWith("]"))
                break;
            end++;
        }

        lines.RemoveRange(start, end - start);
        lines.InsertRange(start, newSection);
    }
}
