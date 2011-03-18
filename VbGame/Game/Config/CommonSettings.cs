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
        static GameConfiguration config;

        static CommonSettings()
        {
            string filePath = Path.Combine(StorageContainer.TitleLocation, @"Config\common.xml");
            config = new GameConfiguration(filePath);
        }

        public static ConfigurationSection this[string sectName] 
        {
            get { return config[sectName]; }
        }
    }
}
