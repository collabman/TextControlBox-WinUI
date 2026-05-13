using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.RegularExpressions;
using TextControlBoxNS.Models;
using Windows.Devices.Power;
using Windows.UI;

namespace TextControlBoxNS;

/// <summary>
/// Represents a code language configuration used for syntax highlighting and auto-pairing in the text content.
/// </summary>
public class SyntaxHighlightLanguage
{
    /// <summary>
    /// Gets or sets the name of the code language.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Gets or sets the description of the code language.
    /// </summary>
    public string Description { get; set; }

    /// <summary>
    /// Gets or sets an array of file filters for the code language.
    /// </summary>
    public string[] Filter { get; set; }

    /// <summary>
    /// Gets or sets the author of the code language definition.
    /// </summary>
    public string Author { get; set; }

    /// <summary>
    /// Gets or sets an array of syntax highlights for the code language.
    /// </summary>
    public SyntaxHighlights[] Highlights { get; set; }

    /// <summary>
    /// Gets or sets an array of auto-pairing pairs for the code language.
    /// </summary>
    public AutoPairingPair[] AutoPairingPair { get; set; }

    /// <summary>
    /// Gets or sets an array of highlight rules for the code language.
    /// This is the new extensible system that supports dynamic highlighting.
    /// </summary>
    [JsonIgnore]
    public IHighlightRule[] HighlightRules { get; set; }

    internal void CompileAllRegex()
    {
        if (Highlights == null) return;

        foreach (var highlight in Highlights)
        {
            highlight.CompileRegex();
        }
    }

    public string ToHtml(string input)
    {
        if (Highlights == null || string.IsNullOrWhiteSpace(input)) return input;
        CompileAllRegex();

        var htmlBuilder = new StringBuilder();
        var stringRule = new byte[input.Length];
        
        // Initialize all values to 255
        Array.Fill(stringRule, (byte)255);

        for (byte ii = 0; ii < Highlights.Length; ii++)
        {
            var rule = Highlights[ii];

            var highlights = rule.PrecompiledRegex.Matches(input);

            if(highlights.Any())
            {
                foreach (Match span in highlights)
                {
                    for (int i = span.Index; i < span.Index + span.Length; i++)
                    {
                        stringRule[i] = ii;
                    }
                }
            }
        }

        byte lastRule = 255;
        var openTag = false;

        for (int i = 0; i < input.Length; i++)
        {
            byte currentRule = stringRule[i];
            if (currentRule != lastRule)
            {
                if (openTag)
                {
                    htmlBuilder.Append("</span>");
                    openTag = false;
                }
                if (currentRule != 255)
                {
                    openTag = true;
                    var rule = Highlights[currentRule];
                    _ = htmlBuilder.Append($"<span style=\"color: {ColorToHex(rule.ColorDark_Clr)}; font-weight: {(rule.CodeStyle?.Bold ?? false? "bold" : "normal")}; font-style: {(rule.CodeStyle?.Italic ?? false? "italic" : "normal")}; text-decoration: {(rule.CodeStyle?.Underlined ?? false? "underline" : "none")};\">");
                }
                lastRule = currentRule;
            }
            if (input[i] != '\r')
                htmlBuilder.Append(input[i] == '\n' ? "<br/>" : System.Net.WebUtility.HtmlEncode(input[i].ToString()));
        }

        return htmlBuilder.ToString();
    }

    /// <summary>
    /// Converts a System.Drawing.Color to a hex string in the format #RRGGBB or #AARRGGBB.
    /// </summary>
    /// <param name="color">The color to convert.</param>
    /// <param name="includeAlpha">Whether to include the alpha channel in the hex string.</param>
    /// <returns>Hexadecimal color string.</returns>
    public static string ColorToHex(Color color, bool includeAlpha = false)
    {
        if (includeAlpha)
        {
            // Include alpha channel
            return $"#{color.A:X2}{color.R:X2}{color.G:X2}{color.B:X2}";
        }
        else
        {
            // Only RGB
            return $"#{color.R:X2}{color.G:X2}{color.B:X2}";
        }
    }


    /// <summary>
    /// Converts the legacy SyntaxHighlights array to the new IHighlightRule system.
    /// This allows gradual migration from the old to the new system.
    /// </summary>
    internal void ConvertToHighlightRules()
    {
        if (Highlights == null || Highlights.Length == 0)
        {
            HighlightRules = null;
            return;
        }

        var rules = new IHighlightRule[Highlights.Length];
        for (int i = 0; i < Highlights.Length; i++)
        {
            rules[i] = new RegexHighlightRule(Highlights[i]);
        }
        HighlightRules = rules;
    }
}
