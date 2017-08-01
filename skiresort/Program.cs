using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace skiresort
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Count() == 0)
            {
                Console.WriteLine("Usage: skiresort path/to/file");
                Environment.Exit(1);
            }

            IEnumerator<string> lines;
            int[,] map;
            try
            {
                lines = File.ReadLines(args[0]).GetEnumerator();
                map = ParseFile(lines);
            }
            catch (IOException e)
            {
                Console.WriteLine("Could not open file: " + e.Message);
                Environment.Exit(2);
            }
            catch (ParseException e)
            {
                Console.WriteLine("An error occured, while reading the file: " + e.Message);
                Environment.Exit(3);
            }
        }

        static int[,] ParseFile(IEnumerator<string> lines)
        {
            if (lines.MoveNext())
            {
                var line = lines.Current;
                var parts = line.Split(' ');
                int cols, rows;

                if (parts.Count() != 2 || !Int32.TryParse(parts[0], out rows) || !Int32.TryParse(parts[1], out cols))
                {
                    throw new ParseException("First line of file must contain row count an column count.");
                }

                int[,] result = new int[rows, cols];
                int row = 0;
                while (lines.MoveNext())
                {
                    if (row > rows)
                    {
                        throw new ParseException("Too much rows.");
                    }

                    line = lines.Current;
                    parts = line.Split(' ');
                    if (parts.Count() != cols)
                    {
                        throw new ParseException("Wrong column count in line " + (row + 1) + ".");
                    }

                    for (int col = 0; col < cols; col++)
                    {
                        int ascent;
                        if (!Int32.TryParse(parts[col], out ascent))
                        {
                            throw new ParseException("Can't parse number in line " + (row + 1) + " in column " + (col + 1) + ".");
                        }
                        result[row, col] = ascent;
                    }

                    row++;
                }
                if (row < rows)
                {
                    throw new ParseException("Not enough rows.");
                }

                return result;
            }

            throw new ParseException("File is empty.");
        }
    }

    public class ParseException : Exception
    {
        public ParseException()
        {
        }

        public ParseException(string message)
            : base(message)
        {
        }

        public ParseException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}
