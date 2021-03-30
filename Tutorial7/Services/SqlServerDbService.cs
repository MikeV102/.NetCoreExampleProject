using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace Tutorial5.Services
{
    public class SqlServerDbService : IDbService
    {
        public bool ExistsIndex(string index)
        {
            using (var connection =
                new SqlConnection(
                    "Data Source=db-mssql.pjwstk.edu.pl;Initial Catalog=s18711;Integrated Security=True"))
            using (var command = new SqlCommand())
            {


                command.CommandText = "SELECT COUNT(1)  FROM Student WHERE IndexNumber = @index;";
                command.Parameters.AddWithValue("index", index);
                command.Connection = connection;

                connection.Open();

                int sqlCount = (int) command.ExecuteScalar();

                if (sqlCount > 0)
                {
                    return true;
                }

                return false;
            }
        }
    }
}
