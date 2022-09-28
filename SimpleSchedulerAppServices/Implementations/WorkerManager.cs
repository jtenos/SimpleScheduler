using Dapper;
using SimpleSchedulerAppServices.Interfaces;
using SimpleSchedulerData;
using SimpleSchedulerDataEntities;
using SimpleSchedulerDomainModels;

namespace SimpleSchedulerAppServices.Implementations;

public sealed class WorkerManager
	: IWorkerManager
{
	private readonly SqlDatabase _db;

	public WorkerManager(SqlDatabase db)
	{
		_db = db;
	}

	async Task IWorkerManager.RunNowAsync(long id)
	{
		DynamicParameters param = new DynamicParameters()
			.AddBigIntArrayParam("@WorkerIDs", new[] { id }.ToArray());

		await _db.NonQueryAsync("[app].[Jobs_RunNow]",
			param
		).ConfigureAwait(false);
	}

	async Task<Worker[]> IWorkerManager.GetAllWorkersAsync(string? workerName, string? directoryName, string? executable,
		bool? activeOnly, bool? inactiveOnly)
	{
		DynamicParameters param = new DynamicParameters();
		if (workerName != null) { param.AddNullableNVarCharParam("@WorkerName", workerName, 100); }
		if (directoryName != null) { param.AddNullableNVarCharParam("@DirectoryName", directoryName, 1000); }
		if (executable != null) { param.AddNullableNVarCharParam("@Executable", executable, 1000); }
		if (activeOnly != null) { param.AddNullableBitParam("@ActiveOnly", activeOnly); }
		if (inactiveOnly != null) { param.AddNullableBitParam("@InactiveOnly", inactiveOnly); }

		return (await _db.GetManyAsync<WorkerEntity>(
			"[app].[Workers_SelectAll]",
			parameters: param
		).ConfigureAwait(false))
		.Select(w => ModelBuilders.GetWorker(w))
		.ToArray();
	}

	async Task<Worker[]> IWorkerManager.GetWorkersAsync(long[] ids)
	{
		DynamicParameters param = new DynamicParameters()
			.AddBigIntArrayParam("@IDs", ids);

		return (await _db.GetManyAsync<WorkerEntity>(
			"[app].[Workers_SelectMany]",
			parameters: param
		).ConfigureAwait(false))
		.Select(w => ModelBuilders.GetWorker(w))
		.ToArray();
	}

	async Task<Worker> IWorkerManager.GetWorkerAsync(long id)
	{
		DynamicParameters param = new DynamicParameters()
			.AddLongParam("@ID", id);

		return ModelBuilders.GetWorker(await _db.GetOneAsync<WorkerEntity>(
			"[app].[Workers_Select]",
			param
		).ConfigureAwait(false));
	}

	private record class AddWorkerResult(bool Success, bool NameAlreadyExists, bool CircularReference);
	async Task IWorkerManager.AddWorkerAsync(
		string workerName, string detailedDescription, string emailOnSuccess, long? parentWorkerID,
		int timeoutMinutes, string directoryName, string executable, string argumentValues,
		string workerPath)
	{
		if (!IsValidExecutable(directoryName, executable, workerPath))
		{
			throw new ApplicationException("Invalid executable");
		}

		DynamicParameters param = new DynamicParameters()
			.AddNVarCharParam("@WorkerName", workerName, 100)
			.AddNVarCharParam("@DetailedDescription", detailedDescription, -1)
			.AddNVarCharParam("@EmailOnSuccess", emailOnSuccess, 100)
			.AddNullableLongParam("@ParentWorkerID", parentWorkerID)
			.AddIntParam("@TimeoutMinutes", timeoutMinutes)
			.AddNVarCharParam("@DirectoryName", directoryName, 1000)
			.AddNVarCharParam("@Executable", executable, 1000)
			.AddNVarCharParam("@ArgumentValues", argumentValues, 1000);

		AddWorkerResult result = await _db.GetOneAsync<AddWorkerResult>(
			"[app].[Workers_Insert]",
			param
		).ConfigureAwait(false);

		if (result.NameAlreadyExists) { throw new ApplicationException("Name already exists"); }
		if (result.CircularReference) { throw new ApplicationException("Circular reference"); }
		if (!result.Success) { throw new ApplicationException("Invalid call to AddWorker"); }
	}

	private record class UpdateWorkerResult(bool Success, bool NameAlreadyExists, bool CircularReference);
	async Task IWorkerManager.UpdateWorkerAsync(
		long workerID, string workerName, string detailedDescription, string emailOnSuccess,
		long? parentWorkerID, int timeoutMinutes, string directoryName, string executable, string argumentValues,
		string workerPath)
	{
		if (!IsValidExecutable(directoryName, executable, workerPath))
		{
			throw new ApplicationException("Invalid executable");
		}

		if (workerID == parentWorkerID)
		{
			throw new ApplicationException("Worker cannot be its own parent");
		}

		DynamicParameters param = new DynamicParameters()
			.AddLongParam("@ID", workerID)
			.AddNVarCharParam("@WorkerName", workerName, 100)
			.AddNVarCharParam("@DetailedDescription", detailedDescription, -1)
			.AddNVarCharParam("@EmailOnSuccess", emailOnSuccess, 100)
			.AddNullableLongParam("@ParentWorkerID", parentWorkerID)
			.AddIntParam("@TimeoutMinutes", timeoutMinutes)
			.AddNVarCharParam("@DirectoryName", directoryName, 1000)
			.AddNVarCharParam("@Executable", executable, 1000)
			.AddNVarCharParam("@ArgumentValues", argumentValues, 1000);

		UpdateWorkerResult result = await _db.GetOneAsync<UpdateWorkerResult>(
			"[app].[Workers_Update]",
			param
		).ConfigureAwait(false);

		if (result.NameAlreadyExists) { throw new ApplicationException("Name already exists"); }
		if (result.CircularReference) { throw new ApplicationException("Circular reference"); }
		if (!result.Success) { throw new ApplicationException("Invalid call to AddWorker"); }
	}

	async Task IWorkerManager.DeactivateWorkerAsync(long id)
	{
		DynamicParameters param = new DynamicParameters()
			.AddLongParam("@ID", id);

		await _db.NonQueryAsync(
			"[app].[Workers_Deactivate]",
			param
		).ConfigureAwait(false);
	}

	async Task IWorkerManager.ReactivateWorkerAsync(long id)
	{
		DynamicParameters param = new DynamicParameters()
			.AddLongParam("@ID", id);

		await _db.NonQueryAsync(
			"[app].[Workers_Reactivate]",
			param
		).ConfigureAwait(false);
	}

	private static bool IsValidExecutable(string directoryName, string executable, string workerPath)
	{
		if (directoryName.Contains('/')
			|| executable.Contains('/')
			|| directoryName.Contains('\\')
			|| executable.Contains('\\'))
		{
			return false;
		}

		string fullPath = Path.Combine(workerPath, directoryName, executable);
		if (!File.Exists(fullPath))
		{
			return false;
		}

		return true;
	}
}
