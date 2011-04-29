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

namespace MyFirstWPF
{
    /// <summary>
    /// SelectRoomPage.xaml 的交互逻辑
    /// </summary>
    public partial class SelectRoomPage : UserControl, IKeyDown, IReload
    {
		int blockState;					//屏幕上第一个roomBlock在全部roomBlock中的顺序
		int flickerState;				//橙色闪烁标记所处的位置
        int roomCount;                  //总房间数
        public const int MaxPeople = 4;       //每个房间的最大人数
        const int Left = 100;
        const int Top = 180;
        const int Right = 682;
        const int Bottom = 502;			//左上角RoomBlock的四个margin值
        const int HorMoveUnit = 580;
        const int VerMoveUnit = 140;
        RoomInfoBlock[] rooms = null;   //房间信息
        Storyboard blockMoveStory;
        Storyboard flickerMoveStory;

        private ClientEvt client;

        delegate void Fun(List<string> team, List<string> map, List<int> counts);
        Fun showRoom;

        public SelectRoomPage()
        {
            InitializeComponent();
            this.KeyDown += new KeyEventHandler(KeyboardDown);
            client = InfoControl.Client;
            client.GotTeamMapList += new ClientEvt.TeamMapList(client_GotTeamMapList);
            roomCount = 0;
            showRoom = new Fun(this.getRoomInfo);
        }

        //生成房间信息
        void client_GotTeamMapList(object sender, List<string> team, List<string> map, List<int> counts)
        {
            object[] para = new object[3];
            para[0] = team;
            para[1] = map;
            para[2] = counts;
            this.Dispatcher.Invoke(showRoom, para);         //调用getRoomInfo方法
        }

        //从Server获取房间信息
        public void getRoomInfo(List<string> team, List<string> map, List<int> counts)   
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
                rooms[i] = new RoomInfoBlock(team[i], map[i], counts[i]);
                rooms[i].Name = "room" + i.ToString();
                RegisterName(rooms[i].Name, rooms[i]);
            }
            if (roomCount != 0)
                initPosition();
        }

        //房间位置初始化
        private void initPosition()
        {
            int i, row_offset, col_offset;

            if (flickerMoveStory != null)
                flickerMoveStory.Remove(this);
            if (blockMoveStory != null)
                blockMoveStory.Remove(this);    //释放被动画占用的依赖属性

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
                rooms[i].Height = 120;
                rooms[i].Width = 500;
                rooms[i].Opacity = ((row_offset >= 0 && row_offset <= 3) ? 1.0 : 0.0);
            }
            flickerCanvas.Margin = new Thickness(Left - 10, Top - 12, Right - 10, Bottom - 12);
        }

        #region IReload 成员

        public void Reload()
        {
            client.GetTeamList();
        }

        #endregion

        #region IKeyDown 成员

        public void KeyboardDown(object sender, KeyEventArgs e)         //重写键盘按下事件响应函数
        {
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
            if (flickerState != -1)
                return MainFrame.INDEX_WAITING_ROOM_PAGE;
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

        #region 移动操作
        //向左移动
        private void moveLeft()
        {
            if (flickerState % 2 == 1)      //奇数，在右边
            {
                flickerState--;
                moveFlicker();
            }
        }

        //向右移动
        private void moveRight()
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
        private void moveUp()
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
        }

        //向下移动
        private void moveDown()
        {
            if (flickerState / 2 < 3)                                    //下面有空间，但并不确定是否有一个房间摆在那里（可能是奇数房间，下面是空的）
            {
                if (blockState + flickerState + 2 < roomCount)           //flicker下面有房间，只需要移动flicker
                { 
                    flickerState += 2;
                    moveFlicker();
                }
                else if (blockState + flickerState + 1 < roomCount)      //flicker必须左移
                {
                    flickerState++;
                    moveFlicker();
                }
            }
            else if (blockState + 8 < roomCount)                         //屏幕下方还有房间，block可以向上移动
            {
                blockState += 2;
                moveBlock();
                if (blockState + flickerState >= roomCount)                 //flicker必须左移
                {
                    flickerState--;
                    moveFlicker();
                }
            }
            else                                                          //动不了了
            {

            }

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
                    Top - 10+ VerMoveUnit * row_offset,
                    Right - 10 - HorMoveUnit * col_offset,
                    Bottom - 10 - VerMoveUnit * row_offset);
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
                opacAnimation[i].To = ((row_offset >= 0 && row_offset <= 3) ? 1.0 : 0.0);
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
        #endregion
    }
}
