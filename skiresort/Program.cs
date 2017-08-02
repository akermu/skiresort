using System;
using System.Collections;
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

                var routes = allRoutes(map);
                var route = bestRoute(routes, map);
                Console.WriteLine("");
                Console.WriteLine("Best Route:");
                foreach (var coordinate in route)
                {
                    Console.Write("({0}, {1}) --> ", coordinate.x + 1, coordinate.y + 1);
                }
                Console.WriteLine("Finish");
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

        static Route bestRoute(List<Route> routes, int[,] map)
        {
            var max = Int32.MinValue;
            foreach (var route in routes)
            {
                if (route.Count() > max)
                {
                    max = route.Count();
                }
            }

            var longest = routes.Where(route => route.Count() == max).ToList();
            max = Int32.MinValue;
            foreach (var route in longest)
            {
                var start = route.First();
                var end = route.Last();
                var diff = map[start.x, start.y] - map[end.x, end.y];
                if (diff > max)
                {
                    max = diff;
                }
            }

            var best = longest.Where(route =>
                {
                    var start = route.First();
                    var end = route.Last();
                    var diff = map[start.x, start.y] - map[end.x, end.y];
                    return diff == max;
                }
            ).ToList();

            return best.First();
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

                int[,] result = new int[cols, rows];
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
                        result[col, row] = ascent;
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

        static List<Route> allRoutes(int[,] map, Route currentRoute = null)
        {
            List<Route> result = new List<Route>();
            int cols = map.GetLength(0);
            int rows = map.GetLength(1);
            if (currentRoute == null)
            {
                for (int row = 0; row < rows; row++)
                {
                    for (int col = 0; col < cols; col++)
                    {
                        var route = new Route();
                        route.Add(new Coordinate(col, row));
                        result.AddRange(allRoutes(map, route));
                    }
                }
            }
            else
            {
                var start = currentRoute.Last();
                var found = false;

                var west = new Coordinate(start.x - 1, start.y);
                if (west.x >= 0 && map[west.x, west.y] > map[start.x, start.y])
                {
                    var route = new Route();
                    route.AddRange(currentRoute);
                    route.Add(west);
                    result.AddRange(allRoutes(map, route));
                    found = true;
                }

                var east = new Coordinate(start.x + 1, start.y);
                if (east.x < cols && map[east.x, east.y] > map[start.x, start.y])
                {
                    var route = new Route();
                    route.AddRange(currentRoute);
                    route.Add(east);
                    result.AddRange(allRoutes(map, route));
                    found = true;
                }

                var north = new Coordinate(start.x, start.y - 1);
                if (north.y >= 0 && map[north.x, north.y] > map[start.x, start.y])
                {
                    var route = new Route();
                    route.AddRange(currentRoute);
                    route.Add(north);
                    result.AddRange(allRoutes(map, route));
                    found = true;
                }

                var south = new Coordinate(start.x, start.y + 1);
                if (south.y < rows && map[south.x, south.y] > map[start.x, start.y])
                {
                    var route = new Route();
                    route.AddRange(currentRoute);
                    route.Add(south);
                    result.AddRange(allRoutes(map, route));
                    found = true;
                }

                if (!found)
                {
                    currentRoute.Reverse();
                    result.Add(currentRoute);
                }
            }

            return result;
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

    public struct Coordinate
    {
        public readonly int x, y;

        public Coordinate(int x, int y)
        {
            this.x = x;
            this.y = y;
        }
    }

    public class Route : List<Coordinate> { };
}
