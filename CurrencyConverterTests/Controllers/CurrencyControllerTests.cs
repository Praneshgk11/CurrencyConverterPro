using NUnit.Framework;
using Microsoft.AspNetCore.Mvc;
using System;
using System.IO;
using System.Text.Json;
using Moq;
using Microsoft.Extensions.Logging;

namespace CurrencyConverter.Controllers.Tests
{
    [TestFixture]
    public class CurrencyControllerTests
    {
        private CurrencyController currencyController;
        private Mock<ILogger<CurrencyController>> mockLogger;

        [SetUp]
        public void Setup()
        {
            // Create a temporary directory for testing
            string testDirectory = Path.Combine(Path.GetTempPath(), "CurrencyControllerTests");
            Directory.CreateDirectory(testDirectory);

            // Set up exchangeRates.json file with sample rates
            string jsonFilePath = Path.Combine(testDirectory, "exchangeRates.json");
            string jsonContent = "{\"USD_TO_INR\": 74.00,\"INR_TO_USD\": 0.013}";
            File.WriteAllText(jsonFilePath, jsonContent);

            // Set up environment variable for rate override
            Environment.SetEnvironmentVariable("USD_TO_INR", "81.00");

            // Initialize the controller with the temporary directory
            Directory.SetCurrentDirectory(testDirectory);
            mockLogger = new Mock<ILogger<CurrencyController>>();
            currencyController = new CurrencyController(mockLogger.Object);
        }

        [TearDown]
        public void TearDown()
        {
            Environment.SetEnvironmentVariable("USD_TO_INR", null);
            currencyController.Dispose();
        }

        [Test]
        public void ConvertCurrency_ValidRequest_ReturnsOkResult()
        {
            // Arrange
            var request = new ConversionRequest { SourceCurrency = "USD", TargetCurrency = "INR", Amount = 100 };

            // Act
            ActionResult<ConversionResult> result = currencyController.ConvertCurrency(request);

            // Assert
            Assert.That(result.Value, Is.Not.Null);
            Assert.That(result.Value.ExchangeRate, Is.EqualTo(81.00m));
            Assert.That(result.Value.ConvertedAmount, Is.EqualTo(8100.00m));
        }

        [Test]
        public void ConvertCurrency_UnsupportedCurrencyPair_ReturnsBadRequest()
        {
            // Arrange
            var request = new ConversionRequest { SourceCurrency = "EUR", TargetCurrency = "GBP", Amount = 100 };

            // Act
            ActionResult<ConversionResult> result = currencyController.ConvertCurrency(request);

            // Assert
            Assert.That(result.Result, Is.TypeOf<BadRequestObjectResult>());
            Assert.That(result.Value, Is.Null);
        }

        [Test]
        public void ConvertCurrency_EnvironmentVariableOverride_ReturnsOverriddenRate()
        {
            // Arrange
            var request = new ConversionRequest { SourceCurrency = "USD", TargetCurrency = "INR", Amount = 100 };

            // Act
            ActionResult<ConversionResult> result = currencyController.ConvertCurrency(request);

            // Assert
            Assert.That( result.Value.ExchangeRate,Is.EqualTo(81.00m));
        }
    }
}