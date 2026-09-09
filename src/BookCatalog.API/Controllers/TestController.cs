using Microsoft.AspNetCore.Mvc;

namespace BookCatalog.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class TestController : ControllerBase
{
    private readonly ILogger<TestController> _logger;

    public TestController(ILogger<TestController> logger)
    {
        _logger = logger;
    }
    /*
     * When the shutdown timeout finish, it cancels the token
     * (cancellationSource.Cancel), which stops the operation
     * and throws a TaskCanceledException: "A task was canceled."
     * which is will caught by global exception handler
     * the operation will stop in very elegant way.
     */
    [HttpGet("graceful-shutdown-with-ct-execute-very-long-operation")]
    public async Task<IActionResult> Test1(CancellationToken ct)
    {
        _logger.LogInformation("Test1 Startred {Path}", HttpContext.Request.Path);
        await Task.Delay(TimeSpan.FromSeconds(200), ct);
        return Ok("Request completed successfully");
    }
    /*
     * When the shutdown timeout finish, the host cannot cancel the
     * operation because we not passed to it a CancellationToken.
     * so request is forced to finish
     * so execution here stops mid-flight without throwing an unhandled exception
     */
    [HttpGet("graceful-shutdown-without-ct-execute-very-long-operation")]
    public async Task<IActionResult> Test2()
    {
        _logger.LogInformation("Test2 Startred {Path}", HttpContext.Request.Path);
        await Task.Delay(TimeSpan.FromSeconds(200));
        return Ok("Request completed successfullyl");
    }
    /*
     * This is the best one 
     * when the shoutdown is requested it will wait for the specified timeout
     * to let the running requests/operations to finish 
     * and will not accept any further requests from clients
     * so because of this operation will finish before the shoutdown timeout
     * it will return to the client successfully because it's operation is completed
     */
    [HttpGet("graceful-shutdown-with-ct-wait-less-than-shutdown-timeout")]
    public async Task<IActionResult> Test3(CancellationToken ct)
    {
        _logger.LogInformation("Test3 Startred {Path}", HttpContext.Request.Path);
        await Task.Delay(TimeSpan.FromSeconds(10), ct);
        return Ok("Request completed successfully");
    }
}
