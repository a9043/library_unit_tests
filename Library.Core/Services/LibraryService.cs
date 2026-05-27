using Library.Core.Fines;
using Library.Core.Models;
using Library.Core.Repositories;
namespace Library.Core.Services;
public class LibraryService
{
    private readonly IBookRepository _bookRepo;
    private readonly IReaderRepository _readerRepo;
    private readonly IBorrowRecordRepository _borrowRepo;
    private readonly FineStrategyFactory _fineFactory;
    public LibraryService(IBookRepository bookRepo, IReaderRepository readerRepo, IBorrowRecordRepository borrowRepo, FineStrategyFactory fineFactory)
    {
        _bookRepo = bookRepo;
        _readerRepo = readerRepo;
        _borrowRepo = borrowRepo;
        _fineFactory = fineFactory;
    }
    public BorrowRecord BorrowBook(int bookId, int readerId)
    {
        var book = _bookRepo.GetById(bookId) ?? throw new InvalidOperationException("Книга не найдена");
        if (book.AvailableCopies <= 0) throw new InvalidOperationException("Нет доступных экземпляров");
        var reader = _readerRepo.GetById(readerId) ?? throw new InvalidOperationException("Читатель не найден");
        if (_borrowRepo.CountActiveByReader(readerId) >= 5) throw new InvalidOperationException("Превышен лимит (5 книг)");
        book.AvailableCopies--;
        _bookRepo.Update(book);
        var record = new BorrowRecord { BookId = bookId, ReaderId = readerId, BorrowDate = DateTime.Now, DueDate = DateTime.Now.AddDays(30), Status = BorrowStatus.Borrowed };
        return _borrowRepo.Add(record);
    }
    public BorrowRecord ReturnBook(int recordId)
    {
        var record = _borrowRepo.GetById(recordId) ?? throw new InvalidOperationException("Запись не найдена");
        if (record.Status == BorrowStatus.Returned) throw new InvalidOperationException("Книга уже возвращена");
        record.ReturnDate = DateTime.Now;
        record.Status = BorrowStatus.Returned;
        if (record.ReturnDate > record.DueDate)
        {
            var reader = _readerRepo.GetById(record.ReaderId)!;
            record.FineAmount = _fineFactory.GetStrategy(reader.ReaderType).CalculateFine(record.DueDate, record.ReturnDate.Value);
        }
        var book = _bookRepo.GetById(record.BookId)!;
        book.AvailableCopies++;
        _bookRepo.Update(book);
        _borrowRepo.Update(record);
        return record;
    }
}
