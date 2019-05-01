using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace GradeCheckerAPI.Models
{
    public class User
    {
        [Key]
        public Guid Id { get; set; }
        public Guid DeviceId { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string Salt { get; set; }
    }
}
