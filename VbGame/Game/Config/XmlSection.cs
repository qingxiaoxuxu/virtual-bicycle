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
using Apoc3D.MathLib;

namespace Apoc3D.Config
{
    /// <summary>
    ///  表示Xml配置文件中的节点
    /// </summary>
    public class XmlSection : ConfigurationSection
    {
        //List<XmlSection> child = new List<XmlSection>();
        Dictionary<string, XmlSection> children = new Dictionary<string, XmlSection>(CaseInsensitiveStringComparer.Instance);

        public XmlSection(XmlConfiguration config, string name, XmlSection parent)
            : base(config, name)
        {
            ParentSection = parent;
            if (parent != null)
                parent.children.Add(this.Name, this);
        }
        public XmlSection(XmlConfiguration config, string name, int capacity, XmlSection parent)
            : base(config, name, capacity)
        {
            ParentSection = parent;
            if (parent != null)
                parent.children.Add(this.Name, this);
        }
        public XmlSection(XmlConfiguration config, string name, IDictionary<string, string> dictionary, XmlSection parent)
            : base(config, name, dictionary)
        {
            ParentSection = parent;
            if (parent != null)
                parent.children.Add(this.Name, this);
        }


        public XmlSection ParentSection
        {
            get;
            protected set;
        }

        public override ConfigurationSection GetSubSection(string key)
        {
            return children[key];
        }

        public override bool TryGetSubSection(string key, out ConfigurationSection sect)
        {
            XmlSection xmlSect;
            bool result = children.TryGetValue(key, out xmlSect);

            sect = xmlSect;
            return result;
        }


        public override bool TryGetPaths(string key, out string[] res)
        {
            throw new NotImplementedException();
        }

        public override string[] GetPaths(string key)
        {
            throw new NotImplementedException();
        }

        public override void GetRectangle(string key, out Rectangle rect)
        {
            throw new NotImplementedException();
        }

        public override bool TryGetColorRGBA(string key, out ColorValue clr)
        {
            throw new NotImplementedException();
        }

        public override ColorValue GetColorRGBA(string key, ColorValue def)
        {
            throw new NotImplementedException();
        }

        public override ColorValue GetColorRGBA(string key)
        {
            throw new NotImplementedException();
        }

        public override int GetColorRGBInt(string key)
        {
            throw new NotImplementedException();
        }

        public override int GetColorRGBInt(string key, int def)
        {
            throw new NotImplementedException();
        }

        public override bool TryGetBool(string key, out bool res)
        {
            throw new NotImplementedException();
        }

        public override bool GetBool(string key)
        {
            throw new NotImplementedException();
        }

        public override bool GetBool(string key, bool def)
        {
            throw new NotImplementedException();
        }

        public override float GetSingle(string key)
        {
            throw new NotImplementedException();
        }

        public override float GetSingle(string key, float def)
        {
            throw new NotImplementedException();
        }

        public override float[] GetSingleArray(string key)
        {
            throw new NotImplementedException();
        }

        public override float[] GetSingleArray(string key, float[] def)
        {
            throw new NotImplementedException();
        }



        public override string GetString(string key, string def)
        {
            throw new NotImplementedException();
        }

        public override string[] GetStringArray(string key)
        {
            throw new NotImplementedException();
        }

        public override string[] GetStringArray(string key, string[] def)
        {
            throw new NotImplementedException();
        }

        public override int GetInt(string key)
        {
            throw new NotImplementedException();
        }

        public override int GetInt(string key, int def)
        {
            throw new NotImplementedException();
        }

        public override int[] GetIntArray(string key)
        {
            throw new NotImplementedException();
        }

        public override int[] GetIntArray(string key, int[] def)
        {
            throw new NotImplementedException();
        }

        public override Size GetSize(string key)
        {
            throw new NotImplementedException();
        }

        public override Size GetSize(string key, Size def)
        {
            throw new NotImplementedException();
        }

        public override Point GetPoint(string key)
        {
            throw new NotImplementedException();
        }

        public override Point GetPoint(string key, Point def)
        {
            throw new NotImplementedException();
        }

        public override float GetPercentage(string key)
        {
            throw new NotImplementedException();
        }

        public override float GetPercentage(string key, float def)
        {
            throw new NotImplementedException();
        }

        public override float[] GetPercetageArray(string key)
        {
            throw new NotImplementedException();
        }
        public override string GetUIString(string key)
        {
            throw new NotImplementedException();
        }
        public override string GetUIString(string key, string def)
        {
            throw new NotImplementedException();
        }
    }
}
