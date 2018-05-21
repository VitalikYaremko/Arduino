using System;
using System.IO.Ports;
using System.Runtime.InteropServices;
using System.Threading;
using System.Collections.Generic;

namespace test
{
    class Program
    {
        static double varVoltX = 2.31;  
        static double varProcessX = 1; 
        static double PcX = 0.0;
        static double GX = 0.0;
        static double PX = 1.0;
        static double XpX = 0.0;
        static double ZpX = 0.0;
        static double XeX = 0.0;

        static double varVoltY = 2;  
        static double varProcessY = 1; 
        static double PcY = 0.0;
        static double GY = 0.0;
        static double PY = 1.0;
        static double XpY = 0.0;
        static double ZpY = 0.0;
        static double XeY = 0.0;
   
        static int BX = 0, BY = 0;
        static int BufXR = 0, BufXL = 0;
        static int BufYU = 0, BufYD = 0;
        static int SoffsetX = 0, SoffsetY = 0;
        static bool Left = false, Right = false;
        static bool Up = false, Down = false;
        static String[] point;

        static int xxx = 0;
        static int yyy = 0;

        static SerialPort port = new SerialPort("COM11", 9600);

        [DllImport("user32.dll")]
        static extern bool SetCursorPos(int xPos, int yPos);

        static Int32 xmax, ymax;
        static List<int> numbersX = new List<int>();
        static List<int> numbersY = new List<int>();

        static void Main(string[] args)
        {
            try
            {
                xmax = System.Windows.Forms.SystemInformation.PrimaryMonitorSize.Width;
                ymax = System.Windows.Forms.SystemInformation.PrimaryMonitorSize.Height;
                Console.WriteLine("MonitorSize\n Width = {0} Height = {1}", xmax, ymax);

                ConectPort();

                System.Threading.Timer time = new System.Threading.Timer(ComputeBoundOp, 5, 0, 100);
                Thread.Sleep(500);
                string S = null;
                S = port.ReadLine();
                Thread.Sleep(500);
                S = port.ReadLine();

                point = S.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                BX = Convert.ToInt32(point[0]);
                BY = Convert.ToInt32(point[1]);
                SoffsetX = xmax / 2 - BX;
                SoffsetY = ymax / 2 - BY;
                SetCursorPos(xmax / 2, ymax / 2);
                int bufX = 0, bufY = 0;

                while (true)
                {
                    Thread thread = new Thread(new ThreadStart(Processing));
                    thread.Start();
                    if (numbersX.Count > 200 && numbersY.Count>200)
                    {
                        Thread thread2 = new Thread(new ThreadStart(ClearRom));
                        thread2.Start();
                    }

                    S = null;
                    S = port.ReadLine();
                    point = S.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

                    BX = Convert.ToInt32(point[0]);
                    BY = Convert.ToInt32(point[1]);

                    if (BX != bufX)
                    {
                        numbersX.Add(BX + SoffsetX);
                    }
                    if (BY != bufY)
                    {
                        numbersY.Add(BY + SoffsetY);
                    }

                    bufX = BX;
                    bufY = BY;

                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                Console.ReadKey();
            }
        }
        public static void ComputeBoundOp(Object state)
        {

            //Console.WriteLine("bx= {0}, by= {1}", BX, BY);
            Console.WriteLine(yyy);
            // Thread.Sleep(1000);
            // Console.WriteLine(numbersX.Count);
        }

        public static void ClearRom()
        {
            Console.WriteLine(" Clear");
            for (int i = 0; i < 50; i++)
            {
                numbersX.RemoveAt(i);
                numbersY.RemoveAt(i);
            }
        }
        public static void Processing()
        {
            try
            {
                if (numbersX.Count > 5)
                {
                    xxx = numbersX[numbersX.Count - 1];
                    yyy = numbersY[numbersY.Count - 1];
                    xxx = Convert.ToInt32(FilterX(xxx));
                    yyy = Convert.ToInt32(FilterY(yyy));

                    int moveX_1 = numbersX[numbersX.Count - 2] - numbersX[numbersX.Count - 1];
                    int moveX_2 = numbersX[numbersX.Count - 3] - numbersX[numbersX.Count - 2];
                    int moveX_3 = numbersX[numbersX.Count - 4] - numbersX[numbersX.Count - 3];

                    if (numbersY.Count > 5)
                    {
                        int moveY_1 = numbersY[numbersY.Count - 2] - numbersY[numbersY.Count - 1];
                        int moveY_2 = numbersY[numbersY.Count - 3] - numbersY[numbersY.Count - 2];
                        int moveY_3 = numbersY[numbersY.Count - 4] - numbersY[numbersY.Count - 3];
                        if (moveY_1 > 0 && moveY_2 > 0 && moveY_3 > 0)
                        {
                            Down = false;
                            Up = true;
                        }
                        else if (moveY_1 < 0 && moveY_2 < 0 && moveY_3 < 0)
                        {
                            Down = true;
                            Up = false;
                        }
                        else
                        {
                            Down = false;
                            Up = false;
                        }
                    }

                    if (moveX_1 > 0 && moveX_2 > 0 && moveX_3 > 0)
                    {
                        Left = true;
                        Right = false;
                    }
                    else if (moveX_1 < 0 && moveX_2 < 0 && moveX_3 < 0)
                    {
                        Right = true;
                        Left = false;
                    }
                    else
                    {
                        Right = false;
                        Left = false;
                    }

                    xxx = xxx + BufXR + BufXL;
                    yyy = yyy + BufYD + BufYU;

                    if (xxx < 0)
                    {
                        BufXR = 0;
                        if (Right == true)
                        {
                            BufXR = -xxx;
                        }
                    }
                    if (xxx > xmax)
                    {
                        BufXL = 0;
                        if (Left == true)
                        {
                            BufXL = -(xxx - xmax);
                        }
                    }

                    if (yyy < 0)
                    {
                        BufYU = 0;
                        if (Down == true)
                        {
                            BufYU = -yyy;
                        }
                    }
                    if (yyy > ymax)
                    {
                        BufYD = 0;
                        if (Up == true)
                        {
                            BufYD = -(yyy - ymax);
                        }
                    }

                    SetCursorPos(xxx, yyy);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                Console.ReadKey();
            }
        }
        public static void ConectPort()
        {
            try
            {
                Console.WriteLine("Whait to connection ...");
                while (port.IsOpen == false)
                {
                    Console.WriteLine("...");
                    port.Open();
                }
                Console.WriteLine("Connect ! ");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                Console.ReadKey();
            }
        }

        static public double FilterX(double val)
        {   
            PcX = PX + varProcessX;
            GX = PcX / (PcX + varVoltX);
            PX = (1 - GX) * PcX;
            XpX = XeX;
            ZpX = XpX;
            XeX = GX * (val - ZpX) + XpX;  
            return (XeX);
        }
        static public double FilterY(double val)
        {   
            PcY = PY + varProcessY;
            GY = PcY / (PcY + varVoltY);
            PY = (1 - GY) * PcY;
            XpY = XeY;
            ZpY = XpY;
            XeY = GY * (val - ZpY) + XpY;  
            return (XeY);
        }
    }
}
