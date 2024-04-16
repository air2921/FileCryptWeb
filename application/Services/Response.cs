namespace application.Services
{
    public class Response
    {
        public Response()
        {
            bool success = Status >= 100 && Status <= 399;

            if (success)
                Message = "Success";
            else
                Message = "An occured unexpected error";
        }

        public int Status { get; internal set; } = 500;
        public string Message { get; internal set; }
        public object? ObjectData { get; internal set; }
    }
}
