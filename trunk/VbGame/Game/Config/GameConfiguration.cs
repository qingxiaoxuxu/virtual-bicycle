/*
-----------------------------------------------------------------------------
This source file is part of Zonelink

Copyright (c) 2009+ Tao Games

This program is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

This program is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with this program.  if not, write to the Free Software Foundation, 
Inc., 59 Temple Place - Suite 330, Boston, MA 02111-1307, USA, or go to
http://www.gnu.org/copyleft/gpl.txt.

-----------------------------------------------------------------------------
*/
using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using Apoc3D.Config;
using System.IO;
namespace Code2015.EngineEx
{

    class GameConfiguration : Configuration
    {
        public static bool MoveToNextElement(XmlTextReader xmlIn)
        {
            if (!xmlIn.Read())
                return false;

            while (xmlIn.NodeType == XmlNodeType.EndElement)
            {
                if (!xmlIn.Read())
                    return false;
            }

            return true;
        }

        public GameConfiguration(string file)
            : base(file, EqualityComparer<string>.Default)
        {
            XmlTextReader xml = new XmlTextReader(File.Open(file, FileMode.Open, FileAccess.Read));
            xml.WhitespaceHandling = WhitespaceHandling.None;

            int depth = xml.Depth;

            GameConfigurationSection currentSection = null;

            string currentAttrib = string.Empty;

            while (MoveToNextElement(xml))
            {
                switch (xml.NodeType)
                {
                    case XmlNodeType.Element:
                    case XmlNodeType.Text:
                    case XmlNodeType.CDATA:
                        switch (xml.Depth)
                        {
                            case 1:
                                currentSection = new GameConfigurationSection(xml.Name);
                                Add(xml.Name, currentSection);
                                break;
                            case 2:
                                currentAttrib = xml.Name;
                                break;
                            case 3:
                                currentSection.Add(currentAttrib, xml.ReadString());
                                break;
                        }
                        break;
                }
            }

            xml.Close();
        }

        public GameConfiguration(string name, int cap)
            : base(name, cap, EqualityComparer<string>.Default)
        { }


        /// <summary>
        ///  见基类
        /// </summary>
        public override void Merge(Configuration config)
        {
            Configuration copy = config.Clone();

            foreach (KeyValuePair<string, ConfigurationSection> e1 in copy)
            {
                ConfigurationSection sect;
                if (!TryGetValue(e1.Key, out sect))
                {
                    Add(e1.Key, e1.Value);
                }
                else
                {
                    foreach (KeyValuePair<string, string> e2 in e1.Value)
                    {
                        if (sect.ContainsKey(e2.Key))
                        {
                            sect.Remove(e2.Key);
                        }

                        sect.Add(e2.Key, e2.Value);

                    }
                }
            }
        }

        /// <summary>
        ///  见基类
        /// </summary>
        public override Configuration Clone()
        {
            GameConfiguration ini = new GameConfiguration(base.Name, this.Count);

            foreach (KeyValuePair<string, ConfigurationSection> e1 in this)
            {
                //Dictionary<string, string> newSectData = new Dictionary<string, string>(e1.Value.Count);
                GameConfigurationSection newSect = new GameConfigurationSection(ini, e1.Key, e1.Value.Count);

                foreach (KeyValuePair<string, string> e2 in e1.Value)
                {
                    newSect.Add(e2.Key, e2.Value);
                }

                ini.Add(e1.Key, newSect);
            }

            return ini;
        }
    }
}
