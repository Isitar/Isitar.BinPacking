using Gurobi;

namespace Model;

public class OneDSolver
{
    public record Product(int Width);

    public record Scenario(IReadOnlyList<Product> Products);

    public double Solve(Scenario scenario)
    {
        var env = new GRBEnv(true);
        env.Start();
        var model = new GRBModel(env);

        var productPositions = model.AddVars(scenario.Products.Count, GRB.CONTINUOUS);
        var productOrder = model.AddVars(scenario.Products.Count, GRB.BINARY);
        var maxWidth = model.AddVar(0, GRB.INFINITY, 0, GRB.CONTINUOUS, "maxWidth");

        for (var p1 = 0; p1 < scenario.Products.Count; p1++)
        {
            for (var p2 = 0; p2 < scenario.Products.Count; p2++)
            {
                if (p1 < p2)
                {

                    model.AddGenConstrIndicator(productOrder[p1], 1,
                        productPositions[p2] >= productPositions[p1] + scenario.Products[p1].Width, $"p{p1}_before_{p2}");
                    model.AddGenConstrIndicator(productOrder[p1], 0,
                        productPositions[p1] >= productPositions[p2] + scenario.Products[p2].Width, $"p{p2}_before_{p1}");
                }
            }
        }

        for (var p = 0; p < scenario.Products.Count; p++)
        {
            model.AddConstr(maxWidth >= productPositions[p] + scenario.Products[p].Width, $"maxWidth_gt_{p}");
        }


        model.SetObjective(1 * maxWidth, GRB.MINIMIZE);

        model.Optimize();

        for (var p = 0; p < scenario.Products.Count; p++)
        {
            Console.WriteLine($"Product {p} at ({productPositions[p].X}, {productPositions[p].X + scenario.Products[p].Width})");
        }

        for (var p1 = 0; p1 < scenario.Products.Count; p1++)
        {
            for (var p2 = 0; p2 < scenario.Products.Count; p2++)
            {
                if (p1 < p2)
                {
                    Console.WriteLine($"Productorder: {productOrder[p1].X}");
                }
            }
        }

        return model.ObjVal;
    }
}