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
    /// Window1.xaml 的交互逻辑
    /// </summary>
    public partial class Window1 : Window
    {
        int state;
        const int IconCount = 3;                        //按钮数量
        const double left = 512;                        //初始Margin.Left
        const double top = 240;                         //初始Margin.Top
        const double right = 514;                       //初始Margin.Right
        const double bottom = 306;                      //初始Margin.Bottom   (这些是正中间图标的坐标)
        const int MoveUnit = 256;                       //图标移动尺寸
        const int ShrinkUnit = 80;                      //图标缩小尺寸
        Storyboard moveStory;
        Canvas[] iconCanvas = new Canvas[IconCount];
        String[] showTexts = new String[IconCount];

        public Window1()
        {
            InitializeComponent();
            state = 0;
            #region Dynamic to add an element
            //Rectangle myrect = new Rectangle();
            //myrect.Name = "myrect";
            //this.RegisterName(myrect.Name, myrect);
            //myrect.Width = 100;
            //myrect.Height = 100;
            //myrect.Fill = Brushes.Blue;
            //bgCanvas.Children.Add(myrect);
            #endregion
            iconCanvas[0] = singleCanvas;
            iconCanvas[1] = createGameCanvas;
            iconCanvas[2] = joinGameCanvas;
            showTexts[0] = "单人游戏";
            showTexts[1] = "建立网络对战";
            showTexts[2] = "加入网络对战";
            initPosition();             //按钮位置初始化
            this.KeyDown += new KeyEventHandler(Window1_KeyDown);
            this.Loaded += new RoutedEventHandler(Window1_Loaded);
            #region others
            /*
            Rectangle rect = new Rectangle();
            rect.Width = canvas.Width / 2;
            rect.Height = canvas.Height / 3;
            rect.RadiusX = 24;
            rect.RadiusY = 24;
            rect.Fill = Brushes.Blue;
            canvas.Children.Add(rect);
            Canvas.SetLeft(rect, 20);
            Canvas.SetTop(rect, 30);
            ImageSource src = new BitmapImage(new Uri("mainMenu_origin.png", UriKind.Relative));
            ImageBrush bru = new ImageBrush(src);
            bgCanvas.Background = bru;
             */
            #endregion
        }

        void Window1_Loaded(object sender, RoutedEventArgs e)
        {
            Storyboard flickerStory = generateFlickerStoryboard();
            flickerStory.Begin(this);
        }

        void Window1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Left)
                moveLeft();
            else if (e.Key == Key.Right)
                moveRight();
            else if (e.Key == Key.Enter)
                choose();
        }

        private void choose()
        {
            if (state == 0) { }
            else if (state == 1) { }
            else { }
            MessageBox.Show(state.ToString());
        }

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
                    new Thickness(left + MoveUnit * offset + (ShrinkUnit / 2) * off_abs, top + (ShrinkUnit / 2) * off_abs,
                        right - MoveUnit * offset + (ShrinkUnit / 2) * off_abs, bottom + (ShrinkUnit / 2) * off_abs);
                iconCanvas[i].Opacity = 1.0 - 0.5 * off_abs;
            }
        }

        private Storyboard generateFlickerStoryboard()
        {
            Storyboard flickerStory = new Storyboard();
            ThicknessAnimation posAnimation = new ThicknessAnimation();
            posAnimation.From = new Thickness(500, 220, 502, 302);
            posAnimation.To = new Thickness(512, 232, 514, 314);
            posAnimation.Duration = new Duration(TimeSpan.FromSeconds(0.5));
            posAnimation.RepeatBehavior = RepeatBehavior.Forever;
            posAnimation.AutoReverse = true;
            Storyboard.SetTargetName(posAnimation, flickerCanvas.Name);
            Storyboard.SetTargetProperty(posAnimation, new PropertyPath(Canvas.MarginProperty));
            flickerStory.Children.Add(posAnimation);
            return flickerStory;
        }

        //生成Storyboard
        private Storyboard generateMoveStoryboard()
        {
            int i;
            #region 初始坐标
            
            #endregion
            Storyboard moveStory = new Storyboard();
            ThicknessAnimation[] posAnimation = new ThicknessAnimation[IconCount];
            DoubleAnimation[] opacAnimation = new DoubleAnimation[IconCount];
            for (i = 0; i < IconCount; i++)
            {
                int offset = i - state;                                             //第i个按钮的偏移量（带符号）
                int off_abs = Math.Abs(offset);                                     //第i个按钮的偏移量（绝对值）
                posAnimation[i] = new ThicknessAnimation();                         //控制图标位置
                posAnimation[i].To =
                    new Thickness(left + MoveUnit * offset + (ShrinkUnit / 2) * off_abs, top + (ShrinkUnit / 2) * off_abs,
                        right - MoveUnit * offset + (ShrinkUnit / 2) * off_abs, bottom + (ShrinkUnit / 2) * off_abs);
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

        private Storyboard generateMoveStoryboard_original()
        {
            int i, j;
            Storyboard moveStory = new Storyboard();
            DoubleAnimation[,] moveAnimation = new DoubleAnimation[IconCount, 2];       //哪些按钮的移动和透明
            #region DoubleAnimation设定
            for (i = 0; i < IconCount; i++)
                for (j = 0; j < 2; j++)
                    moveAnimation[i, j] = new DoubleAnimation();                        //初始化DoubleAnimation
            for (i = 0; i < IconCount; i++)
            {
                moveAnimation[i, 0].To = 512 + 256 * (i - state);                       //第i个按钮位置
                moveAnimation[i, 1].To = 1.0 - 0.5 * Math.Abs(i - state);               //第i个按钮透明度
                Storyboard.SetTargetName(moveAnimation[i, 0], iconCanvas[i].Name);
                Storyboard.SetTargetName(moveAnimation[i, 1], iconCanvas[i].Name);
                Storyboard.SetTargetProperty(moveAnimation[i, 0], new PropertyPath(Canvas.LeftProperty));
                Storyboard.SetTargetProperty(moveAnimation[i, 1], new PropertyPath(Canvas.OpacityProperty));
            }
            foreach (DoubleAnimation tmp in moveAnimation)
            {
                tmp.Duration = new Duration(TimeSpan.FromSeconds(0.3));                 //动画时间
                moveStory.Children.Add(tmp);
            }
            #region unused
            //if (state == 0)                                         //第0个按钮被选中
            //{
            //    for (i = 0; i < IconCount; i++)
            //    {
            //        moveAnimation[i, 0].To = 512 + 256 * i;
            //        moveAnimation[i, 1].To = 1.0 - 0.5 * i;
            //    }
            //    /*
            //    moveAnimation[0, 0].To = 512;                       //第0个按钮的位置
            //    moveAnimation[1, 0].To = 640;                       //第1个按钮的位置
            //    moveAnimation[2, 0].To = 768;                       //第2个按钮的位置
            //    moveAnimation[0, 1].To = 1.0;                       //第0个按钮的透明
            //    moveAnimation[1, 1].To = 0.5;                       //第1个按钮的透明
            //    moveAnimation[2, 1].To = 0.0;                       //第2个按钮的透明
            //    */
            //}
            //else if (state == 1)                                    //第1个按钮被选中
            //{
            //    for (i = 0; i < IconCount; i++)
            //    {
            //        moveAnimation[i, 0].To = 256 + 256 * i;
            //        moveAnimation[i, 1].To = 1.0 - 0.5 * Math.Abs(i - 1);
            //    }
            //    /*
            //    moveAnimation[0, 0].To = 384;                       //第0个按钮的位置
            //    moveAnimation[1, 0].To = 512;                       //第1个按钮的位置
            //    moveAnimation[2, 0].To = 640;                       //第2个按钮的位置
            //    moveAnimation[0, 1].To = 0.5;                       //第0个按钮的透明
            //    moveAnimation[1, 1].To = 1.0;                       //第1个按钮的透明
            //    moveAnimation[2, 1].To = 0.5;                       //第2个按钮的透明
            //    */
            //}
            //else if (state == 2)                                    //第2个按钮被选中
            //{
            //    for (i = 0; i < IconCount; i++)
            //    {
            //        moveAnimation[i, 0].To = 256 + 256 * i;
            //        moveAnimation[i, 1].To = 1.0 - 0.5 * Math.Abs(i - 2);
            //    }
            //    /*
            //    moveAnimation[0, 0].To = 256;                       //第0个按钮的位置
            //    moveAnimation[1, 0].To = 384;                       //第1个按钮的位置
            //    moveAnimation[2, 0].To = 512;                       //第2个按钮的位置
            //    moveAnimation[0, 1].To = 0.0;                       //第0个按钮的透明
            //    moveAnimation[1, 1].To = 0.5;                       //第1个按钮的透明
            //    moveAnimation[2, 1].To = 1.0;                       //第2个按钮的透明
            //     */
            //}
            #endregion
            #endregion
            return moveStory;
        }
        
    }
}
