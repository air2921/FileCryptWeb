namespace application.Master_Services
{
    public class Response
    {
        public Response(bool success = false)
        {
            if (success)
                Message = "Success";
            else
                Message = "An occured unexpected error";
        }

        public int Status { get; internal set; } = 500;
        public bool IsSuccess { get; internal set; } = false;
        public string Message { get; internal set; }
        public object? ObjectData { get; internal set; }
    }
}
