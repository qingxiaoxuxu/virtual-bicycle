using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BuzzWin
{
   public class DeviceDataManager
    {

        public BuzzHandsetDevice m_oBuzzDevice = null;
        public Byte[] readbuffer;

        public Byte[] CommandBuffer = new Byte[] { 0xf5, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };

        public UInt16 p_vid = 0x8888, p_pid = 0x0006;

        private const byte BEGIN_SETTING = 0x00;
        private const byte LEFT_POINT = 0x01;
        private const byte CENTER_POINT = 0x02;
        private const byte RIGHT_POINT = 0x03;
        private byte Step = 0x00;

        #region 事件
        public delegate void F0(DeviceData deviceData);
        public event F0 GetDeviceData;

        public delegate void F1(DeviceStatusData deviceStatusData);
        public event F1 GetDeviceStatusData;

        public delegate void F2(SportStatus sportStatus);
        public event F2 GetSportStatus;

        public delegate void F3(LossPackRate lossPackRate);
        public event F3 GetLossPackRate;

        public delegate void F7(RFID rfid);
        public event F7 GetRFID;

        public delegate void F8(GameControl gameControl);
        public event F8 GetGameControl;

        #endregion


        //设备数据结构体
        public struct DeviceData
        {
            public UInt32 DeviceID;    //设备ID
            public UInt16 DeviceClass;  //设备类型
            public Byte DeviceSubClass;  //设备子类
        }


        //设备状态数据结构体
        public struct DeviceStatusData
        {
            public Byte DeviceSta;   //设备状态
        }


        //运动状态数据结构体
        public struct SportStatus
        {
            public Byte Speed;       //速度
            public UInt32 distance;     //路程
            public Byte HeartRate;    //心率
            public Byte load;      //阻力

        }


        //灵敏度调节数据结构体
        public struct Config
        {
            public Byte SpeedRate;      //速度灵敏度
            public Byte DirectionRate;   //方向灵敏度
            public Byte load;           //阻力
        }


        //丢包率测试数据包结构体
        public struct LossPackRate
        {
            public UInt32 PackSum;
            public UInt32 LossPackSum;
            public float GetLossPackRate()
            {
                float i;
                i = 100 * (float)LossPackSum / PackSum;
                return i;
            }
        }

        //
        public struct RFID
        {
            public int id;
        }

        //
        public struct GameControl
        {
            public bool Btn1;
            public bool Btn2;
            public bool Btn3;
            public bool Btn4;
            public int X;
            public int Y;
        }

        //
        public struct Damp
        {
            public int value;
        }
        
        public DeviceData MyDevice;
        public DeviceStatusData MyDeviceStatusData;
        public SportStatus MySportStatus;
        public Config MyConfig;
        public LossPackRate MyLossPackRate;
        public RFID MyRFID;
        public GameControl MyGameControl;


        public bool OpenDevice(ref BuzzHandsetDevice oDevice, int nVid, int nPid)
        {
            oDevice = BuzzHandsetDevice.FindBuzzHandset(nVid, nPid);
            if (oDevice != null)	// did we find it?
            {
                // Yes! So wire into its events and update the UI
                oDevice.OnReadBuffer += new BuzzReadBufferEventHandler(OnGetReadBuffer);
                oDevice.OnDeviceRemoved += new EventHandler(oDevice_OnDeviceRemoved);
                return true;
            }
            else return false;
            

        }
        /// <summary>
        /// 收到数据后进行数据处理
        /// </summary>
        /// <param name="sender "></param>
        /// <param name="buffer 读取到的数据"></param>
        private void OnGetReadBuffer(object sender, Byte[] buffer)
        {
            uint i = 0;

            readbuffer = new Byte[m_oBuzzDevice.InputReportLength];

            for (i = 0; i < buffer.Length; i++)
            {
                readbuffer[i] = buffer[i];
            }

            ProcessData(readbuffer);

            //Console.WriteLine("OnGetReadBuffer");

        }
        void oDevice_OnDeviceRemoved(object sender, EventArgs e)
        {
            Console.WriteLine("Device removed !!");
        }
        private void ProcessData(Byte[] buffer)
        {

            switch (buffer[1])
            {
                case 0xf0:  //收到设备的ID数据包
                    {
                        MyDevice.DeviceID = 0;
                        MyDevice.DeviceID += buffer[2];
                        MyDevice.DeviceID += (UInt32)buffer[3] << 8;
                        MyDevice.DeviceID += (UInt32)buffer[4] << 16;
                        MyDevice.DeviceID += (UInt32)buffer[5] << 24;

                        MyDevice.DeviceClass = 0;
                        MyDevice.DeviceClass += buffer[6];
                        MyDevice.DeviceClass += (UInt16)(buffer[7] * 256);

                        MyDevice.DeviceSubClass = 0;
                        MyDevice.DeviceSubClass += buffer[8];

                        if (GetDeviceData != null)
                            GetDeviceData(MyDevice);
                        break;
                    }
                case 0xf1:  //收到设备信息数据包
                    {
                        MyDeviceStatusData.DeviceSta = buffer[2];

                        if (GetDeviceStatusData != null)
                            GetDeviceStatusData(MyDeviceStatusData);
                        break;
                    }
                case 0xf2:   //收到运动状态数据包
                    {
                        MySportStatus.Speed = buffer[2];
                        MySportStatus.distance = 0;
                        MySportStatus.distance += buffer[3];
                        MySportStatus.distance += (UInt32)buffer[4] << 8;
                        MySportStatus.distance += (UInt32)buffer[5] << 16;
                        MySportStatus.distance += (UInt32)buffer[6] << 24;

                        MySportStatus.HeartRate = buffer[7];

                        MySportStatus.load = buffer[8];
                        if (GetSportStatus != null)
                            GetSportStatus(MySportStatus);
                        break;
                    }
                case 0xf3:   //收到丢包率测试数据包
                    {
                        MyLossPackRate.PackSum += buffer[2];
                        MyLossPackRate.PackSum += (UInt32)(buffer[3] << 8);

                        MyLossPackRate.LossPackSum += buffer[4];
                        MyLossPackRate.LossPackSum += (UInt32)(buffer[5] << 8);
                        if (GetLossPackRate != null)
                            GetLossPackRate(MyLossPackRate);
                        break;
                    }
                case 0xf7:  //RFID卡号
                    {

                        if (GetRFID != null)
                            GetRFID(MyRFID);
                        break;
                    }
                case 0xf8:  //游戏控制
                    {
                        if ((buffer[2] & 1) == 1) MyGameControl.Btn1 = true;
                        else MyGameControl.Btn1 = false;
                        if (((buffer[2]>>1) & 1) == 1) MyGameControl.Btn2 = true;
                        else MyGameControl.Btn2 = false;
                        if (((buffer[2] >> 2) & 1) == 1) MyGameControl.Btn3 = true;
                        else MyGameControl.Btn3 = false;
                        if (((buffer[2] >> 3) & 1) == 1) MyGameControl.Btn4 = true;
                        else MyGameControl.Btn4 = false;
                        MyGameControl.X = buffer[4];
                        MyGameControl.Y = buffer[5];
                        if (GetGameControl != null)
                            GetGameControl(MyGameControl);
                        break;
                    }
                default:
                    {

                        break;
                    }


            }
            //Console.WriteLine("processData"+buffer.ToString());
            
        }

        public void SetDamp(Damp damp)
        {
            Byte[] DampData = new Byte[8] { 0, 0, 0, 0, 0, 0, 0, 0 };
            DampData[0] = 0xf9;
            DampData[1] = (byte)damp.value;
            WriteData(ref m_oBuzzDevice, DampData);
        }
        public void SetBeginConfigAngle()
        {
            Byte[] Data = new Byte[8] { 0, 0, 0, 0, 0, 0, 0, 0 };
            Data[0] = 0xf6;
            WriteData(ref m_oBuzzDevice, Data);
        }
        public void SetLeftAngle()
        {
            Byte[] Data = new Byte[8] { 0, 0, 0, 0, 0, 0, 0, 0 };
            Data[0] = 0xf6;
            Data[1] = 0x01;
            WriteData(ref m_oBuzzDevice, Data);
        }
        public void SetRightAngle()
        {
            Byte[] Data = new Byte[8] { 0, 0, 0, 0, 0, 0, 0, 0 };
            Data[0] = 0xf6;
            Data[1] = 0x03;
            WriteData(ref m_oBuzzDevice, Data);
        }
        public void SetMiddleAngle()
        {
            Byte[] Data = new Byte[8] { 0, 0, 0, 0, 0, 0, 0, 0 };
            Data[0] = 0xf6;
            Data[1] = 0x02;
            WriteData(ref m_oBuzzDevice, Data);
        }
        public void SendCommand()
        {
            Byte[] Data = new Byte[8] { 0, 0, 0, 0, 0, 0, 0, 0 };
            Data[0] = 0xf5;
            WriteData(ref m_oBuzzDevice, Data);
        }
        private void SendConfigData()
        {
            Byte[] ConfigData = new Byte[8] { 0, 0, 0, 0, 0, 0, 0, 0 };

            ConfigData[0] = 0xf4;
            //ConfigData[0] = MyConfig.load;

            ConfigData[1] = MyConfig.SpeedRate;
            ConfigData[2] = MyConfig.DirectionRate;
            ConfigData[3] = MyConfig.load;

            WriteData(ref m_oBuzzDevice, ConfigData);

        }
        public void WriteData(ref BuzzHandsetDevice oDevice, Byte[] writebuffer)
        {
            if (oDevice != null)	// did we find it?
            {
                if (writebuffer.Length >= oDevice.OutputReportLength - 1)
                {

                    oDevice.WriteReport(writebuffer);
                }
                else
                {
                    Console.WriteLine(String.Format("请输入 {0} 个数据", oDevice.OutputReportLength - 1));
                }
            }
            else
            {
                Console.WriteLine(String.Format("设备未找到！"));

            } 


        }
    }
}
