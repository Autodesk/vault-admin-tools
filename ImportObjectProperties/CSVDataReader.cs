using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Text;

namespace ImportObjectProperties
{
    class CSVDataReader
    {
        static CSVDataReader()
        {
            Separator = ',';
            Encoding = Encoding.UTF8;
        }

        private CSVDataReader()
        {
        }

        public static Encoding Encoding { get; set; }
        public static char Separator { get; set; }

        public static DataTable ReadFile(string fileName)
        {
            DataTable result = new DataTable();

            using (StreamReader reader = new StreamReader(fileName, Encoding))
            {
                string line;
                int lineIndex = 0;

                while (null != (line = reader.ReadLine()))
                {
                    List<string> values = ParseLine(line);

                    if (lineIndex == 0)
                    {
                        foreach (string value in values)
                        {
                            string columnName = value;

                            if (true == result.Columns.Contains(columnName))
                            {
                                columnName = columnName + "_1";
                            }
                            DataColumn col = new DataColumn(columnName, typeof(string));

                            result.Columns.Add(col);
                        }
                    }
                    else
                    {
                        DataRow row = result.NewRow();
                        int colIndex = 0;

                        foreach (string value in values)
                        {
                            if (colIndex < result.Columns.Count)
                            {
                                row[colIndex++] = value;
                            }
                        }
                        result.Rows.Add(row);
                    }
                    lineIndex++;
                }
            }
            return result;
        }

        private static List<string> ParseLine(string line)
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
                if ((c == Separator) && (false == isQuote))
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
