using static Model.TwoDSolver;

namespace CLI;

public class TwoDScenarios
{
    public static Scenario RandomScenario(int numRects) =>
        new(
            Enumerable.Range(0, numRects)
                .Select(_ => new Product(Random.Shared.Next(1, 20), Random.Shared.Next(1, 20)))
                .ToList(),
            new Space(100, 100));

    public static Scenario TenXTenFill() =>
        new(
            Enumerable.Range(0, 100)
                .Select(_ => new Product(10, 10))
                .ToList(),
            new Space(100, 100));

    public static Scenario TenXTwentyFill() =>
        new(
            Enumerable.Range(0, 50)
                .Select(_ => new Product(10, 20))
                .ToList(),
            new Space(100, 100));


    public static Scenario FillOnlyWithRot() =>
        new(
            [
                new Product(100, 10),
                new Product(90, 10),
                new Product(90, 10),
                new Product(90, 10),
                new Product(90, 10),
                new Product(90, 10),
                new Product(90, 10),
                new Product(90, 10),
                new Product(90, 10),
                new Product(90, 10),
                new Product(90, 10),
            ],
            new Space(100, 100));
}