using Dystopia.Info;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace Dystopia.Controllers;

[ApiController]
public class InfoController : ControllerBase
{
    private readonly StartTimeHolder _startTimeHolder;
    private readonly InstanceInfoSettings _settings;

    public InfoController(StartTimeHolder startTimeHolder, IOptionsMonitor<InstanceInfoSettings> options)
    {
        _startTimeHolder = startTimeHolder;

        _settings = options.CurrentValue;
    }

    [HttpGet]
    [Route("/")]
    public IActionResult Get()
    {
        var info = InstanceInfo.Create(_startTimeHolder.StartTime, _settings);
        return Ok(info);
    }
}