using System;
using System.Collections.Generic;
using System.Text;

namespace CalCLI.NET
{
    class Program
    {
        static void Main()
        {
            var sheet = new SpreadSheet(11, 11);
            while(true)
            {
                sheet.Render(); // render the sheet
                sheet.PrintDebug();
                sheet.Browse();
            }
            
        }
    }

    class SpreadSheet
    {
        private List<List<Cell>> Cells; // next row is just calculated based on rows and columns ints
        private int[] selected; // currently selected
        private int columns;
        private int rows;
        /*
         * mode = 0 - browse
         * mode = 1 - edit
         */
        private int mode;
        /*
         * topleft - where does rendering start
         * widthBuffer - how many columns to print
         * heightBuffer - how many rows to print
         */
        private int[] topleft;
        private int widthBuffer;
        private int heightBuffer;
        private int consoleWidth;
        private ConsoleKey lastKeyEvent;
        public SpreadSheet(int columns = 10, int rows = 10)
        {
            this.lastKeyEvent = ConsoleKey.D0;
            this.consoleWidth = Console.WindowWidth;
            this.widthBuffer = 10;
            this.heightBuffer = 9;
            this.mode = 0;
            this.selected = new int[] {1,1};
            this.topleft = new int[] { 1, 1 };
            this.columns = columns;
            this.rows = rows;
            Cells = Generate();
            Update();
        }

        private List<List<Cell>> Generate()
        {
            var list = new List<List<Cell>>();

            list.Add(new List<Cell>());
            list[0].Add(new Cell(false, "chead"));
            for (int i = 1; i < this.columns; i++)
            {
                list[0].Add(new Cell(false, "chead", $"{i}", "", null));
            }
            for (int i = 1; i < this.rows; i++)
            {
                list.Add(new List<Cell>());
                list[i].Add(new Cell(false, "rhead", $"{i}", "", list[0][0].format));
                for (int j = 1; j < this.columns; j++)
                {
                    list[i].Add(new Cell(true, "str", "", "", list[0][j].format));
                }
            }

            return list;
        }

        public void PrintDebug()
        {
            Console.WriteLine("");
            Console.WriteLine($"Mode: {this.mode}");
            Console.WriteLine($"Sheet columns: {this.columns}");
            Console.WriteLine($"Sheet rows: {this.rows}");
            Console.WriteLine($"Console Width: {this.consoleWidth}");
            Console.WriteLine($"Width Buffer: {this.widthBuffer}");
            Console.WriteLine($"Height Buffer: {this.heightBuffer}");
            Console.WriteLine($"Top Left Rendered: [{this.topleft[0]}, {this.topleft[1]}]");
            Console.WriteLine($"Selected: [{this.selected[0]}, {this.selected[1]}]");
            Console.WriteLine($"Last Key Event: {this.lastKeyEvent}");

        }

        public void Render()
        {

            Console.CursorVisible = false; // set cursor invisible for rendering
            Console.SetCursorPosition(0, 0); // move cursor to top left corner

            /* check if console width changed to avoid some potential fuckery */
            if (this.consoleWidth != Console.WindowWidth)
            {
                Console.Clear();
                this.consoleWidth = Console.WindowWidth;
            }

            var sb = new StringBuilder();
            
            /* calculate HR width*/
            int renderedWidth = (this.Cells[0][0].format[0] + (this.Cells[0][0].format[2] * 2));
            for (int i = 0, j = topleft[1]; i < this.widthBuffer && j < this.Cells[0].Count; j++, i++)
                renderedWidth += (this.Cells[0][j].format[0] + (this.Cells[0][j].format[2] * 2) + 1);

            /* render column headers */
            /* hr */
            sb.Append(' ');
            for (int i = 0; i < renderedWidth; i++) 
                sb.Append('─');
            sb.Append('\n');
            sb.Append('|');
            Console.Write(sb.ToString());

            this.Cells[0][0].Print();

            /* rest of colum headers */
            for (int j = this.topleft[1]; j < (j + this.widthBuffer) && j < this.Cells[0].Count; j++)
            {
                this.Cells[0][j].Print();
            }

            /* hr */
            sb.Clear();
            sb.Append('\n');
            sb.Append(' ');
            for (int i = 0; i < renderedWidth; i++) 
                sb.Append('─');
            Console.WriteLine(sb.ToString());
            
            /* rows */
            for (int i = this.topleft[0]; i < (i + this.heightBuffer) && i < this.Cells.Count; i++)
            {
                /* row header*/
                Console.Write('|');
                this.Cells[i][0].Print();

                /* rows */
                for (int j = this.topleft[1]; j < (j + this.widthBuffer) && j < this.Cells[i].Count; j++)
                {
                    if(i == this.selected[0] && j == this.selected[1])
                        this.Cells[i][j].Print(true);
                    else
                        this.Cells[i][j].Print();
                }
                /* hr */
                sb.Clear();
                sb.Append('\n');
                sb.Append(' ');
                for (int j = 0; j < renderedWidth; j++)
                    sb.Append('─');
                sb.Append('\n');
                Console.Write(sb.ToString());
            }
        }

        public void Update()
        {
            /* update cheader info */
            /* update column width info */
        }

        public void Browse()
        {
            var key = Console.ReadKey(true);
            switch (key.Key)
            {
                case ConsoleKey.DownArrow:
                    if (this.selected[0] != (this.rows - 1))
                        this.selected[0]++;
                    break;
                case ConsoleKey.UpArrow:
                    if (this.selected[0] != 1)
                        this.selected[0]--;
                    break;
                case ConsoleKey.LeftArrow:
                    if (this.selected[1] != 1)
                        this.selected[1]--;
                    break;
                case ConsoleKey.RightArrow:
                    if (this.selected[1] != (this.columns - 1))
                        this.selected[1]++;
                    break;
                case ConsoleKey.Enter:
                    this.mode = 1;
                    this.Render();
                    this.PrintDebug();
                    this.Cells[this.selected[0]][this.selected[1]].Edit();
                    this.mode = 0;
                    break;
                default:
                    break;
            }
            this.lastKeyEvent = key.Key;
        }
    }

    class Cell
    {
        /* datatype of the cell
         *      cheader - column
         *      rheader - row
         *          value = Column/Row name
         *  
         */
        public string dataType;
        /* user input */
        public string input;
        /* calculated from input */
        public string value;
        /* is the cell selectable */
        public bool selectable;
        /* cell formatting indexes
         * 0 - width
         * 1 - height
         * 2 - margin
         * 3 - foreground color
         * 4 - background color
         *      0 - black
         *      1 - white
         */
        public int[] format;
        

        public Cell(bool select = true, string dataType = "str", string value = "", string input = "", int[] format = null)
        {
            this.dataType = dataType;
            this.input = input;
            this.value = value;
            this.selectable = select;
            if (format != null) this.format = format;
            else this.format = new int[] { 4, 1, 1, 1, 0 };

        }

        private void Update() 
        {
            if(this.value.Length>this.format[0]) this.format[0] = this.value.Length;
        }

        private void FunctionParser() { }

        public void Type()
        {
            // auto detect cell data type
        }

        public void Edit()
        {
            // cell edit loop
            var pos = Console.GetCursorPosition();
            Console.SetCursorPosition(pos.Left, pos.Top);
            Console.Write(this.value);
            Console.CursorVisible = true;
            this.value = Console.ReadLine();
            this.Update();
            return;
        }

        public void Print(bool selected = false) 
        {
            var sb = new StringBuilder();

            /* margin */
            if(!selected)
            {
                for (int i = 0; i < this.format[2]; i++)
                    sb.Append(' ');
            }
            else
            {
                sb.Append('╠');
                for (int i = 0; i < this.format[2]-1; i++)
                    sb.Append(' ');
            }

            /* cell whitespace for center align */
            for (int i = 0; i < (this.format[0] - this.value.Length) / 2; i++)
                sb.Append(' ');

            /* cell value */
            sb.Append(this.value);

            /* cell whitespace for center align */
            for (int i = 0; i < (this.format[0] - this.value.Length) / 2; i++)
                sb.Append(' ');

            /* cell whitespace correction for center align */
            for (int i = 0; i < (this.format[0] - this.value.Length) % 2; i++)
                sb.Append(' ');

            /* margin */
            if (!selected)
            {
                for (int i = 0; i < this.format[2]; i++)
                    sb.Append(' ');
            }
            else
            {
                sb.Append('╣');
                for (int i = 0; i < this.format[2] - 1; i++)
                    sb.Append(' ');
            }
            sb.Append('|');

            Console.Write(sb.ToString());
        }

        
    }

    class Functions { }
}
