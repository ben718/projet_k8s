namespace Events
{
    public class ApplicantAppliedEvent : IntegrationEvent
    {
        public int JobId { get; set; }
        public int ApplicantId { get; set; }
        public string Title { get; set; }

        public ApplicantAppliedEvent(int jobId, int applicantId, string title)
        {
            JobId = jobId;
            ApplicantId = applicantId;
            Title = title;
        }
        
        // Constructeur sans paramètres nécessaire pour MassTransit
        public ApplicantAppliedEvent()
        {
        }
    }
}
