using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using VbClient.Net;
using System.Threading;

namespace MyFirstWPF
{
    /// <summary>
    /// SelectMap.xaml 的交互逻辑
    /// </summary>
    public partial class SelectMapPage : UserControl, IKeyDown, IReload, IMove
    {
        public const int MODE_SINGLE = 0;
        public const int MODE_MULTI = 1;
        int mapCnt;                                     //地图数量
        int mode;                                       //单人模式（0） 或  联网模式（1）
        int state;										//哪个地图被选中
        int peopleState;                                //选择人数的状态
        int flickerState;                               //箭头的位置
        const double Left = 512;                        //初始Margin.Left
        const double Top = 260;                         //初始Margin.Top
        const double Right = 514;                       //初始Margin.Right
        const double Bottom = 286;                      //初始Margin.Bottom   (这些是正中间图标的坐标)
        const int MoveUnit = 256;                       //图标移动尺寸
        const int ShrinkUnit = 80;                      //图标缩小尺寸
        const int FlickerTop = 330;                     //箭头初始Margin.Top
        const int FlickerMoveUnit = 240;                //箭头上下移动尺寸
        Storyboard moveStory;
        Canvas[] iconCanvas = new Canvas[InfoControl.MapCount];
        string[] msk = new string[] { "Two", "Three", "Four" }; //Storyboard对应名称
        EventWaitHandle createRoomEvent = new EventWaitHandle(true, EventResetMode.ManualReset);
        //控制创建房间的逻辑
        int isCreated;
        private ClientEvt client;

        public SelectMapPage(int md)
        {
            InitializeComponent();
            mapCnt = InfoControl.MapCount;
            mode = md;
            state = 0;									//初始地图是第一幅
            flickerState = 0;                           //flicker初始状态为修改地图
            peopleState = 2;                            //设定人数状态为2（四人）
            iconCanvas[0] = singleCanvas;
            iconCanvas[1] = createGameCanvas;
            iconCanvas[2] = joinGameCanvas;
            initPosition();             				//按钮位置初始化
            mapText.Text = InfoControl.MapTexts[0];
            if (md == MODE_MULTI)
            {
                client = InfoControl.Client;
                client.CreateSuccess += new EventHandler(client_CreateSuccess);
                client.CreateFailure += new EventHandler(client_CreateFailure);
                string str = "Large" + msk[peopleState] + "Story";
                (this.Resources[str] as Storyboard).Begin(this);
                StartText.Text = "建立房间";
            }
            else
            {
                LeftArrow.Visibility = Visibility.Hidden;
                RightArrow.Visibility = Visibility.Hidden;
                iconText2.Visibility = Visibility.Hidden;
                peopleTwo.Visibility = Visibility.Hidden;
                peopleThree.Visibility = Visibility.Hidden;
                peopleFour.Visibility = Visibility.Hidden;
                StartText.Text = "开始游戏！";
            }
        }

        #region client系列事件响应
        //创建房间成功的响应
        void client_CreateSuccess(object sender, EventArgs e)
        {
            isCreated = 1;
            createRoomEvent.Set();
        }

        //创建房间失败的响应
        void client_CreateFailure(object sender, EventArgs e)
        {
            isCreated = -1;
            createRoomEvent.Set();
        }
        #endregion

        #region 界面摆放
        //图标位置初始化
        private void initPosition()
        {
            int i, offset, off_abs;
            for (i = 0; i < mapCnt; i++)
            {
                offset = i - state;                                             //第i个按钮的偏移量（带符号）
                off_abs = Math.Abs(offset);                                     //第i个按钮的偏移量（绝对值）
                iconCanvas[i].Margin =
                    new Thickness(Left + MoveUnit * offset + (ShrinkUnit / 2) * off_abs, Top + (ShrinkUnit / 2) * off_abs,
                        Right - MoveUnit * offset + (ShrinkUnit / 2) * off_abs, Bottom + (ShrinkUnit / 2) * off_abs);
                iconCanvas[i].Opacity = 1.0 - 0.5 * off_abs;
            }
        }
        #endregion

        #region IReload 成员

        public void Reload()
        {
            isCreated = 0;
            flickerState = 0;
            LeftArrow.Margin = new Thickness(
                   LeftArrow.Margin.Left,
                   FlickerTop,
                   0, 0);
            RightArrow.Margin = new Thickness(
                0, FlickerTop,
                RightArrow.Margin.Right, 0);
        }

        #endregion

        #region IKeyDown 成员

        public void KeyboardDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Left)
                moveLeft();
            else if (e.Key == Key.Right)
                moveRight();
            else if (e.Key == Key.Up)
                moveUp();
            else if (e.Key == Key.Down)
                moveDown();
        }

        public int Choose()
        {
            if (mode == 0)                      //单机游戏
            {
                //TODO:Game start
                MessageBox.Show("Game start!");
                return -1;
            }
            else                                //联网游戏
            {
                client.CreateTeam(InfoControl.UserName + "的房间", InfoControl.MapTexts[state]);
                createRoomEvent.Reset();
                if (createRoomEvent.WaitOne())
                {
                    if (isCreated == 1)
                        return MainFrame.INDEX_WAITING_ROOM_PAGE;
                    else if (isCreated == -1)
                    {
                        MessageBox.Show("创建房间失败！");
                        return -1;
                    }
                }
            }
            return -1;
        }

        public int MoveBack()
        {
            if (mode == 0)
                return MainFrame.INDEX_MAIN_PAGE;
            else
                return MainFrame.INDEX_MULTI_SELECT_ROOM_PAGE;
        }

        public int BtnFunction1()
        {
            return -1;
        }

        #endregion

        #region IMove 成员
        //向上移动(仅多人游戏模式有效)
        public void moveUp()
        {
            if (flickerState == 1 && mode == MODE_MULTI)
            {
                flickerState--;
                LeftArrow.Margin = new Thickness(
                    LeftArrow.Margin.Left,
                    FlickerTop,
                    0, 0);
                RightArrow.Margin = new Thickness(
                    0, FlickerTop,
                    RightArrow.Margin.Right, 0);
            }
        }

        //向下移动(仅多人模式有效)
        public void moveDown()
        {
            if (flickerState == 0 && mode == MODE_MULTI)
            {
                flickerState++;
                LeftArrow.Margin = new Thickness(
                    LeftArrow.Margin.Left, 
                    FlickerTop + FlickerMoveUnit, 
                    0, 0);
                RightArrow.Margin = new Thickness(
                    0, FlickerTop + FlickerMoveUnit,
                    RightArrow.Margin.Right, 0);
            }
        }

        //向左移动
        public void moveLeft()
        {
            if (flickerState == 0 && state > 0)
            {
                state--;
                mapText.Text = InfoControl.MapTexts[state];
                moveStory = generateMoveStoryboard();
                moveStory.Begin(this);
            }
            else if (flickerState == 1 && peopleState > 0)
            {
                peopleState--;
                MovePeople(peopleState, peopleState + 1);
            }
        }

        //向右移动
        public void moveRight()
        {
            if (flickerState == 0 && state < mapCnt - 1)
            {
                state++;
                mapText.Text = InfoControl.MapTexts[state];
                moveStory = generateMoveStoryboard();
                moveStory.Begin(this);
            }
            else if (flickerState == 1 && peopleState < 2)
            {
                peopleState++;
                MovePeople(peopleState, peopleState - 1);
            }
        }
        #endregion

        #region 故事版
        //下方显示人数的TextBlock的移动
        private void MovePeople(int state, int lastState)
        {
            string large, small;
            large = "Large" + msk[state] + "Story";
            small = "Small" + msk[lastState] + "Story";
            (this.Resources[large] as Storyboard).Begin(this);
            (this.Resources[small] as Storyboard).Begin(this);
        }

        //生成Storyboard
        private Storyboard generateMoveStoryboard()
        {
            int i;
            Storyboard moveStory = new Storyboard();
            ThicknessAnimation[] posAnimation = new ThicknessAnimation[mapCnt];
            DoubleAnimation[] opacAnimation = new DoubleAnimation[mapCnt];
            for (i = 0; i < mapCnt; i++)
            {
                int offset = i - state;                                             //第i个按钮的偏移量（带符号）
                int off_abs = Math.Abs(offset);                                     //第i个按钮的偏移量（绝对值）
                posAnimation[i] = new ThicknessAnimation();                         //控制图标位置
                posAnimation[i].To =
                    new Thickness(Left + MoveUnit * offset + (ShrinkUnit / 2) * off_abs, Top + (ShrinkUnit / 2) * off_abs,
                        Right - MoveUnit * offset + (ShrinkUnit / 2) * off_abs, Bottom + (ShrinkUnit / 2) * off_abs);
                posAnimation[i].Duration = new Duration(TimeSpan.FromSeconds(0.3));
                Storyboard.SetTargetName(posAnimation[i], iconCanvas[i].Name);
                Storyboard.SetTargetProperty(posAnimation[i], new PropertyPath(Canvas.MarginProperty));
                moveStory.Children.Add(posAnimation[i]);

                opacAnimation[i] = new DoubleAnimation();                           //控制图标透明度
                opacAnimation[i].To = 1.0 - 0.5 * off_abs;
                opacAnimation[i].Duration = new Duration(TimeSpan.FromSeconds(0.3));
                Storyboard.SetTargetName(opacAnimation[i], iconCanvas[i].Name);
                Storyboard.SetTargetProperty(opacAnimation[i], new PropertyPath(Canvas.OpacityProperty));
                moveStory.Children.Add(opacAnimation[i]);
            }
            return moveStory;
        }
        #endregion
    }
}
