using System;
using System.Collections.Generic;
using System.Text;
using System.IO.Ports;

namespace VirtualBicycle.Input
{
    public class WirelessInputProcessor : InputProcessor, IDisposable
    {
        const char HandlebarDataTagC = 'A';
        const char WheelDataTagC = 'B';
        const char ResetDataTagC = 'C';
        const char ButtonDataTagC = 'S';
        const char DataRequestTagC = 'D';
        const char IdentifierTagC = 'V';

        enum DataType : int
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

        public WirelessInputProcessor(InputManager mananger)
            : base(mananger)
        {

        }

        SerialPort SelectPort()
        {
            string[] ports = SerialPort.GetPortNames();
            for (int i = 0; i < ports.Length; i++)
            {
                SerialPort p = new SerialPort(ports[i], 19200, Parity.None, 8, StopBits.One);

                p.ReadTimeout = 50;
                p.WriteTimeout = 50;
                p.Encoding = Encoding.ASCII;

                try
                {
                    p.Open();

                    p.Write("R");

                    try
                    {
                        string str = p.ReadLine();

                        if (str == "VirtualBicycle")
                        {
                            return p;
                        }
                    }
                    catch (TimeoutException) { }

                    p.Close();
                }
                catch { }

            }
            return null;
        }
        public override void Update(float dt)
        {
            throw new NotImplementedException();
        }

        #region IDisposable 成员

        public void Dispose()
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
