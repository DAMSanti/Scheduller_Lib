namespace Scheduler_Lib.Core.Model;

public class RequestedDate {
    public DateTimeOffset Date { get; set; } = DateTimeOffset.Now;
    public DateTimeOffset StartDate { get; set; }
    public DateTimeOffset? EndDate { get; set; }
    public DateTimeOffset? ChangeDate { get; set; }
    public EnumPeriodicity Periodicity { get; set; }
    /// <summary>
    /// PERIODO EN DIAS
    /// </summary>
    public int? Period { get; set; }
    public TimeZoneInfo TimeZonaId { get; set; } = TimeZoneInfo.Local;
}