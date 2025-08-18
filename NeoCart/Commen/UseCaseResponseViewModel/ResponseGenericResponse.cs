using NeoCart.Commen.Helper;

namespace NeoCart.Commen.UseCaseResponseViewModel
{
    public class ResponseGenericResponse<T>
    {
        public bool Succeeded { get; set; } = true;
        public string Message { get; set; } = string.Empty;
        public StatusCode StatusCode { get; set; } = StatusCode.Success;
        public List<string> Errors { get; set; } = new();
        public T Data { get; set; }

        public ResponseGenericResponse() { }

        public ResponseGenericResponse(T data, string message = "", bool succeeded = true, StatusCode statusCode = StatusCode.Success)
        {
            Data = data;
            Message = message;
            Succeeded = succeeded;
            StatusCode = statusCode;
        }

        public static ResponseGenericResponse<T> Success(T data, string message = "", StatusCode statusCode = StatusCode.Success)
        {
            return new ResponseGenericResponse<T>
            {
                Succeeded = true,
                Message = message,
                Data = data,
                StatusCode = statusCode
            };
        }

        public static ResponseGenericResponse<T> Fail(string message, List<string> errors = null, StatusCode statusCode = StatusCode.BadRequest)
        {
            return new ResponseGenericResponse<T>
            {
                Succeeded = false,
                Message = message,
                Errors = errors ?? new List<string>(),
                StatusCode = statusCode
            };
        }
    }
}

