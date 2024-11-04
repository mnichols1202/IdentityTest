using Dapper.Contrib.Extensions;
using System;
using System.Collections.Generic;

namespace IdentityTest.Model;

[Table("[User]")]
public partial class User
{
    [Key]
    public int Id { get; set; }

    public string UserName { get; set; } = null!;

    public string PasswordHash { get; set; } = null!;

    public int RoleId { get; set; }

    public bool IsActive { get; set; }
}
