using System.ComponentModel.DataAnnotations;

namespace TrainChecker.Configuration;

public class ScheduledJobOptions
{
    public const string Quartz = "Quartz";
    
    public CronSchedule? Forward { get; set; }
    public CronSchedule? Reverse { get; set; }
}

public class CronSchedule
{
    [MinLength(1)]
    public string[] Schedules { get; set; } = [];
}