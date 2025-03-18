using static Model.ThreeDSolverMultiBins;

namespace CLI;

public class ThreeDSolverMultiBinsScenarios
{
    public static Scenario Fill25X25 => new Scenario(
        Enumerable.Range(0, 4 * 4 * 4).Select(_ => new Product(25, 25, 25)).ToList(),
        [new BinType(100, 100, 100, 1)]
    );


    public static Scenario OnlyWithRot => new Scenario(
        [
            new Product(100, 100, 10),
            new Product(100, 90, 10),
            new Product(100, 90, 10),
            new Product(100, 90, 10),
            new Product(100, 90, 10),
            new Product(100, 90, 10),
            new Product(100, 90, 10),
            new Product(100, 90, 10),
            new Product(100, 90, 10),
            new Product(100, 90, 10),
            new Product(100, 90, 10),
        ],
        [new BinType(100, 100, 100, 1)]
    );

    public static Scenario Fill50X50TwoCubes => new Scenario(
        Enumerable.Range(0, 2 * 2 * 2 * 2).Select(_ => new Product(50, 50, 50)).ToList(),
        [new BinType(100, 100, 100, 1)]
    );


    public static Scenario OnlyWithRot2Bins => new Scenario(
        [
            new Product(100, 100, 10),
            new Product(100, 90, 10),
            new Product(100, 90, 10),
            new Product(100, 90, 10),
            new Product(100, 90, 10),
            new Product(100, 90, 10),
            new Product(100, 90, 10),
            new Product(100, 90, 10),
            new Product(100, 90, 10),
            new Product(100, 90, 10),
            new Product(100, 90, 10),

            new Product(100, 100, 10),
            new Product(100, 90, 10),
            new Product(100, 90, 10),
            new Product(100, 90, 10),
            new Product(100, 90, 10),
            new Product(100, 90, 10),
            new Product(100, 90, 10),
            new Product(100, 90, 10),
            new Product(100, 90, 10),
            new Product(100, 90, 10),
            new Product(100, 90, 10),
        ],
        [new BinType(100, 100, 100, 1)]
    );

    public static Scenario MultiBinSizes => new Scenario(
        [
            new Product(100, 100, 10),
            new Product(100, 90, 10),
            new Product(100, 90, 10),
            new Product(100, 90, 10),
            new Product(100, 90, 10),
            new Product(100, 90, 10),
            new Product(100, 90, 10),
            new Product(100, 90, 10),
            new Product(100, 90, 10),
            new Product(100, 90, 10),
            new Product(100, 90, 10),

            new Product(100, 100, 10),
            new Product(100, 90, 10),
            new Product(100, 90, 10),
            new Product(100, 90, 10),
            new Product(100, 90, 10),
            new Product(100, 90, 10),
            new Product(100, 90, 10),
            new Product(100, 90, 10),
            new Product(100, 90, 10),
            new Product(100, 90, 10),
            new Product(100, 90, 10),
        ],
        [new BinType(100, 90, 10, 1), new BinType(100, 100, 100, 1)]
    );

    public static Scenario Randoms(int numProducts, int numBins) => new Scenario(
        Enumerable.Range(0, numProducts)
            .Select(_ =>
                new Product(
                    Width: Random.Shared.Next(20, 50),
                    Height: Random.Shared.Next(20, 50),
                    Depth: Random.Shared.Next(20, 50)))
            .ToList(),
        Enumerable.Range(0, numBins)
            .Select(_ =>
            {
                var w = Random.Shared.Next(50, 100);
                var h = Random.Shared.Next(50, 100);
                var d = Random.Shared.Next(50, 100);
                var cost = w * d * h;
                return new BinType(
                    Width: w,
                    Height: d,
                    Depth: h,
                    Cost: cost);
            })
            .ToList()
    );
}