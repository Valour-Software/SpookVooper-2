using System.Diagnostics;
using System.Text.Encodings.Web;

namespace SV2.Helpers;

public static class StringHelper
{
    public static string ToRoman(int number)
    {
        if ((number < 0) || (number > 3999)) throw new ArgumentOutOfRangeException("insert value betwheen 1 and 3999");
        if (number < 1) return string.Empty;
        if (number >= 1000) return "M" + ToRoman(number - 1000);
        if (number >= 900) return "CM" + ToRoman(number - 900);
        if (number >= 500) return "D" + ToRoman(number - 500);
        if (number >= 400) return "CD" + ToRoman(number - 400);
        if (number >= 100) return "C" + ToRoman(number - 100);
        if (number >= 90) return "XC" + ToRoman(number - 90);
        if (number >= 50) return "L" + ToRoman(number - 50);
        if (number >= 40) return "XL" + ToRoman(number - 40);
        if (number >= 10) return "X" + ToRoman(number - 10);
        if (number >= 9) return "IX" + ToRoman(number - 9);
        if (number >= 5) return "V" + ToRoman(number - 5);
        if (number >= 4) return "IV" + ToRoman(number - 4);
        if (number >= 1) return "I" + ToRoman(number - 1);
        throw new UnreachableException("Impossible state reached");
    }
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
