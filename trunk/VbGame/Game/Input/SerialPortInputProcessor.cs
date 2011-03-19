using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using VirtualBicycle.MathLib;

namespace VirtualBicycle.Input
{
    public class SerialPortInputProcessor : InputProcessor, IDisposable
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

        const char EndOfDataTrunk = '#';

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
            string[] ports = SerialPort.GetPortNames();
            for (int i = 0; i < ports.Length; i++)
            {
                
                SerialPort p = new SerialPort(ports[i], 19200, Parity.None, 8, StopBits.One);

                p.NewLine = EndOfDataTrunk.ToString();
                p.ReadTimeout = 50;
                p.WriteTimeout = 50;
                p.Encoding = Encoding.ASCII;

                try
                {
                    if (p.IsOpen)
                        continue;
                    else
                        p.Open();

                    p.Write("R");

                    try
                    {
                        string str = p.ReadLine();

                        if (str == "VIRTUALBICYCLE")
                        {
                            return p;
                        }
                    }
                    catch (TimeoutException) { }

                    p.Close();
                    p.Dispose();
                }
                catch { }

            }
            return null;
        }

        public SerialPortInputProcessor(InputManager manager)
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

                for (int i = 0; i < 124; i += 4)
                {
                    if (recvBuf[i + 3] == EndOfDataTrunk)
                    {
                        DataTrunk dta = new DataTrunk(recvBuf[i], recvBuf[i + 1], recvBuf[i + 2]);

                        switch (dta.type)
                        {
                            case DataType.ResetDataTag:
                                return;
                            case DataType.HandlebarDataTag:
                                {
                                    short angle = (short)(dta.byte1 | (dta.byte2 << 8));

                                    float val = MathEx.PiOver2 * (float)angle / 256f;
                                    float lstval = MathEx.PiOver2 * (float)lastAngle / 256f;

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
                                    break;
                                }
                            case DataType.WheelDataTag:
                                if (dataDt > float.Epsilon)
                                {
                                    ushort wheel = (ushort)(dta.byte1 | (dta.byte2 << 8));

                                    if (lastWheel != wheel)
                                    {
                                        if (wheel < lastWheel)
                                        {
                                            lastWheel = 0;
                                            wheel = 0;
                                        }

                                        float deltaS = 0.001f * (wheel - lastWheel) * 1.75f;

                                        if (deltaS > 0)
                                        {
                                            float v = deltaS / dataDt;

                                            float actV;
                                            qVelocity.Enqueue(v);

                                            if (qVelocity.Count > 15)
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
                                                actV = v;
                                            }

                                            v = actV;
                                            Manager.OnWheelSpeedChanged(v, v - lastSpeed, false);

                                            //if (v > 1f)
                                            //{
                                            //    if (lastEnterTime > 2)
                                            //    {
                                            //        Manager.OnEnter();
                                            //        lastEnterTime = 0;
                                            //    }
                                            //}
                                            //else
                                            //{
                                            //    lastEnterTime += dataDt;
                                            //}

                                            lastSpeed = v;
                                            lastWheel = wheel;
                                        }
                                    }

                                    dataDt = 0;
                                }

                                break;
                            case DataType.ButtonDataTag:

                                int hp = dta.byte2 & (1 << 5);
                                if (hp != lastHPulse)
                                {
                                    Manager.OnHeartPulse();

                                    lastHPulse = hp;
                                }
                                
                                
                                if ((dta.byte2 & (0x80)) == 1)
                                {
                                    short angle = 0;

                                    if (lastAngle != angle)
                                    {
                                        float val = MathEx.PiOver2 * (float)angle / 256f;
                                        float lstval = MathEx.PiOver2 * (float)lastAngle / 256f;

                                        Manager.OnHandlebarRotated(val, val - lstval);

                                        lastAngle = angle;
                                    }
                                }

                                if ((dta.byte2 & ButtonTLId) == 0)
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

                                if ((dta.byte2 & ButtonTRId) == 0)
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

                                if ((dta.byte2 & ButtonMLId) == 0)
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

                                if ((dta.byte2 & ButtonMRId) != 0)
                                {
                                    if (!isResetPressed)
                                    {
                                        //port.Write(ResetDataTag);
                                        Manager.OnReset();

                                        isResetPressed = true;
                                    }
                                }
                                else
                                {
                                    isResetPressed = false;
                                }
                                break;
                            default:
                                return;
                        }
                    }
                }
            }
            catch (TimeoutException)
            {

            }
        }

        public override void Update(float dt)
        {
            if (port != null)
            {
                try
                {
                    ReceiveData();

                    port.Write(DataRequestTag);
                }
                catch { }

                dataDt += dt;
            }
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
