using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Media;

namespace TDL.Configurator.App.Pages;

public partial class DocumentationPage : System.Windows.Controls.UserControl
{
    private const string UiTitle = "TDL Configurator";

    private sealed class DocItem
    {
        public string Id { get; init; } = "";
        public string Title { get; init; } = "";
        public string FileName { get; init; } = "";
        public int Order { get; init; }
        public string FullPath { get; init; } = "";

        public override string ToString() => Title;
    }

    private sealed class ManifestItem
    {
        public string? id { get; set; }
        public string? title { get; set; }
        public string? file { get; set; }
        public int order { get; set; }
    }

    private readonly ObservableCollection<DocItem> _docs = new();
    private ICollectionView? _docsView;

    private string? _docsRoot;     // ...\docs
    private string? _docsContent;  // ...\docs or ...\docs\TDL_Docs
    private string? _currentMd;    // raw markdown for CopyMarkdown

    public DocumentationPage()
    {
        InitializeComponent();

        // Важно: грузим после построения визуального дерева (меньше "дерганий" при старте в оконном режиме)
        Loaded += (_, _) => LoadDocs();
    }

    // ---------------- UI events ----------------

    private void Refresh_Click(object sender, RoutedEventArgs e) => LoadDocs();

    private void OpenDocsFolder_Click(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(_docsRoot) || !Directory.Exists(_docsRoot))
        {
            System.Windows.MessageBox.Show(
                "Папка docs не найдена рядом с приложением.",
                UiTitle,
                MessageBoxButton.OK,
                MessageBoxImage.Warning);
            return;
        }

        Process.Start(new ProcessStartInfo("explorer.exe", $"\"{_docsRoot}\"") { UseShellExecute = true });
    }

    private void OpenSelectedFile_Click(object sender, RoutedEventArgs e)
    {
        if (DocsList.SelectedItem is not DocItem doc)
            return;

        if (!File.Exists(doc.FullPath))
        {
            System.Windows.MessageBox.Show(
                $"Файл не найден:\n{doc.FullPath}",
                UiTitle,
                MessageBoxButton.OK,
                MessageBoxImage.Warning);
            return;
        }

        Process.Start(new ProcessStartInfo(doc.FullPath) { UseShellExecute = true });
    }

    private void CopyMarkdown_Click(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(_currentMd))
            return;

        System.Windows.Clipboard.SetText(_currentMd);
        DocsStatusText.Text = $"Скопировано в буфер ({DateTime.Now:HH:mm:ss})";
    }

    private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        if (_docsView == null)
            return;

        _docsView.Refresh();

        // Если выбранный документ отфильтровался — выбираем первый доступный
        if (_docsView.IsEmpty)
        {
            DocsList.SelectedItem = null;
            DocTitleText.Text = "";
            _currentMd = null;
            DocViewer.Document = BuildInfoDocument("Ничего не найдено по фильтру.");
            DocsStatusText.Text = "Фильтр: 0";
            return;
        }

        if (DocsList.SelectedItem == null)
        {
            DocsList.SelectedIndex = 0;
        }
    }

    private void DocsList_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (DocsList.SelectedItem is DocItem doc)
            ShowDoc(doc);
    }

    // ---------------- Loading ----------------

    private void LoadDocs()
    {
        _docs.Clear();
        _currentMd = null;

        DocTitleText.Text = "";
        DocViewer.Document = BuildInfoDocument("Выбери документ слева.");

        var baseDir = AppDomain.CurrentDomain.BaseDirectory;
        var docsRoot = Path.Combine(baseDir, "docs");

        docsRoot = FindExistingDocsFolder(docsRoot) ?? docsRoot;
        _docsRoot = docsRoot;

        if (!Directory.Exists(docsRoot))
        {
            DocsStatusText.Text = "docs не найдена (положи docs\\ рядом с .exe).";
            return;
        }

        // Поддержка раскладок:
        // 1) docs\*.md + docs\docs_manifest.json
        // 2) docs\TDL_Docs\*.md + docs\TDL_Docs\docs_manifest.json
        var contentDir = Directory.Exists(Path.Combine(docsRoot, "TDL_Docs"))
            ? Path.Combine(docsRoot, "TDL_Docs")
            : docsRoot;

        _docsContent = contentDir;

        var manifestPath = Path.Combine(contentDir, "docs_manifest.json");

        List<DocItem> items = File.Exists(manifestPath)
            ? LoadFromManifest(contentDir, manifestPath)
            : LoadFromFolder(contentDir);

        foreach (var it in items)
            _docs.Add(it);

        _docsView = CollectionViewSource.GetDefaultView(_docs);
        _docsView.Filter = FilterDoc;

        DocsList.ItemsSource = _docsView;

        DocsStatusText.Text = $"Найдено документов: {_docs.Count}";

        if (_docs.Count > 0)
            DocsList.SelectedIndex = 0;
        else
            DocViewer.Document = BuildInfoDocument("Документы не найдены (нет .md файлов).");
    }

    private bool FilterDoc(object obj)
    {
        if (obj is not DocItem d)
            return false;

        var q = (SearchBox.Text ?? "").Trim();
        if (q.Length == 0)
            return true;

        return d.Title.Contains(q, StringComparison.OrdinalIgnoreCase)
               || d.FileName.Contains(q, StringComparison.OrdinalIgnoreCase);
    }

    private static string? FindExistingDocsFolder(string initial)
    {
        // initial = <base>\docs
        // Если запущено из bin\Debug\net8.0-windows — ищем docs выше по дереву
        try
        {
            var start = Path.GetDirectoryName(initial) ?? "";
            var cur = new DirectoryInfo(start);
            for (var i = 0; i < 6 && cur != null; i++)
            {
                var candidate = Path.Combine(cur.FullName, "docs");
                if (Directory.Exists(candidate))
                    return candidate;

                cur = cur.Parent;
            }
        }
        catch
        {
            // ignore
        }

        return null;
    }

    private static List<DocItem> LoadFromManifest(string contentDir, string manifestPath)
    {
        try
        {
            var json = File.ReadAllText(manifestPath);
            var list = JsonSerializer.Deserialize<List<ManifestItem>>(json) ?? new List<ManifestItem>();

            return list
                .Where(x => !string.IsNullOrWhiteSpace(x.file))
                .Select(x =>
                {
                    var fileName = x.file!.Trim();
                    var fullPath = Path.Combine(contentDir, fileName);
                    return new DocItem
                    {
                        Id = x.id?.Trim() ?? fileName,
                        Title = NormalizeWs(x.title?.Trim() ?? Path.GetFileNameWithoutExtension(fileName)),
                        FileName = fileName,
                        Order = x.order,
                        FullPath = fullPath
                    };
                })
                .OrderBy(x => x.Order)
                .ThenBy(x => x.Title, StringComparer.OrdinalIgnoreCase)
                .ToList();
        }
        catch
        {
            return LoadFromFolder(contentDir);
        }
    }

    private static List<DocItem> LoadFromFolder(string contentDir)
    {
        var files = Directory.Exists(contentDir)
            ? Directory.GetFiles(contentDir, "*.md", SearchOption.TopDirectoryOnly)
            : Array.Empty<string>();

        return files
            .Select((p, idx) => new DocItem
            {
                Id = Path.GetFileNameWithoutExtension(p),
                Title = NormalizeWs(Path.GetFileNameWithoutExtension(p)),
                FileName = Path.GetFileName(p),
                Order = idx + 1,
                FullPath = p
            })
            .OrderBy(x => x.Order)
            .ThenBy(x => x.Title, StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    // ---------------- Rendering ----------------

    private void ShowDoc(DocItem doc)
    {
        if (!File.Exists(doc.FullPath))
        {
            DocTitleText.Text = doc.Title;
            _currentMd = null;
            DocViewer.Document = BuildInfoDocument($"Файл не найден:\n{doc.FullPath}");
            DocsStatusText.Text = $"Не найдено: {doc.FileName}";
            return;
        }

        string md;
        try
        {
            // StreamReader с BOM detection (UTF-8/UTF-16 и т.п.)
            md = File.ReadAllText(doc.FullPath);
        }
        catch (Exception ex)
        {
            DocTitleText.Text = doc.Title;
            _currentMd = null;
            DocViewer.Document = BuildInfoDocument("Не удалось прочитать файл:\n" + ex.Message);
            DocsStatusText.Text = "Ошибка чтения";
            return;
        }

        DocTitleText.Text = doc.Title;
        _currentMd = md;

        DocViewer.Document = MarkdownToFlowDocument(md);
        DocsStatusText.Text = $"Открыто: {doc.FileName} ({DateTime.Now:HH:mm:ss})";
    }

    private static FlowDocument BuildInfoDocument(string text)
    {
        var doc = new FlowDocument
        {
            PagePadding = new Thickness(8),
            FontSize = 13,
            TextAlignment = TextAlignment.Left
        };

        doc.Blocks.Add(new Paragraph(new Run(text))
        {
            Margin = new Thickness(0)
            // Opacity = 0.85 // Удалено, так как Paragraph не поддерживает Opacity
        });

        return doc;
    }

    // ---------------- Markdown -> FlowDocument ----------------

    private static string NormalizeWs(string s)
    {
        if (string.IsNullOrWhiteSpace(s)) return "";
        // 2+ пробела/табов -> 1 пробел (для заголовков и обычного текста, НЕ для code blocks)
        return Regex.Replace(s, @"[ \t]{2,}", " ").Trim();
    }

    private static FlowDocument MarkdownToFlowDocument(string md)
    {
        var doc = new FlowDocument
        {
            PagePadding = new Thickness(8),
            FontSize = 13,
            TextAlignment = TextAlignment.Left
        };

        var lines = (md ?? "").Replace("\r\n", "\n").Split('\n');

        var inCode = false;
        var codeBuf = new List<string>();
        var paraBuf = new List<string>();

        void FlushParagraph()
        {
            if (paraBuf.Count == 0) return;

            var text = NormalizeWs(string.Join(" ", paraBuf.Select(x => x.Trim()).Where(x => x.Length > 0)));
            paraBuf.Clear();

            if (text.Length == 0) return;

            var p = new Paragraph { Margin = new Thickness(0, 0, 0, 10) };
            AppendInlines(p.Inlines, text);
            doc.Blocks.Add(p);
        }

        void FlushCode()
        {
            if (codeBuf.Count == 0) return;

            var codeText = string.Join("\n", codeBuf);
            codeBuf.Clear();

            var p = new Paragraph(new Run(codeText))
            {
                Margin = new Thickness(0, 0, 0, 10),
                FontFamily = new System.Windows.Media.FontFamily("Consolas"),
                Background = new SolidColorBrush(System.Windows.Media.Color.FromRgb(245, 245, 245))
            };

            // Padding для Paragraph напрямую недоступен — используем Block UI через Border
            var container = new BlockUIContainer(
                new Border
                {
                    Background = new SolidColorBrush(System.Windows.Media.Color.FromRgb(245, 245, 245)),
                    Padding = new Thickness(8),
                    Child = new TextBlock
                    {
                        FontFamily = new System.Windows.Media.FontFamily("Consolas"),
                        Text = codeText,
                        TextWrapping = TextWrapping.NoWrap
                    }
                })
            { Margin = new Thickness(0, 0, 0, 10) };

            doc.Blocks.Add(container);
        }

        foreach (var raw in lines)
        {
            var line = raw ?? "";
            var t = line.TrimEnd();

            // fenced code
            if (t.StartsWith("```", StringComparison.Ordinal))
            {
                if (inCode)
                {
                    inCode = false;
                    FlushCode();
                }
                else
                {
                    FlushParagraph();
                    inCode = true;
                }

                continue;
            }

            if (inCode)
            {
                codeBuf.Add(line); // сохраняем как есть, без NormalizeWs
                continue;
            }

            // blank line => paragraph break
            if (t.Trim().Length == 0)
            {
                FlushParagraph();
                continue;
            }

            // headings
            if (t.StartsWith("#", StringComparison.Ordinal))
            {
                FlushParagraph();

                var level = 0;
                while (level < t.Length && t[level] == '#') level++;

                var title = NormalizeWs(t.Substring(level));

                var p = new Paragraph(new Run(title))
                {
                    FontWeight = FontWeights.SemiBold,
                    Margin = new Thickness(0, level <= 2 ? 14 : 10, 0, 8)
                };

                p.FontSize = level switch
                {
                    1 => 20,
                    2 => 17,
                    3 => 15,
                    _ => 14
                };

                doc.Blocks.Add(p);
                continue;
            }

            // bullet list
            if (IsBullet(t, out var bulletText))
            {
                FlushParagraph();

                var p = new Paragraph { Margin = new Thickness(18, 0, 0, 6) };
                p.Inlines.Add(new Run("• "));
                AppendInlines(p.Inlines, NormalizeWs(bulletText));
                doc.Blocks.Add(p);
                continue;
            }

            // numbered list (simple)
            if (IsNumbered(t, out var numPrefix, out var numText))
            {
                FlushParagraph();

                var p = new Paragraph { Margin = new Thickness(18, 0, 0, 6) };
                p.Inlines.Add(new Run(numPrefix + " "));
                AppendInlines(p.Inlines, NormalizeWs(numText));
                doc.Blocks.Add(p);
                continue;
            }

            // normal line => paragraph buffer
            paraBuf.Add(t.Trim());
        }

        FlushParagraph();
        if (inCode) FlushCode();

        return doc;
    }

    private static bool IsBullet(string line, out string text)
    {
        var t = line.TrimStart();
        if (t.StartsWith("- ") || t.StartsWith("* ") || t.StartsWith("+ "))
        {
            text = t.Substring(2).Trim();
            return true;
        }

        text = "";
        return false;
    }

    private static bool IsNumbered(string line, out string prefix, out string text)
    {
        var t = line.TrimStart();
        prefix = "";
        text = "";

        var i = 0;
        while (i < t.Length && char.IsDigit(t[i])) i++;
        if (i == 0 || i + 1 >= t.Length) return false;

        if (t[i] == '.' || t[i] == ')')
        {
            var next = i + 1;
            if (next < t.Length && t[next] == ' ')
            {
                prefix = t.Substring(0, i + 1);
                text = t.Substring(next + 1).Trim();
                return true;
            }
        }

        return false;
    }

    // inline: **bold**, *italic*, `code`
    private static void AppendInlines(InlineCollection inlines, string s)
    {
        if (string.IsNullOrEmpty(s))
            return;

        var i = 0;
        while (i < s.Length)
        {
            // inline code
            if (s[i] == '`')
            {
                var j = s.IndexOf('`', i + 1);
                if (j > i)
                {
                    var code = s.Substring(i + 1, j - i - 1);
                    var run = new Run(code)
                    {
                        FontFamily = new System.Windows.Media.FontFamily("Consolas"),
                        Background = new SolidColorBrush(System.Windows.Media.Color.FromRgb(245, 245, 245))
                    };
                    inlines.Add(run);
                    i = j + 1;
                    continue;
                }
            }

            // bold
            if (i + 1 < s.Length && s[i] == '*' && s[i + 1] == '*')
            {
                var j = s.IndexOf("**", i + 2, StringComparison.Ordinal);
                if (j > i)
                {
                    var boldText = s.Substring(i + 2, j - i - 2);
                    inlines.Add(new Bold(new Run(boldText)));
                    i = j + 2;
                    continue;
                }
            }

            // italic (упрощенно)
            if (s[i] == '*')
            {
                var j = s.IndexOf('*', i + 1);
                if (j > i)
                {
                    var italicText = s.Substring(i + 1, j - i - 1);
                    inlines.Add(new Italic(new Run(italicText)));
                    i = j + 1;
                    continue;
                }
            }

            // plain run until next special char
            var nextSpecial = NextSpecialIndex(s, i);
            var chunk = nextSpecial >= 0 ? s.Substring(i, nextSpecial - i) : s.Substring(i);
            inlines.Add(new Run(chunk));
            i = nextSpecial >= 0 ? nextSpecial : s.Length;
        }
    }

    private static int NextSpecialIndex(string s, int start)
    {
        var idx1 = s.IndexOf('`', start);
        var idx2 = s.IndexOf('*', start);

        if (idx1 < 0) return idx2;
        if (idx2 < 0) return idx1;
        return Math.Min(idx1, idx2);
    }
}
