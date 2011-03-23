using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using System.Windows.Shapes;
using System.Windows.Controls;
using System.Windows;
using System.Windows.Input;

namespace VbGameEditor
{
    public class ScenePoint
    {

        public static List<ScenePoint> roadPoint = new List<ScenePoint>();
        public static List<ScenePoint> listPoint = new List<ScenePoint>();
        private PointCategory category;

        public PointCategory Category
        {
            get { return category; }
            set
            {
                category = value;
                switch (category)
                {
                    case PointCategory.Car:
                        this.brush = Brushes.Red;
                        break;
                    case PointCategory.House:
                        this.brush = Brushes.Blue;
                        break;
                    case PointCategory.People:
                        this.brush = Brushes.Orange;
                        break;
                    case PointCategory.Road:
                        this.brush = Brushes.Black;
                        break;
                    case PointCategory.Tree:
                        this.brush = Brushes.Green;
                        break;
                }
                
            }
        }
        private Brush brush;
        public Point3D position;
        public double radius = 10;
        private Ellipse ellipse;

        public Ellipse Ellipse
        {
            get 
            {
                ellipse.Width = ellipse.Height = radius*Window1.Zoom;
                Canvas.SetTop(ellipse, Window1.Zoom*(position.Y - radius / 2+Window1.Shift.Y));
                Canvas.SetLeft(ellipse, Window1.Zoom*(position.X - radius / 2+Window1.Shift.X));
                return ellipse; 
            }
            set { ellipse = value; }
        }

        private Ellipse ellipseh;

        public Ellipse Ellipseh
        {
            get
            {
                ellipseh.Width = ellipseh.Height = radius * Window1.Zoom;
                Canvas.SetTop(ellipseh, 200-Window1.Zoom * (position.Z + radius / 2 ));
                Canvas.SetLeft(ellipseh, Window1.Zoom * (position.X - radius / 2 + Window1.Shift.X));
                return ellipseh;
            }
            set { ellipseh = value; }
        }
        public ScenePoint(PointCategory pc,Point p)
        {
            this.position = new Point3D(p.X, p.Y, 0);
            this.Category = pc;
            listPoint.Add(this);
            if(pc==PointCategory.Road)
                roadPoint.Add(this);
            ellipse = new Ellipse();
            ellipse.Fill = brush;
            ellipse.MouseDown += new System.Windows.Input.MouseButtonEventHandler(ellipse_MouseDown);
            ellipse.MouseMove += new System.Windows.Input.MouseEventHandler(ellipse_MouseMove);
            ellipse.MouseUp += new System.Windows.Input.MouseButtonEventHandler(ellipse_MouseUp);
            ellipse.MouseLeave += new System.Windows.Input.MouseEventHandler(ellipse_MouseLeave);

            ellipseh = new Ellipse();
            ellipseh.Fill = brush;
            ellipseh.MouseDown += new MouseButtonEventHandler(ellipseh_MouseDown);
            ellipseh.MouseMove += new MouseEventHandler(ellipseh_MouseMove);
            ellipseh.MouseUp += new MouseButtonEventHandler(ellipseh_MouseUp);
            ellipseh.MouseLeave += new MouseEventHandler(ellipseh_MouseLeave);
            
        }

        public ScenePoint(PointCategory pc, Point3D p)
        {
            this.position = p;
            this.Category = pc;
            listPoint.Add(this);
            if (pc == PointCategory.Road)
                roadPoint.Add(this);
            ellipse = new Ellipse();
            ellipse.Fill = brush;
            ellipse.MouseDown += new System.Windows.Input.MouseButtonEventHandler(ellipse_MouseDown);
            ellipse.MouseMove += new System.Windows.Input.MouseEventHandler(ellipse_MouseMove);
            ellipse.MouseUp += new System.Windows.Input.MouseButtonEventHandler(ellipse_MouseUp);
            ellipse.MouseLeave += new System.Windows.Input.MouseEventHandler(ellipse_MouseLeave);

            ellipseh = new Ellipse();
            ellipseh.Fill = brush;
            ellipseh.MouseDown += new MouseButtonEventHandler(ellipseh_MouseDown);
            ellipseh.MouseMove += new MouseEventHandler(ellipseh_MouseMove);
            ellipseh.MouseUp += new MouseButtonEventHandler(ellipseh_MouseUp);
            ellipseh.MouseLeave += new MouseEventHandler(ellipseh_MouseLeave);

        }
        bool isDownh = false;
        Point preMouseh = new Point();
        void ellipseh_MouseLeave(object sender, MouseEventArgs e)
        {
            if (isDownh)
            {
                Point curMouse = e.GetPosition((sender as Ellipse).Parent as Canvas);
                Ellipse ellipse = sender as Ellipse;
                Canvas.SetTop(ellipse, Canvas.GetTop(ellipse) + curMouse.Y - preMouseh.Y);
                Canvas.SetLeft(ellipse, Canvas.GetLeft(ellipse) + curMouse.X - preMouseh.X);
                position.X = (Canvas.GetLeft(ellipse) - Window1.Shift.X + radius / 2) / Window1.Zoom;
                position.Z = 200-(Canvas.GetTop(ellipse) + radius / 2) / Window1.Zoom;
                preMouseh = curMouse;
            }
        }

        void ellipseh_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Released)
            {
                isDownh = false;
            }
        }

        void ellipseh_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                if (isDownh)
                {
                    Point curMouse = e.GetPosition((sender as Ellipse).Parent as Canvas);
                    Ellipse ellipse = sender as Ellipse;
                    Canvas.SetTop(ellipse, Canvas.GetTop(ellipse) + curMouse.Y - preMouseh.Y);
                    Canvas.SetLeft(ellipse, Canvas.GetLeft(ellipse) + curMouse.X - preMouseh.X);
                    position.X = (Canvas.GetLeft(ellipse) - Window1.Shift.X + radius / 2) / Window1.Zoom;
                    position.Z = 200-(Canvas.GetTop(ellipse) + radius / 2) / Window1.Zoom;
                    preMouseh = curMouse;
                }
            }
        }

        void ellipseh_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                if (Keyboard.IsKeyDown(Key.LeftShift))
                {
                    foreach (ScenePoint sp in roadPoint)
                    {
                        if (sp.ellipseh == sender as Ellipse)
                        {
                            roadPoint.Remove(sp);
                            listPoint.Remove(sp);
                            break;
                        }
                    }
                }
                else
                {
                    isDownh = true;
                    preMouseh = e.GetPosition((sender as Ellipse).Parent as Canvas);
                    e.Handled = true;
                }
            }
        }

        
        void ellipse_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (isDown)
            {
                Point curMouse = e.GetPosition((sender as Ellipse).Parent as Canvas);
                Ellipse ellipse = sender as Ellipse;
                Canvas.SetTop(ellipse, Canvas.GetTop(ellipse) + curMouse.Y - preMouse.Y);
                Canvas.SetLeft(ellipse, Canvas.GetLeft(ellipse) + curMouse.X - preMouse.X);
                position.X = (Canvas.GetLeft(ellipse) - Window1.Shift.X + radius / 2) / Window1.Zoom;
                position.Y = (Canvas.GetTop(ellipse) - Window1.Shift.Y + radius / 2) / Window1.Zoom;
                preMouse = curMouse;
            }
        }

        void ellipse_MouseUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Released)
            {
                isDown = false;
            }
        }

        void ellipse_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                if (isDown)
                {
                    Point curMouse = e.GetPosition((sender as Ellipse).Parent as Canvas);
                    Ellipse ellipse = sender as Ellipse;
                    Canvas.SetTop(ellipse, Canvas.GetTop(ellipse) + curMouse.Y - preMouse.Y);
                    Canvas.SetLeft(ellipse, Canvas.GetLeft(ellipse) + curMouse.X - preMouse.X);
                    position.X = (Canvas.GetLeft(ellipse) - Window1.Shift.X+radius/2) / Window1.Zoom;
                    position.Y = (Canvas.GetTop(ellipse) - Window1.Shift.Y+radius/2) / Window1.Zoom;
                    preMouse = curMouse;
                }
            }
        }
        bool isDown = false;
        Point preMouse = new Point();
        void ellipse_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                if (Keyboard.IsKeyDown(Key.LeftShift))
                {
                    foreach (ScenePoint sp in roadPoint)
                    {
                        if (sp.ellipse == sender as Ellipse)
                        {
                            roadPoint.Remove(sp);
                            listPoint.Remove(sp);
                            break;
                        }
                    }
                }
                else
                {
                    isDown = true;
                    preMouse = e.GetPosition((sender as Ellipse).Parent as Canvas);
                    e.Handled = true;
                }
            }
            
        }
        
    }
    public enum PointCategory
    {
        Road,
        Tree,
        House,
        People,
        Car
    }

}
