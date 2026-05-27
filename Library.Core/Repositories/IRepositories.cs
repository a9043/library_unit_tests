using Library.Core.Models;
namespace Library.Core.Repositories;
public interface IBookRepository
{
    Book? GetById(int id);
    void Update(Book book);
}
public interface IReaderRepository
{
    Reader? GetById(int id);
}
public interface IBorrowRecordRepository
{
    BorrowRecord? GetById(int id);
    int CountActiveByReader(int readerId);
    BorrowRecord Add(BorrowRecord record);
    void Update(BorrowRecord record);
}
