using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using VirtualBicycle.MathLib;
 

namespace VirtualBicycle.Input
{
    //这个类处理电机力反馈输出。
    public class SerialPortOutputProcessor : InputProcessor, IDisposable
    {
        enum DataType : byte
        {
            Unknown = 0,
            HandlebarDataTag = (byte)HandlebarDataTagC,
            WheelDataTag = (byte)WheelDataTagC,
            ResetDataTag = (byte)ResetDataTagC,
            ButtonDataTag = (byte)ButtonDataTagC,
            DataRequestTag = (byte)DataRequestTagC
        }

        struct DataTrunk
        {
            public DataType type;

            public byte byte1;
            public byte byte2;

            public DataTrunk(byte a, byte b, byte c)
            {
                this.type = (DataType)a;
                this.byte1 = b;
                this.byte2 = c;
            }
        }

        const char HandlebarDataTagC = 'A';
        const char WheelDataTagC = 'B';
        const char ResetDataTagC = 'C';
        const char ButtonDataTagC = 'S';
        const char DataRequestTagC = 'D';

        const char EndOfDataTrunk = '\n';

        // 操纵盒按钮
        // f7 fd
        // fb fe
        // df ef
        //
        // 00001000 00000010
        // 00000100 00000001
        // 00100000 00010000
        // 
        // 0x08 0x02
        // 0x04 0x01
        // 0x20 0x10
        const int ButtonTLId = 0x08;
        const int ButtonTRId = 0x02;
        const int ButtonMLId = 0x04;
        const int ButtonMRId = 0x01;
        const int ButtonBLId = 0x20;
        const int ButtonBRId = 0x10;


        static readonly string DataRequestTag = DataRequestTagC.ToString();
        static readonly string ResetDataTag = ResetDataTagC.ToString();

        bool isVPressed;
        bool isEscPressed;
        bool isEnterPressed;
        bool isResetPressed; 
        //float lastEnterTime;

        int itemLeftCoolDown;
        int itemRightCoolDown;

        short lastAngle;
        ushort lastWheel;
        float lastSpeed;

        int lastHPulse;


        object syncHelper = new object();

        SerialPort port;
        float dataDt;

        Queue<float> qVelocity = new Queue<float>();


        DataTrunk ReadDataTrunk()
        {
            byte[] data = new byte[4];
            port.Read(data, 0, 4);
            //int b1 = port.ReadByte();
            //int b2 = port.ReadByte();
            //int b3 = port.ReadByte();
            //int b4 = port.ReadByte();

            if (data[3] == EndOfDataTrunk)
            {
                return new DataTrunk(data[0], data[1], data[2]);
            }
            return new DataTrunk();
        }

        SerialPort SelectPort()
        {
            string[] ports2 = SerialPort.GetPortNames();
            
            for (int i = 0; i < ports2.Length; i++)
            {
                SerialPort p2 = new SerialPort(ports2[i], 19200, Parity.None, 8, StopBits.One);

                p2.NewLine = "\r\n".ToString();
                p2.ReadTimeout = 50;
                p2.WriteTimeout = 50;
                p2.Encoding = Encoding.ASCII;
                
                try
                {
                    if (p2.IsOpen)
                        continue;
                    else
                        p2.Open();
                    
                    p2.Write("GVER");//发送设备识别码//指令结构以0D结尾
                    p2.Write("\r");
                   

                    try
                    {
                        string str = p2.ReadLine();
                        
                        
                        if (String.Compare(str, 0, "#MLDS3810,V1.24", 0, 9)!=0);//返回识别码,比较前9 位
                        {
                            return p2;
                        }
                    }
                    catch (TimeoutException) { }

                    p2.Close();
                    p2.Dispose();
                }
                catch { }

            }
            return null;
        }

        

        public SerialPortOutputProcessor(InputManager manager)
            : base(manager)
        {
            port = SelectPort();

            if (port != null)
            {
                port.ReadTimeout = 1;

                port.Write(ResetDataTag);
            }
        }

        public bool IsValid
        {
            get { return port != null; }
        }



        void ReceiveData()
        {
            byte[] recvBuf = new byte[128];
            try
            {
                port.Read(recvBuf, 0, 128);
                ProcessReceivedData(recvBuf);
            }
            catch (TimeoutException)
            {
            }
        }

        private static void ProcessReceivedData(byte[] recvBuf)
        {
            for (int i = 0; i < 124; i += 4)
            {
                if (recvBuf[i + 3] == EndOfDataTrunk)
                {
                    DataTrunk dta = new DataTrunk(recvBuf[i], recvBuf[i + 1], recvBuf[i + 2]);

                    switch (dta.type)
                    {
                        case DataType.ResetDataTag:

                            break;
                        case DataType.HandlebarDataTag:
                            break;
                        case DataType.WheelDataTag:
                            break;
                        case DataType.ButtonDataTag:
                            break;
                        default:

                            break;
                    }
                }
            }
        }

        
        public override void Update(float dt)
        {
            if (port != null)
            {
                try
                {
                    
                }
                catch { }

                dataDt += dt;
            }
        }

        public string ReadLine()
        {
            if (this.port != null)
            {
                try
                {
                    return this.port.ReadLine();

                }
                catch (TimeoutException)
                {
                }

            }
            else
            {
               // this.port = SelectPort();
            }
            return null;
 
        }

        public void SentLine(string str)
        {
            if (this.port != null)
            {
                try
                {
                    this.port.WriteLine(str);

                }
                catch (TimeoutException)
                {
                }

            }
            else {
                //this.port = SelectPort();
            }
        }
        public void Sent(string str)
        {
            if (this.port != null)
            {
                try
                {
                    this.port.Write(str);

                }
                catch (TimeoutException)
                {
                }

            }
        }
        public void Sent(byte[] buffer, int offset, int count)
        {
            if (this.port != null)
            {
                try
                {
                    this.port.Write(buffer, offset, count);
                }
                catch (TimeoutException)
                {
                }

            }
        }      
              
        public void SentData(byte[] sentBuf, int offset, int count)
        {
            try
            {
                port.Write(sentBuf, offset, count);
            }
            catch (TimeoutException)
            {
            }
        }

        public SerialPort GetPort()
        {

            return this.port;
        }

        #region IDisposable Members

        public bool Disposed
        {
            get;
            private set;
        }

        public void Dispose()
        {
            if (!Disposed)
            {
                if (port != null)
                {
                    port.Close();
                    port.Dispose();
                }
                port = null;
                Disposed = true;
            }
        }

        #endregion

        
    }
}
