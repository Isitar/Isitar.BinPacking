using static Model.ThreeDSolver;

namespace CLI;

public class ThreeDScenarios
{
    public static Scenario Fill25X25 => new Scenario(
        Enumerable.Range(0, 4*4*4).Select(_ => new Product(25,25,25)).ToList(),
        new Space(100, 100, 100)
    );
    
    
    public static Scenario OnlyWithRot => new Scenario(
        [
            new Product(100,100,10),
            new Product(100,90,10),
            new Product(100,90,10),
            new Product(100,90,10),
            new Product(100,90,10),
            new Product(100,90,10),
            new Product(100,90,10),
            new Product(100,90,10),
            new Product(100,90,10),
            new Product(100,90,10),
            new Product(100,90,10),
        ],
        new Space(100, 100, 100)
    );
}