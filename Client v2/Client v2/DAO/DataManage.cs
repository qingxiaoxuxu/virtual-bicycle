using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MySQLDriverCS;
using System.Data;

namespace Client_v2.DAO
{
    class DataManage
    {
        static MySqlConnector con = new MySqlConnector();

        public static void SetCharName()
        {
            string command = "set names 'gb2312'";
            con.ExecuteUpdate(command);
        }

        public static void AddExerciseInfo(string userId, DateTime exerciseDate, int during, string energy, string heartRate)
        {
            string command = "INSERT INTO vb_exercisemessage_user (userid, exercisedate, duringtime, energy, heartrate)"
                + "VALUES ('" + userId 
                + "', '" + exerciseDate.ToString() 
                + "', '" + during.ToString() 
                + "', '" + energy 
                + "', '" + heartRate + "')";
            Console.WriteLine(command);
            con.ExecuteUpdate(command);
        }

        public static string findUser(string userId, string pw)
        {
            string res = null;
            //Encoding utf8 = Encoding.UTF8;
            //Encoding gb2312 = Encoding.GetEncoding("gb2312");
            String command = "SELECT username FROM vb_usermessage_user WHERE userid = '" + userId + "' AND password = MD5('" + pw + "')";
            //byte[] tmp = utf8.GetBytes(command);
            //byte[] tmp1 = Encoding.Convert(utf8, gb2312, tmp);
            //command = gb2312.GetString(tmp1);


            //Encoding utf8 = Encoding.GetEncoding(65001);
            //Encoding gb2312 = Encoding.GetEncoding("gb2312");
            //byte[] temp = gb2312.GetBytes(command);
            //byte[] temp1 = Encoding.Convert(gb2312, utf8, temp);
            //command = utf8.GetString(temp1);


            //String str = new String(
            //command = new String(command.getbytes
            Console.WriteLine(command);
            DataSet ds = con.ExecuteQuery(command);
            if (ds.Tables[0].Rows.Count != 0)
                res = ds.Tables[0].Rows[0][0].ToString();
            return res;
        }
    }
}
