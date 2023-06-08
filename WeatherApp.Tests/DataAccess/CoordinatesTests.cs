using System.ComponentModel.DataAnnotations;
using NUnit.Framework;

namespace WeatherApp.Tests.DataAccess;

[TestFixture]
public class CoordinatesTests
{
    [TestCase(10.0, 50.0)]
    [TestCase(-0.1, 90.0)]
    [TestCase(0.0, 0.0)]
    [TestCase(90, 90.0)]
    public void Coordinates_WithValidValues_IsValid(double longitude, double latitude)
    {
        // Arrange
        var coordinates = new Coordinates
        {
            longitude = longitude,
            latitude = -30.0
        };

        var context = new ValidationContext(coordinates, null, null);
        var results = new List<ValidationResult>();

        // Act
        bool isValid = Validator.TryValidateObject(coordinates, context, results, true);

        // Assert
        Assert.That(isValid, Is.True);
        Assert.That(results, Is.Empty);
    }

    [TestCase(-100.0, 50.0)]
    [TestCase(100.0, 50.0)]
    [TestCase(0.0, -100.0)]
    [TestCase(0.0, 100.0)]
    public void Coordinates_WithInvalidValues_IsNotValid(double longitude, double latitude)
    {
        // Arrange
        var coordinates = new Coordinates
        {
            longitude = longitude,
            latitude = latitude
        };

        var context = new ValidationContext(coordinates, null, null);
        var results = new List<ValidationResult>();

        // Act
        bool isValid = Validator.TryValidateObject(coordinates, context, results, true);

        // Assert
        Assert.That(isValid, Is.False);
        Assert.That(results, Has.Count.EqualTo(1));
        Assert.That(results[0].MemberNames, Contains.Item(longitude == -100.0 || longitude == 100.0 ? nameof(Coordinates.longitude) : nameof(Coordinates.latitude)));
    }
}
