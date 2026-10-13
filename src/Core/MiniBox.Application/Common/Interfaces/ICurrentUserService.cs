namespace MiniBox.Application.Common.Interfaces;

public interface ICurrentUserService
{
    Guid? UserId { get; }
}

public interface IDateTimeService
{
    DateTime UtcNow { get; }
}
