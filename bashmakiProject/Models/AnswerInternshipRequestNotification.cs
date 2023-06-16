namespace bashmakiProject.Models;

public class AnswerInternshipRequestNotification
{
    public string FromWhomId { get; set; }
    public string ToWhomId { get; set; }
    public Role ToWhomRole { get; set; }
    public Role FromWhomRole { get; set; }
    public string InternshipId { get; set; }
}