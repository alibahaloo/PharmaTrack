using Hangfire;
using Microsoft.AspNetCore.Mvc;
using Drug.API.Services;

namespace Drug.API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class JobsController : ControllerBase
    {
        private readonly DrugJobService _jobService;

        public JobsController(DrugJobService jobService)
        {
            _jobService = jobService;
        }

        [HttpPost("process-drug-data")]
        public IActionResult EnqueueProcessDrugData()
        {
            // Enqueue a background job
            BackgroundJob.Enqueue(() => _jobService.ProcessDrugDataAsync());
            return Ok("Job has been enqueued!");
        }

        [HttpPost("import-drug-data")]
        public IActionResult EnqueueImportDrugData()
        {
            // Enqueue a background job
            BackgroundJob.Enqueue(() => _jobService.ImportDrugInfoDataAsync());
            return Ok("Drug Info Import job has been enqueued!");
        }

        [HttpPost("import-interaction-data")]
        public IActionResult EnqueueImportInteractionData()
        {
            // Enqueue a background job
            BackgroundJob.Enqueue(() => _jobService.ImportDrugInteractionDataAsync());
            return Ok("Drug Interaction Import job has been enqueued!");
        }

        [HttpPost("schedule-drug-data-processing")]
        public IActionResult ScheduleProcessDrugData()
        {
            // Schedule a job to run after 1 minute
            BackgroundJob.Schedule(() => _jobService.ProcessDrugDataAsync(), TimeSpan.FromMinutes(1));
            return Ok("Job has been scheduled!");
        }
    }
}
