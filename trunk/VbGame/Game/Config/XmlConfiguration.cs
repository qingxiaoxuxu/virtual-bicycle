/*
-----------------------------------------------------------------------------
This source file is part of Apoc3D Engine

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
using Apoc3D.Vfs;


namespace Apoc3D.Config
{
    /// <summary>
    ///  表示一个XML配置文件
    /// </summary>
    public class XmlConfiguration : Configuration
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

        public XmlConfiguration(string file)
            : this(new FileLocation(file))
        { }

        public XmlConfiguration(ResourceLocation file) :
            base(file.Name, CaseInsensitiveStringComparer.Instance)
        {

            XmlTextReader xml = new XmlTextReader(file.GetStream);

            //XmlSection parentSection = null;
            int depth = xml.Depth;
            Stack<XmlSection> stack = new Stack<XmlSection>();
            stack.Push(null);

            string name = string.Empty;
            while (MoveToNextElement(xml))
            {
                if (xml.Depth > depth)
                {
                    //string name = string.Empty;
                    //if (stack.Peek() != null)
                    //    name = stack.Peek().Name;
                    XmlSection sect = new XmlSection(this, name, stack.Peek());
                    sect.Value = xml.Value;

                    if (sect.ParentSection == null)
                        Add(sect.Name, sect);

                    stack.Push(sect);
                    depth = xml.Depth;
                }
                else if (xml.Depth == depth)
                {
                    if (stack.Peek() != null)
                    {
                        name = xml.Name;

                        stack.Peek().Add(xml.Name, xml.Value);
                    }
                }
                else
                {
                    stack.Pop();
                    depth = xml.Depth;
                }                
            }

            xml.Close();
        }

        public XmlConfiguration(string name, int cap)
            : base(name, cap, CaseInsensitiveStringComparer.Instance)
        { }

        public override Configuration Clone()
        {
            XmlConfiguration xml = new XmlConfiguration(Name, Count);

            foreach (KeyValuePair<string, ConfigurationSection> e1 in this)
            {
                //Dictionary<string, string> newSectData = new Dictionary<string, string>(e1.Value.Count);
                XmlSection source = (XmlSection)e1.Value;

                XmlSection newSect = new XmlSection(xml, e1.Key, source.Count, source.ParentSection);

                foreach (KeyValuePair<string, string> e2 in source)
                {
                    newSect.Add(e2.Key, e2.Value);
                }

                xml.Add(e1.Key, newSect);
            }
            return xml;
        }

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
    }

    /// <summary>
    ///  表示XML配置文件的格式
    /// </summary>
    public class XmlConfigurationFormat : ConfigurationFormat
    {
        /// <summary>
        ///  扩展名过滤器
        /// </summary>
        public override string[] Filters
        {
            get { return new string[] { ".xml" }; }
        }

        /// <summary>
        ///  从资源中读取xml配置
        /// </summary>
        /// <param name="rl">表示资源的位置的<see cref="ResourceLocation"/></param>
        /// <returns>一个<see cref="Configuration"/>，表示创建好的配置</returns>
        public override Configuration Load(ResourceLocation rl)
        {
            return new XmlConfiguration(rl);
        }
    }
}
