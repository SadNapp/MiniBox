using MiniBox.Domain.Common;

namespace MiniBox.Domain.Entities.Users;

public class UserRole : BaseEntity
{
    public Guid UserId { get; set; }
    public string RoleName { get; set; } = string.Empty;
}