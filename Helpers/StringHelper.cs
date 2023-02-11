using System.Text.Encodings.Web;

namespace SV2.Helpers;

public static class StringHelper
{
    public static string FormatTitle(string input, int length = int.MaxValue)
    {
        string encoded = HtmlEncoder.Default.Encode(input)
                         .Replace("&#x27;", "'")
                         .Replace("&quot;", "\"")
                         .Replace("&#x2019;", "'")
                         .Replace("&lt;", "<")
                         .Replace("&gt;", ">")
                         .Replace("&#xD;&#xA;", "...");

        string cleaned = "";
        bool tag = false;

        // Clean out all html tags
        foreach (char c in encoded)
        {
            if (!tag)
            {
                if (c == '<')
                {
                    tag = true;
                }
                else
                {
                    cleaned += c;
                }
            }
            else
            {
                if (c == '>')
                {
                    tag = false;
                }
            }

        }

        // Max size
        if (cleaned.Length > length)
        {
            cleaned = cleaned.Substring(0, length);

            cleaned += "...";
        }

        return cleaned;
    }
}
