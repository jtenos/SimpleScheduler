namespace SimpleSchedulerModels;

public struct JobStatus
{
    private JobStatus(string statusCode)
    {
        StatusCode = statusCode;
    }

    public string StatusCode { get; }

    public static readonly JobStatus New = new("NEW");
    public static readonly JobStatus Running = new("RUN");
    public static readonly JobStatus Success = new("SUC");
    public static readonly JobStatus Error = new("ERR");
    public static readonly JobStatus Acknowledged = new("ACK");
    public static readonly JobStatus Cancelled = new("CAN");

    public static JobStatus Parse(string statusCode) => new(statusCode);

    public override int GetHashCode() => StatusCode.GetHashCode();
    public override bool Equals(object? obj) => obj is JobStatus other && StatusCode == other.StatusCode;
    public static bool operator ==(JobStatus left, JobStatus right) => left.Equals(right);
    public static bool operator !=(JobStatus left, JobStatus right) => !(left == right);
}
