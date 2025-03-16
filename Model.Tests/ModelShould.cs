namespace Model.Tests;

public class ModelShould
{
    private readonly Solver solver = new Solver();

    [Fact]
    public void SolveOneProduct()
    {
        var oneProduct = new Solver.Scenario(
            [new Solver.Product(Width: 1, Height: 1, Depth: 1)],
            [new Solver.Box(Width: 1, Height: 1, Depth: 1, Price: 1)]);

        var optimum = solver.Solve(oneProduct);
        Assert.Equal(1.0, optimum);
    }
}