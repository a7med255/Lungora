using System.Net;

namespace Lungora.Models
{
    public class API_Resonse
    {
        public HttpStatusCode StatusCode { get; set; } = HttpStatusCode.BadRequest;

        public bool IsSuccess { get; set; } = false;

        public List<string>? Errors { get; set; } = new List<string>();

        public object? Result { get; set; } = null;
    }
}
