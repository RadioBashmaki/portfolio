namespace bashmakiProject.Models;

public class FilterStudentsRequest
{
    public string ComparisonString { get; set; } = "";
    public List<Student> Students { get; set; } = new();
}

public class Student
{
    public User User { get; set; }
    public int ProjectCount { get; set; }
}