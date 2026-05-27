namespace Library.Core.Models;
public class Reader
{
    public int Id { get; set; }
    public string FirstName { get; set; } = "";
    public string LastName { get; set; } = "";
    public ReaderType ReaderType { get; set; }
}
public enum ReaderType { Student, Professor, Staff }
