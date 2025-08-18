namespace NeoCart.Commen.Helper
{
    public enum StatusCode
    {
        Success = 200,
        NotFound = 404,
        BadRequest = 400,
        Unauthorized = 401,
        Forbidden = 403,
        InternalServerError = 500,
        Conflict = 409,
        ServiceUnavailable = 503,
        GatewayTimeout = 504,
        Created = 201,
    }
}
