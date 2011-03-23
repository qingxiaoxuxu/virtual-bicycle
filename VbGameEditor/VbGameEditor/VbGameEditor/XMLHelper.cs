using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.IO;
using System.Windows.Media.Media3D;

namespace VbGameEditor
{
    public class XMLHelper
    {
        string fileName;
        XmlDocument xd = new XmlDocument();

        public XMLHelper(string path,bool isClear)
        {
            fileName = path;
            if (fileName != "")
            {

                XmlElement xmlelem;
                XmlNode xmlnode;
                if (File.Exists(fileName))
                {
                    try
                    {
                        xd.Load(fileName);
                        if (isClear)
                        {
                            xd.RemoveAll();
                            //加入xml声明
                            xmlnode = xd.CreateNode(XmlNodeType.XmlDeclaration, "", "");
                            xd.AppendChild(xmlnode);
                            //加入一个根元素
                            xmlelem = xd.CreateElement("", "ScenePoint", "");
                            xd.AppendChild(xmlelem);
                        }
                    }
                    catch
                    {
                        xd.RemoveAll();
                        //加入xml声明
                        xmlnode = xd.CreateNode(XmlNodeType.XmlDeclaration, "", "");
                        xd.AppendChild(xmlnode);
                        //加入一个根元素
                        xmlelem = xd.CreateElement("", "ScenePoint", "");
                        xd.AppendChild(xmlelem);
                    }
                }
                else
                {
                    //加入xml声明
                    xmlnode = xd.CreateNode(XmlNodeType.XmlDeclaration, "", "");
                    xd.AppendChild(xmlnode);
                    //加入一个根元素
                    xmlelem = xd.CreateElement("", "ScenePoint", "");
                    xd.AppendChild(xmlelem);
                }
            }
        }
        public void Add(ScenePoint sp)
        {
            XmlNode xn;
            xn = xd.ChildNodes.Item(1);
            XmlElement xmlelem;
            xmlelem = xd.CreateElement(sp.Category.ToString());
            xmlelem.AppendChild(xd.CreateTextNode(sp.position.ToString()));
            xn.AppendChild(xmlelem);
            xd.Save(fileName);
        }
        public void Get()
        {
            foreach(XmlElement xe in xd.ChildNodes.Item(1).ChildNodes)
            {
               new ScenePoint(ToPointCategory(xe.Name.ToString()),Point3D.Parse(xe.ChildNodes.Item(0).InnerText)); 
            }
        }
        public static PointCategory ToPointCategory( string s)
        {
            switch (s)
            {
                case "Road":
                    return PointCategory.Road;
                case "Tree":
                    return PointCategory.Tree;
                case "House":
                    return PointCategory.House;
                case "People":
                    return PointCategory.People;
                case "Car":
                    return PointCategory.Car;
                default:
                    return PointCategory.Road;
            }
        }
    }
}
