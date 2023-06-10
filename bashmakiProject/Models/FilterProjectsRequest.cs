namespace bashmakiProject.Models;

public class FilterProjectsRequest
{
    public string? ComparisonString { get; set; }
    public Dictionary<Topic, bool> Topics { get; set; }
    public List<Project> Projects { get; set; }

    public FilterProjectsRequest()
    {
        Projects = new List<Project>();
        ComparisonString = "";
        Topics = new Dictionary<Topic, bool>();
        foreach (var topic in Enum.GetValues<Topic>())
            Topics[topic] = true;
    }
}