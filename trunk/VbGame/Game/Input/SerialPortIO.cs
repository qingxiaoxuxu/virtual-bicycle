using System;
using System.Collections.Generic;
using System.Text;
using System.IO.Ports;
using VirtualBicycle.Collections;
using System.Threading;
using VirtualBicycle.MathLib;

namespace VirtualBicycle.Input
{
    unsafe class SerialPortIO : InputProcessor, IDisposable
    {
        const int PreservedSize = 4;
        const byte EndOfPack = 0xFE;
        const byte PackHeader = 0xff;

        enum PackType : byte
        {
            Recv,
            Send
        }

        struct DataRecvPack
        {
            public byte Header;
            public PackType Type;
            public byte Velocity;
            public byte Angle;
            public byte Btn1;
            public byte Btn2;
            public fixed byte Preserved[PreservedSize];
        }
        struct DataSendPack 
        {
            public byte Header;
            public PackType Type;
            public byte Force;
            public fixed byte Preserved[PreservedSize];

        }


        bool isVPressed;
        bool isEscPressed;
        bool isEnterPressed;
        bool isResetPressed;
        bool lastHeartState;
        //float lastEnterTime;

        int itemLeftCoolDown;
        int itemRightCoolDown;

        byte lastAngle;
        float lastSpeed;

        Queue<float> qVelocity = new Queue<float>();

        object syncHelper = new object();
        FastList<DataRecvPack> dataPackBuffer = new FastList<DataRecvPack>();
        FastList<byte> dataBuffer = new FastList<byte>();
        byte[] recvBuffer = new byte[128];


        FastList<DataSendPack> dataSendBuffer = new FastList<DataSendPack>();
        byte[] sendBuffer = new byte[128];

        SerialPort port;

        public bool IsValid
        {
            get { return port != null; }
        }

        SerialPort SelectPort()
        {
            string[] ports = SerialPort.GetPortNames();
            for (int i = 0; i < ports.Length; i++)
            {
                SerialPort p = new SerialPort(ports[i], 115200, Parity.None, 8, StopBits.One);
                p.DataReceived += new SerialDataReceivedEventHandler(port_DataReceived);
                try
                {
                    if (p.IsOpen)
                        continue;
                    else
                        p.Open();

                    Thread.Sleep(100);


                    if (dataPackBuffer.Count > 0)
                    {
                        return p;
                    }

                    p.Close();
                    p.Dispose();
                }
                catch { }

            }
            return null;
        }

        public SerialPortIO(InputManager manager)
            : base(manager)
        {
            port = SelectPort();

            //if (port != null)
            //{
                //port.ReadTimeout = 1;

                //port.DataReceived += new SerialDataReceivedEventHandler(port_DataReceived);
            //}
        }

        void port_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            SerialPort port = (SerialPort)(sender);
            {
                int count = port.Read(recvBuffer, 0, 128);

                for (int i = 0; i < count; i++)
                {
                    dataBuffer.Add(recvBuffer[i]);
                }
            }


            int numPassed = 0;
            for (int i = 0; i < dataBuffer.Count; i++)
            {
                if (dataBuffer[i] == PackHeader &&
                    dataBuffer[i + 1] == (byte)PackType.Recv)
                {
                    if (i < dataBuffer.Count - sizeof(DataRecvPack))
                    {
                        DataRecvPack pack;
                        pack.Header = dataBuffer[i];
                        pack.Type = (PackType)dataBuffer[i + 1];
                        pack.Velocity = dataBuffer[i + 2];
                        pack.Angle = dataBuffer[i + 3];
                        pack.Btn1 = dataBuffer[i + 4];
                        pack.Btn2 = dataBuffer[i + 5];

                        for (int j = 0; j < PreservedSize; j++)
                        {
                            pack.Preserved[j] = dataBuffer[i + 6 + j];
                        }

                        if (dataBuffer[i + 6 + PreservedSize] == EndOfPack)
                        {
                            lock (syncHelper)
                            {
                                dataPackBuffer.Add(ref pack);
                            }
                            numPassed = i + 6 + PreservedSize + 1;
                        }
                    }
                }
            }

            if (numPassed != 0)
                dataBuffer.RemoveRange(0, numPassed);
            
        }


        #region IDisposable 成员


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


        public override void Feedback(float f)
        {
            DataSendPack pack;
            pack.Header = PackHeader;
            pack.Type = PackType.Send;
            for (int i = 0; i < PreservedSize; i++)
            {
                pack.Preserved[i] = 0;
            }


            int ff = (int)(f * 253);
            if (ff > 253) ff = 253;
            else if (ff < 0) ff = 0;
            pack.Force = (byte)ff;

            dataSendBuffer.Add(ref pack);
        }

        static float ConvertAngle(byte angle)
        {
            return MathEx.Degree2Radian(angle - 128);
        }
        static float ConvertSpeed(byte spd)
        {
            return (spd - 128) * 0.08f * 1.75f; 
        }


        public override void Update(float dt)
        {
            lock (syncHelper)
            {
                for (int i = 0; i < dataPackBuffer.Count; i++)
                {
                    DataRecvPack pack = dataPackBuffer[i];
                    {
                        byte angle = pack.Angle;

                        float val = ConvertAngle(angle);
                        float lstval = ConvertAngle(lastAngle);

                        if (lastAngle != angle)
                        {
                            Manager.OnHandlebarRotated(val, val - lstval);
                            lastAngle = angle;
                        }


                        float SlowSwitchAngle = MathEx.Degree2Radian(20);
                        float FastSwitchAngle = MathEx.Degree2Radian(45);

                        if (val > SlowSwitchAngle && val < FastSwitchAngle)
                        {
                            if (itemLeftCoolDown-- <= 0)
                            {
                                Manager.OnItemMoveRight();
                                itemLeftCoolDown = 30;
                            }
                        }
                        else if (val < -SlowSwitchAngle && val > -FastSwitchAngle)
                        {
                            if (itemRightCoolDown-- <= 0)
                            {
                                Manager.OnItemMoveLeft();
                                itemRightCoolDown = 30;
                            }
                        }
                        else if (val >= FastSwitchAngle)
                        {
                            if (lstval > SlowSwitchAngle && lstval < FastSwitchAngle)
                            {
                                itemLeftCoolDown = 0;
                            }

                            if (itemLeftCoolDown-- <= 0)
                            {
                                Manager.OnItemMoveRight();
                                itemLeftCoolDown = 10;
                            }
                        }
                        else if (val <= -FastSwitchAngle)
                        {
                            if (lstval < -SlowSwitchAngle && lstval > -FastSwitchAngle)
                            {
                                itemRightCoolDown = 0;
                            }

                            if (itemRightCoolDown-- <= 0)
                            {
                                Manager.OnItemMoveLeft();
                                itemRightCoolDown = 10;
                            }
                        }
                        else
                        {
                            itemLeftCoolDown = 0;
                            itemRightCoolDown = 0;
                        }
                    }
                    {
                        byte speed = pack.Velocity;

                        if (speed != 0)
                        {
                            float vel = ConvertSpeed(speed);

                            {
                                float actV;
                                qVelocity.Enqueue(vel);

                                while (qVelocity.Count > 15)
                                {
                                    qVelocity.Dequeue();
                                }
                                if (qVelocity.Count > 0)
                                {
                                    actV = 0;
                                    foreach (float lean in qVelocity)
                                    {
                                        actV += lean;
                                    }
                                    actV /= qVelocity.Count;
                                }
                                else
                                {
                                    actV = vel;
                                }

                                vel = actV;
                                Manager.OnWheelSpeedChanged(vel, vel - lastSpeed, false);

                                lastSpeed = vel;
                            }
                        }

                    }

                    if (i==0)
                    {
                         int flag = (pack.Btn1 >> 6) & 1;
                         if (flag != 0)
                         {
                             if (!isEnterPressed)
                             {
                                 Manager.OnEnter();
                                 isEnterPressed = true;
                             }
                         }
                         else 
                         {
                             isEnterPressed = false;
                         }

                         flag = (pack.Btn1 >> 5) & 1;
                         if (flag!=0)
                         {
                             if (!isResetPressed)
                             {
                                 Manager.OnReset();
                                 isResetPressed = true;
                             }
                         }
                         else
                         {
                             isResetPressed = false;
                         }

                         flag = (pack.Btn1 >> 4) & 1;
                         if (flag != 0)
                         {
                             if (!isEscPressed)
                             {
                                 Manager.OnEscape();
                                 isEscPressed = true;
                             }
                         }
                         else 
                         {
                             isEscPressed = false;
                         }

                         flag = (pack.Btn1 >> 3) & 1;
                         if (flag != 0)
                         {
                             if (!isVPressed)
                             {
                                 Manager.OnViewChanged();
                                 isVPressed = true;
                             }
                         }
                         else
                         {
                             isVPressed = false;
                         }

                         flag = (pack.Btn1 >> 2) & 1;
                         if (flag != 0)
                         {
                             if (!lastHeartState)
                             {
                                 Manager.OnHeartPulse();
                                 lastHeartState = true;
                             }
                         }
                         else
                         {
                             lastHeartState = false;
                         }

                    }



                }

                dataPackBuffer.Clear();
            }


            for (int i = 0; i < dataSendBuffer.Count; i++) 
            {
                DataSendPack pack = dataSendBuffer[i];
                sendBuffer[0] = pack.Header;
                sendBuffer[1] = (byte)pack.Type;
                sendBuffer[2] = pack.Force;
                for (int j = 0; j < PreservedSize; j++) 
                {
                    sendBuffer[3 + j] = pack.Preserved[j];
                }
                sendBuffer[7] = EndOfPack;
                port.Write(sendBuffer, 0, sizeof(DataSendPack) + 1);
            }
            dataSendBuffer.Clear();

        }
    }
}
