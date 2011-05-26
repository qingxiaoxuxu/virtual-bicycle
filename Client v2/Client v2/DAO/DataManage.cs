using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MySQLDriverCS;

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
    }
}
