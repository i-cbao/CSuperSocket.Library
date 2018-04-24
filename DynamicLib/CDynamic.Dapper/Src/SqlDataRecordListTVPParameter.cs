using System;
using System.Collections.Generic;
using System.Data;

namespace Dapper
{
    /// <summary>
    /// Used to pass a IEnumerable&lt;SqlDataRecord&gt; as a SqlDataRecordListTVPParameter
    /// </summary>
    internal sealed class SqlDataRecordListTVPParameter : SqlMapper.ICustomQueryParameter
    {
        private readonly IEnumerable<Npgsql.NpgsqlDataReader> data;
        private readonly string typeName;
        /// <summary>
        /// Create a new instance of <see cref="SqlDataRecordListTVPParameter"/>.
        /// </summary>
        /// <param name="data">The data records to convert into TVPs.</param>
        /// <param name="typeName">The parameter type name.</param>
        public SqlDataRecordListTVPParameter(IEnumerable<Npgsql.NpgsqlDataReader> data, string typeName)
        {
            
            this.data = data;
            this.typeName = typeName;
        }

        void SqlMapper.ICustomQueryParameter.AddParameter(IDbCommand command, string name)
        {
            var param = command.CreateParameter();
            param.ParameterName = name;
            Set(param, data, typeName);
            command.Parameters.Add(param);
        }

        internal static void Set(IDbDataParameter parameter, IEnumerable<Npgsql.NpgsqlDataReader> data, string typeName)
        {
          
            parameter.Value = (object)data ?? DBNull.Value;
            if (parameter is Npgsql.NpgsqlParameter sqlParam)
            {
                sqlParam.DbType = DbType.String;
                sqlParam.ParameterName = typeName;
            }
        }
    }
}
