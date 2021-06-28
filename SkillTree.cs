using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AoETreeEditor
{
    class SkillTree
    {
        private string[,] Tree;
        public int Rows { get; }
        public int Columns { get; }
        private Point Center { get; set; }

        public SkillTree(int rows = 200, int columns = 200, bool emptyCenter = false)
        {
            Tree = new string[rows, columns];
            Rows = rows;
            Columns = columns;
            if (!emptyCenter) SetCenterText();
        }

        public string Set(int x, int y, string value)
        {
            try
            {
                Tree[x, y] = value;
                return Tree[x, y];
            } catch (Exception e)
            {
                Console.WriteLine(e.Message);
                Console.WriteLine($"Failed to write {value} to tree cell: [{x}][{y}]");
                return null;
            }
        }

        public string Get(int x, int y)
        {
            try
            {
                return Tree[y, x];
            }
            catch (Exception e)
            {
                Console.WriteLine($"Failed to read value to tree cell: [{x}][{y}]");
                return null;
            }
        }

        public override string ToString()
        {
            string output = "";

            for (int y = 0; y < Rows; y++)
            {
                for (int x = 0; x < Columns; x++)
                {
                    output += $"{Tree[y,x]}";
                    if (x + 1 < Columns) output += ",";
                }
                output += Environment.NewLine;
            }

            return output;
        }

        private void SetCenterText()
        {
            Tree[Columns / 2, Rows / 2] = "[CENTER]";
            SetCenter(new Point(Columns / 2, Rows / 2));
        }

        private void SetCenterText(Point center)
        {
            Tree[center.Y, center.X] = "[CENTER]";
        }

        public void ReplaceCenter(Point center)
        {
            Set(GetCenter().Y, GetCenter().X, "");
            SetCenter(new Point(center.Y, center.X));
        }

        public Point GetCenter()
        {
            return this.Center;
        }

        public void SetCenter(Point center)
        {
            this.Center = center;
        }

        public static SkillTree Parse(string input)
        {
            String[] lines = input.Split(new String[] { Environment.NewLine }, StringSplitOptions.None);
            int x = 0, y = 0;

            Console.WriteLine("Attempting to parse string into SkillTree...");
            Console.WriteLine("Determining size...");
            foreach (String line in lines)
            {
                Console.WriteLine($"Parsing line: {line}");
                if (line.Length < 10) continue;
                y++;
                int elements = line.Split(new char[] { ',' }).Length;
                if (x < elements)
                {
                    x = elements;
                }
            }
            Console.WriteLine($"Complete! Found max rows: {y} Max columns: {x}");

            SkillTree output = new SkillTree(y, x, true);
            Console.WriteLine($"Generated new skill tree with above dimensions.");
            Console.WriteLine($"Attempting to fill with parsed data...");

            y = 0;
            string debugRow;
            bool foundCenter = false;
            foreach(String line in lines)
            {
                if (line.Length < 5) continue;
                Console.WriteLine($"Row {y}:");
                x = 0;
                debugRow = "";
                foreach (String entry in line.Split(new char[] { ',' }))
                {
                    if (entry == "[CENTER]")
                    {
                        output.SetCenter(new Point(x, y));
                        foundCenter = true;
                    }
                    output.Set(y, x++, entry);
                    debugRow += entry;
                    if (output.Columns > x) debugRow += ',';
                }
                Console.WriteLine($"Contents: {debugRow}");
                y++;
            }
            Console.WriteLine($"Parse complete! Created tree with dimensions: {output.Rows} : {output.Columns}");
            if (foundCenter)
            {
                Console.WriteLine($"and center at { output.GetCenter()}");
            } else
            {
                Console.WriteLine($"With no center found!");
                output.Center = new Point(-1, -1);
            }

            return output;
        }
    }
}
