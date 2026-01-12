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
using System.Windows.Input;
using System.Windows.Navigation;

// В решении включен UseWindowsForms, из-за чего появляются неоднозначности типов
// (System.Drawing.Brush vs System.Windows.Media.Brush, System.Windows.Forms.Application vs System.Windows.Application).
// Дальше используем алиасы, чтобы не ловить CS0104.
using WpfApplication = System.Windows.Application;
using WpfBrush = System.Windows.Media.Brush;
using WpfBrushes = System.Windows.Media.Brushes;

namespace TDL.Configurator.App.Pages;

public partial class DocumentationPage : System.Windows.Controls.UserControl
{
    private static string S(string key, string fallback = "")
    {
        var v = WpfApplication.Current?.TryFindResource(key);
        var s = v?.ToString();
        return string.IsNullOrWhiteSpace(s)
            ? (string.IsNullOrWhiteSpace(fallback) ? key : fallback)
            : s;
    }

    private static string SF(string key, string fallbackFormat, params object[] args)
    {
        var fmt = S(key, fallbackFormat);
        try
        {
            return string.Format(fmt, args);
        }
        catch
        {
            return fmt;
        }
    }

    private static string UiTitle => S("STR_App_Title", "TDL Configurator");

    private static string LocalizeDocTitle(string id, string fallback)
    {
        if (string.IsNullOrWhiteSpace(id))
            return fallback;

        var key = "STR_DocTitle_" + id.Trim();
        var res = WpfApplication.Current?.TryFindResource(key)?.ToString();
        return string.IsNullOrWhiteSpace(res) ? fallback : res;
    }


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


    private string _searchQuery = "";
    private string _searchQueryLower = "";
    private readonly Dictionary<string, string> _docTextCache = new(StringComparer.OrdinalIgnoreCase);
    private string? _docsRoot;     // ...\docs
    private string? _docsContent;  // ...\docs or ...\docs\TDL_Docs
    private string? _currentMd;    // raw markdown for CopyMarkdown

    public DocumentationPage()
    {
        InitializeComponent();

        // Включаем открытие ссылок (Hyperlink) ...
        // FlowDocumentScrollViewer сам по себе не открывает ссылки — ...
        DocViewer.AddHandler(Hyperlink.RequestNavigateEvent, new RequestNavigateEventHandler(OnRequestNavigate));

        // Важно: грузим после построения визуального дерева (меньше "дерганий" при старте в оконном режиме)
        Loaded += (_, _) => LoadDocs();
    }

    private static void OnRequestNavigate(object sender, RequestNavigateEventArgs e)
    {
        try
        {
            var target = e.Uri?.OriginalString;
            if (!string.IsNullOrWhiteSpace(target))
            {
                Process.Start(new ProcessStartInfo(target) { UseShellExecute = true });
            }

            e.Handled = true;
        }
        catch
        {
            // ignore
        }
    }

    // ---------------- UI events ----------------

    private void Refresh_Click(object sender, RoutedEventArgs e) => LoadDocs();

    private void OpenDocsFolder_Click(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(_docsRoot) || !Directory.Exists(_docsRoot))
        {
            System.Windows.MessageBox.Show(
                S("STR_Info_Msg_DocsFolderNotFound", "Docs folder not found next to the app."),
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
                SF("STR_Info_Msg_FileNotFound_Fmt", "File not found:{0}", "\n" + doc.FullPath),
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
        DocsStatusText.Text = SF("STR_Info_Status_Copied_Fmt", "Copied to clipboard ({0})", DateTime.Now.ToString("HH:mm:ss"));
    }


    private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        try
        {
            _searchQueryLower = _searchQuery.ToLowerInvariant();

            // Поиск работает по ТЕКСТУ в документах:
            // 1) фильтрует список слева по содержимому markdown-файлов,
            // 2) подсвечивает совпадения в открытом документе справа.
            _docsView?.Refresh();

            if (DocViewer.Document is FlowDocument doc)
            {
                ClearSearchHighlight(doc);

                if (!string.IsNullOrWhiteSpace(_searchQuery) && _searchQuery.Length <= 128)
                    ApplySearchHighlight(doc, _searchQuery);
            }
        }
        catch
        {
            // Не даём приложению "падать" из-за поиска.
        }
    }

    private bool FilterDocBySearch(object obj)
    {
        if (obj is not DocItem doc)
            return false;

        if (string.IsNullOrWhiteSpace(_searchQueryLower))
            return true;

        // Опционально: совпадение в заголовке
        if (!string.IsNullOrWhiteSpace(doc.Title) &&
            doc.Title.IndexOf(_searchQuery, StringComparison.OrdinalIgnoreCase) >= 0)
        {
            return true;
        }

        var contentLower = GetDocTextLower(doc);
        return contentLower.Contains(_searchQueryLower, StringComparison.Ordinal);
    }

    private string GetDocTextLower(DocItem doc)
    {
        if (string.IsNullOrWhiteSpace(doc.FullPath))
            return "";

        if (_docTextCache.TryGetValue(doc.FullPath, out var cached))
            return cached;

        string text;
        try
        {
            text = File.Exists(doc.FullPath) ? File.ReadAllText(doc.FullPath) : "";
        }
        catch
        {
            text = "";
        }

        // Защита от "гигантских" файлов (нам нужен только поиск).
        if (text.Length > 750_000)
            text = text.Substring(0, 750_000);

        text = text.ToLowerInvariant();
        _docTextCache[doc.FullPath] = text;
        return text;
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


        _docTextCache.Clear();
        DocTitleText.Text = "";
        DocViewer.Document = BuildInfoDocument(S("STR_Info_Placeholder_SelectDoc", "Select a document on the left."));

        var baseDir = AppDomain.CurrentDomain.BaseDirectory;
        var docsRoot = Path.Combine(baseDir, "docs");

        docsRoot = FindExistingDocsFolder(docsRoot) ?? docsRoot;
        _docsRoot = docsRoot;

        if (!Directory.Exists(docsRoot))
        {
            DocsStatusText.Text = S("STR_Info_Status_DocsMissing", "docs folder is missing (place docs next to the .exe).");
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

        _docsView.Filter = FilterDocBySearch;
        DocsList.ItemsSource = _docsView;

        DocsStatusText.Text = SF("STR_Info_Status_FoundCount_Fmt", "Documents found: {0}", _docs.Count);

        if (_docs.Count > 0)
            DocsList.SelectedIndex = 0;
        else
            DocViewer.Document = BuildInfoDocument(S("STR_Info_Placeholder_NoDocs", "No documents found (no .md files)."));
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
                        Title = LocalizeDocTitle(x.id?.Trim() ?? fileName, NormalizeWs(x.title?.Trim() ?? Path.GetFileNameWithoutExtension(fileName))),
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
                Title = LocalizeDocTitle(Path.GetFileNameWithoutExtension(p), NormalizeWs(Path.GetFileNameWithoutExtension(p))),
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
            DocViewer.Document = BuildInfoDocument(SF("STR_Info_Msg_FileNotFound_Fmt", "File not found:{0}", "\n" + doc.FullPath));
            DocsStatusText.Text = SF("STR_Info_Status_NotFound_Fmt", "Not found: {0}", doc.FileName);
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
            DocViewer.Document = BuildInfoDocument(SF("STR_Info_Placeholder_ReadError_Fmt", "Failed to read file:{0}", "\n" + ex.Message));
            DocsStatusText.Text = S("STR_Info_Status_ReadError", "Read error");
            return;
        }

        DocTitleText.Text = doc.Title;
        _currentMd = md;

        var flow = MarkdownToFlowDocument(md);
        DocViewer.Document = flow;
        DocsStatusText.Text = SF("STR_Info_Status_Opened_Fmt", "Opened: {0} ({1})", doc.FileName, DateTime.Now.ToString("HH:mm:ss"));
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

    private static WpfBrush TryFindBrush(string key, WpfBrush fallback)
    {
        try
        {
            return (WpfApplication.Current?.TryFindResource(key) as WpfBrush) ?? fallback;
        }
        catch
        {
            return fallback;
        }
    }

    private static FlowDocument MarkdownToFlowDocument(string md)
    {
        var codeBlockBg = TryFindBrush(
            "TDL.Brush.DocCodeBackground",
            new SolidColorBrush(System.Windows.Media.Color.FromRgb(245, 245, 245)));
        var codeBlockBorder = TryFindBrush(
            "TDL.Brush.DocCodeBorder",
            WpfBrushes.Transparent);
        var codeBlockFg = TryFindBrush(
            "TDL.Brush.Text",
            WpfBrushes.Black);

        // Links: используем Accent + hover, чтобы в Dark/Nexus ссылки не терялись.
        var linkFg = TryFindBrush(
            "TDL.Brush.Accent",
            WpfBrushes.DodgerBlue);
        var linkHoverFg = TryFindBrush(
            "TDL.Brush.AccentHover",
            linkFg);

        var doc = new FlowDocument
        {
            PagePadding = new Thickness(8),
            FontSize = 13,
            TextAlignment = TextAlignment.Left,
            LineHeight = 18,
            LineStackingStrategy = LineStackingStrategy.BlockLineHeight
        };

        // Единая типографика (отступы и ссылки)
        var paragraphStyle = new Style(typeof(Paragraph));
        paragraphStyle.Setters.Add(new Setter(Block.MarginProperty, new Thickness(0, 0, 0, 10)));
        doc.Resources.Add(typeof(Paragraph), paragraphStyle);

        var hyperlinkStyle = new Style(typeof(Hyperlink));
        hyperlinkStyle.Setters.Add(new Setter(Inline.ForegroundProperty, linkFg));
        hyperlinkStyle.Setters.Add(new Setter(Hyperlink.TextDecorationsProperty, TextDecorations.Underline));
        hyperlinkStyle.Setters.Add(new Setter(Hyperlink.CursorProperty, System.Windows.Forms.Cursors.Hand));
        hyperlinkStyle.Triggers.Add(new Trigger
        {
            Property = Hyperlink.IsMouseOverProperty,
            Value = true,
            Setters = { new Setter(Inline.ForegroundProperty, linkHoverFg) }
        });
        doc.Resources.Add(typeof(Hyperlink), hyperlinkStyle);

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

            // Padding для Paragraph напрямую недоступен — используем Block UI через Border
            var container = new BlockUIContainer(
                new Border
                {
                    Background = codeBlockBg,
                    BorderBrush = codeBlockBorder,
                    BorderThickness = new Thickness(1),
                    CornerRadius = new CornerRadius(6),
                    Padding = new Thickness(8),
                    Child = new TextBlock
                    {
                        FontFamily = new System.Windows.Media.FontFamily("Consolas"),
                        Foreground = codeBlockFg,
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

                // Псевдо-list с висящим отступом: переносы строк выравниваются по тексту, а не по маркеру.
                var p = new Paragraph { Margin = new Thickness(24, 0, 0, 6), TextIndent = -12 };
                p.Inlines.Add(new Run("• "));
                AppendInlines(p.Inlines, NormalizeWs(bulletText));
                doc.Blocks.Add(p);
                continue;
            }

            // numbered list (simple)
            if (IsNumbered(t, out var numPrefix, out var numText))
            {
                FlushParagraph();

                var p = new Paragraph { Margin = new Thickness(24, 0, 0, 6), TextIndent = -12 };
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

    private static void ClearSearchHighlight(FlowDocument doc)
    {
        foreach (var b in doc.Blocks)
            ClearHighlightBlock(b);
    }

    private static void ClearHighlightBlock(Block block)
    {
        if (block is Paragraph p)
        {
            ClearHighlightInlines(p.Inlines);
            return;
        }

        if (block is BlockUIContainer ui && ui.Child is Border border)
        {
            // code block (BlockUIContainer -> Border -> TextBlock)
            border.BorderBrush = TryFindBrush("TDL.Brush.DocCodeBorder", WpfBrushes.Transparent);
            if (border.Child is TextBlock tb)
                tb.Background = null;
            return;
        }

        if (block is Section s)
        {
            foreach (var b in s.Blocks)
                ClearHighlightBlock(b);
        }
    }


    private static void ClearHighlightInlines(InlineCollection inlines)
    {
        for (Inline? inline = inlines.FirstInline; inline != null; inline = inline.NextInline)
        {
            switch (inline)
            {
                case Run run:
                    run.Background = null;
                    break;
                case Span span:
                    ClearHighlightInlines(span.Inlines);
                    break;
            }
        }
    }

    // Подсветка совпадений запроса (используем существующий SearchBox: он фильтрует список слева,
    // но при открытии документа мы дополнительно подсвечиваем совпадения в тексте).
    private static void ApplySearchHighlight(FlowDocument doc, string? query)
    {
        var q = (query ?? "").Trim();
        if (q.Length == 0)
            return;

        var bg = TryFindBrush(
            "TDL.Brush.Selection",
            new SolidColorBrush(System.Windows.Media.Color.FromArgb(0x33, 0x4F, 0xC1, 0xFF)));

        foreach (var b in doc.Blocks)
            HighlightBlock(b, q, bg);
    }

    private static void HighlightBlock(Block block, string query, WpfBrush bg)
    {
        if (block is Paragraph p)
        {
            HighlightInlines(p.Inlines, query, bg);
            return;
        }

        if (block is BlockUIContainer ui && ui.Child is Border border && border.Child is TextBlock tb)
        {
            // code block (BlockUIContainer -> Border -> TextBlock)
            if (!string.IsNullOrEmpty(tb.Text) &&
                tb.Text.IndexOf(query, StringComparison.OrdinalIgnoreCase) >= 0)
            {
                border.BorderBrush = bg;
                tb.Background = bg;
            }

            return;
        }

        if (block is Section s)
        {
            foreach (var b in s.Blocks)
                HighlightBlock(b, query, bg);
        }
    }


    private static void HighlightInlines(InlineCollection inlines, string query, WpfBrush bg)
    {
        for (Inline? inline = inlines.FirstInline; inline != null;)
        {
            var next = inline.NextInline;

            switch (inline)
            {
                case Run run:
                    ReplaceRunWithHighlighted(inlines, run, query, bg);
                    break;
                case Span span:
                    HighlightInlines(span.Inlines, query, bg);
                    break;
            }

            inline = next;
        }
    }

    private static void ReplaceRunWithHighlighted(InlineCollection parent, Run run, string query, WpfBrush bg)
    {
        var text = run.Text;
        if (string.IsNullOrEmpty(text))
            return;

        var idx = text.IndexOf(query, StringComparison.OrdinalIgnoreCase);
        if (idx < 0)
            return;

        var cursor = (Inline)run;
        var pos = 0;

        while (idx >= 0)
        {
            if (idx > pos)
                parent.InsertBefore(cursor, CloneRun(text.Substring(pos, idx - pos), run));

            var match = CloneRun(text.Substring(idx, query.Length), run);
            match.Background = bg;
            parent.InsertBefore(cursor, match);

            pos = idx + query.Length;
            idx = text.IndexOf(query, pos, StringComparison.OrdinalIgnoreCase);
        }

        if (pos < text.Length)
            parent.InsertBefore(cursor, CloneRun(text.Substring(pos), run));

        parent.Remove(cursor);
    }

    private static Run CloneRun(string text, Run template)
    {
        return new Run(text)
        {
            Foreground = template.Foreground,
            FontWeight = template.FontWeight,
            FontStyle = template.FontStyle,
            TextDecorations = template.TextDecorations
        };
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

        // Inline `code` is used heavily for paths/names in our docs.
        // In dark themes the default "light code background" looks like a bright highlight,
        // so we render it as subtle italic emphasis instead.
        var inlineCodeFg = TryFindBrush("TDL.Brush.TextDim", WpfBrushes.Gray);

        var i = 0;
        while (i < s.Length)
        {
            // markdown link: [text](url)
            if (s[i] == '[' && TryParseMarkdownLink(s, i, out var linkText, out var linkTarget, out var consumedLink))
            {
                inlines.Add(BuildHyperlink(linkText, linkTarget));
                i += consumedLink;
                continue;
            }

            // auto URL: http(s)://...
            if (TryParseAutoUrl(s, i, out var autoUrl, out var consumedUrl))
            {
                inlines.Add(BuildHyperlink(autoUrl, autoUrl));
                i += consumedUrl;
                continue;
            }

            // inline code
            if (s[i] == '`')
            {
                var j = s.IndexOf('`', i + 1);
                if (j > i)
                {
                    var code = s.Substring(i + 1, j - i - 1);
                    var run = new Run(code) { Foreground = inlineCodeFg };
                    inlines.Add(new Italic(run));
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

    private static Hyperlink BuildHyperlink(string text, string target)
    {
        var link = new Hyperlink(new Run(text))
        {
            ToolTip = target
        };

        if (Uri.TryCreate(target, UriKind.RelativeOrAbsolute, out var uri))
            link.NavigateUri = uri;

        return link;
    }

    private static bool TryParseMarkdownLink(string s, int start, out string text, out string target, out int consumed)
    {
        // Very small subset: [text](target)
        text = "";
        target = "";
        consumed = 0;

        if (start < 0 || start >= s.Length || s[start] != '[')
            return false;

        var closeText = s.IndexOf(']', start + 1);
        if (closeText <= start)
            return false;

        if (closeText + 1 >= s.Length || s[closeText + 1] != '(')
            return false;

        var closeTarget = s.IndexOf(')', closeText + 2);
        if (closeTarget <= closeText)
            return false;

        text = s.Substring(start + 1, closeText - start - 1);
        target = s.Substring(closeText + 2, closeTarget - closeText - 2);

        if (string.IsNullOrWhiteSpace(text) || string.IsNullOrWhiteSpace(target))
            return false;

        consumed = (closeTarget - start) + 1;
        return true;
    }

    private static bool TryParseAutoUrl(string s, int start, out string url, out int consumed)
    {
        url = "";
        consumed = 0;

        // Simple detection: http:// or https://
        if (start < 0 || start >= s.Length)
            return false;

        if (!s.AsSpan(start).StartsWith("http://", StringComparison.OrdinalIgnoreCase)
            && !s.AsSpan(start).StartsWith("https://", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        var end = start;
        while (end < s.Length && !char.IsWhiteSpace(s[end]))
            end++;

        if (end <= start)
            return false;

        var raw = s.Substring(start, end - start);

        // Убираем типичные "хвосты" в тексте (точка в конце предложения, запятая и т.п.)
        url = raw.TrimEnd('.', ',', ';', ':', ')', ']', '"', '\'');
        if (string.IsNullOrWhiteSpace(url))
            return false;

        // Пунктуацию после URL не "съедаем" — она пойдёт как обычный текст.
        consumed = url.Length;
        return true;
    }

    private static int NextSpecialIndex(string s, int start)
    {
        var idx1 = s.IndexOf('`', start);
        var idx2 = s.IndexOf('*', start);
        var idx3 = s.IndexOf('[', start);
        var idx4 = IndexOfHttp(s, start);

        return MinPositive(idx1, idx2, idx3, idx4);
    }

    private static int IndexOfHttp(string s, int start)
    {
        var i1 = s.IndexOf("http://", start, StringComparison.OrdinalIgnoreCase);
        var i2 = s.IndexOf("https://", start, StringComparison.OrdinalIgnoreCase);
        return MinPositive(i1, i2);
    }

    private static int MinPositive(params int[] values)
    {
        var min = -1;
        foreach (var v in values)
        {
            if (v < 0) continue;
            min = min < 0 ? v : Math.Min(min, v);
        }
        return min;
    }
}
