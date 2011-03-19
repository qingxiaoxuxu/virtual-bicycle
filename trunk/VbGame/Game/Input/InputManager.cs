using System;
using System.Collections.Generic;
using System.Text;

namespace VirtualBicycle.Input
{
    public class HandlebarRotatedEventArgs : EventArgs
    {
        public HandlebarRotatedEventArgs(float angle, float delta)
        {
            this.Angle = angle;
            this.DeltaAngle = delta;
        }

        public float Angle
        {
            get;
            set;
        }

        public float DeltaAngle
        {
            get;
            set;
        }
    }

    public class WheelSpeedChangedEventArgs : EventArgs
    {
        public WheelSpeedChangedEventArgs(float speed, float delta, bool reference)
        {
            this.Speed = speed;
            this.DeltaSpeed = delta;
            this.Reference = reference;
        }

        public float Speed
        {
            get;
            set;
        }

        public float DeltaSpeed
        {
            get;
            set;
        }

        public bool Reference
        {
            get;
            set;
        }
    }

    public delegate void HandlebarRotatedHandler(HandlebarRotatedEventArgs e);
    public delegate void WheelSpeedChangedHandler(WheelSpeedChangedEventArgs e);


    public class InputManager : Singleton
    {
        static InputManager singleton;

        public static InputManager Instance
        {
            get
            {
                if (singleton == null)
                {
                    singleton = new InputManager();
                }
                return singleton;
            }
        }

        public event EventHandler Reset;

        public event EventHandler ViewChanged;
        public event EventHandler Enter;
        public event EventHandler Escape;

        public event EventHandler ItemMoveLeft;
        public event EventHandler ItemMoveRight;

        public event HandlebarRotatedHandler HandlebarRotated;
        public event WheelSpeedChangedHandler WheelSpeedChanged;

        public event EventHandler HeartPulse;

        public InputProcessor Processor
        {
            get;
            set;
        }


        public void ForceFeedback(float f)
        {
            if (Processor != null)
                Processor.Feedback(f);
        }


        public void OnReset()
        {
            if (Reset != null)
            {
                Reset(this, EventArgs.Empty);
            }
        }
        public void OnItemMoveLeft() 
        {
            if (ItemMoveLeft != null) 
            {
                ItemMoveLeft(this, EventArgs.Empty);
            }
        }
        public void OnItemMoveRight() 
        {
            if (ItemMoveRight != null)
            {
                ItemMoveRight(this, EventArgs.Empty);
            }
        }

        public void OnWheelSpeedChanged(float speed, float delta, bool reference)
        {
            if (WheelSpeedChanged != null)
            {
                WheelSpeedChanged(new WheelSpeedChangedEventArgs(speed, delta, reference));
            }
        }

        public void OnViewChanged()
        {
            if (ViewChanged != null)
            {
                ViewChanged(this, EventArgs.Empty);
            }
        }
        public void OnEnter()
        {
            if (Enter != null)
            {
                Enter(this, EventArgs.Empty);
            }
        }
        public void OnEscape()
        {
            if (Escape != null)
            {
                Escape(this, EventArgs.Empty);
            }
        }
        public void OnHandlebarRotated(float angle, float delta)
        {
            if (HandlebarRotated != null)
            {
                HandlebarRotated(new HandlebarRotatedEventArgs(angle, delta));
            }
        }
        public void OnHeartPulse()
        {
            if (HeartPulse != null) 
            {
                HeartPulse(this, EventArgs.Empty);
            }
        }
        

        protected override void dispose()
        {
            ViewChanged = null;
            Enter = null;
            HandlebarRotated = null;
        }



        public void Update(float dt)
        {
            if (Processor != null)
            {
                Processor.Update(dt);
            }
            //if use two processor together, cause running slow 
            //if (Processor_bak != null)
            //{
            //    Processor_bak.Update(dt);
            //}
        }
    }
}
