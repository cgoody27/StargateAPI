using MediatR;
using Microsoft.AspNetCore.Mvc;
using StargateAPI.Business.Commands;
using StargateAPI.Business.Data;
using StargateAPI.Business.Queries;
using System.Net;

namespace StargateAPI.Controllers;


[ApiController]
[Route("[controller]")]
public class PersonController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<PersonController> _logger;
    public PersonController(IMediator mediator, ILogger<PersonController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    [HttpGet("")]
    public async Task<IActionResult> GetPeople()
    {
        try
        {
            var result = await _mediator.Send(new GetPeople() { });

            _logger.LogInformation("Fetched all people");

            return this.GetResponse(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in GetPeople");
            return this.GetResponse(new BaseResponse()
            {
                Message = ex.Message,
                Success = false,
                ResponseCode = (int)HttpStatusCode.InternalServerError
            });
        }
    }

    [HttpGet("{name}")]
    public async Task<IActionResult> GetPersonByName(string name)
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
            var result = await _mediator.Send(new GetPersonByName()
            {
                Name = name
            });

            _logger.LogInformation("Fetched person details for {PersonName}", name);

            return this.GetResponse(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error in GetPersonByName for name: {name}");
            return this.GetResponse(new BaseResponse()
            {
                Message = ex.Message,
                Success = false,
                ResponseCode = (int)HttpStatusCode.InternalServerError
            });
        }
    }

    [HttpPut("update")]
    public async Task<IActionResult> UpdatePerson([FromBody] Person person)
    {
        if (person == null || string.IsNullOrWhiteSpace(person.Name))
        {
            return this.GetResponse(new BaseResponse()
            {
                Message = "Person and Name are required",
                Success = false,
                ResponseCode = (int)HttpStatusCode.BadRequest
            });
        }

        try
        {
            var result = await _mediator.Send(new UpdatePerson()
            {
                Name = person!.Name,
                AstronautDetail = person?.AstronautDetail,
                AstronautDuties = person?.AstronautDuties ?? []
            });

            return this.GetResponse(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error in UpdatePerson for name: {person.Name}");

            return this.GetResponse(new BaseResponse()
            {
                Message = ex.Message,
                Success = false,
                ResponseCode = (int)HttpStatusCode.InternalServerError
            });
        }
    }

    [HttpPost("create")]
    public async Task<IActionResult> CreatePerson([FromBody] string name)
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
            var result = await _mediator.Send(new CreatePerson()
            {
                Name = name
            });

            return this.GetResponse(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error in CreatePerson for name: {name}");

            return this.GetResponse(new BaseResponse()
            {
                Message = ex.Message,
                Success = false,
                ResponseCode = (int)HttpStatusCode.InternalServerError
            });
        }

    }
}