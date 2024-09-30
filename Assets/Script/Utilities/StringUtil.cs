using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


public class StringUtil 
{
    static public string Format(string format, params object[] args)
    {
        string output = format;
        if (null != args && 0 != args.Length)
        {
            StringBuilder sb = new StringBuilder();
            output = sb.AppendFormat(format, args).ToString();
        }

        return output;
    }

    static public void Append(ref string src, string args)
    {
        if (null != args && 0 != args.Length)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(src);
            sb.Append(args);

            src = sb.ToString();
        }
    }

    static public string[] Split(string input, string separator, StringSplitOptions options = System.StringSplitOptions.None)
    {

        if (string.IsNullOrEmpty(input))
        {
            return null;
        }

        if (string.IsNullOrEmpty(separator))
        {
            return null;
        }

        char[] charSeparator = separator.ToCharArray();
        if (charSeparator == null || charSeparator.Length == 0)
        {
            return null;
        }

        return input.Split(charSeparator, options);
    }

	public static string DisplayCamelCaseString( string camelCase )
	{
		var chars = new List<char> { camelCase[0] };
		foreach( var c in camelCase.Skip( 1 ) )
		{
			if( char.IsUpper( c ) )
			{
				chars.Add( ' ' );
				chars.Add( char.ToLower( c ) );
			}
			else
			{
				chars.Add( c );
			}
		}

		return new string( chars.ToArray() );
	}

    public static string[] Split(string strValue, string splitValue)
    {
        return strValue.Split(new string[] { splitValue }, StringSplitOptions.None);
    }

    public static string SplitStringByMaxChars(string strValue, int maxChars)
    {
        const string splittedChars = "...";
        maxChars += splittedChars.Length;

        if (strValue.Length + splittedChars.Length > maxChars)
        {
            strValue = strValue.Substring(0, maxChars - splittedChars.Length) + splittedChars;
        }
        return strValue;
    }

    public static string SplitStringByStartEnd(string strValue, string start, string end)
    {
        if (strValue.Contains(start))
        {
            string[] array = strValue.Split(new string[] { start }, StringSplitOptions.None);
            strValue = array[array.Length - 1];
        }

        if (strValue.Contains(end))
        {
            strValue = strValue.Split(new string[] { end }, StringSplitOptions.None)[0];
        }

        return strValue;
    }

    public static string UppercaseFirstLetter(string value)
    {
        if (string.IsNullOrEmpty(value) || value.Length == 0)
            return value;

        if (value.Length == 1)
            return value.ToUpper();

        return string.Format("{0}{1}", value.Substring(0, 1).ToUpper(), value.Substring(1, value.Length - 1).ToLower());
    }

    public static string LowercaseFirstLetter(string value)
    {
        if (string.IsNullOrEmpty(value) || value.Length == 0)
            return value;

        if (value.Length == 1)
            return value.ToLower();

        return string.Format("{0}{1}", value.Substring(0, 1).ToLower(), value.Substring(1, value.Length - 1));
    }

    public static string ReplaceHtmlTags(string value)
    {
        string result = value.Replace("<", "\u3008");
        result = result.Replace(">", "\u3009");
        return result;
    }

    public static string PrepareTextForBubble(string text, int maxLines, int maxLineLength, string ellipsis)
    {
        string[] words = text.Split(' ');
        List<string> result = new List<string>();
        int index = 0, length = 0;
        bool wordCut = true;
        for (int i = 0; i <= maxLines; i++)
        {
            if (i == maxLines)
            {
                if (!wordCut)
                {
                    result.Add(" ");
                    length++;
                    while (true)
                    {
                        if (length == 0 || length + ellipsis.Length <= maxLineLength)
                        {
                            result.Add(ellipsis);
                            break;
                        }
                        length -= result[result.Count - 2].Length + 1;
                        result.RemoveRange(result.Count - 2, 2);
                    }
                }
                break;
            }
            if (i > 0)
            {
                result.Add("\n");
            }
            length = 0;
            wordCut = false;
            while (index < words.Length)
            {
                string word = words[index];
                bool extraSpace = length > 0 && word.Length > 0;
                if (length + (extraSpace ? 1 : 0) + word.Length <= maxLineLength)
                {
                    if (extraSpace)
                    {
                        result.Add(" ");
                        length++;
                    }
                    result.Add(word);
                    length += word.Length;
                }
                else
                {
                    break;
                }
                index++;
            }
            if (length == 0 && index < words.Length)
            {
                result.Add(words[index].Substring(0, Math.Max(1, maxLineLength - ellipsis.Length)));
                result.Add(ellipsis);
                index++;
                wordCut = true;
            }
            if (index == words.Length)
            {
                break;
            }
        }
        return String.Join("", result.ToArray());
    }

    private readonly static StringBuilder _stringBuilder = new StringBuilder();

    public static string Concat(params string[] texts)
    {
        _stringBuilder.Length = 0;

        for (int i = 0; i < texts.Length; i++)
        {
            _stringBuilder.Append(texts[i]);
        }
        return _stringBuilder.ToString();
    }

}
