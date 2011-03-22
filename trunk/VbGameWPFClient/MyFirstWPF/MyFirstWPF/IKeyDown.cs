using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Text;

namespace MyFirstWPF
{
    public interface IKeyDown
    {
        void KeyboardDown(object sender, KeyEventArgs e);
    }
}
