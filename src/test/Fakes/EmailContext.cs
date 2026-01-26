namespace LowlandTech.TinyTools.UnitTests.Fakes;

internal class EmailContext
{
    public string SenderName { get; set; } = null!;
    public string SenderEmail { get; set; } = null!;
    public string SenderTitle { get; set; } = null!;
    public string CompanyName { get; set; } = null!;
    public string Subject { get; set; } = null!;
    public Customer Recipient { get; set; } = null!;
    public DateTime SentDate { get; set; }
}
