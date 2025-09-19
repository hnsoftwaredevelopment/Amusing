using Xunit;
using Amusing.Services;
using Amusing.Models;

public class StageTypeServiceTests
{
    [Fact]
    public async Task SaveAsync_ShouldReturnNewId_WhenRecordIsInserted()
    {
        // Arrange
        var service = new StageTypeService();
        var model = new StageTypeModel { Name = "Test Stage" };

        // Act
        var id = await service.SaveAsync(model);

        // Assert
        Assert.True( id > 0 );
    }
}