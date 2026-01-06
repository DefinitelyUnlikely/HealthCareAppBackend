namespace HealthCareAB_v1.Services.Interfaces;

public interface IEmailService
{
    Task SendEmailAsync(Email email);

    public class Email
    {
        private string _to = string.Empty;
        private string _subject = string.Empty;

        public required string To
        {
            get => _to;
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                {
                    throw new ArgumentException("Value cannot be null or whitespace.", nameof(value));
                }

                _to = value;
            }
        }

        public required string Subject
        {
            get => _subject;
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                {
                    value = "No subject";
                }

                _subject = value;
            }
        }

        public required string PlainContent { get; set; }
        public string? HtmlContent { get; set; }
    }
}
