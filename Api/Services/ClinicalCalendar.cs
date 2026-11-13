namespace Api.Services;

public sealed class ClinicalCalendar(TimeProvider time, IConfiguration configuration)
{
    private readonly TimeZoneInfo zone = TimeZoneInfo.FindSystemTimeZoneById(configuration["Clinic:TimeZone"] ?? "America/Manaus");
    public DateTimeOffset UtcNow => time.GetUtcNow();
    public string TimeZoneId => zone.Id;
    public DateOnly Today => DateOnly.FromDateTime(TimeZoneInfo.ConvertTime(UtcNow, zone).DateTime);
}
