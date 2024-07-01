using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebApplication6.Models
{
    [Table("logins")]
    public class logins
    {
        [Key]
        [Column("id")]
        public int id { get; set; }
        [Column("login")]
        public string login { get; set; }
        [Column("password")]
        public string Password { get; set; }
        [Column("date_created")]
        public DateTimeOffset? d_c { get; set; }
        [Column("post")]
        public string post { get; set; }
    }
}
