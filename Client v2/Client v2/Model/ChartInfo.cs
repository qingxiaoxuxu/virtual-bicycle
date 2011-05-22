using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Client_v2.Model
{
    public class ChartInfo
    {
        private int id;          
        private int currentTime;
        private double speed;
        private double heartBeat;
        private double distance;
        private double energy;

        public ChartInfo(int i, int t, double s, double h, double d, double e)
        {
            id = i; currentTime = t; speed = s; heartBeat = h; distance = d; energy = e;
        }

        public int Id
        {
            get { return id; }
            set { id = value; }
        }

        public int CurrentTime
        {
            get { return currentTime; }
            set { currentTime = value; }
        }

        public double Speed
        {
            get { return speed; }
            set { speed = value; }
        }

        public double HeartBeat
        {
            get { return heartBeat; }
            set { heartBeat = value; }
        }

        public double Distance
        {
            get { return distance; }
            set { distance = value; }
        }

        public double Energy
        {
            get { return energy; }
            set { energy = value; }
        }

    }
}
