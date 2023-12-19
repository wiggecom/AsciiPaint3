namespace AsciiPaint3
{
    internal class Program
    {
        public static int cursorPosX = 0;
        public static int cursorPosY = 0;
        public static int colorIntFG = 8;
        public static int colorIntBG = 0;
        public static int firstPlot = 0;
        public static int toggleRoller = 0;
        // Undo
        public static string rememberChar = "█";
        public static string rememberFG = "8";
        public static string rememberBG = "0";
        public static int rememberPosX = 0;
        public static int rememberPosY = 0;

        public static int addLastFiveOrNot = 0;


        public static string colorSelectFG = "8";
        public static string colorSelectBG = "0";
        public static string characterPlot = "█";
        public static string specialChar = "#";

        // Full Frame 3, 50, 200
        //Skull etc: 27, 87

        public static int drawAreaHeight = 40;  //  37 // 50
        public static int drawAreaWidth = 120;  // 157 // 200
        public static int palettePosX = 1; // Place palette anywhere
        public static int palettePosY = drawAreaHeight + 3;

        public static int drawAreaFrameStartX = 0;
        public static int drawAreaFrameStartY = 0;

        public static string nameDrawing = "empty";
        public static string[] lastFive = new string[5];
        static void Main(string[] args)
        {

            #region Set Window Size
            if (drawAreaWidth >= 80)
            {
                if (drawAreaHeight >= 30)
                {
                    Console.SetWindowSize(drawAreaWidth + 33, drawAreaHeight + 11);
                    Console.SetBufferSize(drawAreaWidth + 33, drawAreaHeight + 11);
                }
                else
                {
                    Console.SetWindowSize(drawAreaWidth + 33, 40);
                    Console.SetBufferSize(drawAreaWidth + 33, 40);
                }
            }
            else
            {
                if (drawAreaHeight >= 30)
                {
                    Console.SetWindowSize(120, drawAreaHeight + 11);
                    Console.SetBufferSize(120, drawAreaHeight + 11);
                }
                else
                {
                    Console.SetWindowSize(120, 40);
                    Console.SetBufferSize(120, 40);
                }
            }
            #endregion

            string[,,] drawing = new string[drawAreaHeight, drawAreaWidth, 3];
            //            string[,,] drawing = new string[drawAreaHeight + 1, drawAreaWidth + 1, 3];   ---------------------------------------------------------------
            int allStrings = (drawing.GetLength(0) * drawing.GetLength(1) * drawing.GetLength(2)); // string for file
            string[] allPixels = new string[allStrings];

            InitColorDrawing(drawing); // Initiate FG Color to zero, add BG
            Console.CursorSize = 100;
            PaletteDrawFG(palettePosX);
            PaletteDrawBG(palettePosX, palettePosY);
            DrawAreaFrame(drawAreaWidth, drawAreaHeight, drawAreaFrameStartX, drawAreaFrameStartY);
            //DrawAreaFrame(drawAreaWidth - 1, drawAreaHeight - 1, drawAreaFrameStartX, drawAreaFrameStartY);
            Console.ForegroundColor = ConsoleColor.Cyan;
            DrawHistoryFrame(drawAreaWidth, drawAreaHeight, drawAreaFrameStartX, drawAreaFrameStartY);
            Console.ForegroundColor = ConsoleColor.Red;
            DrawHotkeysFrame(drawAreaWidth, drawAreaHeight, drawAreaFrameStartX, drawAreaFrameStartY); //
            PrintMenu();
            DrawDrawing(drawing);
            LastFive();
            Console.SetCursorPosition(cursorPosX + 1, cursorPosY + 1);
            while (true)
            {
                drawing = KeysInput(drawing, allPixels, palettePosX, palettePosY, drawAreaHeight, drawAreaWidth, allStrings);
                UpdateDrawing(drawing);
            }
        }
        //-------------------------- Files I/O ------------------------

        public static string[] LoadFile(string[] allPixels, string[,,] drawing, int allStrings)
        {
            if (File.Exists(nameDrawing + ".wci"))
            {
                allPixels = File.ReadAllLines(nameDrawing + ".wci");
                int x = 0;
                for (int i = 0; i < drawing.GetLength(0); i++)
                {
                    for (int j = 0; j < drawing.GetLength(1); j++)
                    {
                        for (int k = 0; k < drawing.GetLength(2); k++)
                        {
                                drawing[i, j, k] = allPixels[x];
                            x++;
                        }
                    }
                }
                DrawDrawing(drawing);
            }
            else if (File.Exists(nameDrawing))
            {
                allPixels = File.ReadAllLines(nameDrawing);
                int x = 0;
                for (int i = 0; i < drawing.GetLength(0); i++)
                {
                    for (int j = 0; j < drawing.GetLength(1); j++)
                    {
                        for (int k = 0; k < drawing.GetLength(2); k++)
                        {
                            drawing[i, j, k] = allPixels[x];
                            x++;
                        }
                    }
                }
                DrawDrawing(drawing);
            }
            else
            {
                allPixels = new string[allStrings];
            }
            return allPixels;
        }

        static void SaveFile(string[] allPixels, string[,,] drawing)
        {
            int x = 0;
            for (int i = 0; i < drawing.GetLength(0); i++)
            {
                for (int j = 0; j < drawing.GetLength(1); j++)
                {
                    for (int k = 0; k < drawing.GetLength(2); k++)
                    {
                        allPixels[x] = drawing[i, j, k];
                        x++;
                    }
                }
            }
            File.WriteAllLines(nameDrawing + ".wci", allPixels);
        }

        static void ExportFile(string[] allPixels, string[,,] drawing, string nameDrawing)
        {
            int arrayColumns = drawing.GetLength(1) - 1;// -1
            int arrayRows = drawing.GetLength(0);
            List<String> pixelRowList = new List<String>(); // list for all data on row
            List<String> pixelSequenceList = new List<String>(); // list for data in sequence

            // Add code to handle positioning and information
            pixelRowList.Add("public static void " + nameDrawing + "(int gfxLeft, int gfxTop)");
            pixelRowList.Add("{");
            pixelRowList.Add("// columns: " + arrayColumns+1 + ", rows: " + (arrayRows) + ".");

            string[] pixel = new string[3]; // one pixel to pick up char and colors

            string pixelSequence = "";

            int colPos = 0;
            int rowPos = 0;

            while (true)
            {
                pixelRowList.Add("Console.SetCursorPosition(gfxLeft, gfxTop + " + (rowPos) + ");");
                while (colPos <= arrayColumns)
                {
                    // get first pixel of row
                    pixel[1] = drawing[rowPos, colPos, 1];
                    pixel[2] = drawing[rowPos, colPos, 2];
                    pixel[0] = drawing[rowPos, colPos, 0];
                    pixelSequenceList.Add(drawing[rowPos, colPos, 0]);

                    // Add colors as rows in data of the full row
                    string fgColString = "Console.ForegroundColor = (ConsoleColor)" + pixel[1] + ";";
                    pixelRowList.Add(fgColString);
                    string bgColString = "Console.BackgroundColor = (ConsoleColor)" + pixel[2] + ";";
                    pixelRowList.Add(bgColString);

                    // grab one section of data
                    while (true)
                    {
                        if (colPos <= arrayColumns - 1 && drawing[rowPos, colPos + 1, 1] == pixel[1] && drawing[rowPos, colPos + 1, 2] == pixel[2] && ((colPos + 1) < arrayColumns))
                        {
                            colPos++;
                            pixelSequenceList.Add(drawing[rowPos, colPos, 0]);
                        }
                        else
                        {
                            colPos++;
                            break;
                        }
                    }
                    pixelSequence = "Console.Write(\"";
                    foreach (String pixelString in pixelSequenceList)
                    {
                        pixelSequence = pixelSequence + pixelString;
                    }
                    pixelSequence = pixelSequence + "\");";

                    pixelRowList.Add(pixelSequence);
                    pixelSequenceList.Clear();
                    pixelSequence = "";
                }
                rowPos++;
                colPos = 0;
                if (rowPos > arrayRows - 1)
                {
                    break;
                }
            }
            pixelRowList.Add("}");
            #region ScrubCode
            for (int i = 0; i < pixelRowList.Count; i++)
            {
                if (pixelRowList[i] == "Console.ForegroundColor = (ConsoleColor)0;")
                {
                    if (pixelRowList[i + 1] == "Console.BackgroundColor = (ConsoleColor)0;")
                    {
                        if (pixelRowList[i + 2] == "Console.Write(\"\");")
                        {
                            pixelRowList.RemoveAt(i);
                            pixelRowList.RemoveAt(i);
                            pixelRowList.RemoveAt(i);
                            i -= 3;
                        }
                    }
                }
            }
            for (int i = 0; i < pixelRowList.Count; i++)
            {
                if (pixelRowList[i].StartsWith("Console.SetCursorPosition"))
                {
                    if (pixelRowList[i + 1].StartsWith("Console.SetCursorPosition"))
                    {
                        pixelRowList.RemoveAt(i);
                        i--;
                    }
                    else if (pixelRowList[i + 1].StartsWith("}"))
                    {
                        pixelRowList.RemoveAt(i);
                        i--;
                    }
                }
            }
            #endregion

            File.WriteAllLines(nameDrawing + ".txt", pixelRowList);
        }
        //-------------------------------------------------------------
        static string[,,] ClearDrawingColor(string[,,] drawing, int drawAreaHeight, int drawAreaWidth)
        {
            int row = drawAreaHeight;
            int col = drawAreaWidth;
            Console.SetCursorPosition(1, 1);
            for (int i = 0; i < row; i++)
            {
                for (int j = 0; j < col; j++)
                {
                    drawing[i, j, 0] = characterPlot;
                    drawing[i, j, 1] = colorSelectFG;
                    drawing[i, j, 2] = colorSelectBG;
                }
                Console.WriteLine();
            }
            Console.SetCursorPosition(1, 1);
            return drawing;
        }
        static void InitColorDrawing(string[,,] drawing)
        {
            int row = drawing.GetLength(0);
            int col = drawing.GetLength(1);
            for (int i = 0; i < row; i++)
            {
                for (int j = 0; j < col; j++)
                {
                    drawing[i, j, 1] = "0";
                    drawing[i, j, 2] = "0";
                    //drawing[i, j, 0] = " ";
                }
                Console.WriteLine();
            }
        }
        static void DrawDrawing(string[,,] drawing)
        {
            int row = drawing.GetLength(0);
            int col = drawing.GetLength(1);
            for (int i = 0; i < row; i++)
            {
                for (int j = 0; j < col; j++)
                {
                    Console.CursorTop = i + 1;
                    Console.CursorLeft = j + 1;
                    if (drawing[i, j, 1] != null && drawing[i, j, 2]  != null)
                    {
                        int colorFG = int.Parse(drawing[i, j, 1]);
                        int colorBG = int.Parse(drawing[i, j, 2]);
                        Console.ForegroundColor = (ConsoleColor)colorFG;
                        Console.BackgroundColor = (ConsoleColor)colorBG;
                        Console.Write(drawing[i, j, 0]);
                    }
                }
                Console.WriteLine();
            }
        }
        static void UpdateDrawing(string[,,] drawing)
        {
            int i = Console.CursorTop - 1;
            int j = Console.CursorLeft - 1;

            if (drawing[i, j, 1] != null && drawing[i, j, 2] != null)
            {
                int colorFG = int.Parse(drawing[i, j, 1]);
                int colorBG = int.Parse(drawing[i, j, 2]);
                Console.ForegroundColor = (ConsoleColor)colorFG;
                Console.BackgroundColor = (ConsoleColor)colorBG;
                Console.Write(drawing[i, j, 0]);
            }
        }
        static string[,,] KeysInput(string[,,] drawing, string[] allPixels, int palettePosX, int palettePosY, int drawAreaHeight, int drawAreaWidth, int allStrings)
        {
            if (firstPlot == 0)
            {
                // Set first colorSelectFG pointer
                Console.SetCursorPosition(palettePosX + 1 + (1 * 3), Program.drawAreaHeight + 6);
                Console.ForegroundColor = ConsoleColor.White;
                Console.Write("F");
                // Set first colorSelectBG pointer
                Console.SetCursorPosition(palettePosX + 1, Program.drawAreaHeight + 7);
                Console.ForegroundColor = ConsoleColor.White;
                Console.Write("B");
                // Set first characterPlot pointer
                Console.SetCursorPosition(palettePosX + 54, palettePosY + 3);
                Console.Write("^");
                Console.SetCursorPosition(1, 1);
                firstPlot = 1;
            }
            else
            {
                Console.SetCursorPosition(cursorPosX, cursorPosY);
            }
            while (true)
            {
                colorIntFG = int.Parse(colorSelectFG);
                colorIntBG = int.Parse(colorSelectBG);
                cursorPosX = Console.CursorLeft;
                cursorPosY = Console.CursorTop;
                KeysDrawBlocks(palettePosX, palettePosY, colorIntFG, colorIntBG);
                KeysCopyChars(palettePosX, palettePosY, colorIntFG, colorIntBG);
                Console.SetCursorPosition(cursorPosX, cursorPosY);

                var userInputKey = Console.ReadKey(true);
                //
                //  ------------ 50 Keys Total -----------------
                //
                // ------------------------------------------------------------- PlotChar - 8 Keys ------------------------------------------
                #region PlotChar
                if (userInputKey.Key == ConsoleKey.Q)
                {
                    characterPlot = "█";
                    ClearCharSelector();
                    Console.SetCursorPosition(palettePosX + 54, palettePosY + 3);
                    Console.Write("^");
                    Console.SetCursorPosition(cursorPosX, cursorPosY);
                }
                if (userInputKey.Key == ConsoleKey.W)
                {
                    characterPlot = "▓";
                    ClearCharSelector();
                    Console.SetCursorPosition(palettePosX + 58, palettePosY + 3);
                    Console.Write("^");
                    Console.SetCursorPosition(cursorPosX, cursorPosY);
                }
                if (userInputKey.Key == ConsoleKey.E)
                {
                    characterPlot = "▒";
                    ClearCharSelector();
                    Console.SetCursorPosition(palettePosX + 62, palettePosY + 3);
                    Console.Write("^");
                    Console.SetCursorPosition(cursorPosX, cursorPosY);
                }
                if (userInputKey.Key == ConsoleKey.R)
                {
                    characterPlot = "░";
                    ClearCharSelector();
                    Console.SetCursorPosition(palettePosX + 66, palettePosY + 3);
                    Console.Write("^");
                    Console.SetCursorPosition(cursorPosX, cursorPosY);
                }
                if (userInputKey.Key == ConsoleKey.T)
                {
                    characterPlot = specialChar;
                    ClearCharSelector();
                    Console.SetCursorPosition(palettePosX + 70, palettePosY + 3);
                    Console.Write("^");
                    Console.SetCursorPosition(cursorPosX, cursorPosY);
                }
                if (userInputKey.Key == ConsoleKey.Y)
                {
                    characterPlot = "▄";
                    ClearCharSelector();
                    Console.SetCursorPosition(palettePosX + 74, palettePosY + 3);
                    Console.Write("^");
                    Console.SetCursorPosition(cursorPosX, cursorPosY);
                }
                if (userInputKey.Key == ConsoleKey.U)
                {
                    characterPlot = "▀";
                    ClearCharSelector();
                    Console.SetCursorPosition(palettePosX + 78, palettePosY + 3);
                    Console.Write("^");
                    Console.SetCursorPosition(cursorPosX, cursorPosY);
                }
                if (userInputKey.Key == ConsoleKey.I)
                {
                    characterPlot = "▬";
                    ClearCharSelector();
                    Console.SetCursorPosition(palettePosX + 82, palettePosY + 3);
                    Console.Write("^");
                    Console.SetCursorPosition(cursorPosX, cursorPosY);
                }
                #endregion
                //
                // ---------------------------------------------------------- FG Color Select - 16 Keys -------------------------------------
                #region FG Color Select
                if (userInputKey.Key == ConsoleKey.F1)
                {
                    colorSelectFG = "0";
                    ClearFGSelector();
                    Console.SetCursorPosition(palettePosX + 1 + (0 * 3), palettePosY + 3);
                    Console.Write("F");
                    Console.SetCursorPosition(cursorPosX, cursorPosY);
                }
                if (userInputKey.Key == ConsoleKey.F2)
                {
                    colorSelectFG = "8";
                    ClearFGSelector();
                    Console.SetCursorPosition(palettePosX + 1 + (1 * 3), palettePosY + 3);
                    Console.Write("F");
                    Console.SetCursorPosition(cursorPosX, cursorPosY);
                }
                if (userInputKey.Key == ConsoleKey.F3)
                {
                    colorSelectFG = "7";
                    ClearFGSelector();
                    Console.SetCursorPosition(palettePosX + 1 + (2 * 3), palettePosY + 3);
                    Console.Write("F");
                    Console.SetCursorPosition(cursorPosX, cursorPosY);
                }
                if (userInputKey.Key == ConsoleKey.F4)
                {
                    colorSelectFG = "15";
                    ClearFGSelector();
                    Console.SetCursorPosition(palettePosX + 1 + (3 * 3), palettePosY + 3);
                    Console.Write("F");
                    Console.SetCursorPosition(cursorPosX, cursorPosY);
                }
                if (userInputKey.Key == ConsoleKey.F5)
                {
                    colorSelectFG = "1";
                    ClearFGSelector();
                    Console.SetCursorPosition(palettePosX + 1 + (4 * 3), palettePosY + 3);
                    Console.Write("F");
                    Console.SetCursorPosition(cursorPosX, cursorPosY);
                }
                if (userInputKey.Key == ConsoleKey.F6)
                {
                    colorSelectFG = "9";
                    ClearFGSelector();
                    Console.SetCursorPosition(palettePosX + 1 + (5 * 3), palettePosY + 3);
                    Console.Write("F");
                    Console.SetCursorPosition(cursorPosX, cursorPosY);
                }
                if (userInputKey.Key == ConsoleKey.F7)
                {
                    colorSelectFG = "3";
                    ClearFGSelector();
                    Console.SetCursorPosition(palettePosX + 1 + (6 * 3), palettePosY + 3);
                    Console.Write("F");
                    Console.SetCursorPosition(cursorPosX, cursorPosY);
                }
                if (userInputKey.Key == ConsoleKey.F8)
                {
                    colorSelectFG = "11";
                    ClearFGSelector();
                    Console.SetCursorPosition(palettePosX + 1 + (7 * 3), palettePosY + 3);
                    Console.Write("F");
                    Console.SetCursorPosition(cursorPosX, cursorPosY);
                }
                if (userInputKey.Key == ConsoleKey.F9)
                {
                    colorSelectFG = "2";
                    ClearFGSelector();
                    Console.SetCursorPosition(palettePosX + 1 + (8 * 3), palettePosY + 3);
                    Console.Write("F");
                    Console.SetCursorPosition(cursorPosX, cursorPosY);
                }
                if (userInputKey.Key == ConsoleKey.F10)
                {
                    colorSelectFG = "10";
                    ClearFGSelector();
                    Console.SetCursorPosition(palettePosX + 1 + (9 * 3), palettePosY + 3);
                    Console.Write("F");
                    Console.SetCursorPosition(cursorPosX, cursorPosY);
                }
                if (userInputKey.Key == ConsoleKey.D1)
                {
                    colorSelectFG = "6";
                    ClearFGSelector();
                    Console.SetCursorPosition(palettePosX + 1 + (10 * 3), palettePosY + 3);
                    Console.Write("F");
                    Console.SetCursorPosition(cursorPosX, cursorPosY);
                }
                if (userInputKey.Key == ConsoleKey.D2)
                {
                    colorSelectFG = "14";
                    ClearFGSelector();
                    Console.SetCursorPosition(palettePosX + 1 + (11 * 3), palettePosY + 3);
                    Console.Write("F");
                    Console.SetCursorPosition(cursorPosX, cursorPosY);
                }
                if (userInputKey.Key == ConsoleKey.D3)
                {
                    colorSelectFG = "5";
                    ClearFGSelector();
                    Console.SetCursorPosition(palettePosX + 1 + (12 * 3), palettePosY + 3);
                    Console.Write("F");
                    Console.SetCursorPosition(cursorPosX, cursorPosY);
                }
                if (userInputKey.Key == ConsoleKey.D4)
                {
                    colorSelectFG = "13";
                    ClearFGSelector();
                    Console.SetCursorPosition(palettePosX + 1 + (13 * 3), palettePosY + 3);
                    Console.Write("F");
                    Console.SetCursorPosition(cursorPosX, cursorPosY);
                }
                if (userInputKey.Key == ConsoleKey.D5)
                {
                    colorSelectFG = "4";
                    ClearFGSelector();
                    Console.SetCursorPosition(palettePosX + 1 + (14 * 3), palettePosY + 3);
                    Console.Write("F");
                    Console.SetCursorPosition(cursorPosX, cursorPosY);
                }
                if (userInputKey.Key == ConsoleKey.D6)
                {
                    colorSelectFG = "12";
                    ClearFGSelector();
                    Console.SetCursorPosition(palettePosX + 1 + (15 * 3), palettePosY + 3);
                    Console.Write("F");
                    Console.SetCursorPosition(cursorPosX, cursorPosY);
                }
                #endregion
                //
                // --------------------------------------------------------- BG Color Select - 16 Keys --------------------------------------
                #region BG Color Select
                if (userInputKey.Key == ConsoleKey.A)
                {
                    colorSelectBG = "0";
                    ClearBGSelector();
                    Console.SetCursorPosition(palettePosX + 1 + (0 * 3), palettePosY + 4);
                    Console.Write("B");
                    Console.SetCursorPosition(cursorPosX, cursorPosY);
                }
                if (userInputKey.Key == ConsoleKey.S)
                {
                    colorSelectBG = "8";
                    ClearBGSelector();
                    Console.SetCursorPosition(palettePosX + 1 + (1 * 3), palettePosY + 4);
                    Console.Write("B");
                    Console.SetCursorPosition(cursorPosX, cursorPosY);
                }
                if (userInputKey.Key == ConsoleKey.D)
                {
                    colorSelectBG = "7";
                    ClearBGSelector();
                    Console.SetCursorPosition(palettePosX + 1 + (2 * 3), palettePosY + 4);
                    Console.Write("B");
                    Console.SetCursorPosition(cursorPosX, cursorPosY);
                }
                if (userInputKey.Key == ConsoleKey.F)
                {
                    colorSelectBG = "15";
                    ClearBGSelector();
                    Console.SetCursorPosition(palettePosX + 1 + (3 * 3), palettePosY + 4);
                    Console.Write("B");
                    Console.SetCursorPosition(cursorPosX, cursorPosY);
                }
                if (userInputKey.Key == ConsoleKey.G)
                {
                    colorSelectBG = "1";
                    ClearBGSelector();
                    Console.SetCursorPosition(palettePosX + 1 + (4 * 3), palettePosY + 4);
                    Console.Write("B");
                    Console.SetCursorPosition(cursorPosX, cursorPosY);
                }
                if (userInputKey.Key == ConsoleKey.H)
                {
                    colorSelectBG = "9";
                    ClearBGSelector();
                    Console.SetCursorPosition(palettePosX + 1 + (5 * 3), palettePosY + 4);
                    Console.Write("B");
                    Console.SetCursorPosition(cursorPosX, cursorPosY);
                }
                if (userInputKey.Key == ConsoleKey.J)
                {
                    colorSelectBG = "3";
                    ClearBGSelector();
                    Console.SetCursorPosition(palettePosX + 1 + (6 * 3), palettePosY + 4);
                    Console.Write("B");
                    Console.SetCursorPosition(cursorPosX, cursorPosY);
                }
                if (userInputKey.Key == ConsoleKey.K)
                {
                    colorSelectBG = "11";
                    ClearBGSelector();
                    Console.SetCursorPosition(palettePosX + 1 + (7 * 3), palettePosY + 4);
                    Console.Write("B");
                    Console.SetCursorPosition(cursorPosX, cursorPosY);
                }
                if (userInputKey.Key == ConsoleKey.L)
                {
                    colorSelectBG = "2";
                    ClearBGSelector();
                    Console.SetCursorPosition(palettePosX + 1 + (8 * 3), palettePosY + 4);
                    Console.Write("B");
                    Console.SetCursorPosition(cursorPosX, cursorPosY);
                }
                if (userInputKey.Key == ConsoleKey.Z)
                {
                    colorSelectBG = "10";
                    ClearBGSelector();
                    Console.SetCursorPosition(palettePosX + 1 + (9 * 3), palettePosY + 4);
                    Console.Write("B");
                    Console.SetCursorPosition(cursorPosX, cursorPosY);
                }
                if (userInputKey.Key == ConsoleKey.X)
                {
                    colorSelectBG = "6";
                    ClearBGSelector();
                    Console.SetCursorPosition(palettePosX + 1 + (10 * 3), palettePosY + 4);
                    Console.Write("B");
                    Console.SetCursorPosition(cursorPosX, cursorPosY);
                }
                if (userInputKey.Key == ConsoleKey.C)
                {
                    colorSelectBG = "14";
                    ClearBGSelector();
                    Console.SetCursorPosition(palettePosX + 1 + (11 * 3), palettePosY + 4);
                    Console.Write("B");
                    Console.SetCursorPosition(cursorPosX, cursorPosY);
                }
                if (userInputKey.Key == ConsoleKey.V)
                {
                    colorSelectBG = "5";
                    ClearBGSelector();
                    Console.SetCursorPosition(palettePosX + 1 + (12 * 3), palettePosY + 4);
                    Console.Write("B");
                    Console.SetCursorPosition(cursorPosX, cursorPosY);
                }
                if (userInputKey.Key == ConsoleKey.B)
                {
                    colorSelectBG = "13";
                    ClearBGSelector();
                    Console.SetCursorPosition(palettePosX + 1 + (13 * 3), palettePosY + 4);
                    Console.Write("B");
                    Console.SetCursorPosition(cursorPosX, cursorPosY);
                }
                if (userInputKey.Key == ConsoleKey.N)
                {
                    colorSelectBG = "4";
                    ClearBGSelector();
                    Console.SetCursorPosition(palettePosX + 1 + (14 * 3), palettePosY + 4);
                    Console.Write("B");
                    Console.SetCursorPosition(cursorPosX, cursorPosY);
                }
                if (userInputKey.Key == ConsoleKey.M)
                {
                    colorSelectBG = "12";
                    ClearBGSelector();
                    Console.SetCursorPosition(palettePosX + 1 + (15 * 3), palettePosY + 4);
                    Console.Write("B");
                    Console.SetCursorPosition(cursorPosX, cursorPosY);
                }
                #endregion
                //
                // ----------------------------------------------------------- Navigation - 4 Keys ----------------------------------------
                #region Navigation
                if (userInputKey.Key == ConsoleKey.UpArrow)
                {
                    if (Console.CursorTop >= 2)
                    {
                        if (toggleRoller == 0)
                        {
                            cursorPosY--;
                            Console.SetCursorPosition(cursorPosX, cursorPosY);
                        }
                        else if (toggleRoller == 1)
                        {
                            int row = Console.CursorTop - 1;
                            int col = Console.CursorLeft - 1;

                            drawing[row, col, 0] = characterPlot;
                            drawing[row, col, 1] = colorSelectFG;
                            drawing[row, col, 2] = colorSelectBG;
                            UpdateDrawing(drawing);
                            cursorPosY--;
                            Console.SetCursorPosition(cursorPosX, cursorPosY);

                        }

                    }
                }
                if (userInputKey.Key == ConsoleKey.DownArrow)
                {
                    if (Console.CursorTop <= drawAreaHeight-1)
                    {
                        if (toggleRoller == 0)
                        {
                            cursorPosY++;
                            Console.SetCursorPosition(cursorPosX, cursorPosY);
                        }
                        else if (toggleRoller == 1)
                        {
                            int row = Console.CursorTop - 1;
                            int col = Console.CursorLeft - 1;

                            drawing[row, col, 0] = characterPlot;
                            drawing[row, col, 1] = colorSelectFG;
                            drawing[row, col, 2] = colorSelectBG;
                            UpdateDrawing(drawing);
                            cursorPosY++;
                            Console.SetCursorPosition(cursorPosX, cursorPosY);
                        }
                    }
                }
                if (userInputKey.Key == ConsoleKey.LeftArrow)
                {
                    if (Console.CursorLeft >= 2)
                    {
                        if (toggleRoller == 0)
                        {
                            cursorPosX--;
                            Console.SetCursorPosition(cursorPosX, cursorPosY);
                        }
                        else if (toggleRoller == 1)
                        {
                            int row = Console.CursorTop - 1;
                            int col = Console.CursorLeft - 1;
                            drawing[row, col, 0] = characterPlot;
                            drawing[row, col, 1] = colorSelectFG;
                            drawing[row, col, 2] = colorSelectBG;
                            UpdateDrawing(drawing);
                            // ------------------------------------------- Plot ------------------------------------
                            cursorPosX--;
                            Console.SetCursorPosition(cursorPosX, cursorPosY);
                        }

                    }
                }
                if (userInputKey.Key == ConsoleKey.RightArrow)
                {
                    if (Console.CursorLeft <= drawAreaWidth-1)
                    {
                        if (toggleRoller == 0)
                        {
                            cursorPosX++;
                            Console.SetCursorPosition(cursorPosX, cursorPosY);
                        }
                        else if (toggleRoller == 1)
                        {
                            int row = Console.CursorTop - 1;
                            int col = Console.CursorLeft - 1;
                            drawing[row, col, 0] = characterPlot;
                            drawing[row, col, 1] = colorSelectFG;
                            drawing[row, col, 2] = colorSelectBG;
                            UpdateDrawing(drawing);
                            // ------------------------------------------- Plot ------------------------------------
                            //cursorPosY = row-1;
                            cursorPosX++;
                            Console.SetCursorPosition(cursorPosX, cursorPosY);
                        }

                    }
                }
                #endregion
                //
                // ----------------------------------------------------------- Plotting / Features - 10 Keys ------------------------------
                if (userInputKey.Key == ConsoleKey.Spacebar)
                {
                    int row = Console.CursorTop-1;
                    int col = Console.CursorLeft-1;
                    rememberChar = drawing[row, col, 0];
                    rememberFG = drawing[row, col, 1];
                    rememberBG = drawing[row, col, 2];
                    rememberPosX = Console.CursorLeft;
                    rememberPosY = Console.CursorTop;
                    drawing[row, col, 0] = characterPlot;
                    drawing[row, col, 1] = colorSelectFG;
                    drawing[row, col, 2] = colorSelectBG;
                    cursorPosX = col+1;
                    cursorPosY = row+1;
                    break;
                }//  Plot
                //---------------------------------------------------------------------
                if (userInputKey.Key == ConsoleKey.P)
                {
                    int row = Console.CursorTop - 1;
                    int col = Console.CursorLeft - 1;

                    rememberChar = drawing[row, col, 0];
                    rememberFG = drawing[row, col, 1];
                    rememberBG = drawing[row, col, 2];
                    rememberPosX = Console.CursorLeft;
                    rememberPosY = Console.CursorTop;
                    int tmpFG = int.Parse(colorSelectFG);
                    int tmpBG = int.Parse(colorSelectBG);
                    cursorPosX = col + 1;
                    cursorPosY = row + 1;
                    FloodFill(drawing, row, col, tmpFG, tmpBG);
                    //Console.SetCursorPosition(rememberPosX, rememberPosY);
                    //DrawDrawing(drawing);
                    Console.SetCursorPosition(rememberPosX, rememberPosY);
                    break;
                }//  FloodFill
                // -----------------------------------------------------------------------
                if (userInputKey.Key == ConsoleKey.OemMinus)
                {
                    int row = rememberPosY;
                    int col = rememberPosX;
                    int drawRow = row - 1;
                    int drawCol = col - 1;
                    cursorPosX = col;
                    cursorPosY = row;
                    Console.SetCursorPosition(col, row);
                    drawing[drawRow, drawCol, 1] = rememberFG;
                    drawing[drawRow, drawCol, 2] = rememberBG;
                    drawing[drawRow, drawCol, 0] = rememberChar;
                    int colorFG = int.Parse(drawing[drawRow, drawCol, 1]);
                    int colorBG = int.Parse(drawing[drawRow, drawCol, 2]);
                    Console.ForegroundColor = (ConsoleColor)colorFG;
                    Console.BackgroundColor = (ConsoleColor)colorBG;

                    if (drawing[drawRow, drawCol, 0] == null)
                    {
                        Console.Write(" ");
                    }
                    else
                    {

                        Console.Write(drawing[drawRow, drawCol, 0]);
                    }
                    colorFG = int.Parse(colorSelectFG);
                    colorBG = int.Parse(colorSelectBG);
                    Console.ForegroundColor = (ConsoleColor)colorFG;
                    Console.BackgroundColor = (ConsoleColor)colorBG;
                    Console.SetCursorPosition(col, row);
                    break;
                }// Undo
                if (userInputKey.Key == ConsoleKey.Delete)
                {
                    int row = Console.CursorTop - 1;
                    int col = Console.CursorLeft - 1;
                    rememberChar = drawing[row, col, 0];
                    rememberFG = drawing[row, col, 1];
                    if (drawing[row, col, 2] != null)
                    {
                        rememberBG = drawing[row, col, 2];
                    }
                    rememberPosX = Console.CursorLeft;
                    rememberPosY = Console.CursorTop;
                    drawing[row, col, 0] = null;
                    drawing[row, col, 1] = "0";
                    drawing[row, col, 2] = "0";
                    Console.SetCursorPosition(col + 1, row + 1);
                    Console.BackgroundColor = ConsoleColor.Black;
                    Console.Write(" ");
                    if (drawing[row, col, 2] != null)
                    {
                        int bgAfterDel = Int16.Parse(rememberBG);
                        Console.BackgroundColor = (ConsoleColor)bgAfterDel;
                    }
                    else
                    {
                        Console.BackgroundColor = ConsoleColor.Black;
                    }

                    cursorPosX = col + 1;
                    cursorPosY = row + 1;
                    break;
                }//  Erase char
                if (userInputKey.Key == ConsoleKey.Tab)
                {

                    int row = Console.CursorTop - 1;
                    int col = Console.CursorLeft - 1;

                    SavePixelInfo();
                    Console.ForegroundColor = ConsoleColor.Cyan;
                    Console.BackgroundColor = ConsoleColor.DarkBlue;
                    Console.SetCursorPosition(drawAreaWidth + 5, palettePosY + 2);
                    Console.Write("Enter character");
                    Console.SetCursorPosition(drawAreaWidth + 5, palettePosY + 3);
                    Console.Write("_______________");
                    Console.SetCursorPosition(drawAreaWidth + 5, palettePosY + 3);
                    string manualChar = Console.ReadLine();
                    int len = manualChar.Length;
                    if (len >= 1)
                    {
                        if (len >= 2)
                        {
                            manualChar = manualChar.Remove(1);
                        }
                        Console.BackgroundColor = ConsoleColor.Black;
                        Console.SetCursorPosition(drawAreaWidth + 5, palettePosY + 2);
                        Console.Write("                              ");
                        Console.SetCursorPosition(drawAreaWidth + 5, palettePosY + 3);
                        Console.Write("                              ");
                        Console.SetCursorPosition(drawAreaWidth + 5, palettePosY + 4);
                        Console.Write("                              ");
                    }
                    else
                    {
                        Console.BackgroundColor = ConsoleColor.Black;
                        Console.SetCursorPosition(drawAreaWidth + 5, palettePosY + 2);
                        Console.Write("                  ");
                        Console.SetCursorPosition(drawAreaWidth + 5, palettePosY + 3);
                        Console.Write("                  ");
                        Console.SetCursorPosition(drawAreaWidth + 5, palettePosY + 4);
                        Console.Write("                  ");
                        RestorePixelInfo();
                    }

                    if (characterPlot == specialChar)
                    {
                        specialChar = manualChar;
                        characterPlot = specialChar;
                    }
                    else
                    {
                        specialChar = manualChar;
                    }
                    KeysDrawBlocks(palettePosX, palettePosY, colorIntFG, colorIntBG);
                    RestorePixelInfo();
                }//  EnterTextChar
                if (userInputKey.Key == ConsoleKey.OemPeriod)
                {
                    int row = Console.CursorTop - 1;
                    int col = Console.CursorLeft - 1;
                    colorSelectFG = drawing[row, col, 1];
                    colorSelectBG = drawing[row, col, 2];
                    characterPlot = drawing[row, col, 0];
                }// Colorsniffer
                if (userInputKey.Key == ConsoleKey.Backspace)
                {
                    SavePixelInfo();
                    Console.ForegroundColor = ConsoleColor.Cyan;
                    Console.BackgroundColor = ConsoleColor.DarkBlue;
                    Console.SetCursorPosition(palettePosX + 74, palettePosY);
                    Console.Write("Clear Y/N?");
                    Console.BackgroundColor = ConsoleColor.Black;

                    var clearYesNo = Console.ReadKey(true);
                    if (clearYesNo.Key == ConsoleKey.Y)
                    {
                        ClearDrawingColor(drawing, Program.drawAreaHeight, drawAreaWidth);
                        DrawDrawing(drawing);
                        Console.SetCursorPosition(1, 1);
                    }
                    else
                    {

                    }
                    Console.SetCursorPosition(palettePosX + 74, palettePosY);
                    Console.Write("                  ");
                    RestorePixelInfo();
                }// Safe Clear / Totalfill
                if (userInputKey.Key == ConsoleKey.Escape)
                {
                    Console.BackgroundColor = ConsoleColor.Black;
                    Console.Clear();
                    Console.CursorVisible = false;
                    PressAnyKey();
                    Console.ForegroundColor = ConsoleColor.Black;
                    Console.BackgroundColor = ConsoleColor.Black;
                    Console.Clear();
                    System.Environment.Exit(1);
                }// Quit
                if (userInputKey.Key == ConsoleKey.End)
                {
                    if (toggleRoller == 0)
                    {
                        toggleRoller = 1;
                    }
                    else
                    {
                        toggleRoller = 0;
                    }
                }// Toggle Roller
                if (userInputKey.Key == ConsoleKey.PageUp)
                {
                    int indexNum = 0;
                    SavePixelInfo();
                    cursorPosX = drawAreaWidth + 3;
                    cursorPosY = drawAreaFrameStartY + 5;
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.CursorVisible = false;
                    Console.SetCursorPosition(cursorPosX + 4, cursorPosY + 5);
                    Console.Write("Cancel");
                    Console.SetCursorPosition(cursorPosX + 3, cursorPosY);
                    Console.Write(">");
                    Console.SetCursorPosition(cursorPosX + 2, cursorPosY);
                    while (true)
                    {
                        var userInputKeyFiles = Console.ReadKey(true);
                        if (userInputKeyFiles.Key == ConsoleKey.UpArrow)
                        {
                            if (indexNum >= 1)
                            {
                                cursorPosY = Console.CursorTop - 1;
                                MoveFileSelector();
                                indexNum--;
                            }
                        }
                        if (userInputKeyFiles.Key == ConsoleKey.DownArrow)
                        {
                            if (indexNum <= 4)
                            {
                                cursorPosY = Console.CursorTop + 1;
                                MoveFileSelector();
                                indexNum++;
                            }

                        }
                        if (userInputKeyFiles.Key == ConsoleKey.Enter)
                        {
                            if (indexNum <= 4 && lastFive[indexNum] != "" && lastFive[indexNum] != null)
                            {
                                if (indexNum <= 4)
                                {
                                    Console.SetCursorPosition(cursorPosX + 3, cursorPosY - indexNum + 5);
                                    Console.Write("                  ");
                                    Console.SetCursorPosition(cursorPosX + 3, cursorPosY);
                                    Console.Write(" ");
                                    Console.SetCursorPosition(cursorPosX, cursorPosY);
                                    nameDrawing = lastFive[indexNum];
                                    allPixels = LoadFile(allPixels, drawing, allStrings);
                                    toggleRoller = 0;
                                    break;
                                }

                            }
                            else if (indexNum <= 4 && lastFive[indexNum] == "" || indexNum <= 4 && lastFive[indexNum] == null)
                            {
                                Console.SetCursorPosition(cursorPosX, cursorPosY - indexNum + 5);
                                Console.Write("                  ");
                                Console.SetCursorPosition(cursorPosX, cursorPosY);
                                Console.Write(" ");
                                break;
                            }
                            else if (indexNum == 5)
                            {
                                Console.Write("                  ");
                                Console.SetCursorPosition(cursorPosX, cursorPosY);
                                break;
                            }
                            else
                            {
                                break;
                            }
                        }
                    }
                    Console.CursorVisible = true;
                    RestorePixelInfo();
                }// Load
                if (userInputKey.Key == ConsoleKey.PageDown)
                {
                    SavePixelInfo();
                    Console.ForegroundColor = ConsoleColor.Cyan;
                    Console.BackgroundColor = ConsoleColor.DarkBlue;
                    Console.SetCursorPosition(drawAreaWidth + 5, palettePosY);
                    Console.Write("What shall we name");
                    Console.SetCursorPosition(drawAreaWidth + 5, palettePosY + 1);
                    Console.Write("this Masterpiece? ");
                    Console.SetCursorPosition(drawAreaWidth + 5, palettePosY + 2);
                    Console.Write("(Enter to cancel) ");
                    Console.SetCursorPosition(drawAreaWidth + 5, palettePosY + 3);
                    Console.Write("__________________");
                    Console.SetCursorPosition(drawAreaWidth + 5, palettePosY + 3);
                    nameDrawing = Console.ReadLine();
                    int len = nameDrawing.Length;
                    if (len > 1)
                    {
                        if (len > 18)
                        {
                            nameDrawing = nameDrawing.Remove(18);
                        }
                        Console.BackgroundColor = ConsoleColor.Black;
                        Console.SetCursorPosition(drawAreaWidth + 5, palettePosY);
                        Console.Write("                              ");
                        Console.SetCursorPosition(drawAreaWidth + 5, palettePosY + 1);
                        Console.Write("                              ");
                        Console.SetCursorPosition(drawAreaWidth + 5, palettePosY + 2);
                        Console.Write("                              ");
                        Console.SetCursorPosition(drawAreaWidth + 5, palettePosY + 3);
                        Console.Write("                              ");
                        Console.SetCursorPosition(drawAreaWidth + 5, palettePosY + 4);
                        Console.Write("                              ");
                        Console.SetCursorPosition(drawAreaWidth + 5, palettePosY + 1);
                        Console.BackgroundColor = ConsoleColor.Black;
                        Console.Write("  -=* Saved *=-               ");
                        Thread.Sleep(1500);
                        Console.SetCursorPosition(drawAreaWidth + 5, palettePosY + 1);
                        Console.Write("                              ");
                        RestorePixelInfo();
                        SaveFile(allPixels, drawing);
                        DrawDrawing(drawing);
                        addLastFiveOrNot = 1; // Flipswitch, only when 1 LastFive() will add entry to list
                        LastFive();
                        RestorePixelInfo();
                        addLastFiveOrNot = 0;
                        //Console.CursorVisible = false;
                    }
                    else
                    {
                        Console.BackgroundColor = ConsoleColor.Black;
                        Console.SetCursorPosition(drawAreaWidth + 5, palettePosY);
                        Console.Write("                  ");
                        Console.SetCursorPosition(drawAreaWidth + 5, palettePosY + 1);
                        Console.Write("                  ");
                        Console.SetCursorPosition(drawAreaWidth + 5, palettePosY + 2);
                        Console.Write("                  ");
                        Console.SetCursorPosition(drawAreaWidth + 5, palettePosY + 3);
                        Console.Write("                  ");
                        Console.SetCursorPosition(drawAreaWidth + 5, palettePosY + 4);
                        Console.Write("                  ");
                        Console.SetCursorPosition(drawAreaWidth + 5, palettePosY + 1);
                        RestorePixelInfo();
                    }
                }// Save
                if (userInputKey.Key == ConsoleKey.Home)
                {
                    SavePixelInfo();
                    Console.ForegroundColor = ConsoleColor.Cyan;
                    Console.BackgroundColor = ConsoleColor.DarkBlue;
                    Console.SetCursorPosition(drawAreaWidth + 5, palettePosY);
                    Console.Write("What shall we name");
                    Console.SetCursorPosition(drawAreaWidth + 5, palettePosY + 1);
                    Console.Write("this Masterpiece? ");
                    Console.SetCursorPosition(drawAreaWidth + 5, palettePosY + 2);
                    Console.Write("(Enter to cancel) ");
                    Console.SetCursorPosition(drawAreaWidth + 5, palettePosY + 3);
                    Console.Write("__________________");
                    Console.SetCursorPosition(drawAreaWidth + 5, palettePosY + 3);
                    nameDrawing = Console.ReadLine();
                    int len = nameDrawing.Length;
                    if (len > 1)
                    {
                        if (len > 18)
                        {
                            nameDrawing = nameDrawing.Remove(18);
                        }
                        Console.BackgroundColor = ConsoleColor.Black;
                        Console.SetCursorPosition(drawAreaWidth + 5, palettePosY);
                        Console.Write("                              ");
                        Console.SetCursorPosition(drawAreaWidth + 5, palettePosY + 1);
                        Console.Write("                              ");
                        Console.SetCursorPosition(drawAreaWidth + 5, palettePosY + 2);
                        Console.Write("                              ");
                        Console.SetCursorPosition(drawAreaWidth + 5, palettePosY + 3);
                        Console.Write("                              ");
                        Console.SetCursorPosition(drawAreaWidth + 5, palettePosY + 4);
                        Console.Write("                              ");
                        Console.SetCursorPosition(drawAreaWidth + 5, palettePosY + 1);
                        Console.BackgroundColor = ConsoleColor.Black;
                        Console.Write("  -=* Saved *=-               ");
                        Thread.Sleep(1500);
                        Console.SetCursorPosition(drawAreaWidth + 5, palettePosY + 1);
                        Console.Write("                              ");
                        ExportFile(allPixels, drawing, nameDrawing);
                        RestorePixelInfo();

                    }
                    else
                    {
                        Console.BackgroundColor = ConsoleColor.Black;
                        Console.SetCursorPosition(drawAreaWidth + 5, palettePosY);
                        Console.Write("                  ");
                        Console.SetCursorPosition(drawAreaWidth + 5, palettePosY + 1);
                        Console.Write("                  ");
                        Console.SetCursorPosition(drawAreaWidth + 5, palettePosY + 2);
                        Console.Write("                  ");
                        Console.SetCursorPosition(drawAreaWidth + 5, palettePosY + 3);
                        Console.Write("                  ");
                        Console.SetCursorPosition(drawAreaWidth + 5, palettePosY + 4);
                        Console.Write("                  ");
                        Console.SetCursorPosition(drawAreaWidth + 5, palettePosY + 1);
                        RestorePixelInfo();
                    }
                }// Export
            }
            return drawing;
        }
        static void FloodFill(string[,,] drawing, int row, int col, int tmpFG, int tmpBG)
        {
            if (row >= 0 && row <= drawing.GetLength(0) - 1 && col >= 0 && col <= drawing.GetLength(1) - 1)
            {
                if (drawing[row, col, 0] == null || drawing[row, col, 0] == rememberChar)
                {
                    if (drawing[row, col, 1] == null || drawing[row, col, 1] == rememberFG)
                    {
                        if (drawing[row, col, 2] == null || drawing[row, col, 2] == rememberBG)
                        {
                            drawing[row, col, 0] = characterPlot;
                            drawing[row, col, 1] = colorSelectFG;
                            drawing[row, col, 2] = colorSelectBG;
                            Console.SetCursorPosition(col+1, row+1);
                            //int tmpFG = int.Parse(colorSelectFG);
                            //int tmpBG = int.Parse(colorSelectBG);
                            Console.ForegroundColor = (ConsoleColor)tmpFG;
                            Console.BackgroundColor = (ConsoleColor)tmpBG;
                            Console.Write(characterPlot);
                            //UpdateDrawing(drawing);
                            if (row >= 0 && row < drawing.GetLength(0) && col >= 0 && col < drawing.GetLength(1))
                            {
                                FloodFill(drawing, row + 1, col, tmpFG, tmpBG);
                            }
                            if (row >= 1 && row < drawing.GetLength(0) && col >= 0 && col < drawing.GetLength(1))
                            {
                                FloodFill(drawing, row - 1, col, tmpFG, tmpBG);
                            }
                            if (row >= 0 && row < drawing.GetLength(0) && col >= 0 && col < drawing.GetLength(1))
                            {
                                FloodFill(drawing, row, col + 1, tmpFG, tmpBG);
                            }
                            if (row >= 0 && row < drawing.GetLength(0) && col >= 0 && col < drawing.GetLength(1))
                            {
                                FloodFill(drawing, row, col - 1, tmpFG, tmpBG);
                            }
                        }
                    }
                }
            }
        }


        static void ClearBGSelector()
        {
            Console.BackgroundColor = ConsoleColor.Black;
            Console.ForegroundColor = ConsoleColor.White;
            Console.SetCursorPosition(palettePosX, palettePosY + 4);
            Console.Write("                                                 ");
        }
        static void ClearFGSelector()
        {
            Console.BackgroundColor = ConsoleColor.Black;
            Console.ForegroundColor = ConsoleColor.White;
            Console.SetCursorPosition(palettePosX, palettePosY + 3);
            Console.Write("                                                 ");
        }
        static void ClearCharSelector()
        {
            Console.BackgroundColor = ConsoleColor.Black;
            Console.ForegroundColor = ConsoleColor.White;
            Console.SetCursorPosition(palettePosX + 52, palettePosY + 3);
            Console.Write("                                 ");
        }
        static string[] LastFive()
        {
            //int pause = 0;
            if (File.Exists("lastfive.txt"))
            {
                string[] allHistory = File.ReadAllLines("lastfive.txt");
                for (int j = 0; j < 5; j++)
                {
                    lastFive[j] = allHistory[j];
                }
            }
            else
            {
                for (int j = 0; j < 5; j++)
                {
                    lastFive[j] = "";
                }
                lastFive[0] = "";
            }
            //pause = 1;
            switch (addLastFiveOrNot)
            {
                case 1:
                    for (int i = 4; i > 0; i--)
                    {
                        lastFive[i] = lastFive[i - 1];
                    }
                    lastFive[0] = nameDrawing.ToString();
                    lastFive[0] = lastFive[0] + ".wci";
                    break;
                default:
                    break;
            }
            Console.ForegroundColor = ConsoleColor.Green;
            Console.BackgroundColor = ConsoleColor.Black;
            Console.SetCursorPosition(drawAreaWidth + 7, drawAreaFrameStartY + 4);
            Console.Write("History:");
            for (int i = 0; i < 5; i++)
            {
                Console.SetCursorPosition(drawAreaWidth + 7, drawAreaFrameStartY + 5 + i);
                Console.Write("                      ");
                Console.SetCursorPosition(drawAreaWidth + 7, drawAreaFrameStartY + 5 + i);
                Console.Write(lastFive[i]);
            }
            File.WriteAllLines("lastfive.txt", lastFive);
            return lastFive;

        }
        static void SavePixelInfo()
        {
            rememberFG = colorSelectFG;
            rememberBG = colorSelectBG;
            rememberPosX = Console.CursorLeft;
            rememberPosY = Console.CursorTop;
        }
        static void RestorePixelInfo()
        {
            colorSelectFG = rememberFG;
            colorSelectBG = rememberBG;
            Console.CursorLeft = rememberPosX;
            Console.CursorTop = rememberPosY;
        }
        static void MoveFileSelector()
        {
            Console.Write("  ");
            Console.SetCursorPosition(cursorPosX + 3, cursorPosY);
            Console.Write(">");
            Console.SetCursorPosition(cursorPosX + 3 - 1, cursorPosY);
        }
        static void PaletteDrawFG(int palettePosX)
        {
            // ---------- Write Color Keys ------------
            int[] colorArray =
                {
                0,  // Black
                8,  // DarkGray
                7,  // Gray
                15, // White
                1,  // DarkBlue
                9,  // Blue
                3,  // DarkCyan
                11, // Cyan
                2,  // DarkGreen
                10, // Green
                6,  // DarkYellow
                14, // Yellow
                5,  // DarkMagenta
                13, // Magenta
                4,  // DarkRed
                12  // Red
                };
            Console.BackgroundColor = ConsoleColor.Black;
            Console.ForegroundColor = ConsoleColor.White;
            for (int i = 0; i < 10; i++)
            {
                Console.SetCursorPosition((palettePosX + (i * 3)), drawAreaHeight + 4);
                Console.Write("F" + (i + 1) + " ");
            }
            for (int i = 0; i < 6; i++)
            {
                Console.SetCursorPosition((palettePosX + (30) + (i * 3)), drawAreaHeight + 4);
                Console.Write(" " + (i + 1) + " ");
            }

            // -------- Draw Colors -------------
            Console.SetCursorPosition(palettePosX + 1, drawAreaHeight + 5);
            Console.ForegroundColor = ConsoleColor.Black;
            for (int i = 0; i < colorArray.GetLength(0); i++)
            {
                Console.SetCursorPosition((palettePosX + (i * 3)), drawAreaHeight + 5);
                Console.BackgroundColor = (ConsoleColor)colorArray[i];
                Console.Write("   ");
            }
            Console.ForegroundColor = ConsoleColor.White;
            Console.BackgroundColor = ConsoleColor.Black;
            Console.WriteLine();

        }
        static void PaletteDrawBG(int palettePosX, int palettePosY)
        {
            // ---------- Write Color Keys ------------
            int[] colorArray =
                {
                0,  // Black        A
                8,  // DarkGray     S
                7,  // Gray         D
                15, // White        F
                1,  // DarkBlue     G
                9,  // Blue         H
                3,  // DarkCyan     J
                11, // Cyan         K
                2,  // DarkGreen    L
                10, // Green        Z
                6,  // DarkYellow   X
                14, // Yellow       C
                5,  // DarkMagenta  V
                13, // Magenta      B
                4,  // DarkRed      N
                12  // Red          M
                };
            string[] keyArray =
    {
                "A",
                "S",
                "D",
                "F",
                "G",
                "H",
                "J",
                "K",
                "L",
                "Z",
                "X",
                "C",
                "V",
                "B",
                "N",
                "M"
                };
            Console.BackgroundColor = ConsoleColor.Black;
            Console.ForegroundColor = ConsoleColor.White;
            for (int i = 0; i < 16; i++)
            {
                Console.SetCursorPosition((palettePosX + (i * 3)), drawAreaHeight + 9);
                Console.Write(" " + keyArray[i] + " ");
            }

            // -------- Draw Colors -------------
            Console.SetCursorPosition(palettePosX + 1, drawAreaHeight + 7);
            Console.ForegroundColor = ConsoleColor.Black;
            for (int i = 0; i < colorArray.GetLength(0); i++)
            {
                Console.SetCursorPosition((palettePosX + (i * 3)), drawAreaHeight + 8);
                Console.BackgroundColor = (ConsoleColor)colorArray[i];
                Console.Write("   ");
            }
            Console.ForegroundColor = ConsoleColor.White;
            Console.BackgroundColor = ConsoleColor.Black;
            Console.WriteLine();

        }
        static void KeysDrawBlocks(int palettePosX, int palettePosY, int colorIntFG, int colorIntBG)
        {
            palettePosX = palettePosX + 52; // Place Key-table anywhere
            palettePosY = palettePosY + 1;
            Console.ForegroundColor = ConsoleColor.White;
            Console.BackgroundColor = ConsoleColor.Black;
            Console.SetCursorPosition(palettePosX, palettePosY);
            Console.Write("║ q ║ w ║ e ║ r ║ t ║ y ║ u ║ i ║");
            Console.SetCursorPosition(palettePosX, palettePosY + 1);
            Console.Write("║ ");
            Console.ForegroundColor = (ConsoleColor)colorIntFG;
            Console.BackgroundColor = (ConsoleColor)colorIntBG;
            Console.Write("█");
            Console.ForegroundColor = ConsoleColor.White;
            Console.BackgroundColor = ConsoleColor.Black;
            Console.Write(" ║ ");
            Console.ForegroundColor = (ConsoleColor)colorIntFG;
            Console.BackgroundColor = (ConsoleColor)colorIntBG;
            Console.Write("▓");
            Console.ForegroundColor = ConsoleColor.White;
            Console.BackgroundColor = ConsoleColor.Black;
            Console.Write(" ║ ");
            Console.ForegroundColor = (ConsoleColor)colorIntFG;
            Console.BackgroundColor = (ConsoleColor)colorIntBG;
            Console.Write("▒");
            Console.ForegroundColor = ConsoleColor.White;
            Console.BackgroundColor = ConsoleColor.Black;
            Console.Write(" ║ ");
            Console.ForegroundColor = (ConsoleColor)colorIntFG;
            Console.BackgroundColor = (ConsoleColor)colorIntBG;
            Console.Write("░");
            Console.ForegroundColor = ConsoleColor.White;
            Console.BackgroundColor = ConsoleColor.Black;
            Console.Write(" ║ ");
            Console.ForegroundColor = (ConsoleColor)colorIntFG;
            Console.BackgroundColor = (ConsoleColor)colorIntBG;
            Console.Write(specialChar);
            Console.ForegroundColor = ConsoleColor.White;
            Console.BackgroundColor = ConsoleColor.Black;
            Console.Write(" ║ ");
            Console.ForegroundColor = (ConsoleColor)colorIntFG;
            Console.BackgroundColor = (ConsoleColor)colorIntBG;
            Console.Write("▄");
            Console.ForegroundColor = ConsoleColor.White;
            Console.BackgroundColor = ConsoleColor.Black;
            Console.Write(" ║ ");
            Console.ForegroundColor = (ConsoleColor)colorIntFG;
            Console.BackgroundColor = (ConsoleColor)colorIntBG;
            Console.Write("▀");
            Console.ForegroundColor = ConsoleColor.White;
            Console.BackgroundColor = ConsoleColor.Black;
            Console.Write(" ║ ");
            Console.ForegroundColor = (ConsoleColor)colorIntFG;
            Console.BackgroundColor = (ConsoleColor)colorIntBG;
            Console.Write("▬");
            Console.ForegroundColor = ConsoleColor.White;
            Console.BackgroundColor = ConsoleColor.Black;
            Console.Write(" ║ ");


        }
        static void KeysCopyChars(int palettePosX, int palettePosY, int colorIntFG, int colorIntBG)
        {
            int palettePosX_ = palettePosX + 92; // Place Key-table anywhere
            palettePosY = palettePosY + 1;
            Console.ForegroundColor = (ConsoleColor)colorIntFG;
            Console.BackgroundColor = (ConsoleColor)colorIntBG;

            Console.SetCursorPosition(palettePosX_, palettePosY);
            Console.Write("Copy/Tab/Paste          ╔═╦═╗ ┌─┬─┐");
            Console.SetCursorPosition(palettePosX_, palettePosY + 1);
            Console.Write("☺ ☻ ♥ ♦ ♣ ♠ º ¿ ¡ ‼     ╠═╬═╣ ├─┼─┤");
            Console.SetCursorPosition(palettePosX_, palettePosY + 2);
            Console.Write("¶ § ® © ⌐ ¬ ª « »       ║ ║ ║ │ │ │");
            Console.SetCursorPosition(palettePosX_, palettePosY + 3);
            Console.Write("↑ ↓ ► ◄ ▲ ▼ ↔ ↕ ↨       ╚═╩═╝ └─┴─┘");
            //

        }
        static void PrintMenu()
        {
            int drawX = drawAreaWidth + 6;
            Console.SetCursorPosition(drawX, drawAreaFrameStartY + 15);
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write("Keyboard Commands");

            Console.SetCursorPosition(drawX, drawAreaFrameStartY + 17);
            #region MOVE CURSOR
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.Write("Arrowkeys");
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write(" - ");
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Write("Move Cursor");
            #endregion

            Console.SetCursorPosition(drawX, drawAreaFrameStartY + 18);
            #region PLOT CHAR
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.Write("Spacebar");
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write(" ---- ");
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Write("Plot Char");
            #endregion

            Console.SetCursorPosition(drawX, drawAreaFrameStartY + 19);
            #region DELETE CHAR
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.Write("Delete");
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write(" ---- ");
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Write("Delete Char");
            #endregion

            Console.SetCursorPosition(drawX, drawAreaFrameStartY + 20);
            #region ENTER CHAR
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.Write("Tab");
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write(" -------- ");
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Write("Enter Char");
            #endregion

            Console.SetCursorPosition(drawX, drawAreaFrameStartY + 21);
            #region CLEAR / FILL
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.Write("Backspace");
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write(" -- ");
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Write("Clear/Fill");
            #endregion

            Console.SetCursorPosition(drawX, drawAreaFrameStartY + 22);
            #region ROLLER TOGGLE
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.Write("End");
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write(" ----- ");
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Write("Roller On/Off");
            #endregion

            Console.SetCursorPosition(drawX, drawAreaFrameStartY + 23);
            #region SNIFF COLOR
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.Write("Period");
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write(" ");
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Write("Sniff Color/Char");
            #endregion

            Console.SetCursorPosition(drawX, drawAreaFrameStartY + 24);
            #region UNDO LAST PLOT
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.Write("Minus");
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write(" -- ");
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Write("Undo last plot");
            #endregion

            Console.SetCursorPosition(drawX, drawAreaFrameStartY + 25);
            #region SAVE
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.Write("PageDown");
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write(" --- ");
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Write("Save!");
            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write(" Yey!");
            #endregion

            Console.SetCursorPosition(drawX, drawAreaFrameStartY + 26);
            #region LOAD
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.Write("PageUp");
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write(" ---- ");
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Write("Load!");
            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write(" Whee!");
            #endregion

            Console.SetCursorPosition(drawX, drawAreaFrameStartY + 27);
            #region EXPORT CODE
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.Write("Home  ");
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write(" --- ");
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Write("Export");
            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write(" :O´ ´");
            #endregion

            Console.SetCursorPosition(drawX, drawAreaFrameStartY + 28);
            #region QUIT
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.Write("Esc");
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write(" -------- ");
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Write("Quit");
            Console.ForegroundColor = ConsoleColor.Red;
            Console.Write(" Booo!");
            #endregion


            Console.SetCursorPosition(drawX, drawAreaFrameStartY + 29);
            #region Fill
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.Write("P");
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write(" ------- ");
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Write("Fill");
            Console.ForegroundColor = ConsoleColor.Red;
            Console.Write(" MAX 50%");
            #endregion

        } // ------------------ MENU TEXT -------------------
        static void DrawAreaFrame(int drawAreaWidth, int drawAreaHeight, int drawAreaFrameStartX, int drawAreaFrameStartY)
        {
            char[] charArray = // Best chars for ascii/ansi
                {
                '╔', // 0 
                '╗', // 1
                '║', // 2
                '═', // 3
                '╚', // 4
                '╝', // 5
                };
            //drawAreaWidth = drawAreaWidth + 1;
            //drawAreaHeight = drawAreaHeight + 1;

            // Line 1
            Console.SetCursorPosition(drawAreaFrameStartX, drawAreaFrameStartY);
            Console.Write(charArray[0]);
            for (int i = 1; i <= (drawAreaWidth); i++)
            {
                Console.Write(charArray[3]);
            }
            Console.Write(charArray[1]);

            // Line 2 to Line drawAreaHigh - 2
            for (int i = 1; i <= (drawAreaHeight + 1); i++)
            {
                Console.SetCursorPosition(drawAreaFrameStartX, (drawAreaFrameStartY + i));
                Console.Write(charArray[2]);
                Console.SetCursorPosition(drawAreaWidth + 1, drawAreaFrameStartY + i);
                Console.Write(charArray[2]);
            }
            Console.SetCursorPosition(drawAreaFrameStartX, (drawAreaFrameStartY + 6));
            Console.Write(charArray[2]);
            // Line Last
            Console.SetCursorPosition(drawAreaFrameStartX, (drawAreaFrameStartY + drawAreaHeight + 1));
            Console.Write(charArray[4]);
            for (int i = 0; i <= (drawAreaWidth - 1); i++)
            {
                Console.Write(charArray[3]);
            }
            Console.Write(charArray[5]);
            Console.WriteLine();
        }
        static void DrawHistoryFrame(int drawAreaWidth, int drawAreaHeigth, int drawAreaFrameStartX, int drawAreaFrameStartY)
        {
            char[] charArray = // Best chars for ascii/ansi
                {
                '╔', // 0 
                '╗', // 1
                '║', // 2
                '═', // 3
                '╚', // 4
                '╝', // 5
                };
            drawAreaWidth = drawAreaWidth + 4;

            // Top Row
            Console.SetCursorPosition(drawAreaWidth, drawAreaFrameStartY + 2);
            Console.Write("╔");
            for (int i = 0; i < 25; i++)
            {
                Console.SetCursorPosition(drawAreaWidth + 1 + i, drawAreaFrameStartY + 2);
                Console.Write("═");
            }
            Console.SetCursorPosition(drawAreaWidth + 26, drawAreaFrameStartY + 2);
            Console.Write("╗");

            Console.SetCursorPosition(drawAreaWidth, drawAreaFrameStartY + 4);
            Console.Write("║");
            Console.SetCursorPosition(drawAreaWidth, drawAreaFrameStartY + 3);
            Console.Write("║");





            Console.SetCursorPosition(drawAreaWidth - 1, drawAreaFrameStartY + 5);
            Console.Write("═");
            Console.SetCursorPosition(drawAreaWidth, drawAreaFrameStartY + 5);
            Console.Write("╝");

            Console.SetCursorPosition(drawAreaWidth - 1, drawAreaFrameStartY + 8);
            Console.Write("═");
            Console.SetCursorPosition(drawAreaWidth, drawAreaFrameStartY + 8);
            Console.Write("╗");
            Console.SetCursorPosition(drawAreaWidth, drawAreaFrameStartY + 9);
            Console.Write("║");
            Console.SetCursorPosition(drawAreaWidth, drawAreaFrameStartY + 10);
            Console.Write("║");
            Console.SetCursorPosition(drawAreaWidth, drawAreaFrameStartY + 11);
            Console.Write("║");
            Console.SetCursorPosition(drawAreaWidth, drawAreaFrameStartY + 12);
            Console.Write("╚");
            for (int i = 0; i < 25; i++)
            {
                Console.SetCursorPosition(drawAreaWidth + 1 + i, drawAreaFrameStartY + 12);
                Console.Write("═");
            }
            Console.SetCursorPosition(drawAreaWidth + 26, drawAreaFrameStartY + 12);
            Console.Write("╝");
            for (int i = 0; i <= 8; i++)
            {
                Console.SetCursorPosition(drawAreaWidth + 26, drawAreaFrameStartY + 3 + i);
                Console.Write("║");
            }


        }
        static void DrawHotkeysFrame(int drawAreaWidth, int drawAreaHeigth, int drawAreaFrameStartX, int drawAreaFrameStartY)
        {
            char[] charArray = // Best chars for ascii/ansi
                {
                '╔', // 0 
                '╗', // 1
                '║', // 2
                '═', // 3
                '╚', // 4
                '╝', // 5
                };
            drawAreaWidth = drawAreaWidth + 4;
            // Top Row
            Console.SetCursorPosition(drawAreaWidth, drawAreaFrameStartY + 13);
            Console.Write("╔");
            for (int i = 0; i < 25; i++)
            {
                Console.SetCursorPosition(drawAreaWidth + 1 + i, drawAreaFrameStartY + 13);
                Console.Write("═");
            }
            Console.SetCursorPosition(drawAreaWidth + 26, drawAreaFrameStartY + 13);
            Console.Write("╗");

            Console.SetCursorPosition(drawAreaWidth, drawAreaFrameStartY + 14);
            Console.Write("║");

            Console.SetCursorPosition(drawAreaWidth, drawAreaFrameStartY + 19);
            Console.Write("║");

            // upper grip ------------------
            Console.SetCursorPosition(drawAreaWidth - 1, drawAreaFrameStartY + 15);
            Console.Write("═");
            Console.SetCursorPosition(drawAreaWidth, drawAreaFrameStartY + 15);
            Console.Write("╝");
            Console.SetCursorPosition(drawAreaWidth - 1, drawAreaFrameStartY + 18);
            Console.Write("═");
            Console.SetCursorPosition(drawAreaWidth, drawAreaFrameStartY + 18);
            Console.Write("╗");
            Console.SetCursorPosition(drawAreaWidth, drawAreaFrameStartY + 20);
            Console.Write("║");
            Console.SetCursorPosition(drawAreaWidth, drawAreaFrameStartY + 21);
            Console.Write("║");

            // --- lower grip ---------
            Console.SetCursorPosition(drawAreaWidth - 1, drawAreaFrameStartY + 22);
            Console.Write("═");
            Console.SetCursorPosition(drawAreaWidth, drawAreaFrameStartY + 22);
            Console.Write("╝");

            Console.SetCursorPosition(drawAreaWidth - 1, drawAreaFrameStartY + 25);
            Console.Write("═");
            Console.SetCursorPosition(drawAreaWidth, drawAreaFrameStartY + 25);
            Console.Write("╗");


            for (int q = 0; q < 8; q++)
            {
                Console.SetCursorPosition(drawAreaWidth, drawAreaFrameStartY + 26 + q);
                Console.Write("║");
            }
            // ------------------- Bottom ---------------------
            Console.SetCursorPosition(drawAreaWidth, drawAreaFrameStartY + 34);
            Console.Write("╚");
            for (int i = 0; i < 25; i++)
            {
                Console.SetCursorPosition(drawAreaWidth + 1 + i, drawAreaFrameStartY + 34);
                Console.Write("═");
            }
            Console.SetCursorPosition(drawAreaWidth + 26, drawAreaFrameStartY + 34);
            Console.Write("╝");
            // ------------------ Far Side --------------------
            for (int i = 0; i <= 19; i++)
            {
                Console.SetCursorPosition(drawAreaWidth + 26, drawAreaFrameStartY + 14 + i);
                Console.Write("║");
            }


        }
        public static void PressAnyKey()
        {
            string pressKey = "Yeah, sorry, you really need to press \"any-key\" to exit now..." +
                "  Thank you for painting happy little clouds and whatnot, bringing out that " +
                "Bob Ross-mentality living deep inside you.     Greetz to all rockin' peeps in System23!" +
                "  Y'all made me push my limits on this one <3    Now, on to the next task at hand, cheers! " +
                " // Wigge" +
                "                                    ";

            while (!(Console.KeyAvailable && Console.ReadKey(true).Key == ConsoleKey.Spacebar))
            {
                int x = 40;
                int y = 18;
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.BackgroundColor = ConsoleColor.DarkBlue;
                Console.SetCursorPosition(x, y);
                Console.WriteLine("╔══════════════════════════════════╗");
                Console.SetCursorPosition(x, y + 1);
                Console.WriteLine("║                                   "); //34 chars inside
                Console.SetCursorPosition(x, y + 2);
                Console.WriteLine("╚══════════════════════════════════╝");
                Console.SetCursorPosition(x + 2, y + 1);

                int delay = 0;
                foreach (char c in pressKey)
                {
                    if (delay >= 30)
                    {
                        Boom();
                        delay = 0;
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Console.BackgroundColor = ConsoleColor.DarkBlue;
                    }

                    Console.SetCursorPosition(x + 34, y + 1);
                    Console.Write(c);
                    Thread.Sleep(80);
                    Console.SetCursorPosition(x + 35, y + 1);
                    Console.Write(" "); //34 chars inside
                    Console.SetCursorPosition(x + 35, y + 1);
                    Console.Write("║"); //34 chars inside ║
                    Console.MoveBufferArea(x + 2, y + 1, 33, 1, x + 1, y + 1);
                    delay++;

                    if (Console.KeyAvailable)
                    {
                        ConsoleKeyInfo key = Console.ReadKey(true);

                        switch (key.Key)
                        {
                            case ConsoleKey.Spacebar:
                                //JustBoom();
                                return;
                            default:
                                break;
                        }
                    }
                }
            }
            Console.ResetColor();
            Console.Clear();
        }
        public static void Boom()
        {
            Random rnd = new Random();
            string[,] boomGfx = new string[7, 10];
            int x = 0;
            int randomColor = 0;

            boomGfx[0, 0] = "          ";
            boomGfx[1, 0] = "          ";
            boomGfx[2, 0] = "          ";
            boomGfx[3, 0] = "          ";
            boomGfx[4, 0] = "          ";
            boomGfx[5, 0] = "          ";
            boomGfx[6, 0] = "    ██    ";
            //----------------------------- █ ▓ ▒ ░
            boomGfx[0, 1] = "          ";
            boomGfx[1, 1] = "          ";
            boomGfx[2, 1] = "          ";
            boomGfx[3, 1] = "          ";
            boomGfx[4, 1] = "          ";
            boomGfx[5, 1] = "    ██    ";
            boomGfx[6, 1] = "    ▓▓    ";
            //----------------------------- █ ▓ ▒ ░
            boomGfx[0, 2] = "          ";
            boomGfx[1, 2] = "          ";
            boomGfx[2, 2] = "          ";
            boomGfx[3, 2] = "          ";
            boomGfx[4, 2] = "    ██    ";
            boomGfx[5, 2] = "    ▓▓    ";
            boomGfx[6, 2] = "    ▒▒    ";
            //----------------------------- █ ▓ ▒ ░
            boomGfx[0, 3] = "          ";
            boomGfx[1, 3] = "          ";
            boomGfx[2, 3] = "          ";
            boomGfx[3, 3] = "    ██    ";
            boomGfx[4, 3] = "    ▓▓    ";
            boomGfx[5, 3] = "    ▒▒    ";
            boomGfx[6, 3] = "    ░░    ";
            //----------------------------- █ ▓ ▒ ░
            boomGfx[0, 4] = "          ";
            boomGfx[1, 4] = "          ";
            boomGfx[2, 4] = "    ██    ";
            boomGfx[3, 4] = "    ▓▓    ";
            boomGfx[4, 4] = "    ▒▒    ";
            boomGfx[5, 4] = "    ░░    ";
            boomGfx[6, 4] = "          ";
            //----------------------------- █ ▓ ▒ ░
            boomGfx[0, 5] = "          ";
            boomGfx[1, 5] = "   ▓  ▓   ";
            boomGfx[2, 5] = " █  ▓▓  █ ";
            boomGfx[3, 5] = "    ▒▒    ";
            boomGfx[4, 5] = "    ░░    ";
            boomGfx[5, 5] = "          ";
            boomGfx[6, 5] = "          ";
            //----------------------------- █ ▓ ▒ ░

            boomGfx[0, 6] = "          ";
            boomGfx[1, 6] = "   ▒  ▒   ";
            boomGfx[2, 6] = " ▒  ░░  ▒ ";
            boomGfx[3, 6] = "          ";
            boomGfx[4, 6] = "▓        ▓";
            boomGfx[5, 6] = "          ";
            boomGfx[6, 6] = "          ";
            //----------------------------- █ ▓ ▒ ░
            boomGfx[0, 7] = "          ";
            boomGfx[1, 7] = "   ░  ░   ";
            boomGfx[2, 7] = " ▒      ▒ ";
            boomGfx[3, 7] = "          ";
            boomGfx[4, 7] = "          ";
            boomGfx[5, 7] = "▒        ▒";
            boomGfx[6, 7] = "          ";
            //----------------------------- █ ▓ ▒ ░
            boomGfx[0, 8] = "          ";
            boomGfx[1, 8] = "          ";
            boomGfx[2, 8] = " ░      ░ ";
            boomGfx[3, 8] = "          ";
            boomGfx[4, 8] = "          ";
            boomGfx[5, 8] = "          ";
            boomGfx[6, 8] = "░        ░";
            //----------------------------- █ ▓ ▒ ░
            boomGfx[0, 9] = "          ";
            boomGfx[1, 9] = "          ";
            boomGfx[2, 9] = "          ";
            boomGfx[3, 9] = "          ";
            boomGfx[4, 9] = "          ";
            boomGfx[5, 9] = "          ";
            boomGfx[6, 9] = "          ";
            //----------------------------- █ ▓ ▒ ░
            randomColor = rnd.Next(1, 16);
            Console.ForegroundColor = (ConsoleColor)randomColor;
            Console.BackgroundColor = ConsoleColor.Black;
            //while (x < 5) 
            //{

            int rndX = 0;
            int rndY = 0;
            rndX = rnd.Next(10, 100);

            while (true)
            {
                rndY = rnd.Next(1, 32);
                if (rndY <= 10 || rndY >= 20)
                {
                    break;
                }
            }

            for (int col = 0; col < boomGfx.GetLength(1); col++)
            {
                int yStep = 0;
                for (int j = 0; j < boomGfx.GetLength(0); j++)
                {
                    Console.SetCursorPosition(rndX, rndY + yStep);
                    Console.Write(boomGfx[j, col]);
                    yStep++;
                    Thread.Sleep(5);
                }
            }
            //Thread.Sleep(70);
        }
    }
}
/*
░░░░  ░░░░ ░░░░░░░░░░ ░░░░░░    ░░░░░░░░░░ ░░░░░░░░░░░ ░░░░░░░░░░░░░░░░░░░░░░    ░░░░░░░░░░░░ ░░░░░░░░░░░
░██░  ░██░ ░░██████░░ ░░██░░    ░░██████░░ ░░███████░░ ░░███░░███░░ ░░██████░    ░░████████░░ ░░███████░░
░██░░░░██░ ░░██░░░░░░ ░░██░░    ░░██░░░░░░ ░░██░░░██░░ ░░████████░░ ░░██░░░░░    ░░░░░██░░░░░ ░░██   ██░░
░██░██░██░ ░░█████░░░ ░░██░░    ░░██░░     ░░██░░░██░░ ░░██░██░██░░ ░░█████░░       ░░██░░    ░░██   ██░░
░████████░ ░░██░░░░░░ ░░██░░░░░ ░░██░░░░░░ ░░██░░░██░░ ░░██░░░░██░░ ░░██░░░░░       ░░██░░    ░░██   ██░░
░███░░███░ ░░██████░░ ░░█████░░ ░░██████░░ ░░███████░░ ░░██░░░░██░░ ░░██████░       ░░██░░    ░░███████░░
░░░░░░░░░░ ░░░░░░░░░░ ░░░░░░░░░ ░░░░░░░░░░ ░░░░░░░░░░░ ░░░░░░░░░░░░ ░░░░░░░░░       ░░░░░░    ░░░░░░░░░░░
*/


/*


               

// ╔═╦═╗ ┌─┬─┐ ╓─╥─╖ ╒═╤═╕
// ╠═╬═╣ ├─┼─┤ ╟─╫─╢ ╞═╪═╡
// ║ ║ ║ │ │ │ ║ ║ ║ │ │ │
// ╚═╩═╝ └─┴─┘ ╙─╨─╜ ╘═╧═╛















 */


// ☺ ☻ ♥ ♦ ♣ ♠ • ◘ ○ ◙ ♂ ♀ ♪ ♫ ☼ ► ◄ ↕ ‼ ¶ § ▬ ↨ ↑ ↓
// → ← ∟ ↔ ▲ ▼ ^ ` ⌂ Ü ü º¿¡ ⌐ ¬ ª ® © « » ░ ▒ ▓ █ ▄ ▌▐ ▀
// │ ┤ ╡ ╢ ╖ ╕ ╣ ║ ╗ ╝ ╜ ╛ ┐ └ ┴ ├ ├ ─ ┼ ╞
//  ╟ ╚ ╔ ╩ ╦ ╠ ═ ╬ ╧ ╨ ╤ ╥ ╙ ╘ ╒ ╓ ╫ ╪ ┘ ┌
// - _ / | \
//
// DrawHotkeysFrame

/*
║ Arrowkeys - Move Cursor ║
║ Spacebar ---- Plot Char ║  █▓▒░
║ Delete --- Clear & Fill ║  
║ End ----- Roller On/Off ║  
║ O ---- Sniff Color/Char ║  
║ U+U ---- Undo last plot ║
║ PageDown --- Save! Yey! ║
║ PageUp ---- Load! Whee! ║
║ Esc -------- Quit Booo! ║
║ F12 ---- Boom (no exit) ║


Console.WriteLine = ("░░████████████░░░▒░▒▒▒▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓█▓█▓▓▓█▓▒░░▒▒░░▒░░░████████░░█░");
Console.WriteLine = ("█░░██████████▓░░▒░░░░▒▒▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓█▓▓██▓▓▓█▓▓█▒▒▒░░░░░░░░███████░█░░░");
Console.WriteLine = ("█░███████████▓░█░░▓▒▒▒▒▒▒▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓█▓▓█▓▓▓▓█▓░▒▒░▒░▓▒░░▒░██░████████░█░");
Console.WriteLine = ("░█░█████████▓▓█░░░▒▓▒▒▓▒▒▒▒▓██▓█▓██▓▓▓▓▓▓▓▓█▓▓▓█▓▓▓▓▒█▓▒▓▓▓▓░▒▒▓▒░░▒▒░▓░░░█▓██████░██░█");
Console.WriteLine = ("░░░█░███████▓░█░▒▒▒▒▒▓▓▒▓▓▒▒▓▓▓▓███▓██▓▓▓█▓█▓▓██▓██▒▓█▒▓▓▓▒▓▒▓░░▒▒▓▒▓░▒▒░█████████░█░░█");
Console.WriteLine = ("░█░█░███████▓░█▒▒░▒▒░▒▒▒▒▒▓▓▒▒▒▓█▒█▓▓██▒▓▓▒▓██▓███▓█░▒▓░▒░░▒░░▒░▒▓▒░░▒▒░██▓▓█████░░█░░░");
Console.WriteLine = ("███░█░███████▓░█░░▒▒▒▒▒▓▓▓▒░▒▒▒░░▒▒▒░▒▓▓▒░▓▒▓▓▓▓▒▓▓▒░░░░░░░░▓░░▓▓░▓▒▓░░░█▓▓██████░█░██░");
Console.WriteLine = ("█░█░██░██████▓░▒░▓▓▓█░▓░▓░░▓░░▓░▓▓░▒▒░░▒▒░░░░▒▒░░░░░▓░▓░░░░▓░░░▓░░▓░░▓▓░█▓░▓░███░░█░░░░");
Console.WriteLine = ("███░█░██████▓░░▓▓░██████████░█░░░░▓░▓░▓▓░░█▒█░░░▓▓░▓░░░░████████████░░▓▓█░▓▓████░█░█░█░");
Console.WriteLine = ("░████░██████▓░██░████████░░██████░░░░▓░░█░▒▓▒░█░░░▓██████░░██████████░███▓░░▓█████░░███");
Console.WriteLine = ("░███░░█░███▓▒█░███████░▒▒▓▓▒▒░████████░░░▒▒▒▓▒░░███████░▒▓▓▒░██████████▓███░▓███░█░░█░░");
Console.WriteLine = ("░███░█░░░██▓█▒▒▓░████░▒▒▒▓▓▒▒░████████▓▒▒░███▒▒▓░░███░░░▒▓▓▒░░░██████░█▓░███░███░█░██░░");
Console.WriteLine = ("░░██░█░░███░█▒▒▓█████████░░█████░███▒▓▒░▒█████▒▒▓▒░██████▒▒██████████░█▒▒░█░████░░░█░░░");
Console.WriteLine = ("░██████░███░░▒▒▒▓█████████████░██▓▒▒▒░░▒███░███░░░░▒▒░██░░███████░░█░█░▒▒░░░█████░░░█░░");
Console.WriteLine = ("░███░█░████░░░█▒▒▒▓▓█░░███░░░█▓▒▒▒▒░░▒▒██░█▓█░█▒░░▓░▓▒▒░███░░░░████░▓▒▓░██░░████░█░░█░░");
Console.WriteLine = ("░░░█░█░█░███░░░█▓▓▓▒▒▓░░░░▓▓▓▓▒▒▒▒░░▒▒░█▓░█░█░░█░▓░▓▓░▓▒▒▓▓▓▒▓▒▓▓▓▒▓░▒▓▓█░█░████░░█░█░░");
Console.WriteLine = ("░░█░██░███████░░░░░▒▓▓▒▒░▒▒▒▒▓▒░░░░░▒▒█░░░░░░░░░█░▒▓▓▓▒▒▓▓▓▓▒░▓▒▒▓▓▒░▒░░░░██████░░██░░░");
Console.WriteLine = ("░░███░█████████▓▓░░░░░░░░░░░░░░░░▒░░░░░░▒░░▓░▓░░░░▒▓▓▒▒▒▒▒▒▒▒▒░▒░▒░▓▒░░▒░▓████████░██░░");
Console.WriteLine = ("░░░█░██░░█████████▓░▒▒▒▒▒░▒░▒▒▒▒▒▒▒▒▒▒░░▒▒░▒░▒▒░▓▒▓▒▓▓░▓▒▒▒▒░░▒▒▒▒▒░▒▓▒░▓██████░███░░█░");
Console.WriteLine = ("░░░░██░░░███░███████░▒▓▓░░░░▓▓▒▓▒▒▒▒▒▒▒▒▒▒▒▒▒▒░▒▒▒▒▒░░▒▓▒▓▓▓▓░░░░▓▓░░░░████████░░██░░░█");
Console.WriteLine = ("░░█░█░██████████░████░░██████░▒▓▒▒▒▓░░▒▒▒░▒░▒▒░▒▒░▓░▓█▓▒▒░░░████████░░░██████░█░░██░░█░");
Console.WriteLine = ("░░███░░░██░██░█░█░███████████░░▒▓▓▒▓░▒░▓░▒▓█▓▓▒▒█▒▓░▒▓▒▒█░███████████████████░█░░█░██░░");
Console.WriteLine = ("░░░██░░██░░█░█░░█░████████████░▓▓▓▓▓▓▒█▓▒▓▓█▓▒█▓▓▒▓▓█▓▒▓▓█████████████████░██░░░░█░██░░");
Console.WriteLine = ("░░░░█░█░░░████░█░░██░░█████████░░█▒░▓▒▒░▒▓▓▒░▒▒█▓▒░▓░▒▒░███████████░███████░░█░░██░█░░░");
Console.WriteLine = ("░░░██░█░░░██░░░░█░██░░░████████░█░█░▓█▒▒░▓▒░▓█░█▓▒▒█▓▒▒░████████████░████░░░░░░░█░░░░░░");
Console.WriteLine = ("░░░██░░█░░██░░░█░░░█░░█░██░█████░░█░▓█░▒▒█▒░▓▒░█▒░█▒███████████████░█░████░░░░░░█░░░░░░");
Console.WriteLine = ("░░░█░░█░░░██░░░█░░░░█░░░░░░░░██████░█░░░░░▓░▓░░░░██░█████████████░█░░░█░░░░░░░░░█░░░░░░");


*/