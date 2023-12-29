using System.Collections.Generic;

namespace med.common.library.model
{
    public class EmailNotificationEvent : DomainEvent
    {
        public EmailNotificationEvent(string subject, string to, string message, List<string> cc = default)
        {
            Subject = subject;
            CC = cc;
            To = to;
            Message = message;
        }

        public string Subject { get; }
        public List<string> CC { get; }
        public string To { get; }
        public string Message { get; }
    }
    public class EmailViewModel
    {
        public string EmailTo { get; private set; }
        public string Subject { get; private set; }
        public string Content { get; private set; }
        public int ContentType { get; private set; }
        public List<string> EmailCC { get; private set; }
        public EmailViewModel(string emailTo, string subject, string content, int contentType, List<string> emailCC = null)
        {
            this.Content = content;
            this.ContentType = contentType;
            this.EmailCC = emailCC ?? new List<string>();
            this.EmailTo = emailTo;
            this.Subject = subject;

        }
    }
    public class EmailConfiguration
    {
        public string EmailDisplayName { get; set; }
        public string EmailFrom { get; set; }
        public string Server { get; set; }
        public string Port { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
    }


}
