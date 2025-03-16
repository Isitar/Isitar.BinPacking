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

var solver = new TwoDSolver();
var result = solver.Solve(TwoDScenarios.FillOnlyWithRot());
// new TwoDSolver.Scenario([
//     new TwoDSolver.Product(10, 10),
//     new TwoDSolver.Product(10, 10),
//     new TwoDSolver.Product(20, 10),
//     new TwoDSolver.Product(10, 10),
//     new TwoDSolver.Product(10, 45),
//     new TwoDSolver.Product(10, 10),
//     new TwoDSolver.Product(10, 10),
//     new TwoDSolver.Product(10, 10),
//     new TwoDSolver.Product(30, 30),
//     new TwoDSolver.Product(10, 10),
//     new TwoDSolver.Product(10, 10),
//     new TwoDSolver.Product(10, 10),
//     new TwoDSolver.Product(10, 10),
//     new TwoDSolver.Product(15, 10),
//     new TwoDSolver.Product(15, 46),
//     new TwoDSolver.Product(10, 10),
// ], new TwoDSolver.Space(100, 100)));

Draw2DSolution.Draw(result);

// Console.WriteLine($"Result: {result}");