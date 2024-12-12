namespace MYMC.AutoUpdate;

public class UpdateProgressEventArgs(double progressPercentage, string status) : EventArgs
{
    public double ProgressPercentage { get; } = progressPercentage;
    public string Status { get; } = status;
}