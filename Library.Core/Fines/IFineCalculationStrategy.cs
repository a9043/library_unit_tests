namespace Library.Core.Fines;
public interface IFineCalculationStrategy
{
    decimal CalculateFine(DateTime dueDate, DateTime returnDate);
    string StrategyName { get; }
}
