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

namespace Apoc3D.Config
{
    /// <summary>
    ///  提供配置的抽象接口
    /// </summary>
    public abstract class Configuration : Dictionary<string, ConfigurationSection>
    {
        string name;

        protected Configuration(string name, IEqualityComparer<string> comparer)
            : base(comparer)
        {
            this.name = name;
        }


        protected Configuration(string name, int cap, IEqualityComparer<string> comparer)
            : base(cap, comparer)
        {
            this.name = name;
        }

        public string Name
        {
            get { return name; }
        }

        /// <summary>
        ///  复制该配置，创建令一个相同的。其中的Section也会被复制，不会引用。
        /// </summary>
        /// <returns></returns>
        public abstract Configuration Clone();

        /// <summary>
        ///  将该配置与另一个配置合并
        /// </summary>
        /// <param name="config">另一个配置</param>
        public abstract void Merge(Configuration config);
    }
}
