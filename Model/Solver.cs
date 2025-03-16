using Gurobi;

namespace Model;

public class Solver
{
    public record Product(int Width, int Height, int Depth);

    public record Box(int Width, int Height, int Depth, int Price);

    public record Scenario(IReadOnlyList<Product> Products, IReadOnlyList<Box> BoxTypes);

    public double Solve(Scenario scenario)
    {
        var (products, boxTypes) = scenario;
        if (boxTypes.Distinct().Count() != boxTypes.Count)
        {
            boxTypes = boxTypes.Distinct().ToList();
        }

        var env = new GRBEnv(true);
        env.Start();
        var model = new GRBModel(env);

        var numBoxes = products.Count;

        // Product position as in Product a is in box Product b at Position xyz
        // we limit the number of boxes to the number of products
        var productPosition = new GRBVar[products.Count, numBoxes, 3];
        // product a is in box b
        var productInBox = new GRBVar[products.Count, numBoxes];
        // what type box a is
        var boxTypeSelection = new GRBVar[numBoxes, boxTypes.Count];
        // product a overlaps product b
        var productOverlaps = new GRBVar[products.Count, products.Count];


        for (var p = 0; p < products.Count; p++)
        {
            for (var b = 0; b < numBoxes; b++)
            {
                productInBox[p, b] = model.AddVar(0, 1, 0, GRB.BINARY, "product_{p}_in_box_{b}");
                for (var dimension = 0; dimension < 3; dimension++)
                {
                    productPosition[p, b, dimension] = model.AddVar(0, GRB.INFINITY, 0, GRB.INTEGER,
                        $"productPosition_{p}_{b}_{dimension}");
                }
            }
        }

        for (var b = 0; b < numBoxes; b++)
        {
            for (var t = 0; t < boxTypes.Count; t++)
            {
                boxTypeSelection[b, t] = model.AddVar(0, 1, 0, GRB.BINARY, $"boxTypeSelection_{b}_{t}");
            }
        }


        for (var p = 0; p < products.Count; p++)
        {
            for (var p2 = 0; p2 < products.Count; p2++)
            {
                productOverlaps[p, p2] = model.AddVar(0, 1, 0, GRB.BINARY, $"product_{p}_overlaps_{p2}");
            }
        }


        var objective = new GRBLinExpr();
        for (var b = 0; b < numBoxes; b++)
        {
            for (var t = 0; t < boxTypes.Count; t++)
            {
                objective += boxTypes[t].Price * boxTypeSelection[b, t];
            }
        }

        model.SetObjective(objective, GRB.MINIMIZE);

        model.AddConstraintProductFitsInBox(products, boxTypes, productPosition, boxTypeSelection, productInBox);
        model.AddConstraintProductIsUsed(productInBox);
        // model.AddProductOverlapConstraint(productInBox);

        model.Optimize();

        // Print results
        for (int b = 0; b < numBoxes; b++)
        {
            for (int t = 0; t < boxTypes.Count; t++)
            {
                if (boxTypeSelection[b, t].X > 0.5)
                {
                    Console.WriteLine($"Box {b} is of type {t}");
                }
            }
        }

        for (int p = 0; p < products.Count; p++)
        {
            for (int b = 0; b < numBoxes; b++)
            {
                if (productInBox[p, b].X > 0.5)
                {
                    Console.WriteLine(
                        $"Product {p} in Box {b} at ({productPosition[p, b, 0].X}, {productPosition[p, b, 1].X}, {productPosition[p, b, 2].X})");
                }
            }
        }

        return model.ObjVal;
    }
}

public static class ModelConstraintExtensions
{
    public static void AddConstraintProductFitsInBox(this GRBModel model,
        IReadOnlyList<Solver.Product> products, IReadOnlyList<Solver.Box> boxTypes,
        GRBVar[,,] productPosition, GRBVar[,] boxTypeSelection, GRBVar[,] productInBox)
    {
        for (var p = 0; p < productPosition.GetLength(0); p++)
        {
            for (var b = 0; b < productPosition.GetLength(1); b++)
            {
                for (var t = 0; t < boxTypeSelection.GetLength(1); t++)
                {
                    model.AddGenConstrIndicator(productInBox[p, b], 1,
                        productPosition[p, b, 0] + products[p].Width <= boxTypes[t].Width * boxTypeSelection[b, t],
                        $"product{p}_fits_in_box_{b}_with_type_{t}_width");
                    model.AddGenConstrIndicator(productInBox[p, b], 1,
                        productPosition[p, b, 1] + products[p].Height <= boxTypes[t].Height * boxTypeSelection[b, t],
                        $"product{p}_fits_in_box_{b}_with_type_{t}_height");
                    model.AddGenConstrIndicator(productInBox[p, b], 1,
                        productPosition[p, b, 2] + products[p].Depth <= boxTypes[t].Depth * boxTypeSelection[b, t],
                        $"product{p}_fits_in_box_{b}_with_type_{t}_depth");
                }
            }
        }
    }

    public static void AddConstraintProductIsUsed(this GRBModel model, GRBVar[,] productInBox)
    {
        for (var p = 0; p < productInBox.GetLength(0); p++)
        {
            var productUsed = new GRBLinExpr();
            for (var b = 0; b < productInBox.GetLength(1); b++)
            {
                productUsed += productInBox[p, b];
            }

            model.AddConstr(productUsed >= 1, $"product_{p}_used");
        }
    }
    //
    // public static void AddConstraintProductIsUsed(this GRBModel model, GRBVar[,] productP1BeforeP2,
    //     GRBVar[,,] productPosition, IReadOnlyList<Solver.Product> products)
    // {
    //     for (int p1 = 0; p1 < productP1BeforeP2.GetLength(0); p1++)
    //     {
    //         for (int p2 = 0; p2 < productP1BeforeP2.GetLength(1); p2++)
    //         {  
    //             if (p1 < p2)
    //             {
    //                 for (int b = 0; b < productPosition.GetLength(1); b++)
    //                 {
    //                     model.AddGenConstrIndicator(productP1BeforeP2[p1, p2], 1, productPosition[p1, b, 0]  <= productPosition[p2, b, 0] ,
    //                         $"overlap_lb_{p1}_{p2}_{b}");
    //                     model.AddGenConstrIndicator(productP1BeforeP2[p1, p2], 1, productPosition[p2, b, 0]  <= productPosition[p1, b, 0] + products[p1].Width,
    //                         $"overlap_ub_{p1}_{p2}_{b}");
    //                 }
    //
    //                 // Indicator constraint 2: If overlap = 1, then posP2 ≤ posP1 + Width
    //                 model.AddGenConstrIndicator(overlap, 1, posP2 <= posP1 + widthP1, $"overlap_ub_{p1}_{p2}_{b}");
    //
    //                 for (int b = 0; b < productPosition.GetLength(1); b++)
    //                 {
    //                     
    //                     model.AddConstr(productPosition[p1, b, 0] + products[p1].Width 
    //                                     <= productPosition[p2, b, 0] + (1 - productP1BeforeP2[p1,p2]) * 1000, $"no_overlap_x_{p1}_{p2}_{b}");
    //                     
    //                     model.AddConstr(
    //                         (productPosition[p1, b, 0] + products[p1].Width <= productPosition[p2, b, 0]) +
    //                         (productPosition[p2, b, 0] + products[p2].Width <= productPosition[p1, b, 0]) +
    //                         (productPosition[p1, b, 1] + products[p1].Height <= productPosition[p2, b, 1]) +
    //                         (productPosition[p2, b, 1] + products[p2].Height <= productPosition[p1, b, 1]) +
    //                         (productPosition[p1, b, 2] + products[p1].Depth <= productPosition[p2, b, 2]) +
    //                         (productPosition[p2, b, 2] + products[p2].Depth <= productPosition[p1, b, 2])
    //                         >= 1 - productP1BeforeP2[p1, p2], $"no_overlap_{p1}_{p2}_{b}");
    //                 }
    //             }
    //         }
    //     }
    // }
}