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
                new MySQLConnectionString("localhost", "vb_databases", "root", "123456").AsString
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

        public MySQLCommand CreateProcedureCommand(string procedureName, MySQLParameter[] dp)
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
        private DataSet ExecuteQuery(string procedureName, MySQLParameter[] dp)
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
        /// MySql更改数据调用
        /// </summary>
        /// <param name="procedureName">存储过程名称</param>
        /// <param name="dp">存储过程参数</param>
        private void ExecuteUpdate(string procedureName, MySQLParameter[] dp)
        {
            Connect();
            MySQLCommand cmd = CreateProcedureCommand(procedureName, dp);
            //MySQLDataAdapter mda = new MySQLDataAdapter();
            //mda.SelectCommand = cmd;
            try { cmd.ExecuteNonQuery(); }
            catch (Exception ex) { throw ex; }
            finally { Terminate(); }
        }
    }
}
