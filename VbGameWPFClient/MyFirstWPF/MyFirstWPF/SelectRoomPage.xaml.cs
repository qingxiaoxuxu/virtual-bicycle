﻿using System;
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
using System.Threading;
using VbClient.Net;
using System.Windows.Threading;

namespace MyFirstWPF
{
    /// <summary>
    /// SelectRoomPage.xaml 的交互逻辑
    /// </summary>
    public partial class SelectRoomPage : UserControl, IKeyDown, IReload, IMove
    {
		int blockState;					//屏幕上第一个roomBlock在全部roomBlock中的顺序
		int flickerState;				//橙色闪烁标记所处的位置
        int roomCount;                  //总房间数
        public const int MaxPeople = 4;       //每个房间的最大人数
        const int Left = 100;
        const int Top = 190;
        const int Right = 682;
        const int Bottom = 492;			//左上角RoomBlock的四个margin值
        const int HorMoveUnit = 580;
        const int VerMoveUnit = 120;
        int isEntered;                  //判断是否成功进入房间
        RoomInfoBlock[] rooms = null;   //房间信息
        Storyboard blockMoveStory;
        Storyboard flickerMoveStory;
        EventWaitHandle addRoomEvent = new EventWaitHandle(true, EventResetMode.ManualReset);
        //控制进入房间的逻辑
        private ClientEvt client;
        DispatcherTimer refreshTimer;

        delegate void Fun(List<string> team, List<string> map, List<int> counts, List<int> maxCount);
        Fun showRoom;

        public SelectRoomPage()
        {
            InitializeComponent();
            this.KeyDown += new KeyEventHandler(KeyboardDown);
            roomCount = 0;
            flickerCanvas.Visibility = Visibility.Hidden;
            showRoom = new Fun(this.getRoomInfo);
            refreshTimer = new DispatcherTimer();
            refreshTimer.Interval = TimeSpan.FromSeconds(5);
            refreshTimer.Tick += new EventHandler(refreshTimer_Tick);
        }

        //每次刷新时向Server端发出查询房间列表请求
        void refreshTimer_Tick(object sender, EventArgs e)
        {
            isEntered = 0;
            flickerState = 0;
            client.GetTeamList();
        }

        #region client系列事件响应
        //生成房间信息
        void client_GotTeamMapList(object sender, List<string> team, 
            List<string> map, List<int> counts, List<int> maxCount)
        {
            object[] para = new object[4];
            para[0] = team;
            para[1] = map;
            para[2] = counts;
            para[3] = maxCount;
            this.Dispatcher.Invoke(showRoom, para);         //调用getRoomInfo方法
        }

        //进入房间成功
        void client_AddSuccess(object sender, string map)
        {
            isEntered = 1;
            addRoomEvent.Set();
        }

        //进入房间失败
        void client_AddFailure(object sender, EventArgs e)
        {
            isEntered = -1;
            addRoomEvent.Set();
        }
        #endregion

        #region 房间生成及摆放
        
        //从Server获取房间信息
        public void getRoomInfo(List<string> team, List<string> map, List<int> counts, List<int> maxCount)   
        {
            if (rooms != null)              //控件注销
                for (int i = 0; i < rooms.Length; i++)
                {
                    LayoutRoot.Children.Remove(rooms[i]);
                    UnregisterName(rooms[i].Name);
                    rooms[i] = null;
                }
            roomCount = team.Count;
            rooms = new RoomInfoBlock[roomCount];
            for (int i = 0; i < roomCount; i++)
            {
                rooms[i] = new RoomInfoBlock(team[i], map[i], counts[i], maxCount[i]);
                rooms[i].Name = "room" + i.ToString();
                RegisterName(rooms[i].Name, rooms[i]);
            }
            if (roomCount != 0)
            {
                flickerCanvas.Visibility = Visibility.Visible;
                initPosition();
            }
            else
            {
                (this.Resources["ArrowFlicker"] as Storyboard).Stop(this);
                flickerCanvas.Visibility = Visibility.Hidden;
                flickerState = blockState = -1;
            }
        }

        //房间位置初始化
        private void initPosition()
        {
            int i, row_offset, col_offset;

            if (flickerMoveStory != null)
                flickerMoveStory.Remove(this);
            if (blockMoveStory != null)
                blockMoveStory.Remove(this);    //释放被动画占用的依赖属性

            upArrow.Visibility = downArrow.Visibility = Visibility.Hidden;
            flickerState = blockState = 0;      //初始定位
            for (i = 0; i < roomCount; i++)
            {
                row_offset = i / 2 - blockState / 2;
                col_offset = i % 2 - blockState % 2;
                LayoutRoot.Children.Add(rooms[i]);
                Thickness pos = new Thickness(
                    Left + HorMoveUnit * col_offset,
                    Top + VerMoveUnit * row_offset,
                    Right - HorMoveUnit * col_offset,
                    Bottom - VerMoveUnit * row_offset);
                rooms[i].Margin = pos;
                rooms[i].Height = 100;
                rooms[i].Width = 500;
                rooms[i].Opacity = ((row_offset >= 0 && row_offset <= 3) ? 0.9 : 0.0);
            }
            flickerCanvas.Margin = new Thickness(
                Left - 10,
                Top,
                Right - 10,
                Bottom
            );
            if (roomCount > 8)
            {
                (this.Resources["ArrowFlicker"] as Storyboard).Begin(this);
                upArrow.Visibility = Visibility.Hidden;
                downArrow.Visibility = Visibility.Visible;
            }
        }

        #endregion

        #region IReload 成员

        public void Reload()
        {
            #region 关联client
            client = InfoControl.Client;
            client.GotTeamMapList += client_GotTeamMapList;
            client.AddSuccess += client_AddSuccess;
            client.AddFailure += client_AddFailure;
            #endregion
            refreshTimer.Start();
            isEntered = 0;
            flickerState = 0;
            client.GetTeamList();
        }

        public void Leave()
        {
            refreshTimer.Stop();
            #region 注销client
            client.GotTeamMapList -= client_GotTeamMapList;
            client.AddSuccess -= client_AddSuccess;
            client.AddFailure -= client_AddFailure;
            client = null;
            #endregion
        }

        #endregion

        #region IKeyDown 成员

        public void KeyboardDown(object sender, KeyEventArgs e)         //重写键盘按下事件响应函数
        {
            if (flickerCanvas.Visibility == Visibility.Hidden)
                return;                                                 //没有任何房间，不进行处理
            if (e.Key == Key.Up)
                moveUp();
            else if (e.Key == Key.Down)
                moveDown();
            else if (e.Key == Key.Left)
                moveLeft();
            else if (e.Key == Key.Right)
                moveRight();
        }

        public int Choose()
        {
            if (flickerCanvas.Visibility == Visibility.Visible)
            {
                client.AddTeam(rooms[blockState + flickerState].GetRoomName());
                addRoomEvent.Reset();
                if (addRoomEvent.WaitOne())
                {
                    if (isEntered == 1)
                    {
                        (this.Resources["ArrowFlicker"] as Storyboard).Stop(this);
                        return MainFrame.INDEX_WAITING_ROOM_PAGE;
                    }
                    else if (isEntered == -1)
                    {
                        MessageBox.Show("进入房间失败！");
                        client.GetTeamList();       //重新获取房间信息
                        return -1;
                    }
                }
            }
            return -1;
        }

        public int MoveBack()
        {
            return MainFrame.INDEX_MAIN_PAGE;
        }

        public int BtnFunction1()               //按下第一个功能键，跳转到联网地图选择
        {
            return MainFrame.INDEX_MULTI_SELECT_MAP_PAGE;
        }

        #endregion

        #region IMove 成员
        //向左移动
        public void moveLeft()
        {
            if (flickerState % 2 == 1)      //奇数，在右边
            {
                flickerState--;
                moveFlicker();
            }
        }

        //向右移动
        public void moveRight()
        {
            if (flickerState % 2 == 0)      //偶数，在左边
            {
                if (blockState + flickerState + 1 < roomCount)      //有房间
                {
                    flickerState++;
                    moveFlicker();
                }
            }
        }

        //向上移动
        public void moveUp()
        {
            if (flickerState / 2 > 0)              //上面有空间，只需要移动flicker
            {
                flickerState -= 2;
                moveFlicker();
            }
            else if (blockState / 2 > 0)           //屏幕上方还有房间，block可以向下移动
            {
                blockState -= 2;
                moveBlock();
            }
            else                                    //动不了了
            {
                        
            }
            if (blockState < 2)
                upArrow.Visibility = Visibility.Hidden;
            if (blockState + 8 < roomCount)
                downArrow.Visibility = Visibility.Visible;
        }

        //向下移动
        public void moveDown()
        {
            if (flickerState / 2 < 3)                                    //下面有空间，但并不确定是否有一个房间摆在那里（可能是奇数房间，下面是空的）
            {
                if (blockState + flickerState + 2 < roomCount)           //flicker下面有房间，只需要移动flicker
                { 
                    flickerState += 2;
                    moveFlicker();
                }
                else if (blockState + flickerState + 1 < roomCount
                    && flickerState % 2 == 1)                            //flicker必须左移
                {
                    flickerState++;
                    moveFlicker();
                }
            }
            else if (blockState + 8 < roomCount)                         //屏幕下方还有房间，block可以向上移动
            {
                blockState += 2;
                moveBlock();
                if (blockState + flickerState >= roomCount)              //flicker必须左移
                {
                    flickerState--;
                    moveFlicker();
                }
            }
            else                                                          //动不了了
            {

            }
            if (blockState >= 2)
                upArrow.Visibility = Visibility.Visible;
            if (blockState + 8 >= roomCount)
                downArrow.Visibility = Visibility.Hidden;
        }
        #endregion

        #region 故事版
        //生成选择Flicker移动的故事版
        private Storyboard generateFlickerMoveStoryboard()
        {
            Storyboard flickerStory = new Storyboard();
            int row_offset = flickerState / 2;
            int col_offset = flickerState % 2;
            ThicknessAnimation posAnimation = new ThicknessAnimation();
            posAnimation.To = new Thickness(
                    Left - 10 + HorMoveUnit * col_offset,
                    Top + VerMoveUnit * row_offset,
                    Right - 10 - HorMoveUnit * col_offset,
                    Bottom - VerMoveUnit * row_offset);
            posAnimation.Duration = new Duration(TimeSpan.FromSeconds(0.3));
            Storyboard.SetTargetName(posAnimation, flickerCanvas.Name);
            Storyboard.SetTargetProperty(posAnimation, new PropertyPath(Canvas.MarginProperty));
            flickerStory.Children.Add(posAnimation);
            return flickerStory;
        }

        //生成block移动的故事板
        private Storyboard generateBlockMoveStoryboard()  
        {
            int i, row_offset, col_offset;
            Storyboard moveStory = new Storyboard();
            ThicknessAnimation[] posAnimation = new ThicknessAnimation[roomCount];
            DoubleAnimation[] opacAnimation = new DoubleAnimation[roomCount];

            for (i = 0; i < roomCount; i++)
            {
                row_offset = i / 2 - blockState / 2;
                col_offset = i % 2 - blockState % 2;

                Thickness pos = new Thickness(
                    Left + HorMoveUnit * col_offset,
                    Top + VerMoveUnit * row_offset,
                    Right - HorMoveUnit * col_offset,
                    Bottom - VerMoveUnit * row_offset);
                posAnimation[i] = new ThicknessAnimation();
                posAnimation[i].To = pos;
                posAnimation[i].Duration = new Duration(TimeSpan.FromSeconds(0.2));
                Storyboard.SetTargetName(posAnimation[i], rooms[i].Name);
                Storyboard.SetTargetProperty(posAnimation[i], new PropertyPath(RoomInfoBlock.MarginProperty));
                moveStory.Children.Add(posAnimation[i]);

                opacAnimation[i] = new DoubleAnimation();
                opacAnimation[i].To = ((row_offset >= 0 && row_offset <= 3) ? 0.9 : 0.0);
                opacAnimation[i].Duration = new Duration(TimeSpan.FromSeconds(0.1));
                Storyboard.SetTargetName(opacAnimation[i], rooms[i].Name);
                Storyboard.SetTargetProperty(opacAnimation[i], new PropertyPath(RoomInfoBlock.OpacityProperty));
                moveStory.Children.Add(opacAnimation[i]);
            }
            return moveStory;
        }

        //移动flickerCanvas
        private void moveFlicker()
        {
            flickerMoveStory = generateFlickerMoveStoryboard();
            flickerMoveStory.Begin(this, HandoffBehavior.SnapshotAndReplace, true);
        }

        //移动roomBlock
        private void moveBlock()
        {
            blockMoveStory = generateBlockMoveStoryboard();
            blockMoveStory.Begin(this, HandoffBehavior.SnapshotAndReplace, true);
        }

        private void arrowFlicker()
        { 
        }
        #endregion
    }
}
