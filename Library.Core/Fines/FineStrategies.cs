namespace Library.Core.Fines;
public class StandardFineStrategy : IFineCalculationStrategy
{
    public string StrategyName => "Стандартный (10 руб/день)";
    public decimal CalculateFine(DateTime dueDate, DateTime returnDate)
    {
        if (returnDate <= dueDate) return 0;
        return (decimal)(returnDate.Date - dueDate.Date).Days * 10;
    }
}
public class ProfessorFineStrategy : IFineCalculationStrategy
{
    public string StrategyName => "Льготный для преподавателей (5 руб/день)";
    public decimal CalculateFine(DateTime dueDate, DateTime returnDate)
    {
        if (returnDate <= dueDate) return 0;
        return (decimal)(returnDate.Date - dueDate.Date).Days * 5;
    }
}
public class NoFineStrategy : IFineCalculationStrategy
{
    public string StrategyName => "Без штрафа";
    public decimal CalculateFine(DateTime dueDate, DateTime returnDate) => 0;
}
