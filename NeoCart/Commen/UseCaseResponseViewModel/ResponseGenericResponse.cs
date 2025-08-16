namespace NeoCart.Commen.UseCaseResponseViewModel
{
    public class ResponseGenericResponse<T>
    {
        public bool Succeeded { get; set; } = true;
        public string Message { get; set; } = string.Empty;
        public List<string> Errors { get; set; } = new();
        public T Data { get; set; }

        public ResponseGenericResponse() { }

        public ResponseGenericResponse(T data, string message = "", bool succeeded = true)
        {
            Data = data;
            Message = message;
            Succeeded = succeeded;
        }

        public static ResponseGenericResponse<T> Success(T data, string message = "")
        {
            return new ResponseGenericResponse<T>
            {
                Succeeded = true,
                Message = message,
                Data = data
            };
        }

        public static ResponseGenericResponse<T> Fail(string message, List<string> errors = null)
        {
            return new ResponseGenericResponse<T>
            {
                Succeeded = false,
                Message = message,
                Errors = errors ?? new List<string>()
            };
        }
    }
}

