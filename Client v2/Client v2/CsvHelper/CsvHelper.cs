using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace VbCsvHelper
{
    public class CsvHelper
    {
        //获取原始数据
        public List<Object> getOriginData(int time, double speed, double heartBeat, double distance, double energy)
        {
            List<Object> res = new List<object>();
            res.Add(time);
            res.Add(speed);
            res.Add(heartBeat);
            res.Add(distance);
            res.Add(energy);
            return res;
        }

        //生成csv格式的数据
        private string genCsvData(List<Object> data)
        {
            string res = "";
            int i, j, state;
            for (i = 0; i < data.Count; i++)
            {
                Object ob = data[i];
                string tmp = ob.ToString();
                string val = "";
                for (j = 0, state = 0; j < tmp.Length; j++)
                {
                    if (tmp[j] == ',' || tmp[j] == '\"')
                    {
                        state = 1;
                        if (tmp[j] == '\"')
                            val += '\"';
                    }
                    val += tmp[j];
                }
                if (state == 1)
                    res += ('\"' + val + '\"');
                else
                    res += val;
                if (i != data.Count - 1)
                    res += ',';
            }
            return res;
        }

        //将数据写入csv文件
        public void writeDataInCsv(List<Object> data, string path)
        {
            StreamWriter sw = new StreamWriter(path, true);
            string appendData = genCsvData(data);
            sw.WriteLine(appendData);
            sw.Close();
        }

        //批量导入文件
        public void writeManyDataInCsv(List<List<Object>> data, string path)
        {
            string appendData;
            StreamWriter sw = new StreamWriter(path, true);
            foreach (List<Object> d in data)
            {
                appendData = genCsvData(d);
                sw.WriteLine(appendData);
            }
            sw.Close();
        }

    }
}
