using MediatR;
using MediatR.Pipeline;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using StargateAPI.Business.Data;
using StargateAPI.Controllers;

namespace StargateAPI.Business.Commands;

public class UpdatePerson : IRequest<UpdatePersonResult>
{
    public required string Name { get; set; } = string.Empty;
    public virtual AstronautDetail? AstronautDetail { get; set; }
    public virtual ICollection<AstronautDuty> AstronautDuties { get; set; } = new HashSet<AstronautDuty>();
}

public class UpdatePersonPreProcessor : IRequestPreProcessor<UpdatePerson>
{
    private readonly StargateContext _context;
    private readonly ILogger<UpdatePersonPreProcessor> _logger;

    public UpdatePersonPreProcessor(StargateContext context, ILogger<UpdatePersonPreProcessor> logger)
    {
        _context = context;
        _logger = logger;
    }
    public Task Process(UpdatePerson request, CancellationToken cancellationToken)
    {
        if (request.Name == string.Empty)
            throw new BadHttpRequestException("Bad Request - Name is required");

        var person = _context.People.AsNoTracking().FirstOrDefault(z => z.Name == request.Name);

        if (person is null) {
            throw new BadHttpRequestException("Bad Request");
        };

        _logger.LogInformation("Pre-processing UpdatePerson request for Name {PersonName}", request.Name);

        return Task.CompletedTask;
    }
}

public class UpdatePersonHandler : IRequestHandler<UpdatePerson, UpdatePersonResult>
{
    private readonly StargateContext _context;
    private readonly ILogger<UpdatePersonHandler> _logger;

    public UpdatePersonHandler(StargateContext context, ILogger<UpdatePersonHandler> logger)
    {
        _context = context;
        _logger = logger;
    }
    public async Task<UpdatePersonResult> Handle(UpdatePerson request, CancellationToken cancellationToken)
    {
        if (request.Name == string.Empty)
            throw new BadHttpRequestException("Bad Request - Name is required");

        var person = await _context.People.FirstOrDefaultAsync(z => z.Name == request.Name, cancellationToken);

        if (person is null)
        {
            throw new Exception($"{request.Name} not found");
        }

        person.AstronautDetail = request.AstronautDetail;
        person.AstronautDuties = request.AstronautDuties;

        _context.People.Update(person);

        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Updated person {PersonName} with Id {PersonId}", person.Name, person.Id);

        return new UpdatePersonResult()
        {
            Id = person.Id,
            Name = person.Name,
            AstronautDetail = person.AstronautDetail,
            AstronautDuties = person.AstronautDuties
        };
    }
}

public class UpdatePersonResult : BaseResponse
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public virtual AstronautDetail? AstronautDetail { get; set; }
    public virtual ICollection<AstronautDuty> AstronautDuties { get; set; } = new HashSet<AstronautDuty>();
}
