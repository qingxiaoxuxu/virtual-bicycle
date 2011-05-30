using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

namespace MyFirstWPF
{
    class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            App app = new App();
            Random rd = new Random(DateTime.Now.Millisecond);
            int id = rd.Next(100000);
            if (args.Length < 2)
                app.Run(new MainFrame("testUser" + id.ToString(), id.ToString()));
            else
                app.Run(new MainFrame(args[0], args[1]));
        }
    }
}
