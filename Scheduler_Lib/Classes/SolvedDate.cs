namespace Scheduler_Lib.Classes;

public class SolvedDate
{
    public string Description { get; set; } = string.Empty;
    public DateTimeOffset NewDate { get; set; }
}






/*
 public override bool Equals(object obj)
 {
     return obj is SolvedDate other &&
            Description == other.Description &&
            NewDate.Equals(other.NewDate);
 }

 public override int GetHashCode()
 {
     return HashCode.Combine(Description, NewDate);
 }
*/
