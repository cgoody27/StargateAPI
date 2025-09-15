using Dapper;
using MediatR;
using MediatR.Pipeline;
using Microsoft.EntityFrameworkCore;
using StargateAPI.Business.Data;
using StargateAPI.Controllers;

namespace StargateAPI.Business.Commands;

public class CreateAstronautDuty : IRequest<CreateAstronautDutyResult>
{
    public required string Name { get; set; }

    public required string Rank { get; set; }

    public required string DutyTitle { get; set; }

    public DateTime DutyStartDate { get; set; }
}

public class CreateAstronautDutyPreProcessor : IRequestPreProcessor<CreateAstronautDuty>
{
    private readonly StargateContext _context;
    private readonly ILogger<CreateAstronautDutyPreProcessor> _logger;

    public CreateAstronautDutyPreProcessor(StargateContext context, ILogger<CreateAstronautDutyPreProcessor> logger)
    {
        _context = context;
        _logger = logger;

    }

    public Task Process(CreateAstronautDuty request, CancellationToken cancellationToken)
    {
        if (request.Name == string.Empty || request.Rank == string.Empty || request.DutyTitle == string.Empty)
            throw new BadHttpRequestException("Bad Request - Name, Rank and DutyTitle are required");

        var person = _context.People.AsNoTracking().FirstOrDefault(z => z.Name == request.Name);

        if (person is null) 
            throw new BadHttpRequestException("Bad Request");
        
        var verifyNoPreviousDuty = _context.AstronautDuties.FirstOrDefault(z => z.DutyTitle == request.DutyTitle && z.DutyStartDate == request.DutyStartDate);

        if (verifyNoPreviousDuty is not null)
            throw new BadHttpRequestException("Bad Request");

        _logger.LogInformation("Pre-processing CreateAstronautDuty request for Name {PersonName}", request.Name);

        return Task.CompletedTask;
    }
}

public class CreateAstronautDutyHandler : IRequestHandler<CreateAstronautDuty, CreateAstronautDutyResult>
{
    private readonly StargateContext _context;
    private readonly ILogger<CreateAstronautDutyHandler> _logger;

    public CreateAstronautDutyHandler(StargateContext context, ILogger<CreateAstronautDutyHandler> logger)
    {
        _context = context;
        _logger = logger;

    }
    public async Task<CreateAstronautDutyResult> Handle(CreateAstronautDuty request, CancellationToken cancellationToken)
    {
        var query = $"SELECT * FROM [Person] WHERE \'{request.Name}\' = Name";

        var person = await _context.Connection.QueryFirstOrDefaultAsync<Person>(query);

        query = $"SELECT * FROM [AstronautDetail] WHERE {person!.Id} = PersonId";

        var astronautDetail = await _context.Connection.QueryFirstOrDefaultAsync<AstronautDetail>(query);

        if (astronautDetail == null)
        {
            astronautDetail = new AstronautDetail();
            astronautDetail.PersonId = person.Id;
            astronautDetail.CurrentDutyTitle = request.DutyTitle;
            astronautDetail.CurrentRank = request.Rank;
            astronautDetail.CareerStartDate = request.DutyStartDate.Date;
            if (request.DutyTitle == "RETIRED")
            {
                astronautDetail.CareerEndDate = request.DutyStartDate.Date;
            }

            await _context.AstronautDetails.AddAsync(astronautDetail);

        }
        else
        {
            astronautDetail.CurrentDutyTitle = request.DutyTitle;
            astronautDetail.CurrentRank = request.Rank;
            if (request.DutyTitle == "RETIRED")
            {
                astronautDetail.CareerEndDate = request.DutyStartDate.AddDays(-1).Date;
            }
            _context.AstronautDetails.Update(astronautDetail);
        }

        query = $"SELECT * FROM [AstronautDuty] WHERE {person.Id} = PersonId Order By DutyStartDate Desc";

        var astronautDuty = await _context.Connection.QueryFirstOrDefaultAsync<AstronautDuty>(query);

        if (astronautDuty != null)
        {
            astronautDuty.DutyEndDate = request.DutyStartDate.AddDays(-1).Date;
            _context.AstronautDuties.Update(astronautDuty);
        }

        var newAstronautDuty = new AstronautDuty()
        {
            PersonId = person.Id,
            Rank = request.Rank,
            DutyTitle = request.DutyTitle,
            DutyStartDate = request.DutyStartDate.Date,
            DutyEndDate = null
        };

        await _context.AstronautDuties.AddAsync(newAstronautDuty);

        await _context.SaveChangesAsync();

        _logger.LogInformation("Created new AstronautDuty with ID {AstronautDutyId} for Person ID {PersonId}", newAstronautDuty.Id, person.Id);

        return new CreateAstronautDutyResult()
        {
            Id = newAstronautDuty.Id
        };
    }
}

public class CreateAstronautDutyResult : BaseResponse
{
    public int? Id { get; set; }
}
