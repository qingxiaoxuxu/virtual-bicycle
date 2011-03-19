using System;
using System.Collections.Generic;
using System.Text;
using Code2015.EngineEx;
using System.IO;
using Microsoft.Xna.Framework.Storage;
using Apoc3D.Config;
using Apoc3D;

namespace Game.Config
{
    class CommonSettings : Singleton
    {
        static volatile CommonSettings singleton;
        static object syncHelper = new object();
        public static CommonSettings Instance 
        {
            get 
            {                
                if (singleton == null)
                {
                    lock (syncHelper)
                    {
                        if (singleton == null)
                        {
                            singleton = new CommonSettings();
                        }
                    }
                }
                return singleton;
            }
        }

        GameConfiguration config;

        private CommonSettings()
        {
            string filePath = Path.Combine(StorageContainer.TitleLocation, @"Config\common.xml");
            config = new GameConfiguration(filePath);
        }

        public ConfigurationSection this[string sectName] 
        {
            get { return config[sectName]; }
        }
    }
}
