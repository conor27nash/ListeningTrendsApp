using System.ComponentModel.DataAnnotations;

namespace SpotifyTrendsApp.Server.Attributes
{
    public class SpotifyTimeRangeAttribute : ValidationAttribute
    {
        private static readonly string[] ValidTimeRanges = { "short_term", "medium_term", "long_term" };

        public override bool IsValid(object? value)
        {
            if (value is string timeRange)
            {
                return ValidTimeRanges.Contains(timeRange);
            }
            return false;
        }

        public override string FormatErrorMessage(string name)
        {
            return $"{name} must be one of: {string.Join(", ", ValidTimeRanges)}";
        }
    }

    public class BearerTokenAttribute : ValidationAttribute
    {
        public override bool IsValid(object? value)
        {
            if (value is string token)
            {
                return !string.IsNullOrEmpty(token) && token.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase);
            }
            return false;
        }

        public override string FormatErrorMessage(string name)
        {
            return $"{name} must be a valid Bearer token";
        }
    }

    public class SpotifyLimitAttribute : ValidationAttribute
    {
        public int MinValue { get; set; } = 1;
        public int MaxValue { get; set; } = 50;

        public override bool IsValid(object? value)
        {
            if (value is int limit)
            {
                return limit >= MinValue && limit <= MaxValue;
            }
            return false;
        }

        public override string FormatErrorMessage(string name)
        {
            return $"{name} must be between {MinValue} and {MaxValue}";
        }
    }
}
