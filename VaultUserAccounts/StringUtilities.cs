using System.Collections.Generic;

namespace VaultUserAccounts.IO
{
    static class StringUtilities
    {
        public static List<string> StringToList(string text, char separator)
        {
            List<string> result = new List<string>();
            string[] subItems = text.Split(';');

            if (null != subItems)
            {
                foreach (string subItem in subItems)
                {
                    result.Add(subItem);
                }
            }
            return result;
        }

        public static string ListToString(IEnumerable<string> names)
        {
            string result = string.Empty;

            foreach (string tmp in names)
            {
                if (0 < result.Length)
                {
                    result += ";";
                }
                result += tmp;
            }
            return result;
        }

        public static string FormatString(string text)
        {
            string result = string.Empty;

            if (text.Contains(","))
            {
                result = string.Format("\"{0}\"", text);
            }
            else
            {
                result = text;
            }
            return result;
        }

        public static List<string> ParseLine(string line, char separator)
        {
            List<string> values = new List<string>();
            string value = string.Empty;
            bool isQuote = false;
            char prevChar = ' ';

            foreach (char c in line)
            {
                if (c == '"')
                {
                    isQuote = !isQuote;
                }
                //VSK-233 - Configurable separator
                if ((c == separator) && (false == isQuote))
                {
                    value = value.TrimStart();
                    values.Add(value);
                    value = string.Empty;
                }
                else
                {
                    bool add = true;

                    if ((c == '"') && prevChar != '"')
                    {
                        add = false;
                    }
                    if (true == add)
                    {
                        value += c;
                    }
                }
                prevChar = c;
            }
            value = value.TrimStart();
            if (0 < value.Length)
            {
                values.Add(value);
            }
            return values;
        }

    }
}
