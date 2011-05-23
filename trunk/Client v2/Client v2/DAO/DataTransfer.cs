using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MySQLDriverCS;

namespace Client_v2.DAO
{
    class DataTransfer
    {
        MySqlConnector con = new MySqlConnector();

        private MySQLParameter CreateParameter(string parameterName, System.Data.DbType dbType,
            int size, object val, System.Data.ParameterDirection parameterDirection)
        {
            MySQLParameter res = new MySQLParameter();
            res.ParameterName = parameterName;
            res.DbType = dbType;
            res.Size = size;
            res.Value = val;
            res.Direction = parameterDirection;
            return res;
        }

        public void AddExerciseInfo()
        {
            MySQLParameter[] dp = new MySQLParameter[5];
        }
    }
}
