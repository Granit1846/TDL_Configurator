using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace TDL.Configurator.Core
{
    public sealed class IniEntry
    {
        public string Key { get; set; } = "";
        public string Value { get; set; } = "";
    }

    public static class IniFile
    {
        public static List<IniEntry> ReadSection(string filePath, string sectionName)
        {
            if (!File.Exists(filePath))
                return new List<IniEntry>();

            var lines = File.ReadAllLines(filePath, Encoding.UTF8);
            var result = new List<IniEntry>();

            bool inSection = false;

            foreach (var raw in lines)
            {
                var line = raw.Trim();

                if (IsSectionHeader(line, out var name))
                {
                    inSection = name.Equals(sectionName, StringComparison.OrdinalIgnoreCase);
                    continue;
                }

                if (!inSection)
                    continue;

                if (IsSectionHeader(line, out _))
                    break;

                if (TryParseKeyValue(line, out var key, out var value))
                {
                    result.Add(new IniEntry { Key = key, Value = value });
                }
            }

            return result;
        }

        public static void WriteSection(string filePath, string sectionName, IEnumerable<IniEntry> entries)
        {
            var safeEntries = entries
                .Where(e => !string.IsNullOrWhiteSpace(e.Key))
                .GroupBy(e => e.Key.Trim(), StringComparer.OrdinalIgnoreCase)
                .Select(g => g.Last())
                .Select(e => new IniEntry { Key = e.Key.Trim(), Value = e.Value?.Trim() ?? "" })
                .ToList();

            var newSectionLines = new List<string> { $"[{sectionName}]" };
            newSectionLines.AddRange(safeEntries.Select(e => $"{e.Key}={e.Value}"));

            List<string> lines;
            if (File.Exists(filePath))
                lines = File.ReadAllLines(filePath, Encoding.UTF8).ToList();
            else
                lines = new List<string>();

            int start = -1;
            int end = -1;

            for (int i = 0; i < lines.Count; i++)
            {
                var t = lines[i].Trim();
                if (IsSectionHeader(t, out var name) &&
                    name.Equals(sectionName, StringComparison.OrdinalIgnoreCase))
                {
                    start = i;
                    end = lines.Count;
                    for (int j = i + 1; j < lines.Count; j++)
                    {
                        if (IsSectionHeader(lines[j].Trim(), out _))
                        {
                            end = j;
                            break;
                        }
                    }
                    break;
                }
            }

            if (start >= 0)
            {
                // Replace existing section
                lines.RemoveRange(start, end - start);
                lines.InsertRange(start, newSectionLines);
            }
            else
            {
                // Append new section
                if (lines.Count > 0 && !string.IsNullOrWhiteSpace(lines.Last()))
                    lines.Add("");

                lines.AddRange(newSectionLines);
            }

            var dir = Path.GetDirectoryName(filePath);
            if (!string.IsNullOrWhiteSpace(dir))
                Directory.CreateDirectory(dir);

            File.WriteAllLines(filePath, lines, new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
        }

        private static bool IsSectionHeader(string line, out string name)
        {
            name = "";
            if (line.Length >= 3 && line.StartsWith("[") && line.EndsWith("]"))
            {
                name = line.Substring(1, line.Length - 2).Trim();
                return name.Length > 0;
            }
            return false;
        }

        private static bool TryParseKeyValue(string line, out string key, out string value)
        {
            key = "";
            value = "";

            if (string.IsNullOrWhiteSpace(line))
                return false;

            // Comments
            if (line.StartsWith(";") || line.StartsWith("#"))
                return false;

            int eq = line.IndexOf('=');
            if (eq <= 0)
                return false;

            key = line.Substring(0, eq).Trim();
            value = line.Substring(eq + 1).Trim();

            return key.Length > 0;
        }
    }
}
