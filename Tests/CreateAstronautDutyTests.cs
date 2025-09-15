using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using StargateAPI.Business.Commands;
using StargateAPI.Business.Data;
using Xunit;

namespace StargateAPI.Tests;

public class CreateAstronautDutyTests
{
    [Fact]
    public async Task PreProcessor_Throws_When_Required_Fields_Are_Empty()
    {
        var mockContext = new Mock<StargateContext>(new DbContextOptions<StargateContext>());
        var mockLogger = new Mock<ILogger<CreateAstronautDutyPreProcessor>>();
        var preProcessor = new CreateAstronautDutyPreProcessor(mockContext.Object, mockLogger.Object);

        var request = new CreateAstronautDuty
        {
            Name = "",
            Rank = "",
            DutyTitle = "",
            DutyStartDate = DateTime.UtcNow
        };

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

        var mockLogger = new Mock<ILogger<CreateAstronautDutyPreProcessor>>();
        var preProcessor = new CreateAstronautDutyPreProcessor(mockContext.Object, mockLogger.Object);

        var request = new CreateAstronautDuty
        {
            Name = "NotFound",
            Rank = "Captain",
            DutyTitle = "Pilot",
            DutyStartDate = DateTime.UtcNow
        };

        await Assert.ThrowsAsync<BadHttpRequestException>(() =>
            preProcessor.Process(request, CancellationToken.None));
    }

    [Fact]
    public async Task PreProcessor_Throws_When_Duty_Exists()
    {
        var person = new Person { Id = 1, Name = "John" };
        var people = new[] { person }.AsQueryable();

        var mockPeopleSet = new Mock<DbSet<Person>>();
        mockPeopleSet.As<IQueryable<Person>>().Setup(m => m.Provider).Returns(people.Provider);
        mockPeopleSet.As<IQueryable<Person>>().Setup(m => m.Expression).Returns(people.Expression);
        mockPeopleSet.As<IQueryable<Person>>().Setup(m => m.ElementType).Returns(people.ElementType);
        mockPeopleSet.As<IQueryable<Person>>().Setup(m => m.GetEnumerator()).Returns(people.GetEnumerator());

        var duty = new AstronautDuty { DutyTitle = "Pilot", DutyStartDate = DateTime.Today };
        var duties = new[] { duty }.AsQueryable();

        var mockDutySet = new Mock<DbSet<AstronautDuty>>();
        mockDutySet.As<IQueryable<AstronautDuty>>().Setup(m => m.Provider).Returns(duties.Provider);
        mockDutySet.As<IQueryable<AstronautDuty>>().Setup(m => m.Expression).Returns(duties.Expression);
        mockDutySet.As<IQueryable<AstronautDuty>>().Setup(m => m.ElementType).Returns(duties.ElementType);
        mockDutySet.As<IQueryable<AstronautDuty>>().Setup(m => m.GetEnumerator()).Returns(duties.GetEnumerator());

        var mockContext = new Mock<StargateContext>(new DbContextOptions<StargateContext>());
        mockContext.Setup(c => c.People).Returns(mockPeopleSet.Object);
        mockContext.Setup(c => c.AstronautDuties).Returns(mockDutySet.Object);

        var mockLogger = new Mock<ILogger<CreateAstronautDutyPreProcessor>>();
        var preProcessor = new CreateAstronautDutyPreProcessor(mockContext.Object, mockLogger.Object);

        var request = new CreateAstronautDuty
        {
            Name = "John",
            Rank = "Captain",
            DutyTitle = "Pilot",
            DutyStartDate = DateTime.Today
        };

        await Assert.ThrowsAsync<BadHttpRequestException>(() =>
            preProcessor.Process(request, CancellationToken.None));
    }

    [Fact]
    public async Task PreProcessor_Succeeds_When_Valid()
    {
        var person = new Person { Id = 1, Name = "John" };
        var people = new[] { person }.AsQueryable();

        var mockPeopleSet = new Mock<DbSet<Person>>();
        mockPeopleSet.As<IQueryable<Person>>().Setup(m => m.Provider).Returns(people.Provider);
        mockPeopleSet.As<IQueryable<Person>>().Setup(m => m.Expression).Returns(people.Expression);
        mockPeopleSet.As<IQueryable<Person>>().Setup(m => m.ElementType).Returns(people.ElementType);
        mockPeopleSet.As<IQueryable<Person>>().Setup(m => m.GetEnumerator()).Returns(people.GetEnumerator());

        var mockDutySet = new Mock<DbSet<AstronautDuty>>();
        mockDutySet.As<IQueryable<AstronautDuty>>().Setup(m => m.Provider).Returns(Enumerable.Empty<AstronautDuty>().AsQueryable().Provider);
        mockDutySet.As<IQueryable<AstronautDuty>>().Setup(m => m.Expression).Returns(Enumerable.Empty<AstronautDuty>().AsQueryable().Expression);
        mockDutySet.As<IQueryable<AstronautDuty>>().Setup(m => m.ElementType).Returns(Enumerable.Empty<AstronautDuty>().AsQueryable().ElementType);
        mockDutySet.As<IQueryable<AstronautDuty>>().Setup(m => m.GetEnumerator()).Returns(Enumerable.Empty<AstronautDuty>().GetEnumerator());

        var mockContext = new Mock<StargateContext>(new DbContextOptions<StargateContext>());
        mockContext.Setup(c => c.People).Returns(mockPeopleSet.Object);
        mockContext.Setup(c => c.AstronautDuties).Returns(mockDutySet.Object);

        var mockLogger = new Mock<ILogger<CreateAstronautDutyPreProcessor>>();
        var preProcessor = new CreateAstronautDutyPreProcessor(mockContext.Object, mockLogger.Object);

        var request = new CreateAstronautDuty
        {
            Name = "John",
            Rank = "Captain",
            DutyTitle = "Pilot",
            DutyStartDate = DateTime.Today
        };

        await preProcessor.Process(request, CancellationToken.None);

        // how to check if end date is null
        // obviously we're not setting it, but it could be an Assert here
    }

    [Fact]
    public async Task Handler_Throws_When_Required_Fields_Are_Empty()
    {
        var mockContext = new Mock<StargateContext>(new DbContextOptions<StargateContext>());
        var mockLogger = new Mock<ILogger<CreateAstronautDutyHandler>>();
        var handler = new CreateAstronautDutyHandler(mockContext.Object, mockLogger.Object);

        var request = new CreateAstronautDuty
        {
            Name = "",
            Rank = "",
            DutyTitle = "",
            DutyStartDate = DateTime.UtcNow
        };

        await Assert.ThrowsAsync<BadHttpRequestException>(() =>
            handler.Handle(request, CancellationToken.None));
    }

    [Fact]
    public async Task Handler_Succeeds_When_Current_Duty_End_Date_Is_Null_And_Previous_End_Date_Is_The_Previous_Day()
    {
        var person = new Person { Id = 1, Name = "John" };
        var people = new[] { person }.AsQueryable();

        var mockPeopleSet = new Mock<DbSet<Person>>();
        mockPeopleSet.As<IQueryable<Person>>().Setup(m => m.Provider).Returns(people.Provider);
        mockPeopleSet.As<IQueryable<Person>>().Setup(m => m.Expression).Returns(people.Expression);
        mockPeopleSet.As<IQueryable<Person>>().Setup(m => m.ElementType).Returns(people.ElementType);
        mockPeopleSet.As<IQueryable<Person>>().Setup(m => m.GetEnumerator()).Returns(people.GetEnumerator());

        var mockDutySet = new Mock<DbSet<AstronautDuty>>();
        mockDutySet.As<IQueryable<AstronautDuty>>().Setup(m => m.Provider).Returns(Enumerable.Empty<AstronautDuty>().AsQueryable().Provider);
        mockDutySet.As<IQueryable<AstronautDuty>>().Setup(m => m.Expression).Returns(Enumerable.Empty<AstronautDuty>().AsQueryable().Expression);
        mockDutySet.As<IQueryable<AstronautDuty>>().Setup(m => m.ElementType).Returns(Enumerable.Empty<AstronautDuty>().AsQueryable().ElementType);
        mockDutySet.As<IQueryable<AstronautDuty>>().Setup(m => m.GetEnumerator()).Returns(Enumerable.Empty<AstronautDuty>().GetEnumerator());

        var mockContext = new Mock<StargateContext>(new DbContextOptions<StargateContext>());
        mockContext.Setup(c => c.People).Returns(mockPeopleSet.Object);
        mockContext.Setup(c => c.AstronautDuties).Returns(mockDutySet.Object);

        var mockLogger = new Mock<ILogger<CreateAstronautDutyPreProcessor>>();
        var preProcessor = new CreateAstronautDutyPreProcessor(mockContext.Object, mockLogger.Object);

        var request = new CreateAstronautDuty
        {
            Name = "John",
            Rank = "Captain",
            DutyTitle = "Pilot",
            DutyStartDate = DateTime.Today.AddDays(-10)
        };

        await preProcessor.Process(request, CancellationToken.None);

        var request2 = new CreateAstronautDuty
        {
            Name = "John",
            Rank = "Captain",
            DutyTitle = "Engineer",
            DutyStartDate = DateTime.Today
        };

        await preProcessor.Process(request2, CancellationToken.None);

        // how to query astronaut duties to see the end dates?


    }
}
