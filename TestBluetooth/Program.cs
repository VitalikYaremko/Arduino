using System;
using System.IO.Ports;
using System.Runtime.InteropServices;
 
namespace test
{
    class Program
    {
        static int XM = 0, YM = 0;
        static int BX = 0, BY = 0;
        static int BufXR = 0, BufXL = 0;
        static int tempX = 0, tempY = 0;
        static int BufYU = 0, BufYD = 0;
        static bool Left = false, Right = false;
        static bool Up = false, Down = false;

        [DllImport("user32.dll")]
        static extern bool SetCursorPos(int xPos, int yPos);

        static void Main(string[] args)
        {
            labelGo:;

            Int32 xmax = System.Windows.Forms.SystemInformation.PrimaryMonitorSize.Width;
            Int32 ymax = System.Windows.Forms.SystemInformation.PrimaryMonitorSize.Height;
            Console.WriteLine("MonitorSize\n Width = {0} Height = {1}", xmax, ymax);


            System.Threading.Timer time = new System.Threading.Timer(ComputeBoundOp, 5, 0, 100);


            SerialPort port = new SerialPort("COM11", 9600);
            try
            {
                port.Open();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                goto labelGo;
            }

            double coef = 1;
            int pX = 0, pY = 0;
            int countLoop = 0;
            int resX = 0, resY = 0;

            try
            {
                while (true)
                {

                    string S = port.ReadLine();

                    String[] point = S.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

                    if (S != null)
                    {
                        if (BX != Convert.ToInt32(point[0]))
                        {
                            BX = Convert.ToInt32(point[0]);
                        }
                        if (BY != Convert.ToInt32(point[1]))
                        {
                            BY = Convert.ToInt32(point[1]);
                        }
                    }
                   

                    pX = (int)Math.Round(BX * coef);
                    pY = (int)Math.Round(BY * coef);

                    XM = pX + BufXR + BufXL + 683;
                    YM = pY + BufYU + BufYD - 384;

                    long Map(long x, long in_min, long in_max, long out_min, long out_max)
                    {
                        return (x - in_min) * (out_max - out_min) / (in_max - in_min) + out_min;
                    }

                    if (countLoop > 10)
                    {
                        countLoop = 0;
                        resX = XM - tempX;// right or left move
                        tempX = XM;

                        resY = YM - tempY;
                        tempY = YM;

                    }

                    if (resX < -1)
                    {
                        Left = true;
                        Right = false;
                    }
                    else if (resX > 1)
                    {
                        Right = true;
                        Left = false;
                    }
                    else
                    {
                        Right = false;
                        Left = false;
                    }

                    if (resY < -1)
                    {
                        Up = true;
                        Down = false;
                    }
                    else if (resY > 1)
                    {
                        Down = true;
                        Up = false;
                    }
                    else
                    {
                        Up = false;
                        Down = false;
                    }

                    if (XM < 0 && countLoop > 5)
                    {
                        BufXR = 0;
                        if (Right == true)
                        {
                            BufXR = -XM;
                        }
                    }
                    if (XM > 1366 && countLoop > 5)
                    {
                        BufXL = 0;
                        if (Left == true)
                        {
                            BufXL = -(XM - 1366);
                        }
                    }
                    if (YM < 0 && countLoop > 5)
                    {
                        BufYU = 0;
                        if (Up == true)
                        {
                            BufYU = -YM;
                        }
                    }
                    if (YM > 768 && countLoop > 5)
                    {
                        BufYD = 0;
                        if (Down == true)
                        {
                            BufYD = -(YM - 768);
                        }
                    }


                  // XM = Convert.ToInt32(Map(pX, -1366, 1366,0, 1366));
                 //  YM = Convert.ToInt32(Map(pY, -768, 768, 0, 768));

                    SetCursorPos(XM, YM);
                    countLoop++;

                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                Console.ReadKey();
                goto labelGo;
            }

            //port.Close();
        }
        public static void ComputeBoundOp(Object state)
        {

            //Console.Write("BX={0},BY={1}", BX, BY);
            //Console.Write("              ");
            //Console.Write("XM={0},YM={1},BUFXR={2},BUFXL={3}", XM, YM, BufXR, BufXL);
            //Console.Write("    ");
            if (Right is true)
            {
                Console.Write("=>");
            }
            if (Left is true)
            {
                Console.Write("<=");
            }
            Console.WriteLine();

        }


    }
}
