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
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using Apoc3D.Core;
using VBC = Apoc3D.Core;

namespace Apoc3D.Vfs
{
    public sealed class FileNameTokens : Singleton
    {
        class Comparer : IEqualityComparer<string>
        {
            public bool IsCaseSensitive
            {
                get;
                set;
            }

            #region IEqualityComparer<string> 成员

            public bool Equals(string x, string y)
            {
                return string.Compare(x, y, StringComparison.OrdinalIgnoreCase) == 0;
            }

            public int GetHashCode(string obj)
            {
                return VBC.Resource.GetHashCode(obj);
            }

            #endregion
        }

        Dictionary<string, string> replaceTable;
        Comparer comparer;

        static FileNameTokens singleton;

        public static FileNameTokens Instance
        {
            get
            {
                if (singleton == null)
                {
                    singleton = new FileNameTokens();
                }
                return singleton;
            }
        }

        private FileNameTokens()
        {
            comparer = new Comparer();
            replaceTable = new Dictionary<string, string>(comparer);
        }

        public void RegisterToken(string token, string replacement)
        {
            replaceTable.Add(token, replacement);
        }

        public void UnregisterToken(string token)
        {
            replaceTable.Remove(token);
        }

        protected override void dispose()
        {
            replaceTable.Clear();
            replaceTable = null;
        }

        public bool IsCaseSensitive
        {
            get { return comparer.IsCaseSensitive; }
            set { comparer.IsCaseSensitive = value; }
        }

        public string Filter(string path)
        {
            path = path.Trim();
            foreach (KeyValuePair<string, string> e in replaceTable)
            {
                path = path.Replace(e.Key, e.Value);
            }
            return path;
        }
    }


    /// <summary>
    ///  文件系统。用来定位，查找文件。
    /// </summary>
    public sealed class FileSystem : Singleton
    {
        public const string dotDll = ".dll";

        static FileSystem singleton;

        /// <summary>
        ///  获取文件系统的唯一实例
        /// </summary>
        public static FileSystem Instance
        {
            get
            {
                if (singleton == null)
                {
                    singleton = new FileSystem();
                }
                return singleton;
            }
        }
        static readonly char[] DirSepCharArray = new char[] { Path.DirectorySeparatorChar };

        Dictionary<string, Archive> stdPack;
        Dictionary<string, ArchiveFactory> factories;


        List<string> workingDirs;

        public void AddWorkingDir(string path)
        {
            path = Path.GetFullPath(path);
            if (!path.EndsWith(Path.DirectorySeparatorChar.ToString()))
                path += Path.DirectorySeparatorChar;
            workingDirs.Add(path);

        }


        private FileSystem()
        {
            factories = new Dictionary<string, ArchiveFactory>(CaseInsensitiveStringComparer.Instance);
            stdPack = new Dictionary<string, Archive>(50, CaseInsensitiveStringComparer.Instance);
            CurrentArchiveSet = new List<Archive>();
            workingDirs = new List<string>();

            RegisterArchiveType(new LpkArchiveFactory());
        }

        public void RegisterArchiveType(ArchiveFactory fac)
        {
            factories.Add(fac.Type, fac);
        }
        public bool UnregisterArchiveType(ArchiveFactory fac)
        {
            return factories.Remove(fac.Type);
        }
        public bool UnregisterArchiveType(string type)
        {
            return factories.Remove(type);
        }

        public DirectoryInfo[] GetWorkingDirectories()
        {
            DirectoryInfo[] res = new DirectoryInfo[workingDirs.Count];
            for (int i = 0; i < workingDirs.Count; i++)
            {
                res[i] = new DirectoryInfo(workingDirs[i]);
            }
            return res;
        }
        public List<Archive> CurrentArchiveSet
        {
            get;
            private set;
        }



        public static string CombinePath(string path1, string path2)
        {
            if ((path1 == null) || (path2 == null))
            {
                throw new ArgumentNullException((path1 == null) ? "path1" : "path2");
            }
            if (path2.Length == 0)
            {
                return path1;
            }
            if (path1.Length == 0)
            {
                return path2;
            }
            char ch = path1[path1.Length - 1];
            if (((ch != Path.DirectorySeparatorChar) && (ch != Path.AltDirectorySeparatorChar)) && (ch != Path.VolumeSeparatorChar))
            {
                return (path1 + Path.DirectorySeparatorChar + path2);
            }
            return (path1 + path2);
        }
        public string GetPath(string fullPath)
        {
            fullPath = Path.GetDirectoryName(fullPath).ToUpper() + Path.DirectorySeparatorChar;

            for (int i = 0; i < workingDirs.Count; i++)
            {
                int pos = fullPath.IndexOf(workingDirs[i].ToUpper());
                if (pos != -1)
                {
                    return fullPath.Substring(pos + workingDirs[i].Length);
                }
            }
            return fullPath;
        }
        public static string GetArchivePath(string path)
        {
            path = path.ToUpper();

            string root = Path.GetPathRoot(path).ToUpper();
            while (root != path)
            {
                path = Path.GetDirectoryName(path);
                if (File.Exists(path))
                {
                    return path;
                }
            }
            return path;
        }
        public string[] SearchFile(string path)
        {
            List<string[]> matches = new List<string[]>(workingDirs.Count);
            int count = 0;
            for (int i = 0; i < workingDirs.Count; i++)
            {
                string dirName = CombinePath(workingDirs[i], path);

                int pos = dirName.LastIndexOf(Path.DirectorySeparatorChar);

                string filter = path;
                if (pos != -1)
                {
                    filter = dirName.Substring(pos + 1);

                    dirName = dirName.Substring(0, pos + 1);
                }

                if (Directory.Exists(dirName))
                {
                    string[] sm = Directory.GetFiles(dirName, filter);
                    count += sm.Length;
                    matches.Add(sm);
                }
            }

            string[] allMatches = new string[count];

            int idx = 0;
            for (int i = 0; i < matches.Count; i++)
            {
                Array.Copy(matches[i], 0, allMatches, idx, matches[i].Length);
                idx += matches[i].Length;
            }
            return allMatches;
        }

        bool IsOpened(string filePath, out Archive entry)
        {
            return stdPack.TryGetValue(filePath, out entry);
        }

        Archive CreateArchive(FileLocation fl)
        {
            string ext = Path.GetExtension(fl.Path);
            ArchiveFactory fac;

            if (factories.TryGetValue(ext, out fac))
            {
                return fac.CreateInstance(fl);
            }
            else
                throw new NotSupportedException();
        }
        Archive CreateArchive(string file)
        {
            string ext = Path.GetExtension(file);
            ArchiveFactory fac;

            if (factories.TryGetValue(ext, out fac))
            {
                return fac.CreateInstance(file);
            }
            else
                throw new NotSupportedException();
        }

        public bool Exists(string path)
        {
            for (int i = 0; i < workingDirs.Count; i++)
            {
                string fullPath = CombinePath(workingDirs[i], path);
                if (Directory.Exists(fullPath))
                {
                    return true;
                }

                string root = Path.GetPathRoot(fullPath).ToUpper();
                while (!CaseInsensitiveStringComparer.Compare(root, fullPath))
                {
                    fullPath = Path.GetDirectoryName(fullPath);
                    if (File.Exists(fullPath))
                    {
                        return true;
                    }
                }

            }
            return false;
        }
        public bool Exists(string path, out string result)
        {
            for (int i = 0; i < workingDirs.Count; i++)
            {
                string fullPath = CombinePath(workingDirs[i], path);
                if (Directory.Exists(fullPath))
                {
                    result = fullPath;
                    return true;
                }

                string root = Path.GetPathRoot(fullPath).ToUpper();
                while (!CaseInsensitiveStringComparer.Compare(root, fullPath))
                {
                    fullPath = Path.GetDirectoryName(fullPath);
                    if (File.Exists(fullPath))
                    {
                        result = CombinePath(workingDirs[i], path);
                        return true;
                    }
                }

            }
            result = null;
            return false;
        }
        public bool Exists(string path, out List<string> result, out List<string> archivePath)
        {
            result = new List<string>();
            archivePath = new List<string>();

            for (int i = 0; i < workingDirs.Count; i++)
            {
                string fullPath = CombinePath(workingDirs[i], path);
                if (Directory.Exists(fullPath))
                {
                    //archivePath = null;
                    //result = fullPath;
                    //return true;
                    result.Add(fullPath);
                    archivePath.Add(null);

                }

                string root = Path.GetPathRoot(fullPath).ToUpper();
                while (!CaseInsensitiveStringComparer.Compare(root, fullPath))
                {
                    if (File.Exists(fullPath))
                    {
                        result.Add(CombinePath(workingDirs[i], path));
                        archivePath.Add(fullPath);
                        return true;
                    }
                    fullPath = Path.GetDirectoryName(fullPath);

                }

            }
            return result.Count > 0;
        }

        /// <summary>
        ///  定位文件包的位置
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="rule"></param>
        /// <returns></returns>
        public Archive LocateArchive(string filePath, FileLocateRule rule)
        {
            Archive res;
            if (IsOpened(filePath, out res))
            {
                return res;
            }
            FileLocation fl = Locate(filePath, rule);
            res = CreateArchive(fl);
            stdPack.Add(res.FilePath, res);
            return res;
        }

        /// <summary>
        ///  使用规则定位一个文件
        /// </summary>
        /// <param name="filePath">文件路径</param>
        /// <param name="rule">文件定位规则</param>
        /// <returns>一个<see cref="FileLocation"/>，表示文件的位置</returns>
        /// <exception cref="FileNotFoundException">当文件没有找到时，引发此异常</exception>
        public FileLocation Locate(string filePath, FileLocateRule rule)
        {
            FileLocation res = TryLocate(filePath, rule);
            if (res == null)
                 throw new FileNotFoundException(filePath);
            return res;
        }
        /// <summary>
        ///  从可能的文件路径，定位文件
        /// </summary>
        /// <param name="filePath">一个String数组，包含所有可能的路径</param>
        /// <param name="defPath">默认路径</param>
        /// <param name="rule">文件定位规则</param>
        /// <returns>一个<see cref="FileLocation"/>，表示文件的位置</returns>
        /// <exception cref="FileNotFoundException">当文件没有找到时，引发此异常</exception>
        public FileLocation Locate(string[] filePath, string defPath, FileLocateRule rule)
        {
            StringBuilder sb = new StringBuilder();

            for (int i = 0; i < filePath.Length; i++)
            {
                FileLocation res = TryLocate(CombinePath(defPath, filePath[i]), rule);
                if (res != null)
                    return res;
                sb.AppendLine(filePath[i]);
            }
            throw new FileNotFoundException(sb.ToString());
        }
        /// <summary>
        ///  从可能的文件路径，定位文件
        /// </summary>
        /// <param name="filePath">一个String数组，包含所有可能的路径</param>
        /// <param name="rule">文件定位规则</param>
        /// <returns>一个<see cref="FileLocation"/>，表示文件的位置</returns>
        /// <exception cref="FileNotFoundException">当文件没有找到时，引发此异常</exception>
        public FileLocation Locate(string[] filePath, FileLocateRule rule)
        {
            StringBuilder sb = new StringBuilder();

            for (int i = 0; i < filePath.Length; i++)
            {
                FileLocation res = TryLocate(filePath[i], rule);
                if (res != null)
                    return res;

                sb.AppendLine(filePath[i]);
            }
            throw new FileNotFoundException(sb.ToString());
        }

        /// <summary>
        ///  从可能的文件路径，尝试定位文件，找不到文件时，不会引发异常
        /// </summary>
        /// <param name="filePath">一个String数组，包含所有可能的路径</param>
        /// <param name="rule">文件定位规则</param>
        /// <returns>一个<see cref="FileLocation"/>，表示文件的位置，若没有找到，则返回null</returns>
        public FileLocation TryLocate(string[] filePath, FileLocateRule rule)
        {
            StringBuilder sb = new StringBuilder();

            for (int i = 0; i < filePath.Length; i++)
            {
                FileLocation res = TryLocate(filePath[i], rule);
                if (res != null)
                    return res;

                sb.AppendLine(filePath[i]);
            }
            return null;
        }

        public FileLocation Locate(string filePath, string[] paths, FileLocateRule rule)
        {
            for (int i = 0; i < paths.Length; i++)
            {
                FileLocation res = TryLocate(CombinePath(paths[i], filePath), rule);
                if (res != null)
                    return res;
            }
            throw new FileNotFoundException(filePath);
        }

        public FileLocation TryLocate(string filePath, string[] paths, FileLocateRule rule)
        {
            for (int i = 0; i < paths.Length; i++)
            {
                FileLocation res = TryLocate(CombinePath(paths[i], filePath), rule);
                if (res != null)
                    return res;
            }
            return null;
        }

        /// <summary>
        ///  尝试定位文件，找不到文件时，不会引发异常
        /// </summary>
        /// <param name="filePath">文件路径</param>
        /// <param name="rule">文件定位规则</param>
        /// <returns>如果找到文件返回一个<see cref="FileLocation"/>对象，否则返回null</returns>
        public FileLocation TryLocate(string filePath, FileLocateRule rule)
        {
            // 遍历规则的所有检查点
            for (int cp = 0; cp < rule.Count; cp++)
            {
                LocateCheckPoint checkPt = rule[cp];

                for (int j = 0; j < checkPt.Count; j++)
                {
                    // 如果检查点要求搜索CurrectArchiveSet
                    if (checkPt.SearchesCurrectArchiveSet)
                    {
                        for (int i = 0; i < CurrentArchiveSet.Count; i++)
                        {
                            Stream entStm = CurrentArchiveSet[i].GetEntryStream(filePath);

                            if (entStm != null)
                                return new FileLocation(CurrentArchiveSet[i], CurrentArchiveSet[i].FilePath + Path.DirectorySeparatorChar + filePath, entStm);
                        }
                    }

                    
                    if (!checkPt.HasArchivePath(j))
                    {
                        string fullPath = CombinePath(checkPt.GetPath(j), filePath);
                        if (File.Exists(fullPath))
                        {
                            return new FileLocation(fullPath);
                        }
                    }
                    else
                    {
                        string arcPath = checkPt.GetArchivePath(j);

                        try
                        {
#if !XBOX
                            string[] locs = filePath.Split(DirSepCharArray, StringSplitOptions.RemoveEmptyEntries);
#else
                            string[] locs = filePath.Split(DirSepCharArray);
                            List<string> locs2 = new List<string>(locs.Length);
                            for (int i = 0; i < locs.Length; i++) 
                            {
                                if (locs[i].Length > 0) 
                                {
                                    locs2.Add(locs[i]);
                                }
                            }
                            locs = locs2.ToArray();
#endif


                            if (locs.Length > 1)
                            {
                                StringBuilder sb = new StringBuilder();

                                Archive entry = null;
                                Archive last;

                                bool found = true;

                                for (int i = 0; i < locs.Length - 1; i++)
                                {
                                    if (i > 0)
                                        sb.Append(Path.DirectorySeparatorChar + locs[i]);
                                    else
                                        sb.Append(locs[i]);

                                    last = entry;


                                    // 如果当前的资源包未打开过
                                    if (!IsOpened(sb.ToString(), out entry))
                                    {
                                        // 如果在资源包中
                                        if (last != null)
                                        {
                                            Stream entStm = last.GetEntryStream(locs[i]);

                                            if (entStm != null)
                                            {
                                                entry = CreateArchive(new FileLocation(last, CombinePath(arcPath, sb.ToString()), entStm));
                                                stdPack.Add(entry.FilePath, entry);
                                            }
                                            else
                                            {
                                                found = false;
                                                break;
                                            }
                                        }
                                        else
                                        {
                                            string arc = sb.ToString();
                                            if (File.Exists(arc))
                                            {
                                                entry = CreateArchive(arc);
                                                stdPack.Add(entry.FilePath, entry);
                                            }
                                            else
                                            {
                                                found = false;
                                                break;
                                            }
                                        }
                                    }
                                }
                                if (found && entry != null)
                                {
                                    Stream entStm = entry.GetEntryStream(locs[locs.Length - 1]);

                                    if (entStm != null)
                                        return new FileLocation(entry, CombinePath(arcPath, filePath), entStm);
                                }
                            }
                            else
                            {
                                Archive entry;
                                if (!IsOpened(arcPath, out entry))
                                {
                                    if (File.Exists(arcPath))
                                    {
                                        entry = CreateArchive(arcPath);
                                        stdPack.Add(entry.FilePath, entry);
                                    }
                                }
                                if (entry != null)
                                {
                                    Stream entStm = entry.GetEntryStream(locs[0]);

                                    if (entStm != null)
                                        return new FileLocation(entry, CombinePath(arcPath, filePath), entStm);
                                }
                            }
                        }
                        catch (InvalidFormatException)
                        {
                            EngineConsole.Instance.Write("文件格式不对", ConsoleMessageType.Warning);
                        }
                    } // if archive

                }

            }
            return null;
        }

        protected override void dispose()
        {
            Dictionary<string, Archive>.ValueCollection vals = stdPack.Values;
            foreach (Archive arc in vals)
            {
                arc.Dispose();
            }
        }
    }
}
