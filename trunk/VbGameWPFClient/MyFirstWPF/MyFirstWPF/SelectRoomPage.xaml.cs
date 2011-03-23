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

namespace MyFirstWPF
{
    /// <summary>
    /// SelectRoomPage.xaml 的交互逻辑
    /// </summary>
    public partial class SelectRoomPage : UserControl, IKeyDown
    {
		
		int blockState;					//屏幕上第一个roomBlock在全部roomBlock中的顺序
		int flickerState;				//橙色闪烁标记所处的位置
        int roomCount;                  //总房间数
        const int Left = 100;
        const int Top = 180;
        const int Right = 682;
        const int Bottom = 502;
        const int HorMoveUnit = 580;
        const int VerMoveUnit = 140;
        RoomInfoBlock[] rooms = null;   //房间信息
        Storyboard blockMoveStory;
        Storyboard flickerMoveStory;

        public SelectRoomPage()
        {
            InitializeComponent();
			blockState = 0;
            flickerState = 0;
            roomCount = getRoomInfo();
            initPosition();
            this.KeyDown +=new KeyEventHandler(KeyboardDown);
        }

        public int getRoomInfo()        //获取房间信息
        {
            int roomCount = 13;
            rooms = new RoomInfoBlock[roomCount];
            for (int i = 0; i < roomCount; i++)
            {
                rooms[i] = new RoomInfoBlock();
                rooms[i].Name = "room" + i.ToString();
                RegisterName(rooms[i].Name, rooms[i]);
                //TODO
            }
            return roomCount;
        }

        //房间位置初始化
        private void initPosition()
        {
            int i, row_offset, col_offset;
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
                rooms[i].Opacity = ((row_offset >= 0 && row_offset <= 3) ? 1.0 : 0.0);
            }
        }

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
            return -1;
        }

        public int MoveBack()
        {
            return 0;
        }

        #endregion

        //向左移动
        private void moveLeft()
        {
            if (flickerState % 2 == 1)      //奇数，在右边
            {
                flickerState--;
                flickerMoveStory = generateFlickerMoveStoryboard();
                flickerMoveStory.Begin(this);
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
                    flickerMoveStory = generateFlickerMoveStoryboard();
                    flickerMoveStory.Begin(this);
                }
            }
        }

        //向上移动
        private void moveUp()
        {
            if (flickerState / 2 > 0)              //上面有空间，只需要移动flicker
            {
                flickerState -= 2;
                flickerMoveStory = generateFlickerMoveStoryboard();
                flickerMoveStory.Begin(this);
            }
            else if (blockState / 2 > 0)           //屏幕上方还有房间，block可以向下移动
            {
                blockState -= 2;
                blockMoveStory = generateBlockMoveStoryboard();
                blockMoveStory.Begin(this);
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
                    flickerMoveStory = generateFlickerMoveStoryboard();
                    flickerMoveStory.Begin(this);
                }
                else                                                     //flicker必须左移
                {
                    flickerState++;
                    flickerMoveStory = generateFlickerMoveStoryboard();
                    flickerMoveStory.Begin(this);
                }
            }
            else if (blockState + 8 < roomCount)                         //屏幕下方还有房间，block可以向上移动
            {
                blockState += 2;
                blockMoveStory = generateBlockMoveStoryboard();
                blockMoveStory.Begin(this);
                if (blockState + flickerState >= roomCount)           //flicker必须左移
                {
                    flickerState--;
                    flickerMoveStory = generateFlickerMoveStoryboard();
                    flickerMoveStory.Begin(this);
                }
            }
            else                                                          //动不了了
            {

            }
            
        }

        //生成选择Canvas闪烁摇摆的故事版
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
                posAnimation[i].Duration = new Duration(TimeSpan.FromSeconds(0.1));
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
    }
}
