using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace Convert
{
    public class Entry
    {
        public Entry()
        {
        }

        public Entry(string time, long durationInMs)
        {
            Time = time;
            DurationInMs = durationInMs;
        }

        public string Time { get; set; }
        public long DurationInMs { get; set; }
    }



    class Program
    {
        // Policy Constants
        private static readonly int TIME = 3;
        private static readonly int MS = 2;

        static void Main(string[] args)
        {
            string filename = "";
            string delimiter = "";

            // Assumptions:
            // The first argument is the name of the file to read
            // The file is using a delimiter specified by the second argument
            if (args[0] == "")
            {
                Console.WriteLine("First argument must be a valid filename, including path.");
            }
            else
            {
                filename = args[0];
            }

            if (args[1] == "")
            {
                Console.WriteLine("Second argument must be the delimiter used in the file to separate fields.");
            }
            else
            {
                delimiter = args[1];
            }

            var series = ProcessPolicyFile(filename, delimiter);
            WriteSeries(filename, series);
        }


        private static List<Entry> ProcessPolicyFile(string fileName, string delimiter)
        {
            bool firstLine = true;
            List<Entry> rules = new List<Entry>();
            string r = File.ReadAllText(fileName, Encoding.ASCII);
            string[] lines = r.Split("\r\n");

            foreach (string line in lines)
            {
                string[] elements = line.Split(delimiter);
                if (firstLine)
                {
                    // The first line of the file should contain headings. Save them minus the first few columns
                    // which contain none metric information.
                    firstLine = false;
                }
                else
                {
                    if (elements.Length == 9)
                    {
                        // Get the time
                        var time = GetTime(elements[TIME]);

                        // For each line create an object with the data retrieved
                        var rule = new Entry(time,
                            long.Parse(elements[MS])
                            );
                        rules.Add(rule);
                    }
                    else
                    {
                        Console.WriteLine("Line in file is missing parameters, element 0 = {0}.", elements[0]);
                    }
                }
            }

            return rules;
        }

        private static string GetTime(string dateTime)
        {
            // find this is an IP address or a range
            var expr = @"\d+-\d+-\d+T(\d+):(\d+):(\d+).(\d+)-";
            MatchCollection mc = Regex.Matches(dateTime, expr);
            string returnVal = "";

            switch (mc.Count)
            {
                case 1:
                    // Treat as the IP address and erase the other fields
                    returnVal = mc[0].Groups[1] + ":" + mc[0].Groups[2] + ":" + mc[0].Groups[3] + "." + mc[0].Groups[4];
                    break;
                default:
                    Console.WriteLine("No match for time pattern in dateTime = {0}.", dateTime);
                    break;
            }

            return returnVal;
        }


        private static void WriteSeries(string filename, List<Entry> series)
        {
            //before your loop
            var csv = new StringBuilder();

            foreach (Entry e in series)
            {
                var newLine = string.Format("{0},{1}", e.Time, e.DurationInMs);
                csv.AppendLine(newLine);
            }

            //after your loop
            var csvFilename = Path.GetDirectoryName(filename) + Path.DirectorySeparatorChar + Path.GetFileNameWithoutExtension(filename) + ".csv";
            File.WriteAllText(csvFilename, csv.ToString());
        }

    }
}
