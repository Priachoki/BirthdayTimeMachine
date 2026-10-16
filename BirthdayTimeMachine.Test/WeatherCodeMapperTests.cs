using BirthdayTimeMachine.Helpers;

namespace BirthdayTimeMachine.Test
{
    public class WeatherCodeMapperTests
    {
        [Fact]
        public void GetDescription_ClearSkyCode_ReturnsClearSky()
        {
            // Arrange
            int code = 0;

            // Act
            string result = WeatherCodeMapper.GetDescription(code);

            // Assert
            Assert.Equal("Clear sky", result);
        }

        [Theory]
        [InlineData(61, "Light rain")]
        [InlineData(71, "Light snow")]
        [InlineData(95, "Thunderstorm")]
        public void GetDescription_KnownCodes_ReturnCorrectText(int code, string expected)
        {
            string result = WeatherCodeMapper.GetDescription(code);

            Assert.Equal(expected, result);
        }

        [Fact]
        public void GetDescription_UnknownCode_ReturnsUnknown()
        {
            string result = WeatherCodeMapper.GetDescription(1234);

            Assert.Equal("Unknown", result);
        }

        [Fact]
        public void GetDescription_NullCode_ReturnsUnknown()
        {
            string result = WeatherCodeMapper.GetDescription(null);

            Assert.Equal("Unknown", result);
        }

        [Fact]
        public void GetIconClass_SnowCode_ReturnsSnowIcon()
        {
            string result = WeatherCodeMapper.GetIconClass(71);

            Assert.Equal("bi-cloud-snow", result);
        }

        [Fact]
        public void GetIconClass_UnknownCode_ReturnsQuestionIcon()
        {
            string result = WeatherCodeMapper.GetIconClass(1234);

            Assert.Equal("bi-question-circle", result);
        }
    }
}