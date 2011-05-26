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
        MySqlConnector con = new MySqlConnector();

        public void AddExerciseInfo(string userId, DateTime exerciseDate, int during, string energy, string heartRate)
        {
            string command = "INSERT INTO vb_exercisemessage_user (userid, exercisedate, duringtime, energy, heartrate)"
                + "VALUES ('" + userId 
                + "', '" + exerciseDate.ToString() 
                + "', '" + during.ToString() 
                + "', '" + energy 
                + "', '" + heartRate + "')";
            con.ExecuteUpdate(command);
        }

        public string findUser(string userName, string pw)
        {
            string res = null;
            string command = "SELECT userid FROM vb_usermessage_user WHERE username = '" + userName + "' AND password = '" + pw + "'";
            DataSet ds = con.ExecuteQuery(command);
            if (ds.Tables[0].Rows.Count != 0)
                res = ds.Tables[0].Rows[0][0].ToString();
            return res;
        }
    }
}
