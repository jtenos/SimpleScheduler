namespace SimpleSchedulerApiModels.Request.Workers;

public record class GetAllWorkersRequest(
	string? WorkerName = null,
	string? DirectoryName = null,
	string? Executable = null,
	bool? ActiveOnly = null,
	bool? InactiveOnly = null
);
