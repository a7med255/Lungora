namespace Lungora.Models
{
    public class EmailMetadata
    {
        public string ToAddress { get; }
        public string Subject { get; }
        public string Body { get; }

        public EmailMetadata(string toAddress, string subject, string body)
        {
            ToAddress = toAddress;
            Subject = subject;
            Body = body;
        }
    }
}
