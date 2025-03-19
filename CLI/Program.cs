using CLI;
using CromulentBisgetti.ContainerPacking;
using CromulentBisgetti.ContainerPacking.Algorithms;
using CromulentBisgetti.ContainerPacking.Entities;
using Model;

if (false)
{
    var solver = new OneDSolver();
    var result = solver.Solve(OneDScenarios.Big);
}

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

if (false)
{
    var solver = new ThreeDSolverMultiBins();
    var result = solver.Solve(ThreeDSolverMultiBinsScenarios.Randoms(15, 4));
    Draw3DMultiBinSolution.Draw(result);
}

if (true)
{
    var binTypes = new List<ThreeDSolverMultiBins.BinType>
    {
        new(Width: 230, Height: 108, Depth: 190, Cost: 10),
        new(Width: 344, Height: 108, Depth: 244, Cost: 15),
        new(Width: 394, Height: 238, Depth: 344, Cost: 50),
        new(Width: 515, Height: 315, Depth: 395, Cost: 55),
        new(Width: 495, Height: 415, Depth: 615, Cost: 60),
        new(Width: 525, Height: 394, Depth: 735, Cost: 70),
        new(Width: 745, Height: 455, Depth: 555, Cost: 80),
        new(Width: 965, Height: 255, Depth: 365, Cost: 90),
        new(Width: 990, Height: 572, Depth: 588, Cost: 100)
    };

    var productsByParcel = new Dictionary<long, List<ThreeDSolverMultiBins.Product>>();
    foreach (var line in File.ReadAllLines("mpo_jan25.txt"))
    {
        if (line.Contains("Id"))
        {
            continue;
        }

        var converToInt = (string s) => (int)Math.Ceiling(double.Parse(s) * 1000);

        var split = line.Split(";");
        var parcelId = long.Parse(split[0]);

        var productId = split[1];
        var width = converToInt(split[2]);
        var height = converToInt(split[3]);
        var length = converToInt(split[4]);

        if (!productsByParcel.ContainsKey(parcelId))
        {
            productsByParcel[parcelId] = new List<ThreeDSolverMultiBins.Product>();
        }

        productsByParcel[parcelId].Add(new ThreeDSolverMultiBins.Product(width, height, length));
    }

    var scenarios = productsByParcel
        .Select(kvp => new ThreeDSolverMultiBins.Scenario(kvp.Value, binTypes, kvp.Key.ToString())).ToList();

    Console.WriteLine($"Found {scenarios.Count} scenarios");
    var solver = new ThreeDSolverMultiBins();

    foreach (var scenario in scenarios)
    {
        try
        {
            if (scenario.Products.Count > 30)
            {
                continue;
            }

            Console.WriteLine($"Solving scenario with {scenario.Products.Count} products");
            var ilpSolution = solver.Solve(scenario);
            Draw3DMultiBinSolution.Draw(ilpSolution, $"{scenario.Name}.obj", 0.01);
            var chapmanSol = PackingService.Pack(
                scenario.BinTypes.Select((bt, i) => new Container(i, bt.Width, bt.Depth, bt.Height)).ToList(),
                scenario.Products.Select((p, i) => new Item(i, p.Width, p.Depth, p.Height, 1)).ToList(),
                [(int)AlgorithmType.EB_AFIT]
            );
            var usedSolutionInDg = chapmanSol
                .Where(pc => pc.AlgorithmPackingResults.Single().IsCompletePack)
                .OrderByDescending(pc => pc.AlgorithmPackingResults.Single().PercentContainerVolumePacked)
                .FirstOrDefault();

            var usedSolutionInDgBinType = usedSolutionInDg is null ? null : binTypes[usedSolutionInDg.ContainerID];

            var ilpCost = ilpSolution.UsedBins.Sum(b => b.BinType.Cost);
            var dgCost = usedSolutionInDgBinType?.Cost ?? 0;

            File.AppendAllLines("comparison.csv", [$"{scenario.Name};{ilpCost};{dgCost};{dgCost - ilpCost}"]);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
    }
}