using System.ComponentModel.DataAnnotations;

namespace TrainChecker.Configuration;

public class QuartzOptions
{
    public const string Quartz = "Quartz";
    
    public CronSchedule? Forward { get; set; }
    public CronSchedule? Reverse { get; set; }
}

public class CronSchedule
{
    [Required] public string[] Schedules { get; } = [];
}