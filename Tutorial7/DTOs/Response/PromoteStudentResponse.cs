using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Tutorial5.DTOs.Response
{
    public class PromoteStudentResponse
    {
        public int  IdEnrollment { get; set; }
        public int IdStudies { get; set; }

        public int Semester { get; set; }
    }
}
