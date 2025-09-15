using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using StargateAPI.Business.Commands;
using StargateAPI.Business.Data;
using Xunit;

namespace StarGateAPITests;

public class CreatePersonTests
{
    [Fact]
    public async Task PreProcessor_Throws_WhenNameIsEmpty()
    {

        var mockLogger = new Mock<ILogger<CreatePersonPreProcessor>>();

        // Arrange
        var mockContext = new Mock<StargateContext>(new DbContextOptions<StargateContext>());
        var preProcessor = new CreatePersonPreProcessor(mockContext.Object, mockLogger.Object);
        var request = new CreatePerson { Name = string.Empty };

        // Act & Assert
        await Assert.ThrowsAsync<BadHttpRequestException>(() =>
            preProcessor.Process(request, CancellationToken.None));
    }

    [Fact]
    public async Task PreProcessor_Throws_WhenPersonExists()
    {
        var mockLogger = new Mock<ILogger<CreatePersonPreProcessor>>();

        // Arrange
        var person = new Person { Id = 1, Name = "John" };
        var people = new[] { person }.AsQueryable();

        var mockSet = new Mock<DbSet<Person>>();
        mockSet.As<IQueryable<Person>>().Setup(m => m.Provider).Returns(people.Provider);
        mockSet.As<IQueryable<Person>>().Setup(m => m.Expression).Returns(people.Expression);
        mockSet.As<IQueryable<Person>>().Setup(m => m.ElementType).Returns(people.ElementType);
        mockSet.As<IQueryable<Person>>().Setup(m => m.GetEnumerator()).Returns(people.GetEnumerator());

        var mockContext = new Mock<StargateContext>(new DbContextOptions<StargateContext>());
        mockContext.Setup(c => c.People).Returns(mockSet.Object);

        var preProcessor = new CreatePersonPreProcessor(mockContext.Object, mockLogger.Object);
        var request = new CreatePerson { Name = "John" };

        // Act & Assert
        await Assert.ThrowsAsync<BadHttpRequestException>(() =>
            preProcessor.Process(request, CancellationToken.None));
    }

    [Fact]
    public async Task PreProcessor_Succeeds_WhenPersonDoesNotExist()
    {
        var mockLogger = new Mock<ILogger<CreatePersonPreProcessor>>();
        // Arrange
        var people = new Person[] { }.AsQueryable();

        var mockSet = new Mock<DbSet<Person>>();
        mockSet.As<IQueryable<Person>>().Setup(m => m.Provider).Returns(people.Provider);
        mockSet.As<IQueryable<Person>>().Setup(m => m.Expression).Returns(people.Expression);
        mockSet.As<IQueryable<Person>>().Setup(m => m.ElementType).Returns(people.ElementType);
        mockSet.As<IQueryable<Person>>().Setup(m => m.GetEnumerator()).Returns(people.GetEnumerator());

        var mockContext = new Mock<StargateContext>(new DbContextOptions<StargateContext>());
        mockContext.Setup(c => c.People).Returns(mockSet.Object);

        var preProcessor = new CreatePersonPreProcessor(mockContext.Object, mockLogger.Object);
        var request = new CreatePerson { Name = "Jane" };

        // Act & Assert
        await preProcessor.Process(request, CancellationToken.None);
    }

    [Fact]
    public async Task Handler_Throws_WhenNameIsEmpty()
    {
        var mockLogger = new Mock<ILogger<CreatePersonHandler>>();

        // Arrange
        var mockContext = new Mock<StargateContext>(new DbContextOptions<StargateContext>());
        var handler = new CreatePersonHandler(mockContext.Object, mockLogger.Object);
        var request = new CreatePerson { Name = string.Empty };

        // Act & Assert
        await Assert.ThrowsAsync<BadHttpRequestException>(() =>
            handler.Handle(request, CancellationToken.None));
    }

    [Fact]
    public async Task Handler_CreatesPerson_AndReturnsResult()
    {
        var mockLogger = new Mock<ILogger<CreatePersonHandler>>();

        // Arrange
        var mockSet = new Mock<DbSet<Person>>();
        var mockContext = new Mock<StargateContext>(new DbContextOptions<StargateContext>());
        mockContext.Setup(c => c.People).Returns(mockSet.Object);

        mockSet.Setup(m => m.AddAsync(It.IsAny<Person>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Person p, CancellationToken ct) => { p.Id = 42; return default; });

        mockContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var handler = new CreatePersonHandler(mockContext.Object, mockLogger.Object);
        var request = new CreatePerson { Name = "Sam" };

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(42, result.Id);
    }
}