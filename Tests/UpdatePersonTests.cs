using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.InMemory;
using Microsoft.Extensions.Logging;
using Moq;
using StargateAPI.Business.Commands;
using StargateAPI.Business.Data;
using Xunit;

namespace StargateAPI.Tests;

public class UpdatePersonTests
{
    [Fact]
    public async Task PreProcessor_Throws_When_Name_Is_Empty()
    {
        var mockContext = new Mock<StargateContext>(new DbContextOptions<StargateContext>());
        var mockLogger = new Mock<ILogger<UpdatePersonPreProcessor>>();
        var preProcessor = new UpdatePersonPreProcessor(mockContext.Object, mockLogger.Object);

        var request = new UpdatePerson { Name = "" };

        await Assert.ThrowsAsync<BadHttpRequestException>(() =>
            preProcessor.Process(request, CancellationToken.None));
    }

    [Fact]
    public async Task PreProcessor_Throws_When_Person_Not_Found()
    {
        var mockSet = new Mock<DbSet<Person>>();
        mockSet.As<IQueryable<Person>>().Setup(m => m.Provider).Returns(Enumerable.Empty<Person>().AsQueryable().Provider);
        mockSet.As<IQueryable<Person>>().Setup(m => m.Expression).Returns(Enumerable.Empty<Person>().AsQueryable().Expression);
        mockSet.As<IQueryable<Person>>().Setup(m => m.ElementType).Returns(Enumerable.Empty<Person>().AsQueryable().ElementType);
        mockSet.As<IQueryable<Person>>().Setup(m => m.GetEnumerator()).Returns(Enumerable.Empty<Person>().GetEnumerator());

        var mockContext = new Mock<StargateContext>(new DbContextOptions<StargateContext>());
        mockContext.Setup(c => c.People).Returns(mockSet.Object);

        var mockLogger = new Mock<ILogger<UpdatePersonPreProcessor>>();
        var preProcessor = new UpdatePersonPreProcessor(mockContext.Object, mockLogger.Object);

        var request = new UpdatePerson { Name = "NotFound" };

        await Assert.ThrowsAsync<BadHttpRequestException>(() =>
            preProcessor.Process(request, CancellationToken.None));
    }

    [Fact]
    public async Task PreProcessor_Succeeds_When_Person_Exists()
    {
        var person = new Person { Id = 1, Name = "John" };
        var people = new[] { person }.AsQueryable();

        var mockSet = new Mock<DbSet<Person>>();
        mockSet.As<IQueryable<Person>>().Setup(m => m.Provider).Returns(people.Provider);
        mockSet.As<IQueryable<Person>>().Setup(m => m.Expression).Returns(people.Expression);
        mockSet.As<IQueryable<Person>>().Setup(m => m.ElementType).Returns(people.ElementType);
        mockSet.As<IQueryable<Person>>().Setup(m => m.GetEnumerator()).Returns(people.GetEnumerator());

        var mockContext = new Mock<StargateContext>(new DbContextOptions<StargateContext>());
        mockContext.Setup(c => c.People).Returns(mockSet.Object);

        var mockLogger = new Mock<ILogger<UpdatePersonPreProcessor>>();
        var preProcessor = new UpdatePersonPreProcessor(mockContext.Object, mockLogger.Object);

        var request = new UpdatePerson { Name = "John" };

        await preProcessor.Process(request, CancellationToken.None);
    }

    [Fact]
    public async Task Handler_Throws_When_Name_Is_Empty()
    {
        var mockContext = new Mock<StargateContext>(new DbContextOptions<StargateContext>());
        var mockLogger = new Mock<ILogger<UpdatePersonHandler>>();
        var handler = new UpdatePersonHandler(mockContext.Object, mockLogger.Object);

        var request = new UpdatePerson { Name = "" };

        await Assert.ThrowsAsync<BadHttpRequestException>(() =>
            handler.Handle(request, CancellationToken.None));
    }

    [Fact]
    public async Task Handler_Throws_When_Person_Not_Found()
    {
        var people = new List<Person>().AsQueryable();

        var mockSet = new Mock<DbSet<Person>>();
        mockSet.As<IQueryable<Person>>().Setup(m => m.Provider).Returns(people.Provider);
        mockSet.As<IQueryable<Person>>().Setup(m => m.Expression).Returns(people.Expression);
        mockSet.As<IQueryable<Person>>().Setup(m => m.ElementType).Returns(people.ElementType);
        mockSet.As<IQueryable<Person>>().Setup(m => m.GetEnumerator()).Returns(people.GetEnumerator());

        var mockContext = new Mock<StargateContext>(new DbContextOptions<StargateContext>());
        mockContext.Setup(c => c.People).Returns(mockSet.Object);

        var mockLogger = new Mock<ILogger<UpdatePersonHandler>>();
        var handler = new UpdatePersonHandler(mockContext.Object, mockLogger.Object);

        var request = new UpdatePerson { Name = "NotFound" };

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            handler.Handle(request, CancellationToken.None));
    }

    [Fact]
    public async Task Handler_Updates_Person_When_Valid()
    {
        var options = new DbContextOptionsBuilder<StargateContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        using var context = new StargateContext(options);

        var person = new Person { Id = 1, Name = "John" };
        context.People.Add(person);
        await context.SaveChangesAsync();

        var logger = new Mock<ILogger<UpdatePersonHandler>>();
        var handler = new UpdatePersonHandler(context, logger.Object);

        var astronautDetail = new AstronautDetail
        {
            Id = 2,
            PersonId = 1,
            CurrentRank = "Major",
            CurrentDutyTitle = "Pilot",
            CareerStartDate = DateTime.Today
        };
        context.AstronautDetails.Add(astronautDetail);

        var duties = new List<AstronautDuty>
        {
            new AstronautDuty
            {
                Id = 3,
                PersonId = 1,
                Rank = "Major",
                DutyTitle = "Pilot",
                DutyStartDate = DateTime.Today
            }
        };
        context.AstronautDuties.AddRange(duties);

        await context.SaveChangesAsync();

        var request = new UpdatePerson
        {
            Name = "John",
            AstronautDetail = astronautDetail,
            AstronautDuties = duties
        };

        var result = await handler.Handle(request, CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal(person.Id, result.Id);
        Assert.Equal(person.Name, result.Name);
        Assert.Equal(astronautDetail, result.AstronautDetail);
        Assert.Equal(duties, result.AstronautDuties);
    }
}
