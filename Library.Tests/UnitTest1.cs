﻿using FluentAssertions;
using Library.Core.Fines;
using Library.Core.Models;
using Library.Core.Repositories;
using Library.Core.Services;
using Moq;

namespace Library.Tests.Services;

public class LibraryServiceTests
{
    private readonly Mock<IBookRepository> _bookRepoMock = new();
    private readonly Mock<IReaderRepository> _readerRepoMock = new();
    private readonly Mock<IBorrowRecordRepository> _borrowRepoMock = new();
    private readonly LibraryService _sut;

    public LibraryServiceTests()
    {
        _sut = new LibraryService(
            _bookRepoMock.Object,
            _readerRepoMock.Object,
            _borrowRepoMock.Object,
            new FineStrategyFactory());
    }

    // Выдача книг
    [Fact]
    public void BorrowBook_Should_Decrease_AvailableCopies()
    {
        var book = new Book { Id = 1, AvailableCopies = 3 };
        _bookRepoMock.Setup(r => r.GetById(1)).Returns(book);
        _readerRepoMock.Setup(r => r.GetById(1)).Returns(new Reader());
        _borrowRepoMock.Setup(r => r.CountActiveByReader(1)).Returns(0);
        _borrowRepoMock.Setup(r => r.Add(It.IsAny<BorrowRecord>()))
            .Returns((BorrowRecord r) => r);

        _sut.BorrowBook(1, 1);

        book.AvailableCopies.Should().Be(2);
        _bookRepoMock.Verify(r => r.Update(book), Times.Once);
    }

    [Fact]
    public void BorrowBook_Should_Throw_When_No_Copies()
    {
        _bookRepoMock.Setup(r => r.GetById(1))
            .Returns(new Book { AvailableCopies = 0 });

        var act = () => _sut.BorrowBook(1, 1);
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*доступных экземпляров*");
    }

    [Fact]
    public void BorrowBook_Should_Throw_When_Book_Not_Found()
    {
        _bookRepoMock.Setup(r => r.GetById(1)).Returns((Book?)null);

        var act = () => _sut.BorrowBook(1, 1);
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*Книга не найдена*");
    }

    [Fact]
    public void BorrowBook_Should_Throw_When_Reader_Not_Found()
    {
        _bookRepoMock.Setup(r => r.GetById(1)).Returns(new Book { AvailableCopies = 1 });
        _readerRepoMock.Setup(r => r.GetById(1)).Returns((Reader?)null);

        var act = () => _sut.BorrowBook(1, 1);
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*Читатель не найден*");
    }

    [Fact]
    public void BorrowBook_Should_Throw_When_Limit_Exceeded()
    {
        _bookRepoMock.Setup(r => r.GetById(1)).Returns(new Book { AvailableCopies = 1 });
        _readerRepoMock.Setup(r => r.GetById(1)).Returns(new Reader());
        _borrowRepoMock.Setup(r => r.CountActiveByReader(1)).Returns(5);

        var act = () => _sut.BorrowBook(1, 1);
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*лимит*");
    }

    // Возврат книг
    [Fact]
    public void ReturnBook_Should_Calculate_Fine_When_Overdue()
    {
        var record = new BorrowRecord
        {
            Id = 1,
            Status = BorrowStatus.Borrowed,
            DueDate = DateTime.Now.AddDays(-5),
            BookId = 1,
            ReaderId = 1
        };
        _borrowRepoMock.Setup(r => r.GetById(1)).Returns(record);
        _readerRepoMock.Setup(r => r.GetById(1))
            .Returns(new Reader { ReaderType = ReaderType.Student });
        _bookRepoMock.Setup(r => r.GetById(1))
            .Returns(new Book { AvailableCopies = 0 });

        var result = _sut.ReturnBook(1);

        result.FineAmount.Should().BeGreaterThan(0);
        result.Status.Should().Be(BorrowStatus.Returned);
    }

    [Fact]
    public void ReturnBook_Should_Throw_When_Record_Not_Found()
    {
        _borrowRepoMock.Setup(r => r.GetById(1)).Returns((BorrowRecord?)null);

        var act = () => _sut.ReturnBook(1);
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*Запись не найдена*");
    }

    [Fact]
    public void ReturnBook_Should_Throw_When_Already_Returned()
    {
        var record = new BorrowRecord { Id = 1, Status = BorrowStatus.Returned };
        _borrowRepoMock.Setup(r => r.GetById(1)).Returns(record);

        var act = () => _sut.ReturnBook(1);
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*уже возвращена*");
    }

    [Fact]
    public void ReturnBook_Should_Increase_AvailableCopies()
    {
        var book = new Book { Id = 1, AvailableCopies = 0 };
        var record = new BorrowRecord
        {
            Id = 1,
            Status = BorrowStatus.Borrowed,
            DueDate = DateTime.Now.AddDays(5),
            BookId = 1,
            ReaderId = 1
        };
        _borrowRepoMock.Setup(r => r.GetById(1)).Returns(record);
        _readerRepoMock.Setup(r => r.GetById(1)).Returns(new Reader());
        _bookRepoMock.Setup(r => r.GetById(1)).Returns(book);

        _sut.ReturnBook(1);

        book.AvailableCopies.Should().Be(1);
        _bookRepoMock.Verify(r => r.Update(book), Times.Once);
    }
}

// Тесты стратегий штрафов
public class FineStrategyTests
{
    [Fact]
    public void Standard_Should_Charge_10_Per_Day()
    {
        var strategy = new StandardFineStrategy();
        var due = DateTime.Today.AddDays(-3);
        var returned = DateTime.Today;

        var fine = strategy.CalculateFine(due, returned);

        fine.Should().Be(30);
    }

    [Fact]
    public void Standard_Should_Return_Zero_When_Not_Overdue()
    {
        var strategy = new StandardFineStrategy();
        var due = DateTime.Today.AddDays(5);
        var returned = DateTime.Today;

        var fine = strategy.CalculateFine(due, returned);

        fine.Should().Be(0);
    }

    [Fact]
    public void Professor_Should_Charge_5_Per_Day()
    {
        var strategy = new ProfessorFineStrategy();
        var due = DateTime.Today.AddDays(-4);
        var returned = DateTime.Today;

        var fine = strategy.CalculateFine(due, returned);

        fine.Should().Be(20);
    }

    [Fact]
    public void Professor_Should_Return_Zero_When_Not_Overdue()
    {
        var strategy = new ProfessorFineStrategy();
        var due = DateTime.Today.AddDays(5);
        var returned = DateTime.Today;

        var fine = strategy.CalculateFine(due, returned);

        fine.Should().Be(0);
    }

    [Fact]
    public void NoFine_Should_Always_Return_Zero()
    {
        var strategy = new NoFineStrategy();

        var fine1 = strategy.CalculateFine(DateTime.Today.AddDays(-10), DateTime.Today);
        var fine2 = strategy.CalculateFine(DateTime.Today.AddDays(10), DateTime.Today);

        fine1.Should().Be(0);
        fine2.Should().Be(0);
    }

    [Fact]
    public void StrategyName_Should_Not_Be_Empty()
    {
        new StandardFineStrategy().StrategyName.Should().NotBeEmpty();
        new ProfessorFineStrategy().StrategyName.Should().NotBeEmpty();
        new NoFineStrategy().StrategyName.Should().NotBeEmpty();
    }
}

// Тесты фабрики стратегий
public class FineStrategyFactoryTests
{
    [Fact]
    public void Should_Return_Standard_For_Student()
    {
        var factory = new FineStrategyFactory();
        var strategy = factory.GetStrategy(ReaderType.Student);
        strategy.Should().BeOfType<StandardFineStrategy>();
    }

    [Fact]
    public void Should_Return_Professor_For_Professor()
    {
        var factory = new FineStrategyFactory();
        var strategy = factory.GetStrategy(ReaderType.Professor);
        strategy.Should().BeOfType<ProfessorFineStrategy>();
    }

    [Fact]
    public void Should_Return_NoFine_For_Staff()
    {
        var factory = new FineStrategyFactory();
        var strategy = factory.GetStrategy(ReaderType.Staff);
        strategy.Should().BeOfType<NoFineStrategy>();
    }
}