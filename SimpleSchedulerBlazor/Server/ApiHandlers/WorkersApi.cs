//using Microsoft.AspNetCore.Authorization;
//using SimpleSchedulerBusiness;

//namespace SimpleSchedulerBlazor.Server.ApiHandlers;

//public static class WorkersApiExtensions
//{
//    public static WebApplicationBuilder AddWorkersApi(this WebApplicationBuilder builder)
//    {
//        builder.Services.AddScoped<WorkersApi>();
//        return builder;
//    }

//    public static WebApplication MapWorkersApi(this WebApplication app)
//    {
//        [Authorize]
//        IResult GetAllWorkers(WorkersApi workersApi, CancellationToken cancellationToken)
//        {
//            return workersApi.GetAllWorkers(cancellationToken);
//        }
//        app.MapGet("/Workers/GetAllWorkers", [Authorize](WorkersApi workersApi, CancellationToken cancellationToken) =>
//        {
//            return workersApi.GetAllWorkers(cancellationToken);
//        });

//        return app;
//    }
//}

//internal class WorkersApi
//{
//    private readonly IWorkerManager _workerManager;

//    public WorkersApi(IWorkerManager workerManager, DatabaseFactory databaseFactory)
//        {
//            _workerManager = workerManager;
//            _databaseFactory = databaseFactory;
//        }

//        public async Task<IResult> GetAllWorkers(CancellationToken cancellationToken,
//        bool getActive = true, bool getInactive = true)
//    {
//        return Ok(await _workerManager.GetAllWorkerDetailsAsync(cancellationToken, getActive, getInactive));
//    }
//}
///*
// using Microsoft.AspNetCore.Authorization;
//using Microsoft.AspNetCore.Mvc;
//using SimpleSchedulerBusiness;
//using SimpleSchedulerData;
//using SimpleSchedulerModels;
//using SimpleSchedulerModels.Exceptions;
//using System.Collections.Immutable;

//namespace SimpleSchedulerBlazor.Server.Controllers;

//[ApiController]
//[Route("[controller]")]
////[Authorize("ValidUser")]
//public class WorkersController
//    : ControllerBase
//{
//    private readonly DatabaseFactory _databaseFactory;


//    [HttpGet]
//    [Route("[action]")]

//    [HttpGet]
//    [Route("[action]")]
//    public async Task<Worker> GetWorker(long workerID, CancellationToken cancellationToken)
//        => await _workerManager.GetWorkerAsync(workerID, cancellationToken);

//    [HttpPost]
//    [Route("[action]")]
//    public async Task<IActionResult> CheckCanDeleteWorker([FromBody] DeleteWorkerRequest deleteWorkerRequest,
//        CancellationToken cancellationToken)
//        => Ok(new { Message = await _workerManager.CheckCanDeactivateWorkerAsync(deleteWorkerRequest.WorkerID, cancellationToken) });

//    [HttpPost]
//    [Route("[action]")]
//    public async Task DeleteWorker([FromBody] DeleteWorkerRequest deleteWorkerRequest,
//        CancellationToken cancellationToken)
//        => await _workerManager.DeactivateWorkerAsync(deleteWorkerRequest.WorkerID, cancellationToken);

//    [HttpPost]
//    [Route("[action]")]
//    public async Task ReactivateWorker([FromBody] ReactivateWorkerRequest reactivateWorkerRequest,
//        CancellationToken cancellationToken)
//        => await _workerManager.ReactivateWorkerAsync(reactivateWorkerRequest.WorkerID, cancellationToken);

//    [HttpPost]
//    [Route("[action]")]
//    public async Task RunNow([FromBody] RunNowRequest runNowRequest, CancellationToken cancellationToken)
//        => await _workerManager.RunNowAsync(runNowRequest.WorkerID, cancellationToken);

//    [HttpPost]
//    [Route("[action]")]
//    public async Task<IActionResult> SaveWorker(Worker worker, CancellationToken cancellationToken)
//    {
//        try
//        {
//            if (worker.WorkerID > 0)
//            {
//                await _workerManager.UpdateWorkerAsync(worker.WorkerID, worker.IsActive, worker.WorkerName,
//                    worker.DetailedDescription, worker.EmailOnSuccess, worker.ParentWorkerID,
//                    worker.TimeoutMinutes, worker.DirectoryName,
//                    worker.Executable, worker.ArgumentValues, cancellationToken);
//                return Ok(new { Success = true });
//            }

//            await _workerManager.AddWorkerAsync(worker.IsActive, worker.WorkerName,
//                    worker.DetailedDescription, worker.EmailOnSuccess, worker.ParentWorkerID,
//                    worker.TimeoutMinutes, worker.DirectoryName,
//                    worker.Executable, worker.ArgumentValues, cancellationToken);
//            return Ok(new { Success = true });
//        }
//        catch (WorkerAlreadyExistsException)
//        {
//            _databaseFactory.MarkForRollback();
//            return BadRequest($"Worker with name {worker.WorkerName} already exists");
//        }
//        catch (CircularWorkerRelationshipException)
//        {
//            _databaseFactory.MarkForRollback();
//            return BadRequest("This would create a circular relationship (parent->child->parent)");
//        }
//        catch (InvalidExecutableException)
//        {
//            _databaseFactory.MarkForRollback();
//            return BadRequest("Executable not found");
//        }
//        catch
//        {
//            _databaseFactory.MarkForRollback();
//            return BadRequest("Unknown error, please try again");
//        }
//    }

//    public record DeleteWorkerRequest(long WorkerID);
//    public record ReactivateWorkerRequest(long WorkerID);
//    public record RunNowRequest(long WorkerID);
//}

// */