namespace Library.Core.Models;
public class BorrowRecord
{
    public int Id { get; set; }
    public int BookId { get; set; }
    public int ReaderId { get; set; }
    public DateTime BorrowDate { get; set; }
    public DateTime DueDate { get; set; }
    public DateTime? ReturnDate { get; set; }
    public BorrowStatus Status { get; set; }
    public decimal FineAmount { get; set; }
}
public enum BorrowStatus { Borrowed, Returned }
