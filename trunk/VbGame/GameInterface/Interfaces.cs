
using System;
using Microsoft.Xna.Framework;
using System.Runtime.InteropServices;
using Microsoft.Xna.Framework.Graphics;
namespace RacingGame
{
    /// <summary>
    ///  负责记录当前偏好的接口
    /// </summary>
    public class InterfaceFactory
    {
        static InterfaceFactory singleton;

        public static InterfaceFactory Instance
        {
            get
            {
                if (singleton == null)
                {
                    singleton = new InterfaceFactory();
                }


                return singleton;
            }
        }


        INetInterface currentNetInterface;
        IInputInterface currentInputInterface;

        private InterfaceFactory()
        {
            currentInputInterface = new KeyboardInterface();
            currentNetInterface = new DebugNetInterface();
        }

        /// <summary>
        ///  注册一个网络接口，代替默认接口
        /// </summary>
        /// <param name="i"></param>
        public void RegisterNewNetwork(INetInterface i) { currentNetInterface = i; }
        /// <summary>
        ///  注册一个输入接口，代替默认接口
        /// </summary>
        /// <param name="i"></param>
        public void RegisterNewInput(IInputInterface i) { currentInputInterface = i; }


        /// <summary>
        ///  获得当前的网络接口
        /// </summary>
        /// <returns></returns>
        public INetInterface GetNetwork() { return currentNetInterface; }
        /// <summary>
        ///  获得当前的输入接口
        /// </summary>
        /// <returns></returns>
        public IInputInterface GetInput() { return currentInputInterface; }
    }

    public struct StartUpParameters
    {
        public struct PlayerInfo
        {
            public string ID;
            public string Name;
            public string CarID;
            public Color CarColor;

        }
        public string TeamName;
        public string MapName;
        public PlayerInfo[] Players;
    }
    public unsafe struct BikeState
    {
        public string ID;

        public fixed float Transform[16];
    }


    /// <summary>
    ///  定义游戏与联网通信模块的接口
    /// </summary>
    public interface INetInterface
    {
        /// <summary>
        ///  连接服务器
        /// </summary>
        void Connect();

        /// <summary>
        ///  从服务器获得当前进行游戏的信息。
        ///  此方法会阻断程序执行，直到得到信息。
        /// </summary>
        /// <returns></returns>
        StartUpParameters DownloadStartUpParameters();

        /// <summary>
        ///  向服务器发送若干个玩家车的状态
        /// </summary>
        /// <param name="state"></param>
        void SendBikeState(BikeState[] state);
        /// <summary>
        ///  从服务器获取若干个玩家车的状态
        /// </summary>
        /// <returns></returns>
        BikeState[] DownloadBikeState();

        /// <summary>
        ///  告诉服务器已经准备就绪，可以立即开始游戏
        /// </summary>
        void TellReady();

        /// <summary>
        ///  获得一个Bool，表示现在是否可以开始游戏
        /// </summary>
        /// <returns></returns>
        bool CanStartGame();

        /// <summary>
        ///  断开服务器连接
        /// </summary>
        void Disconnect();
    }

    #region 输入事件
    public class HandlebarRotatedEventArgs : EventArgs
    {
        public HandlebarRotatedEventArgs(float angle)
        {
            this.Angle = angle;
        }
        public float Angle
        {
            get;
            private set;
        }

    }
    public class WheelSpeedChangedEventArgs : EventArgs
    {
        public WheelSpeedChangedEventArgs(float speed)
        {
            this.Speed = speed;
        }

        public float Speed
        {
            get;
            set;
        }

   
    }
    public delegate void HandlebarRotatedHandler(HandlebarRotatedEventArgs e);
    public delegate void WheelSpeedChangedHandler(WheelSpeedChangedEventArgs e);
    #endregion


    /// <summary>
    ///  定义游戏与输入设备的接口
    /// </summary>
    public interface IInputInterface
    {
        /// <summary>
        ///  连接设备
        /// </summary>
        void Connect();

        /// <summary>
        ///  断开对设备的连接
        /// </summary>
        void Disconnect();

        /// <summary>
        ///  设置力反馈大小
        /// </summary>
        /// <param name="f"></param>
        void ForceFeedBack(float f);

        /// <summary>
        ///  当按下“重置”按钮时引发
        /// </summary>
        event EventHandler Reset;

        /// <summary>
        ///  当按下“视角”按钮时引发
        /// </summary>
        event EventHandler ViewChanged;
        /// <summary>
        ///  当按下“确定”按钮时引发
        /// </summary>
        event EventHandler Enter;
        /// <summary>
        ///  当按下“退出”按钮时引发
        /// </summary>
        event EventHandler Escape;

        /// <summary>
        ///  当车把方向发生变化时引发
        /// </summary>
        event HandlebarRotatedHandler HandlebarRotated;

        /// <summary>
        ///  当车轮速度发生变化时引发
        /// </summary>
        event WheelSpeedChangedHandler WheelSpeedChanged;

        /// <summary>
        ///  当心率脉冲到达时引发
        /// </summary>
        event EventHandler HeartPulse;

        /// <summary>
        ///  游戏每一帧会调用
        ///  所有事件要在这个方法内引发，不能再其他线程引发
        /// </summary>
        /// <param name="time"></param>
        void Update(GameTime time);
    }

    class DebugNetInterface : INetInterface
    {
        bool isReady;
        #region INetInterface 成员

        public void Connect()
        {
        }

        public StartUpParameters DownloadStartUpParameters()
        {
            StartUpParameters startUpParams;
            startUpParams.MapName = "Beginner";
            startUpParams.TeamName = "Test Team";
            startUpParams.Players = new StartUpParameters.PlayerInfo[1];
            startUpParams.Players[0].CarID = "0";
            startUpParams.Players[0].ID = "1";
            startUpParams.Players[0].Name = "Test Player";
            return startUpParams;
        }

        public void SendBikeState(BikeState[] state)
        {

        }

        public BikeState[] DownloadBikeState()
        {
            return new BikeState[0];
        }

        public void TellReady()
        {
            isReady = true;
        }

        public bool CanStartGame()
        {
            return isReady;
        }

        public void Disconnect()
        {
          
        }

        #endregion
    }
    class KeyboardInterface : IInputInterface
    {
        enum VKeys : short
        {
            VK_LBUTTON = 1,
            VK_RBUTTON = 2,
            VK_CANCEL = 3,
            VK_MBUTTON = 4,  //   NOT contiguous with L & RBUTTON  
            VK_XBUTTON1 = 5,
            VK_XBUTTON2 = 6,
            VK_BACK = 8,
            VK_TAB = 9,
            VK_CLEAR = 12,
            VK_RETURN = 13,
            VK_SHIFT = 0x10,
            VK_CONTROL = 17,
            VK_MENU = 18,
            VK_PAUSE = 19,
            VK_CAPITAL = 20,
            VK_KANA = 21,
            VK_HANGUL = 21,
            VK_JUNJA = 23,
            VK_FINAL = 24,
            VK_HANJA = 25,
            VK_KANJI = 25,
            VK_CONVERT = 28,
            VK_NONCONVERT = 29,
            VK_ACCEPT = 30,
            VK_MODECHANGE = 31,
            VK_ESCAPE = 27,
            VK_SPACE = 0x20,
            VK_PRIOR = 33,
            VK_NEXT = 34,
            VK_END = 35,
            VK_HOME = 36,
            VK_LEFT = 37,
            VK_UP = 38,
            VK_RIGHT = 39,
            VK_DOWN = 40,
            VK_SELECT = 41,
            VK_PRINT = 42,
            VK_EXECUTE = 43,
            VK_SNAPSHOT = 44,
            VK_INSERT = 45,
            VK_DELETE = 46,
            VK_HELP = 47,
            VK_A = 65,
            VK_D = 68,
            VK_S = 83,
            VK_W = 87,

            //VK_0 thru VK_9 are the same as ASCII '0' thru '9' ($30 - $39)  
            //VK_A thru VK_Z are the same as ASCII 'A' thru 'Z' ($41 - $5A)  
            VK_LWIN = 91,
            VK_RWIN = 92,
            VK_APPS = 93,
            VK_SLEEP = 95,
            VK_NUMPAD0 = 96,
            VK_NUMPAD1 = 97,
            VK_NUMPAD2 = 98,
            VK_NUMPAD3 = 99,
            VK_NUMPAD4 = 100,
            VK_NUMPAD5 = 101,
            VK_NUMPAD6 = 102,
            VK_NUMPAD7 = 103,
            VK_NUMPAD8 = 104,
            VK_NUMPAD9 = 105,
            VK_MULTIPLY = 106,
            VK_ADD = 107,
            VK_SEPARATOR = 108,
            VK_SUBTRACT = 109,
            VK_DECIMAL = 110,
            VK_DIVIDE = 111,
            VK_F1 = 112,
            VK_F2 = 113,
            VK_F3 = 114,
            VK_F4 = 115,
            VK_F5 = 116,
            VK_F6 = 117,
            VK_F7 = 118,
            VK_F8 = 119,
            VK_F9 = 120,
            VK_F10 = 121,
            VK_F11 = 122,
            VK_F12 = 123,
            VK_F13 = 124,
            VK_F14 = 125,
            VK_F15 = 126,
            VK_F16 = 127,
            VK_F17 = 128,
            VK_F18 = 129,
            VK_F19 = 130,
            VK_F20 = 131,
            VK_F21 = 132,
            VK_F22 = 133,
            VK_F23 = 134,
            VK_F24 = 135,
            VK_NUMLOCK = 144,
            VK_SCROLL = 145,
            // VK_L & VK_R - left and right Alt, Ctrl and Shift virtual keys.
            // Used only as parameters to GetAsyncKeyState() and GetKeyState().
            // No other API or message will distinguish left and right keys in this way.  
            VK_LSHIFT = 160,
            VK_RSHIFT = 161,
            VK_LCONTROL = 162,
            VK_RCONTROL = 163,
            VK_LMENU = 164,
            VK_RMENU = 165,

            VK_BROWSER_BACK = 166,
            VK_BROWSER_FORWARD = 167,
            VK_BROWSER_REFRESH = 168,
            VK_BROWSER_STOP = 169,
            VK_BROWSER_SEARCH = 170,
            VK_BROWSER_FAVORITES = 171,
            VK_BROWSER_HOME = 172,
            VK_VOLUME_MUTE = 173,
            VK_VOLUME_DOWN = 174,
            VK_VOLUME_UP = 175,
            VK_MEDIA_NEXT_TRACK = 176,
            VK_MEDIA_PREV_TRACK = 177,
            VK_MEDIA_STOP = 178,
            VK_MEDIA_PLAY_PAUSE = 179,
            VK_LAUNCH_MAIL = 180,
            VK_LAUNCH_MEDIA_SELECT = 181,
            VK_LAUNCH_APP1 = 182,
            VK_LAUNCH_APP2 = 183,

            VK_OEM_1 = 186,
            VK_OEM_PLUS = 187,
            VK_OEM_COMMA = 188,
            VK_OEM_MINUS = 189,
            VK_OEM_PERIOD = 190,
            VK_OEM_2 = 191,
            VK_OEM_3 = 192,
            VK_OEM_4 = 219,
            VK_OEM_5 = 220,
            VK_OEM_6 = 221,
            VK_OEM_7 = 222,
            VK_OEM_8 = 223,
            VK_OEM_102 = 226,
            VK_PACKET = 231,
            VK_PROCESSKEY = 229,
            VK_ATTN = 246,
            VK_CRSEL = 247,
            VK_EXSEL = 248,
            VK_EREOF = 249,
            VK_PLAY = 250,
            VK_ZOOM = 251,
            VK_NONAME = 252,
            VK_PA1 = 253,
            VK_OEM_CLEAR = 254
        }

        [DllImport("user32")]
        static extern bool GetAsyncKeyState(VKeys vKey);
        [DllImport("user32")]
        static extern bool GetAsyncKeyState(short vKey);

        bool isVPressed;
        bool isEscPressed;
        bool isEnterPressed;

        bool isTPressed;

        float handlebarAngle;
        float wheelSpeed;

        #region IInputInterface 成员

        public void Connect()
        {
        }

        public void Disconnect()
        {
        }

        public void ForceFeedBack(float f)
        {
        }

        public event EventHandler Reset;

        public event EventHandler ViewChanged;

        public event EventHandler Enter;

        public event EventHandler Escape;

        public event HandlebarRotatedHandler HandlebarRotated;

        public event WheelSpeedChangedHandler WheelSpeedChanged;

        public event EventHandler HeartPulse;

        public  void Update(GameTime time)
        {
            float dt = (float)time.ElapsedGameTime.TotalSeconds;
            if (GetAsyncKeyState((short)'R'))
            {
                OnReset();
            }

            if (GetAsyncKeyState(VKeys.VK_A) || GetAsyncKeyState(VKeys.VK_LEFT))
            {
                float na = -MathHelper.PiOver2 * 0.5f;
                if (handlebarAngle != na)
                {
                    OnHandlebarRotated(na);
                    handlebarAngle = na;
                }
            }

            if (GetAsyncKeyState(VKeys.VK_D) || GetAsyncKeyState(VKeys.VK_RIGHT))
            {
                float na = MathHelper.PiOver2 * 0.5f;
                if (handlebarAngle != na)
                {
                    OnHandlebarRotated(na);
                    handlebarAngle = na;
                }
            }

            if (GetAsyncKeyState(VKeys.VK_W) || GetAsyncKeyState(VKeys.VK_UP))
            {
                float ns = wheelSpeed + dt * 45;

                OnWheelSpeedChanged(ns);
                wheelSpeed = ns;
            }
            else if (GetAsyncKeyState(VKeys.VK_S) || GetAsyncKeyState(VKeys.VK_DOWN))
            {
                float ns = wheelSpeed - dt * 45;

                OnWheelSpeedChanged(ns);
                wheelSpeed = ns;
            }
            else
            {
                float ns = wheelSpeed - dt * 5;

                OnWheelSpeedChanged(ns);
                wheelSpeed = ns;
            }

            if (GetAsyncKeyState((short)'T'))
            {
                if (!isTPressed)
                {
                    OnHeartPulse();
                    isTPressed = true;
                }
            }
            else 
            {
                isTPressed = false;
            }

            if (GetAsyncKeyState(VKeys.VK_ESCAPE))
            {
                if (!isEscPressed)
                {
                    OnEscape();
                    isEscPressed = true;
                }
            }
            else
            {
                isEscPressed = false;
            }

            if (GetAsyncKeyState(VKeys.VK_RETURN))
            {
                if (!isEnterPressed)
                {
                    OnEnter();
                    isEnterPressed = true;
                }
            }
            else 
            {
                isEnterPressed = false;
            }

            if (GetAsyncKeyState((short)'V'))
            {
                if (!isVPressed)
                {
                    OnViewChanged();
                    isVPressed = true;
                }
            }
            else 
            {
                isVPressed = false;
            }
        }


        void OnReset()
        {
            if (Reset != null)
            {
                Reset(this, EventArgs.Empty);
            }
        }
       
        void OnWheelSpeedChanged(float speed)
        {
            if (WheelSpeedChanged != null)
            {
                WheelSpeedChanged(new WheelSpeedChangedEventArgs(speed));
            }
        }

        void OnViewChanged()
        {
            if (ViewChanged != null)
            {
                ViewChanged(this, EventArgs.Empty);
            }
        }
        void OnEnter()
        {
            if (Enter != null)
            {
                Enter(this, EventArgs.Empty);
            }
        }
        void OnEscape()
        {
            if (Escape != null)
            {
                Escape(this, EventArgs.Empty);
            }
        }
        void OnHandlebarRotated(float angle)
        {
            if (HandlebarRotated != null)
            {
                HandlebarRotated(new HandlebarRotatedEventArgs(angle));
            }
        }
        void OnHeartPulse()
        {
            if (HeartPulse != null)
            {
                HeartPulse(this, EventArgs.Empty);
            }
        }
     

        

        #endregion
    }
}
