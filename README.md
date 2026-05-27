# Library Unit Tests

Unit-тесты сервисов электронной библиотеки. Лабораторная работа №1 «Тестирование ПО».

## Стек

.NET 10
- xUnit
- Moq
- FluentAssertions
- coverlet

## Запуск

```bash
dotnet test
```

## Покрытие
```bash
dotnet test --collect:"XPlat Code Coverage"
reportgenerator -reports:"Library.Tests\TestResults\*\coverage.cobertura.xml" -targetdir:"coveragereport" -reporttypes:Html
```

## Результаты
17 тестов
пройдено 17
покрытие ~95%
