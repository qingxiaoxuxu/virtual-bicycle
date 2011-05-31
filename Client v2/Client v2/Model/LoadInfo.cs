using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Client_v2.Model
{
    class LoadInfo
    {
        private int load = 0;
        private int currentTime = 0;

        public int CurrentTime
        {
            get { return currentTime; }
            set { currentTime = value; }
        }
        public int Load
        {
            get { return load; }
            set { load = value; }
        }
        public LoadInfo(int l,int t)
        {
            load = l;
            currentTime = t;
        }
    }
}
