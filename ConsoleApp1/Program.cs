using System;
using System.IO.Ports;
using System.Runtime.InteropServices;
using System.Threading;
using System.Collections.Generic;
using MouseEvents;
using System.Threading.Tasks;
using System.IO;

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
        static bool StopMove = false;
        static bool btnClick = false;

        static int StmX = 0, StmY = 0;
        static int StmX_Spos = 0, StmY_Spos = 0;
        static String[] point;

        static int xxx = 0;
        static int yyy = 0;

        static int btn_res = 0, btn_1 = 0, btn_2 = 0, btn_3 = 0;
        static int bufX = 0, bufY = 0;
 
        static string S = null;
     //   static SerialPort port = new SerialPort("COM11", 9600);
        static SerialPort port;
        [DllImport("user32.dll")]
        static extern bool SetCursorPos(int xPos, int yPos);

        static Int32 xmax, ymax;
        static List<int> numbersX = new List<int>();
        static List<int> numbersY = new List<int>();
 
        static void Main(string[] args)
        {
            try
            {
                String NumberPortDefault =  System.IO.File.ReadAllText(@"config.txt").Replace("\n", " ");
                Console.WriteLine("Default num PORT : ={0}", NumberPortDefault);

                port = new SerialPort();

                Console.WriteLine("To change the port, open the configuration file (config.txt) ");

                string[] Ports = SerialPort.GetPortNames();
                for(int i = 1; i <= Ports.Length; i++)
                {
                    Console.WriteLine(i + " - " + Ports[i-1]);
                }
                port.PortName =  NumberPortDefault;
                port.BaudRate = 9600;
 
                xmax = System.Windows.Forms.SystemInformation.PrimaryMonitorSize.Width;
                ymax = System.Windows.Forms.SystemInformation.PrimaryMonitorSize.Height;
                Console.WriteLine("MonitorSize\n Width = {0} Height = {1}", xmax, ymax);

                ConectPort();

                System.Threading.Timer time = new System.Threading.Timer(ComputeBoundOp, 5, 0, 100);
                Thread.Sleep(500);
 
                S = port.ReadLine();
                Thread.Sleep(500);
                S = port.ReadLine();
               
                point = S.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

                BX = Convert.ToInt32(point[0]);
                BY = Convert.ToInt32(point[1]);
                SoffsetX = xmax / 2 - BX;
                SoffsetY = ymax / 2 - BY;
                SetCursorPos(xmax / 2, ymax / 2);

                while (true)
                {
  
                    Task ProcessingTask = new Task(Processing);
                    Task ReadTask = new Task(ReadPort);
                    Task ClearTask = new Task(ClearRom);

                    ReadTask.Start();
                    ReadTask.Wait();

                    ProcessingTask.Start();
                    ProcessingTask.Wait();
 
                    if (numbersX.Count > 500 && numbersY.Count > 500)
                    {
                        ClearTask.Start();
                    }
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
            //Console.WriteLine("btn_1={0},btn_2={1},btn_3={2}", btn_1, btn_2, btn_3);
            // Thread.Sleep(1000);
           // Console.WriteLine(btn_res);
        }

        public static void ReadPort()
        {
            // Console.WriteLine("StartRead");
            Task ButtonTask = new Task(BtnController);
            try
            {
                S = null;
                S = port.ReadLine();
                point = S.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                if (S != null)
                {
                    BX = Convert.ToInt32(point[0]);
                    BY = Convert.ToInt32(point[1]);
                    btn_res = Convert.ToInt32(point[2]);// 1 12 123
                }
                
                ButtonTask.Start();
                
                if (BX != bufX)
                {
                    Console.WriteLine("X={0}",StmX);
                    numbersX.Add(BX + SoffsetX + StmX);//edit
                }
                if (BY != bufY)
                {
                    Console.WriteLine("         Y={0}",StmY);
                    numbersY.Add(BY + SoffsetY + StmY);//edit
                }
                bufX = BX;  
                bufY = BY;
              //  Console.WriteLine("EndRead");
                 
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
           
        }
        public static void ClearRom()
        {
            try
            {
                Console.WriteLine(" Clear");
                for (int i = 0; i < 50; i++)
                {
                    numbersX.RemoveAt(i);
                    numbersY.RemoveAt(i);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
          
        }
        public static void Processing()
        {
           // Console.WriteLine("StartProcessing");
            
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

                    if(StopMove != true)
                    {
                        SetCursorPos(xxx, yyy );
                    }
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
        public static void StopMoveFix()
        {
                StmX = StmX_Spos - numbersX[numbersX.Count - 1];
                StmY = StmY_Spos - numbersY[numbersY.Count - 1];

                Console.WriteLine("Stop");
        }

        static bool clickLeft = false, clickRight = false, Set_StartPos = false;
        public static void BtnController()
        {
             Task MoveStop = new Task(StopMoveFix);
           // Thread MoveStop = new Thread(StopMoveFix);
            switch (btn_res)
            {
                case 0:
                   // Console.WriteLine("case 0 ");
                    if (clickLeft == true)
                    {
                        MEvents.Up(MButtons.LEFT);
                    }
                    if(clickRight == true)
                    {
                        MEvents.Up(MButtons.RIGHT);
                    }
                    clickLeft = false;
                    clickRight = false;

                    Set_StartPos = false;

                    if (StopMove is true)
                    {
                        StopMove = false;
                        MoveStop.Start();
                    }
                    break;
                case 5://1
                    Console.WriteLine("case 1 ");

                    if (clickLeft is false)
                     {
                        Console.Beep();
                        MEvents.Down(MButtons.LEFT);
                        clickLeft = true;
                    }
                    break;
                case 2:
                    Console.WriteLine("case 2 ");
                    if(clickRight is false)
                    {
                        MEvents.Down(MButtons.RIGHT);
                        clickRight = true;
                    }
                    break;
                case 1:
                    if (Set_StartPos == false)
                    {
                        StmX_Spos = numbersX[numbersX.Count - 1];
                        StmY_Spos = numbersY[numbersY.Count - 1];
                        Set_StartPos = true;
                    }
                    StopMove = true;
                    break;
            }
            //  Console.WriteLine("BtnControllerEnd");
        }
    }
}
