using Gurobi;

namespace Model;

public class TwoDSolver
{
    public record Product(int Width, int Height);

    public record Space(int Width, int Height);

    public record Scenario(IReadOnlyList<Product> Products, Space Space);

    public record Point(double X, double Y);

    public record SolvedProduct(Product Product, Point Point, bool Rotation)
    {
        public int AdjustedWidth => Rotation ? Product.Height : Product.Width;
        public int AdjustedHeight => Rotation ? Product.Width : Product.Height;
    }

    public record Solution(IReadOnlyList<SolvedProduct> SolvedProducts, Space Space);

    public Solution Solve(Scenario scenario)
    {
        var (products, space) = scenario;

        var env = new GRBEnv(true);
        env.Start();
        var model = new GRBModel(env);

        var productPositionsX = model.AddVars(products.Count, GRB.CONTINUOUS);
        var productPositionsY = model.AddVars(products.Count, GRB.CONTINUOUS);
        var rotation = model.AddVars(products.Count, GRB.BINARY);
        
        
        var productXIsLeftOfY = new GRBVar[products.Count, products.Count];
        var productXIsBelowOfY = new GRBVar[products.Count, products.Count];
        for (var p = 0; p < products.Count; p++)
        {
            for (var p2 = 0; p2 < products.Count; p2++)
            {
                productXIsLeftOfY[p, p2] = model.AddVar(0, 1, 0, GRB.BINARY, "product_{p}_left_of_{p2}");
                productXIsBelowOfY[p, p2] = model.AddVar(0, 1, 0, GRB.BINARY, "product_{p}_belof_of_{p2}");
            }
        }

        for (var p = 0; p < products.Count; p++)
        {
            var width = products[p].Width * (1 - rotation[p]) + rotation[p] * products[p].Height;
            var height = products[p].Height * (1 - rotation[p]) + rotation[p] * products[p].Width;
            
            model.AddConstr(productPositionsX[p] + width <= space.Width, $"product_{p}_within_x_space");
            model.AddConstr(productPositionsY[p] + height <= space.Height, $"product_{p}_within_y_space");

            for (var p2 = 0; p2 < products.Count; p2++)
            {
                if (p >= p2) continue;

                model.AddGenConstrIndicator(productXIsLeftOfY[p, p2], 1,
                    productPositionsX[p] + width <= productPositionsX[p2], $"product_{p}_left_of_{p2}");

                model.AddGenConstrIndicator(productXIsBelowOfY[p, p2], 1,
                    productPositionsY[p] + height <= productPositionsY[p2], $"product_{p}_belof_of_{p2}");

                model.AddConstr(
                    productXIsLeftOfY[p, p2] + productXIsBelowOfY[p, p2] >= 1,
                    $"no_overlap_between_{p}_{p2}");
            }
        }

        model.Optimize();

        for (var p = 0; p < products.Count; p++)
        {
            Console.WriteLine(
                $"Product {p} at ({productPositionsX[p].X}, {productPositionsY[p].X}), rotation: {rotation[p].X > 0.5}");
        }

        return new Solution(products
            .Select((p, i) => new SolvedProduct(p, new Point(productPositionsX[i].X, productPositionsY[i].X), rotation[i].X > 0.5))
            .ToList(), space);
    }
}