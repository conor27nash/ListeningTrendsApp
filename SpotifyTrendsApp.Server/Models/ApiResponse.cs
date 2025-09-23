namespace SpotifyTrendsApp.Server.Models
{
    public class ApiResponse<T>
    {
        public bool Success { get; set; }
        public T? Data { get; set; }
        public string? ErrorMessage { get; set; }
        public string? ErrorCode { get; set; }
        public Dictionary<string, string[]>? ValidationErrors { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        public static ApiResponse<T> SuccessResult(T data)
        {
            return new ApiResponse<T>
            {
                Success = true,
                Data = data
            };
        }

        public static ApiResponse<T> ErrorResult(string errorMessage, string? errorCode = null)
        {
            return new ApiResponse<T>
            {
                Success = false,
                ErrorMessage = errorMessage,
                ErrorCode = errorCode
            };
        }

        public static ApiResponse<T> ValidationErrorResult(Dictionary<string, string[]> validationErrors)
        {
            return new ApiResponse<T>
            {
                Success = false,
                ErrorMessage = "Validation failed",
                ErrorCode = "VALIDATION_ERROR",
                ValidationErrors = validationErrors
            };
        }
    }

    public class ApiResponse : ApiResponse<object?>
    {
        public static ApiResponse SuccessResponse()
        {
            return new ApiResponse
            {
                Success = true
            };
        }
        
        public static ApiResponse ErrorResponse(string errorMessage, string? errorCode = null)
        {
            return new ApiResponse
            {
                Success = false,
                ErrorMessage = errorMessage,
                ErrorCode = errorCode
            };
        }
    }
}
