using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Tutorial5.DTOs.Request;
using Tutorial5.DTOs.Response;
using Tutorial5.Models;

namespace Tutorial5.Services
{
    public class SqlServerStudentDbService :ControllerBase, IStudentsDbService
    {

        public Student CheckIfInDb(LoginRequest loginRequest)
        {
            Student student;


            using (var connection = new SqlConnection("Data Source=db-mssql.pjwstk.edu.pl;Initial Catalog=s18711;Integrated Security=True"))
            using (var command = new SqlCommand())
            {

                command.CommandText = "SELECT * FROM Student WHERE IndexNumber = @IndexNumber AND Password = @Password";//should be fields names instead fo *

                string pass = Hashing.Hashing.Create(loginRequest.Password, "a");
                
                command.Parameters.AddWithValue("IndexNumber", loginRequest.Login);
                command.Parameters.AddWithValue("Password", pass);

                command.Connection = connection;
                connection.Open();

                var dataReader = command.ExecuteReader();

                if (!dataReader.Read())//not in db
                {
                    dataReader.Close();
                    return null;
                }

                 student = new Student()
                {
                    IndexNumber = int.Parse(dataReader["IndexNumber"].ToString()),
                    FirstName = dataReader["FirstName"].ToString(),
                    LastName =  dataReader["LastName"].ToString(),
                    BirthDate = DateTime.Parse(dataReader["BirthDate"].ToString()),
                    Password = dataReader["Password"].ToString()
                };
            }

            return student;//is in db
        }

        public Student GetFromRefreshToken(string token)
        {
            Student student;
            try
            {
                using (var connection =
                    new SqlConnection(
                        "Data Source=db-mssql.pjwstk.edu.pl;Initial Catalog=s18711;Integrated Security=True"))
                using (var command = new SqlCommand())
                {

                    command.CommandText =
                        "SELECT StudentIndex FROM Token WHERE RefreshToken = @RefreshToken ; "; //should be fields names instead fo *

                    command.Parameters.AddWithValue("RefreshToken", token);

                    command.Connection = connection;
                    connection.Open();

                    var dataReader = command.ExecuteReader();


                    if (!dataReader.Read())//not in db
                    {
                        dataReader.Close();
                        return null;
                    }

                    int studentIndex = int.Parse(dataReader["StudentIndex"].ToString());

                    dataReader.Close();

                    command.CommandText = "SELECT * FROM Student WHERE IndexNumber = @IndexNumber;";
                    command.Parameters.AddWithValue("IndexNumber", studentIndex);

                    dataReader = command.ExecuteReader();

                    if (!dataReader.Read())//not in db
                    {
                        dataReader.Close();
                        return null;
                    }

                    student = new Student()
                    {
                        IndexNumber = int.Parse(dataReader["IndexNumber"].ToString()),
                        FirstName = dataReader["FirstName"].ToString(),
                        LastName = dataReader["LastName"].ToString(),
                        BirthDate = DateTime.Parse(dataReader["BirthDate"].ToString()),
                        Password = dataReader["Password"].ToString()
                    };

                }
            }
            catch (Exception exception)
            {
                return null;
            }

            return student;
        }

        public bool AddTokenToDb(string token, string studentIndex)
        {
            try
            {
                using (var connection =
                    new SqlConnection(
                        "Data Source=db-mssql.pjwstk.edu.pl;Initial Catalog=s18711;Integrated Security=True"))
                using (var command = new SqlCommand())
                {

                    command.CommandText =
                        "INSERT INTO Token(StudentIndex, RefreshToken) VALUES (@StudentIndex, @RefreshToken); "; //should be fields names instead fo *

                    command.Parameters.AddWithValue("StudentIndex", studentIndex);
                    command.Parameters.AddWithValue("RefreshToken", token);

                    command.Connection = connection;
                    connection.Open();

                    Console.WriteLine(command.ExecuteNonQuery());

                    return true;
                }
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception.StackTrace);
                return false;
            }
        }

        public IActionResult EnrollStudent(EnrollStudentRequest request)
        {


            using (var connection =
                new SqlConnection(
                    "Data Source=db-mssql.pjwstk.edu.pl;Initial Catalog=s18711;Integrated Security=True"))
            using (var command = new SqlCommand())
            {


                command.CommandText = "SELECT * FROM Studies WHERE Name = @Name";
                command.Parameters.AddWithValue("Name", request.Studies);
                command.Connection = connection;

                connection.Open();

                var transaction = connection.BeginTransaction();
                command.Transaction = transaction;

                var dataReader = command.ExecuteReader();

                if (!dataReader.Read())
                {
                    dataReader.Close();
                    transaction.Rollback();
                    return NotFound("such studies do not exist in the database");
                }

                int IdStudy = (int)dataReader["IdStudy"];

                dataReader.Close();
                command.CommandText = "SELECT TOP 1 IdEnrollment From Enrollment Order by IdEnrollment DESC";
                int lastEnrollmentId = (int)command.ExecuteScalar();


                command.CommandText = "SELECT TOP 1 * FROM Enrollment WHERE IdStudy =  @IdStudy AND Semester = 1 ORDER BY StartDate Desc ";
                command.Parameters.AddWithValue("IdStudy", IdStudy);



                dataReader.Close();
                dataReader = command.ExecuteReader();




                if (!dataReader.Read())
                {
                    lastEnrollmentId++;

                    command.CommandText =
                        "INSERT INTO Enrollment(IdEnrollment,Semester,IdStudy,StartDate) VALUES(@IdEnrollment, 1, @IdStudy2, GETDATE()) ";
                    command.Parameters.AddWithValue("IdEnrollment", lastEnrollmentId);
                    command.Parameters.AddWithValue("IdStudy2", IdStudy);

                    dataReader.Close();
                    Console.WriteLine(command.ExecuteNonQuery());
                }

                command.CommandText = "SELECT * FROM Student WHERE @NewId IN (SELECT IndexNumber FROM Student)";
                command.Parameters.AddWithValue("NewId", request.IndexNumber);

                dataReader.Close();
                dataReader = command.ExecuteReader();

                if (dataReader.Read())
                {
                    dataReader.Close();
                    transaction.Rollback();
                    return BadRequest("in database there already exists student with such IndexNumber");
                }




                command.CommandText =
                    "INSERT INTO Student(IndexNumber,FirstName, LastName, BirthDate, IdEnrollment) VALUES(@IndexNumber,@FirstName,@LastName,@BirthDate,@IdEnrollment2)";

                command.Parameters.AddWithValue("IndexNumber", request.IndexNumber);
                command.Parameters.AddWithValue("FirstName", request.FirstName);
                command.Parameters.AddWithValue("LastName", request.LastName);
                command.Parameters.AddWithValue("BirthDate", DateTime.Now);
                command.Parameters.AddWithValue("IdEnrollment2", lastEnrollmentId);


                dataReader.Close();
                Console.WriteLine(
                     command.ExecuteNonQuery());


                transaction.Commit();
            }


            var response = new EnrollStudentResponse();
            response.Semester = 1;

            return CreatedAtAction("EnrollStudent", response);
        }



        public IActionResult PromoteStudents(PromoteStudentsRequest request)
        {
            int changedEnrollment = 0;
            var promoteStudnetResponse = new PromoteStudentResponse();

            using (var connection = new SqlConnection(
                "Data Source=db-mssql.pjwstk.edu.pl;Initial Catalog=s18711;Integrated Security=True"))
            using (var command = new SqlCommand("dbo.Promote", connection))
            using (var command2 = new SqlCommand())
            {
                command2.Connection = connection;

                command.CommandType = CommandType.StoredProcedure;

                command.Parameters.Add("@StudiesName", SqlDbType.VarChar, 100);
                command.Parameters.Add("@Semester", SqlDbType.Int);
                command.Parameters.Add("@myOut", SqlDbType.Int).Direction = ParameterDirection.Output;

                command.Parameters["@StudiesName"].Value = request.Studies;
                command.Parameters["@Semester"].Value = request.Semester;

                connection.Open();
                command.ExecuteNonQuery();

                changedEnrollment = Convert.ToInt32(command.Parameters["@myOut"].Value);

                command2.CommandText = "SELECT * FROM Enrollment WHERE IdEnrollment = @IdE";
                command2.Parameters.AddWithValue("IdE", changedEnrollment);

                var dataReader = command2.ExecuteReader();

                if (dataReader.Read())
                {
                    promoteStudnetResponse.IdEnrollment = int.Parse(dataReader["IdEnrollment"].ToString());
                    promoteStudnetResponse.IdStudies = int.Parse(dataReader["IdStudy"].ToString());
                    promoteStudnetResponse.Semester = int.Parse(dataReader["Semester"].ToString());
                }




            }





            return CreatedAtAction("PromoteStudents", promoteStudnetResponse);
        }
    }
}
