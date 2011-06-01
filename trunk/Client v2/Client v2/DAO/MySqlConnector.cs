using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MySQLDriverCS;
using System.Data;

namespace Client_v2.DAO
{
    public class MySqlConnector
    {
        MySQLConnection conn;
        MySQLCommand command;
        DataSet ds;

        public MySqlConnector()
        {
            conn = new MySQLConnection(
                new MySQLConnectionString("192.168.137.124", "vb_databases", "root", "123456").AsString
                );
        }

        private void Connect()
        {   
            try { conn.Open(); }
            catch (Exception ex) { throw ex; }
        }

        private void Terminate()
        {
            try { conn.Close(); }
            catch (Exception ex) { throw ex; }
        }

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

        private MySQLCommand CreateProcedureCommand(string procedureName, MySQLParameter[] dp)
        {
            MySQLCommand cmd = new MySQLCommand();
            cmd.Connection = conn;
            cmd.CommandType = System.Data.CommandType.StoredProcedure;
            cmd.Parameters.AddRange(dp);
            return cmd;
        }

        private MySQLCommand CreateTextCommand(string commandText)
        {
            MySQLCommand cmd = new MySQLCommand(commandText, conn);
            return cmd;
        }

        /// <summary>
        /// MySQL查询调用
        /// </summary>
        /// <param name="procedureName">存储过程名称</param>
        /// <param name="dp">存储过程参数</param>
        /// <returns>查询结果数据集合</returns>
        public DataSet ExecuteQuery(string procedureName, MySQLParameter[] dp)
        {
            Connect();
            MySQLCommand cmd = CreateProcedureCommand(procedureName, dp);
            //MySQLDataReader mdr = cmd.ExecuteReaderEx();
            MySQLDataAdapter mda = new MySQLDataAdapter();
            mda.SelectCommand = cmd;
            try { mda.Fill(ds); }
            catch (Exception ex) { throw ex; }
            finally { Terminate(); }
            return ds;
        }

        /// <summary>
        /// MySql查询调用
        /// </summary>
        /// <param name="cmdText">查询字符串</param>
        /// <returns>查询数据集合</returns>
        public DataSet ExecuteQuery(string cmdText)
        {
            Connect();
            ds = new DataSet();
            //MySQLCommand cmd = CreateProcedureCommand(procedureName, dp);
            //MySQLDataReader mdr = cmd.ExecuteReaderEx();
            MySQLCommand cmd = CreateTextCommand(cmdText);
            MySQLDataAdapter mda = new MySQLDataAdapter();
            mda.SelectCommand = cmd;
            try { mda.Fill(ds); }
            catch (Exception ex) { throw ex; }
            finally { Terminate(); }
            return ds;
        }

        /// <summary>
        /// MySql更改数据调用
        /// </summary>
        /// <param name="procedureName">存储过程名称</param>
        /// <param name="dp">存储过程参数</param>
        public void ExecuteUpdate(string procedureName, MySQLParameter[] dp)
        {
            Connect();
            MySQLCommand cmd = CreateProcedureCommand(procedureName, dp);
            //MySQLDataAdapter mda = new MySQLDataAdapter();
            //mda.SelectCommand = cmd;
            try { cmd.ExecuteNonQuery(); }
            catch (Exception ex) { throw ex; }
            finally { Terminate(); }
        }

        /// <summary>
        /// MySql更改数据调用
        /// </summary>
        /// <param name="cmdText">查询字符串</param>
        public void ExecuteUpdate(string cmdText)
        {
            Connect();
            MySQLCommand cmd = CreateTextCommand(cmdText);
            //MySQLDataAdapter mda = new MySQLDataAdapter();
            //mda.SelectCommand = cmd;
            try { cmd.ExecuteNonQuery(); }
            catch (Exception ex) { throw ex; }
            finally { Terminate(); }
        }
    }
}
