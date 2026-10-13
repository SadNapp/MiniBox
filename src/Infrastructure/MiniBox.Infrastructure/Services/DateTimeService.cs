using MiniBox.Application.Common.Interfaces;

namespace MiniBox.Infrastructure.Services;

public sealed class DateTimeService : IDateTimeService
{
    public DateTime UtcNow => DateTime.UtcNow;
}
