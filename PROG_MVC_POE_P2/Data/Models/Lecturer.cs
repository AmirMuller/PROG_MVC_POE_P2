using System;
using System.Collections.Generic;

namespace PROG_MVC_POE_P2.Data.Models;

public partial class Lecturer
{
    public int LecturerId { get; set; }

    public string Name { get; set; } = null!;
    public string Surname { get; set; } = null!;
    public required string Role { get; set; }
    public int HourlyRate { get; set; }
    public string Faculty { get; set; } = null!;

    public string Position { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string PasswordHash { get; set; } = null!;
    public string PasswordSalt { get; set; } = null!;

    public virtual ICollection<Claim> Claims { get; set; } = new List<Claim>();
}
