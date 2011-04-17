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
	/// mainPage.xaml 的交互逻辑
	/// </summary>
	public partial class MainPage :UserControl, IKeyDown
	{
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
		
		public MainPage()
		{
			this.InitializeComponent();
            state = 0;									//初始状态为0
            iconCanvas[0] = singleCanvas;
            iconCanvas[1] = createGameCanvas;
            iconCanvas[2] = joinGameCanvas;
            showTexts[0] = "单人游戏";
            showTexts[1] = "建立网络对战";
            showTexts[2] = "加入网络对战";
            initPosition();             				//按钮位置初始化
            iconText.Text = showTexts[0];
            this.KeyDown += new KeyEventHandler(KeyboardDown);
            this.Loaded += new RoutedEventHandler(MainPage_Loaded);
        }

        #region IKeyDown 成员

        public void KeyboardDown(object sender, KeyEventArgs e)         //重写键盘按下事件响应函数
        {
            if (e.Key == Key.Left)
                moveLeft();
            else if (e.Key == Key.Right)
                moveRight();
        }

        public int Choose()
        {
            return state + 1;
        }

        public int MoveBack()
        {
            return -1;
        }

        #endregion

		void MainPage_Loaded(object sender, RoutedEventArgs e)
        {
            Storyboard flickerStory = generateFlickerStoryboard();
            flickerStory.Begin(this);
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
                    new Thickness(Left + MoveUnit * offset + (ShrinkUnit / 2) * off_abs, Top + (ShrinkUnit / 2) * off_abs,
                        Right - MoveUnit * offset + (ShrinkUnit / 2) * off_abs, Bottom + (ShrinkUnit / 2) * off_abs);
                iconCanvas[i].Opacity = 1.0 - 0.5 * off_abs;
            }
        }
		
		//生成选择Canvas闪烁摇摆的故事版
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