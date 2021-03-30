using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Tutorial5.DTOs.Request
{
    public class EnrollStudentRequest
    {
        [Required]
        
        public int  IndexNumber { get; set; }
        [Required]
        public string FirstName { get; set; }
        [Required]
        public string LastName { get; set; }
        [Required]
        public string Studies { get; set; }

    }
}
