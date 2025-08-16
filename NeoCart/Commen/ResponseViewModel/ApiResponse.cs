namespace NeoCart.Commen.ResponseViewModel
{
    public class ApiResponse<T>
    {
        public bool Status { get; set; }
        public string Message { get; set; }
        public T Data { get; set; }
        public List<string> Errors { get; set; }

        public ApiResponse()
        {
            Status = true;
            Message = string.Empty;
            Errors = new List<string>();
        }

        public ApiResponse(T data, string message = "", bool status = true)
        {
            Data = data;
            Message = message;
            Status = status;
            Errors = new List<string>();
        }

        public static ApiResponse<T> Success(T data, string message = "") =>
            new ApiResponse<T>(data, message, true);

        public static ApiResponse<T> Fail(string message, List<string> errors = null) =>
            new ApiResponse<T>
            {
                Status = false,
                Message = message,
                Errors = errors ?? new List<string>()
            };
    }
}
