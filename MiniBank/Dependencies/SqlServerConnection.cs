﻿using Microsoft.Data.SqlClient;
using System.Data;

namespace MiniBank.Dependencies
{
    public class SqlServerConnection
    {
        private SqlConnection? Connection { get; set; }

        private static SqlServerConnection? _instance;

        private static SqlServerConnection Instance
        {
            get
            {
                if (_instance is null)
                {
                    _instance = new SqlServerConnection();
                    _instance.Open();
                }

                if (_instance.Connection.State == ConnectionState.Closed) 
                    _instance.Open();

                return _instance;
            }
        }

        public static SqlServerConnection GetInstance()
        {
            return Instance;
        }

        private void Open()
        {
            Connection = new SqlConnection(string.Format(
                    "Server={0};User Id={1};Database={2};Password={3};TrustServerCertificate=True",
                    Environment.Variables.GetValue<string>("DbCredentials:Host"),
                    Environment.Variables.GetValue<string>("DbCredentials:Username"),
                    Environment.Variables.GetValue<string>("DbCredentials:DbName"),
                    Environment.Variables.GetValue<string>("DbCredentials:Password")));

            Connection.Open();
        }

        public List<Dictionary<string, string>> Query(string sql)
        {
            string _sql = sql.Trim().ToUpper();

            using SqlCommand command = new(_sql, Connection);
            using SqlDataReader reader = command.ExecuteReader();

            var data = new List<Dictionary<string, string>>();
            while (reader.Read())
            {
                Dictionary<string, string> values = new Dictionary<string, string>();
                for (int i = 0; i < reader.FieldCount; i++)
                    values.Add(reader.GetName(i).ToUpper(), reader.GetValue(i)?.ToString() ?? "");

                data.Add(values);
            }

            return (List<Dictionary<string, string>>)data;
        }

        public List<Dictionary<string, string>> Query(string sql, Dictionary<string, object> parameters)
        {
            string _sql = sql.Trim().ToUpper();

            using SqlCommand command = new(_sql, Connection);

            foreach (var parameter in parameters)
                command.Parameters.AddWithValue($"@{parameter.Key}", parameter.Value);

            using SqlDataReader reader = command.ExecuteReader();

            var data = new List<Dictionary<string, string>>();
            while (reader.Read())
            {
                Dictionary<string, string> values = new Dictionary<string, string>();
                for (int i = 0; i < reader.FieldCount; i++)
                    values.Add(reader.GetName(i).ToUpper(), reader.GetValue(i)?.ToString() ?? "");

                data.Add(values);
            }

            return (List<Dictionary<string, string>>)data;
        }

        public string OutputValue(string sql)
        {
            string _sql = sql.Trim().ToUpper();

            using SqlCommand command = new(_sql, Connection);
            var reader = command.ExecuteScalar();

            return reader?.ToString() ?? "";
        }

        public string OutputValue(string sql, Dictionary<string, object> parameters)
        {
            string _sql = sql.Trim().ToUpper();

            using SqlCommand command = new(_sql, Connection);

            foreach (var parameter in parameters)
                command.Parameters.AddWithValue($"@{parameter.Key}", parameter.Value);

            var reader = command.ExecuteScalar();

            return reader?.ToString() ?? "";
        }

        public bool NonQuery(string sql)
        {
            using SqlCommand command = new(sql, Connection);

            int result = command.ExecuteNonQuery();

            return result != 0;
        }

        public bool NonQuery(string sql, Dictionary<string, object> parameters)
        {
            using SqlCommand command = new(sql, Connection);

            foreach (var parameter in parameters)
                command.Parameters.AddWithValue($"@{parameter.Key}", parameter.Value);

            int result = command.ExecuteNonQuery();

            return result != 0;
        }

        public void Close()
        {
            Connection?.Close();
        }
    }
}
