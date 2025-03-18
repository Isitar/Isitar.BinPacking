using CLI;
using Model;

var oneProduct = new Solver.Scenario(
    [new Solver.Product(Width: 1, Height: 1, Depth: 1)],
    [new Solver.Box(Width: 1, Height: 1, Depth: 1, Price: 1)]);

var sameProductFillsOneBox = new Solver.Scenario(
    Products:
    [
        new Solver.Product(Width: 1, Height: 1, Depth: 1),
        new Solver.Product(Width: 1, Height: 1, Depth: 1),
        new Solver.Product(Width: 1, Height: 1, Depth: 1)
    ],
    BoxTypes: [new Solver.Box(Width: 1, Height: 1, Depth: 1, Price: 1)]);


// var solver = new OneDSolver();
// var result = solver.Solve(OneDScenarios.Big);
if (false)
{
    var twoDSolver = new TwoDSolver();
    // var result = twoDSolver.Solve(TwoDScenarios.RandomScenario(80));
    var result = twoDSolver.Solve(TwoDScenarios.FillOnlyWithRot());
    Draw2DSolution.Draw(result);
}

if (false)
{
    var solver = new ThreeDSolver();
    var result = solver.Solve(ThreeDScenarios.Randoms(15));

    Draw3DSolution.Draw(result);
}

if (true)
{
    var solver = new ThreeDSolverMultiBins();
    var result = solver.Solve(ThreeDSolverMultiBinsScenarios.Randoms(15,4));
    Draw3DMultiBinSolution.Draw(result);
}


// Console.WriteLine($"Result: {result}");