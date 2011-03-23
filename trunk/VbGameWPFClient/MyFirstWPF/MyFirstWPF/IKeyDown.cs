using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Text;

namespace MyFirstWPF
{
    public interface IKeyDown
    {
        void KeyboardDown(object sender, KeyEventArgs e);       //键盘事件响应

        int Choose();                                           //切换到哪个窗体

        int MoveBack();                                         //返回到哪个窗体
    }
}
