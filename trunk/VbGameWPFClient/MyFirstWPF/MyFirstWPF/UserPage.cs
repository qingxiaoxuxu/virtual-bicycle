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
    public partial class UserPage : UserControl
    {
        public void KeyboardDown(object sender, KeyEventArgs e) {} 
        public int Choose() {return -1;}
        public int MoveBack() {return -1;}
    }
}
