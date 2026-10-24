using System.Text;
using System.Text.RegularExpressions;
using HtmlAgilityPack;

namespace AtCoderStudy.Infrastructure;

internal static class AtCoderStatementMarkdownConverter
{
    public static string Convert(string html, Uri source)
    {
        var document = new HtmlDocument();
        document.LoadHtml(html);
        var root = document.DocumentNode.SelectSingleNode("//*[@id='task-statement']")
            ?? throw new InvalidDataException("公式ページから問題文を見つけられませんでした。ログインや公開状態を確認してください。");
        var japanese = root.Descendants().FirstOrDefault(node => HasClass(node, "lang-ja")) ?? root;

        var blocks = new List<string>();
        foreach (var child in japanese.ChildNodes)
        {
            if (child.Name == "p" && RenderInline(child, source).StartsWith("配点", StringComparison.Ordinal))
                continue;
            AppendBlocks(child, blocks, source);
        }

        if (blocks.Count == 0)
            throw new InvalidDataException("公式ページの問題文が空です。");
        return string.Join("\n\n", blocks.Where(x => x.Length > 0)).Trim() + "\n";
    }

    private static void AppendBlocks(HtmlNode node, List<string> blocks, Uri source)
    {
        switch (node.Name)
        {
            case "h1":
            case "h2":
            case "h3":
            case "h4":
                blocks.Add($"{(node.Name == "h4" ? "###" : "##")} {RenderInline(node, source)}");
                return;
            case "p":
                blocks.Add(RenderInline(node, source));
                return;
            case "pre":
                blocks.Add(RenderPre(node));
                return;
            case "ul":
            case "ol":
                blocks.Add(RenderList(node, source));
                return;
            case "table":
                blocks.Add(RenderTable(node, source));
                return;
            case "img":
                blocks.Add(RenderInline(node, source));
                return;
            case "script":
            case "style":
            case "button":
            case "hr":
                return;
            case "#text":
                var text = NormalizeText(node.InnerText).Trim();
                if (text.Length > 0) blocks.Add(text);
                return;
            default:
                foreach (var child in node.ChildNodes) AppendBlocks(child, blocks, source);
                return;
        }
    }

    private static string RenderInline(HtmlNode node, Uri source)
    {
        if (node.Name == "#text") return NormalizeText(node.InnerText);
        if (node.Name == "br") return "  \n";
        if (node.Name is "script" or "style" or "button") return "";

        var content = string.Concat(node.ChildNodes.Select(child => RenderInline(child, source)));
        return node.Name switch
        {
            "var" => $"${Decode(node.InnerText).Trim()}$",
            "code" => $"`{Decode(node.InnerText).Trim()}`",
            "strong" or "b" => $"**{content.Trim()}**",
            "em" or "i" => $"*{content.Trim()}*",
            "a" => RenderLink(node, content.Trim(), source),
            "img" => RenderImage(node, source),
            _ => node.Name is "p" or "h1" or "h2" or "h3" or "h4"
                ? CleanInline(content)
                : content
        };
    }

    private static string RenderPre(HtmlNode node)
    {
        var content = RenderPreContent(node).Replace("\r\n", "\n").Trim('\n');
        if (node.Descendants("var").Any())
            return string.Join("  \n", content.Split('\n').Select(line => line.TrimEnd()));
        return $"```text\n{content}\n```";
    }

    private static string RenderPreContent(HtmlNode node) => node.Name switch
    {
        "#text" => Decode(node.InnerText),
        "br" => "\n",
        "var" => $"${Decode(node.InnerText).Trim()}$",
        _ => string.Concat(node.ChildNodes.Select(RenderPreContent))
    };

    private static string RenderList(HtmlNode node, Uri source, string indent = "")
    {
        var lines = new List<string>();
        var ordered = node.Name == "ol";
        var number = 1;
        foreach (var item in node.ChildNodes.Where(child => child.Name == "li"))
        {
            var body = string.Concat(item.ChildNodes
                .Where(child => child.Name is not ("ul" or "ol"))
                .Select(child => RenderInline(child, source))).Trim();
            lines.Add($"{indent}{(ordered ? $"{number++}." : "-")} {body}");
            foreach (var nested in item.ChildNodes.Where(child => child.Name is "ul" or "ol"))
                lines.Add(RenderList(nested, source, indent + "  "));
        }
        return string.Join("\n", lines);
    }

    private static string RenderTable(HtmlNode node, Uri source)
    {
        var rows = node.Descendants("tr")
            .Select(row => row.ChildNodes.Where(cell => cell.Name is "th" or "td").ToList())
            .Where(row => row.Count > 0).ToList();
        if (rows.Count == 0) return "";
        var width = rows.Max(row => row.Count);
        var hasHeader = rows[0].Any(cell => cell.Name == "th");
        var lines = new List<string>
        {
            hasHeader ? TableRow(rows[0], width, source) : TableRow(Enumerable.Repeat(" ", width)),
            TableRow(Enumerable.Repeat("---", width))
        };
        foreach (var row in rows.Skip(hasHeader ? 1 : 0))
            lines.Add(TableRow(row, width, source));
        return string.Join("\n", lines);
    }

    private static string TableRow(IReadOnlyCollection<HtmlNode> cells, int width, Uri source) =>
        TableRow(cells.Select(cell => CleanInline(RenderInline(cell, source))
            .Replace("|", "\\|").Replace("\n", " "))
            .Concat(Enumerable.Repeat(" ", width - cells.Count)));

    private static string TableRow(IEnumerable<string> cells) => "| " + string.Join(" | ", cells) + " |";

    private static string RenderLink(HtmlNode node, string label, Uri source)
    {
        var href = node.GetAttributeValue("href", "");
        return Uri.TryCreate(source, href, out var url) ? $"[{label}]({url})" : label;
    }

    private static string RenderImage(HtmlNode node, Uri source)
    {
        var src = node.GetAttributeValue("src", "");
        var alt = node.GetAttributeValue("alt", "画像");
        return Uri.TryCreate(source, src, out var url) ? $"![{alt}]({url})" : "";
    }

    private static bool HasClass(HtmlNode node, string name) =>
        node.GetAttributeValue("class", "").Split(' ', StringSplitOptions.RemoveEmptyEntries).Contains(name);

    private static string Decode(string text) => HtmlEntity.DeEntitize(text);

    private static string NormalizeText(string text) => Regex.Replace(Decode(text), @"\s+", " ");

    private static string CleanInline(string text)
    {
        var lines = Regex.Replace(text, @"[ \t]*\n[ \t]*", "  \n");
        return Regex.Replace(lines, @"[ \t]{2,}(?!\n)", " ").Trim();
    }
}
