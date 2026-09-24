namespace MiniBox.Domain.Common;

public class BaseAuditableEntity : BaseEntity
{
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public string? CreatedBy { get; set; }
    public DateTime? LastModifiedAt { get; set; }
    public string? LastModifiedBy { get; set; }
}