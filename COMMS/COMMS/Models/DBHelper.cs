using Microsoft.Ajax.Utilities;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;

namespace COMMS.Models
{
    public class DBHelper
    {
        private String ConnectionString;
        private SqlConnection _connection;

        public DBHelper()
        {
            ConnectionString = ConfigurationManager.ConnectionStrings["LocalServer"].ConnectionString;
        }

        public bool TestConnection()
        {
            try
            {

                using (var connection = new SqlConnection(ConnectionString))
                {
                    connection.Open();
                    return true;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"数据库连接失败: {ex.Message}");
                return false;
            }
        }
        // 获取数据库连接
        // 获取数据库连接
        public SqlConnection GetConnection()
        {
            if (_connection == null)
            {
                _connection = new SqlConnection(ConnectionString);
            }

            if (_connection.State != ConnectionState.Open)
            {
                _connection.Open();
            }

            return _connection;
        }

        public void Dispose()
        {
            _connection?.Close();
            _connection?.Dispose();
        }

        // 执行查询返回 DataTable
        public DataTable ExecuteQuery(string query, params SqlParameter[] parameters)
        {
            var dataTable = new DataTable();

            using (var connection = new SqlConnection(ConnectionString))
            using (var command = new SqlCommand(query, connection))
            {
                if (parameters != null)
                    command.Parameters.AddRange(parameters);

                connection.Open();
                using (var adapter = new SqlDataAdapter(command))
                {
                    adapter.Fill(dataTable);
                }
            }

            return dataTable;
        }

        // 执行非查询操作
        public int ExecuteNonQuery(string commandText, params SqlParameter[] parameters)
        {
            using (var connection = new SqlConnection(ConnectionString))
            using (var command = new SqlCommand(commandText, connection))
            {
                if (parameters != null)
                    command.Parameters.AddRange(parameters);

                connection.Open();
                return command.ExecuteNonQuery();
            }
        }
    }
}