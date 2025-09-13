using MediatR;
using Microsoft.AspNetCore.Mvc;
using StargateAPI.Business.Commands;
using StargateAPI.Business.Queries;
using System.Net;

namespace StargateAPI.Controllers;

[ApiController]
[Route("[controller]")]
public class AstronautDutyController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<AstronautDutyController> _logger;

    public AstronautDutyController(IMediator mediator, ILogger<AstronautDutyController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    [HttpGet("{name}")]
    public async Task<IActionResult> GetAstronautDutiesByName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return this.GetResponse(new BaseResponse()
            {
                Message = "Name is required",
                Success = false,
                ResponseCode = (int)HttpStatusCode.BadRequest
            });
        }

        try
        {
            var result = await _mediator.Send(new GetAstronautDutiesByName()
            {
                Name = name
            });

            if (result == null)
            {
                _logger.LogInformation(name + " not found.");

                return this.GetResponse(new BaseResponse()
                {
                    Message = "No data found",
                    Success = false,
                    ResponseCode = (int)HttpStatusCode.NotFound
                });
            }

            _logger.LogInformation($"GetAstronautDutiesByName: {name} - Result: {result?.AstronautDuties?.Count ?? 0} duties found.");

            return this.GetResponse(result!);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error in GetAstronautDutiesByName for name: {name}");
            return this.GetResponse(new BaseResponse()
            {
                Message = ex.Message,
                Success = false,
                ResponseCode = (int)HttpStatusCode.InternalServerError
            });
        }
    }

    [HttpPost("")]
    public async Task<IActionResult> CreateAstronautDuty([FromBody] CreateAstronautDuty request)
    {
        if (request == null)
        {
            return this.GetResponse(new BaseResponse()
            {
                Message = "Bad Request - No data provided",
                Success = false,
                ResponseCode = (int)HttpStatusCode.BadRequest
            });
        }
        try
        {
            var result = await _mediator.Send(request);
            return this.GetResponse(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in CreateAstronautDuty");
            return this.GetResponse(new BaseResponse()
            {
                Message = ex.Message,
                Success = false,
                ResponseCode = (int)HttpStatusCode.InternalServerError
            });
        }
    }
}