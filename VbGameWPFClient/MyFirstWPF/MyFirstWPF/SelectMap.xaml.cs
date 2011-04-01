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

namespace MyFirstWPF
{
    /// <summary>
    /// SelectMap.xaml 的交互逻辑
    /// </summary>
    public partial class SelectMapPage : UserControl, IKeyDown
    {
        int mode;                                       //单人模式（0） 或  联网模式（1）
        int state;										//哪个按钮被选中
        const int IconCount = 3;                        //按钮数量
        const double Left = 512;                        //初始Margin.Left
        const double Top = 240;                         //初始Margin.Top
        const double Right = 514;                       //初始Margin.Right
        const double Bottom = 306;                      //初始Margin.Bottom   (这些是正中间图标的坐标)
        const int MoveUnit = 256;                       //图标移动尺寸
        const int ShrinkUnit = 80;                      //图标缩小尺寸
        Storyboard moveStory;
        Canvas[] iconCanvas = new Canvas[IconCount];
        String[] showTexts = new String[IconCount];

        public SelectMapPage(int md)
        {
            InitializeComponent();
            mode = md;
            state = 0;									//初始状态为0
            iconCanvas[0] = singleCanvas;
            iconCanvas[1] = createGameCanvas;
            iconCanvas[2] = joinGameCanvas;
            showTexts[0] = "Scene1";
            showTexts[1] = "Scene2";
            showTexts[2] = "Scene3";
            initPosition();             				//按钮位置初始化
            iconText.Text = showTexts[0];
        }

        #region IKeyDown 成员

        public void KeyboardDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Left)
                moveLeft();
            else if (e.Key == Key.Right)
                moveRight();
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
                MessageBox.Show("Waiting!");
                return -1;  //return 3;
            }
        }

        public int MoveBack()
        {
            return 0;
        }

        #endregion

        //向左移动
        private void moveLeft()
        {
            if (state > 0)
            {
                state--;
                iconText.Text = showTexts[state];
                moveStory = generateMoveStoryboard();
                moveStory.Begin(this);
            }
        }

        //向右移动
        private void moveRight()
        {
            if (state < IconCount - 1)
            {
                state++;
                iconText.Text = showTexts[state];
                moveStory = generateMoveStoryboard();
                moveStory.Begin(this);
            }
        }

        //图标位置初始化
        private void initPosition()
        {
            int i, offset, off_abs;
            for (i = 0; i < IconCount; i++)
            {
                offset = i - state;                                             //第i个按钮的偏移量（带符号）
                off_abs = Math.Abs(offset);                                     //第i个按钮的偏移量（绝对值）
                iconCanvas[i].Margin =
                    new Thickness(Left + MoveUnit * offset + (ShrinkUnit / 2) * off_abs, Top + (ShrinkUnit / 2) * off_abs,
                        Right - MoveUnit * offset + (ShrinkUnit / 2) * off_abs, Bottom + (ShrinkUnit / 2) * off_abs);
                iconCanvas[i].Opacity = 1.0 - 0.5 * off_abs;
            }
        }

        //生成Storyboard
        private Storyboard generateMoveStoryboard()
        {
            int i;
            Storyboard moveStory = new Storyboard();
            ThicknessAnimation[] posAnimation = new ThicknessAnimation[IconCount];
            DoubleAnimation[] opacAnimation = new DoubleAnimation[IconCount];
            for (i = 0; i < IconCount; i++)
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
    }
}