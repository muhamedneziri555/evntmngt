using FluentAssertions;
using EventMngt.DTOs;
using EventMngt.Validators;
using Xunit;

namespace EventMngt.Tests.Validators;

public class CategoryValidatorTests
{
    private readonly CreateCategoryDTOValidator _createValidator;
    private readonly UpdateCategoryDTOValidator _updateValidator;

    public CategoryValidatorTests()
    {
        _createValidator = new CreateCategoryDTOValidator();
        _updateValidator = new UpdateCategoryDTOValidator();
    }

    [Fact]
    public void CreateCategoryDTO_WithValidData_ShouldPassValidation()
    {
        // Arrange
        var dto = new CreateCategoryDTO
        {
            Name = "Test Category",
            Description = "Test Description"
        };

        // Act
        var result = _createValidator.Validate(dto);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData("", "Description")]
    [InlineData(null, "Description")]
    [InlineData("Name", "")]
    [InlineData("Name", null)]
    public void CreateCategoryDTO_WithInvalidData_ShouldFailValidation(string name, string description)
    {
        // Arrange
        var dto = new CreateCategoryDTO
        {
            Name = name,
            Description = description
        };

        // Act
        var result = _createValidator.Validate(dto);

        // Assert
        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public void UpdateCategoryDTO_WithValidData_ShouldPassValidation()
    {
        // Arrange
        var dto = new UpdateCategoryDTO
        {
            Name = "Updated Category",
            Description = "Updated Description"
        };

        // Act
        var result = _updateValidator.Validate(dto);

        // Assert
        result.IsValid.Should().BeTrue();
    }
} 