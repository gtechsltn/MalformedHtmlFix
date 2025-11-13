using System;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;

using Aspose.Email;

using Ganss.Xss;

using HtmlAgilityPack;

namespace MalformedHtmlFix
{
    /// <summary>
    /// Main method to fix malformed HTML
    /// </summary>
    public static class HtmlHelper
    {
        /// <summary>
        /// Sanitizes a full HTML document:
        /// - Preserves <html> and <body>
        /// - Fixes broken <a> tags
        /// - Normalizes empty/malformed hrefs to "#"
        /// - Sanitizes unsafe HTML
        /// - Validates URLs
        /// </summary>
        /// <param name="html">Input HTML string</param>
        /// <returns>Safe HTML ready to render</returns>
        public static string SanitizeHtmlDocument(string html)
        {
            if (string.IsNullOrWhiteSpace(html))
                html = "<html><body></body></html>";

            // 1. Preprocessing: Normalize empty/malformed hrefs
            html = Regex.Replace(
                html,
                @"href\s*=\s*(['""])?\s*(?:\1)?", // matches href="", href='', href=
                "href=\"#\"",
                RegexOptions.IgnoreCase
            );

            // 2. Parse HTML and fix structural tags
            var doc = new HtmlDocument { OptionFixNestedTags = true };
            doc.LoadHtml(html);

            // Ensure <html> exists
            var htmlNode = doc.DocumentNode.SelectSingleNode("//html");
            if (htmlNode == null)
            {
                htmlNode = doc.CreateElement("html");
                htmlNode.AppendChild(doc.DocumentNode.CloneNode(true));
                doc.DocumentNode.RemoveAllChildren();
                doc.DocumentNode.AppendChild(htmlNode);
            }

            // Ensure <body> exists
            var bodyNode = htmlNode.SelectSingleNode("//body");
            if (bodyNode == null)
            {
                bodyNode = doc.CreateElement("body");

                var contentNodes = htmlNode.ChildNodes.Where(n => n.Name != "head" && n.Name != "body").ToList();
                foreach (var node in contentNodes)
                {
                    htmlNode.RemoveChild(node);
                    bodyNode.AppendChild(node);
                }

                htmlNode.AppendChild(bodyNode);
            }

            // 3. Fix <a> tags: validate URLs
            foreach (var aTag in bodyNode.SelectNodes(".//a") ?? Enumerable.Empty<HtmlNode>())
            {
                var href = aTag.GetAttributeValue("href", null);

                if (string.IsNullOrWhiteSpace(href))
                {
                    // Empty href already replaced by preprocessing
                    aTag.SetAttributeValue("href", "#");
                }
                else if (href.TrimStart().StartsWith("javascript:", StringComparison.OrdinalIgnoreCase))
                {
                    // Unsafe javascript replaced with safe "#"
                    aTag.SetAttributeValue("href", "#");
                }
                else if (!IsValidUrl(href))
                {
                    // Malformed URLs replaced with safe "#"
                    aTag.SetAttributeValue("href", "#");
                }
                else
                {
                    // Others replaced with safe "#"
                    aTag.SetAttributeValue("href", "#");
                }
            }

            var fixedHtml = doc.DocumentNode.OuterHtml;

            // 4. Sanitize HTML
            var sanitizer = new HtmlSanitizer();

            // Structural tags
            sanitizer.AllowedTags.Add("html");
            sanitizer.AllowedTags.Add("head");
            sanitizer.AllowedTags.Add("title");
            sanitizer.AllowedTags.Add("meta");
            sanitizer.AllowedTags.Add("style");
            sanitizer.AllowedTags.Add("body");

            // Content tags
            var contentTags = new[] { "p", "a", "b", "i", "strong", "em", "ul", "li", "br", "span", "div" };
            foreach (var tag in contentTags)
                sanitizer.AllowedTags.Add(tag);

            // Attributes
            sanitizer.AllowedAttributes.Add("href");
            sanitizer.AllowedAttributes.Add("class");
            sanitizer.AllowedAttributes.Add("id");
            sanitizer.AllowedAttributes.Add("style"); // optional

            var safeHtml = sanitizer.Sanitize(fixedHtml);

            // Ensure <html> and <body> are preserved
            if (!safeHtml.Contains("<html>"))
                safeHtml = $"<html>{safeHtml}</html>";
            if (!safeHtml.Contains("<body>"))
                safeHtml = safeHtml.Replace("<html>", "<html><body>").Replace("</html>", "</body></html>");

            // Replace empty hrefs (single or double quotes, any case) with href="#"
            safeHtml = safeHtml
                .Replace("href=\"\"", "href=\"#\"")
                .Replace("href=''", "href=\"#\"")
                .Replace("href='#'", "href=\"#\"");

            // Regex to remove empty <p> tags (including whitespace)
            string pattern = @"<p>\s*</p>";
            string finalHtml = Regex.Replace(safeHtml, pattern, string.Empty, RegexOptions.IgnoreCase);
            Debug.WriteLine(finalHtml);
            return finalHtml;
        }

        /// <summary>
        /// Validates URLs to prevent unsafe schemes
        /// </summary>
        private static bool IsValidUrl(string url)
        {
            if (string.IsNullOrWhiteSpace(url))
                return false;

            url = url.Trim();

            // Reject unsafe schemes
            if (url.StartsWith("javascript:", StringComparison.OrdinalIgnoreCase) ||
                url.StartsWith("data:", StringComparison.OrdinalIgnoreCase))
                return false;

            return Uri.TryCreate(url, UriKind.Absolute, out _);
        }


        /// <summary>
        /// Creates an email using Aspose.Email with fixed HTML
        /// </summary>
        public static void CreateFixedEmail(string html, string from, string to, string subject, string pathSave)
        {
            string fixedHtml = SanitizeHtmlDocument(html);

            var message = new MailMessage();
            message.From = from;
            message.To.Add(to);
            message.Subject = subject;
            message.HtmlBody = fixedHtml;

            message.Save(pathSave, SaveOptions.DefaultMsgUnicode);
        }
    }
}