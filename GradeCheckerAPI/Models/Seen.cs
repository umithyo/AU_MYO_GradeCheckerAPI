using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace GradeCheckerAPI.Models
{
    public class Seen
    {
        [Key]
        public Guid Id { get; set; }
        public virtual User User { get; set; }
        public string ClassName { get; set; }
    }
}
