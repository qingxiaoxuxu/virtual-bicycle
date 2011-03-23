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
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using WinForm=System.Windows.Forms;

namespace VbGameEditor
{
    /// <summary>
    /// Window1.xaml 的交互逻辑
    /// </summary>
    public partial class Window1 : Window
    {
        public Window1()
        {
            InitializeComponent();
            btnSave.Click += new RoutedEventHandler(btnSave_Click);
            btnLoad.Click += new RoutedEventHandler(btnLoad_Click);
            MapCanvas.MouseDown+=new MouseButtonEventHandler(MapCanvas_MouseDown);
            MapCanvas.MouseMove += new MouseEventHandler(MapCanvas_MouseMove);
            MapCanvas.MouseUp += new MouseButtonEventHandler(MapCanvas_MouseUp);
            MapCanvas.MouseWheel += new MouseWheelEventHandler(MapCanvas_MouseWheel);
            this.KeyDown += new KeyEventHandler(MapCanvas_KeyDown);
            rbtnRoad.Click += new RoutedEventHandler(rbtnRoad_Click);
            rbtnPeople.Click += new RoutedEventHandler(rbtnPeople_Click);
            rbtnTree.Click += new RoutedEventHandler(rbtnTree_Click);
            rbtnHouse.Click += new RoutedEventHandler(rbtnHouse_Click);
            rbtnCar.Click += new RoutedEventHandler(rbtnCar_Click);
            HeightCanvas.MouseMove += new MouseEventHandler(HeightCanvas_MouseMove);
        }

        void btnLoad_Click(object sender, RoutedEventArgs e)
        {
            WinForm.OpenFileDialog ofd = new System.Windows.Forms.OpenFileDialog();
            if (ofd.ShowDialog() == WinForm.DialogResult.OK)
            {
                ScenePoint.listPoint.Clear();
                ScenePoint.roadPoint.Clear();
                new XMLHelper(ofd.FileName,false).Get();
                Update();
            }
        }

        void btnSave_Click(object sender, RoutedEventArgs e)
        {
            WinForm.SaveFileDialog sfd = new WinForm.SaveFileDialog();
            if (sfd.ShowDialog() == WinForm.DialogResult.OK)
            {
                Save(sfd.FileName);
            }
        }
        void Save(string name)
        {
            XMLHelper xmlhelper = new XMLHelper(name,true);
            foreach (ScenePoint sp in ScenePoint.listPoint)
            {
                xmlhelper.Add(sp);
            }
        }
        void HeightCanvas_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                Update();
            }
        }

        PointCategory currentCate = PointCategory.Road;
        void rbtnCar_Click(object sender, RoutedEventArgs e)
        {
            currentCate = PointCategory.Car;
        }

        void rbtnHouse_Click(object sender, RoutedEventArgs e)
        {
            currentCate = PointCategory.House;
        }

        void rbtnTree_Click(object sender, RoutedEventArgs e)
        {
            currentCate = PointCategory.Tree;
        }

        void rbtnPeople_Click(object sender, RoutedEventArgs e)
        {
            currentCate = PointCategory.People;
        }

        void rbtnRoad_Click(object sender, RoutedEventArgs e)
        {
            currentCate = PointCategory.Road;
        }

        void MapCanvas_KeyDown(object sender, KeyEventArgs e)
        {
            if (Keyboard.IsKeyDown(Key.Z))
            {
                ScenePoint.listPoint.Clear();
                ScenePoint.roadPoint.Clear();
                Update();
            }
        }

        void MapCanvas_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (e.Delta > 0)
            {
                if(Zoom<10)
                    Zoom += 0.1;
            }
            else
            {
                if(Zoom>0)
                    Zoom -= 0.1;
            }
            Update();
        }

        void MapCanvas_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (e.RightButton == MouseButtonState.Released)
            {
                isDown = false;
            }
            UpdateRoad();
        }

        void MapCanvas_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.RightButton == MouseButtonState.Pressed)
            {
                if (isDown)
                {
                    Point curMouse = e.GetPosition(MapCanvas);
                    Shift = new Point(Shift.X + (curMouse.X - preMouse.X) / Zoom, Shift.Y + (curMouse.Y - preMouse.Y) / Zoom);
                    preMouse = curMouse;
                    Update();
                }         
                   
            }
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                UpdateRoad();
                UpdateHeightCanvas();
            }
        }
        public static double Zoom = 1;
        public static Point Shift = new Point(0, 0);
        bool isDown = false;
        Point preMouse = new Point();
        private void MapCanvas_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                if (Keyboard.IsKeyDown(Key.LeftShift))
                {
                    Update();
                }
                else
                {
                    Point mouseP = new Point((e.GetPosition(MapCanvas).X - Shift.X) / Zoom, (e.GetPosition(MapCanvas).Y - Shift.Y) / Zoom);
                    ScenePoint sp = new ScenePoint(currentCate, mouseP);

                    MapCanvas.Children.Add(sp.Ellipse);
                    UpdateHeightCanvas();
                }
            }
            else if (e.RightButton == MouseButtonState.Pressed)
            {
                isDown = true;
                preMouse = e.GetPosition(MapCanvas);
            }
            UpdateRoad();
        }
        public void Update()
        {
            MapCanvas.Children.Clear();
            foreach (ScenePoint sp in ScenePoint.listPoint)
            {
                MapCanvas.Children.Add(sp.Ellipse);
            }
            UpdateRoad();
            UpdateHeightCanvas();
        }
        Path path;
        public void UpdateRoad()
        {
            MapCanvas.Children.Remove(path);
            if (ScenePoint.roadPoint.Count > 0)
            {
                List<Point> list = new List<Point>();
                foreach (ScenePoint sp in ScenePoint.roadPoint)
                {
                    list.Add(new Point((sp.position.X + Shift.X) * Zoom, (sp.position.Y + Shift.Y) * Zoom));
                }
                PathFigure pf = new PathFigure();

                pf.StartPoint = list[0];
                List<Point> controls = new List<Point>();
                for (int i = 0; i < list.Count; i++)
                {
                    controls.AddRange(Control1(list, i));
                }
                for (int i = 1; i < list.Count; i++)
                {
                    BezierSegment bs = new BezierSegment(controls[i * 2 - 1], controls[i * 2], list[i], true);
                    bs.IsSmoothJoin = true;

                    pf.Segments.Add(bs);
                }
                PathFigureCollection pfc = new PathFigureCollection();
                pfc.Add(pf);
                PathGeometry pg = new PathGeometry(pfc);

                path = new Path();
                path.Stroke = Brushes.Black;
                path.Data = pg;
                MapCanvas.Children.Add(path);
            }
        }
        public void UpdateHeightCanvas()
        {
            HeightCanvas.Children.Clear();
            foreach (ScenePoint sp in ScenePoint.listPoint)
            {
                HeightCanvas.Children.Add(sp.Ellipseh);
            }
        }
        public List<Point> Control1(List<Point> list, int n)
        {
            List<Point> point = new List<Point>();
            point.Add(new Point());
            point.Add(new Point());
            if (n == 0)
            {
                point[0] = list[0];
            }
            else
            {
                point[0] = Average(list[n - 1], list[n]);
            }
            if (n == list.Count - 1)
            {
                point[1] = list[list.Count - 1];
            }
            else
            {
                point[1] = Average(list[n], list[n+1]);
            }
            Point ave = Average(point[0], point[1]);
            Point sh = Sub(list[n], ave);
            point[0] = Mul(Add(point[0], sh),list[n],0.6);
            point[1] = Mul(Add(point[1], sh),list[n],0.6);
            //Line line = new Line();
            //line.X1 = point[0].X;
            //line.Y1 = point[0].Y;
            //line.X2 = point[1].X;
            //line.Y2 = point[1].Y;
            //line.Stroke = Brushes.Red;
            //MapCanvas.Children.Add(line);
            return point;
        }
        public Point Average(Point x, Point y)
        {
            return new Point((x.X+y.X)/2,(x.Y+y.Y)/2);
        }
        public Point Add(Point x, Point y)
        {
            return new Point(x.X + y.X, x.Y + y.Y);
        }
        public Point Sub(Point x, Point y)
        {
            return new Point(x.X - y.X, x.Y - y.Y);
        }
        public Point Mul(Point x, Point y,double d)
        {
            Point temp = Sub(x, y);
            temp = new Point(temp.X * d, temp.Y * d);
            temp = Add(y, temp);
            return temp;
        }
    }
}
