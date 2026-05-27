using Library.Core.Models;
namespace Library.Core.Fines;
public class FineStrategyFactory
{
    public IFineCalculationStrategy GetStrategy(ReaderType readerType) => readerType switch
    {
        ReaderType.Professor => new ProfessorFineStrategy(),
        ReaderType.Staff => new NoFineStrategy(),
        _ => new StandardFineStrategy()
    };
}
