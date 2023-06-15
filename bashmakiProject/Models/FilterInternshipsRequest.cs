namespace bashmakiProject.Models;

public class FilterInternshipsRequest
{
    public string? ComparisonString { get; set; }
    public Dictionary<Topic, bool> Topics { get; set; }
    public bool ExperienceDemanded { get; set; }
    public List<Internship> Internships { get; set; }

    public FilterInternshipsRequest()
    {
        Internships = new List<Internship>();
        ComparisonString = "";
        Topics = new Dictionary<Topic, bool>();
        foreach (var topic in Enum.GetValues<Topic>())
            Topics[topic] = true;
    }
}