using Gurobi;

namespace Model;

public class ThreeDSolver
{
    public record Product(int Width, int Height, int Depth);

    public record Space(int Width, int Height, int Depth);

    public record Scenario(IReadOnlyList<Product> Products, Space Space);

    public record Point(double X, double Y, double Z);

    public record SolvedProduct(Product Product, Point Point, int Position)
    {
        public double AdjustedWidth = Position switch
        {
            0 => Product.Width,
            1 => Product.Height,
            2 => Product.Depth,
            3 => Product.Depth,
            4 => Product.Width,
            5 => Product.Height,
        };

        public double AdjustedDepth = Position switch
        {
            0 => Product.Depth,
            1 => Product.Depth,
            2 => Product.Width,
            3 => Product.Height,
            4 => Product.Height,
            5 => Product.Width,
        };

        public double AdjustedHeight = Position switch
        {
            0 => Product.Height,
            1 => Product.Width,
            2 => Product.Height,
            3 => Product.Width,
            4 => Product.Depth,
            5 => Product.Depth,
        };
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
        var productPositionsZ = model.AddVars(products.Count, GRB.CONTINUOUS);
        var rotation1 = model.AddVars(products.Count, GRB.BINARY);
        var rotation2 = model.AddVars(products.Count, GRB.BINARY);
        var rotation3 = model.AddVars(products.Count, GRB.BINARY);
        var rotation4 = model.AddVars(products.Count, GRB.BINARY);
        var rotation5 = model.AddVars(products.Count, GRB.BINARY);


        var productXIsLeftOfY = new GRBVar[products.Count, products.Count];
        var productXIsBelowOfY = new GRBVar[products.Count, products.Count];
        var productXIsBehindOfY = new GRBVar[products.Count, products.Count];
        for (var p = 0; p < products.Count; p++)
        {
            for (var p2 = 0; p2 < products.Count; p2++)
            {
                productXIsLeftOfY[p, p2] = model.AddVar(0, 1, 0, GRB.BINARY, "product_{p}_left_of_{p2}");
                productXIsBelowOfY[p, p2] = model.AddVar(0, 1, 0, GRB.BINARY, "product_{p}_belof_of_{p2}");
                productXIsBehindOfY[p, p2] = model.AddVar(0, 1, 0, GRB.BINARY, "product_{p}_behind_of_{p2}");
            }
        }

        for (var p = 0; p < products.Count; p++)
        {
            var product = products[p];

            // only one rotation allowed
            model.AddConstr(rotation1[p]
                            + rotation2[p]
                            + rotation3[p]
                            + rotation4[p]
                            + rotation5[p]
                            <= 1, $"only_one_rotation_for_{p}");

            var width = product.Width * (1 - rotation1[p] - rotation2[p] - rotation3[p] - rotation5[p])
                        + product.Depth * (rotation2[p] + rotation3[p])
                        + product.Height * (rotation1[p] + rotation5[p]);

            var depth = products[p].Depth * (1 - rotation2[p] - rotation3[p] - rotation4[p] - rotation5[p])
                        + product.Width * (rotation2[p] + rotation5[p])
                        + product.Height * (rotation3[p] + rotation4[p]);

            var height = products[p].Height * (1 - rotation1[p] - rotation3[p] - rotation4[p] - rotation5[p])
                         + product.Width * (rotation1[p] + rotation3[p])
                         + product.Depth * (rotation4[p] + rotation5[p]);

            model.AddConstr(productPositionsX[p] + width <= space.Width, $"product_{p}_within_x_space");
            model.AddConstr(productPositionsY[p] + depth <= space.Depth, $"product_{p}_within_y_space");
            model.AddConstr(productPositionsZ[p] + height <= space.Height, $"product_{p}_within_z_space");

            for (var p2 = 0; p2 < products.Count; p2++)
            {
                if (p >= p2) continue;

                model.AddGenConstrIndicator(productXIsLeftOfY[p, p2], 1,
                    productPositionsX[p] + width <= productPositionsX[p2], $"product_{p}_left_of_{p2}");


                model.AddGenConstrIndicator(productXIsBehindOfY[p, p2], 1,
                    productPositionsY[p] + depth <= productPositionsY[p2], $"product_{p}_behind_of_{p2}");

                model.AddGenConstrIndicator(productXIsBelowOfY[p, p2], 1,
                    productPositionsZ[p] + height <= productPositionsZ[p2], $"product_{p}_below_of_{p2}");

                model.AddConstr(
                    productXIsLeftOfY[p, p2] + productXIsBehindOfY[p, p2] + productXIsBelowOfY[p, p2] >= 1,
                    $"no_overlap_between_{p}_{p2}");
            }
        }

        model.Optimize();

        for (var p = 0; p < products.Count; p++)
        {
            Console.WriteLine(
                $"Product {p} at ({productPositionsX[p].X}, {productPositionsY[p].X}, {productPositionsZ[p].X}), rotation: {0}");
        }

        return new Solution(products
            .Select((p, i) => new SolvedProduct(p,
                new Point(productPositionsX[i].X, productPositionsY[i].X, productPositionsZ[i].X),
                rotation1[i].X > 0.5 ? 1
                : rotation2[i].X > 0.5 ? 2
                : rotation3[i].X > 0.5 ? 3
                : rotation4[i].X > 0.5 ? 4
                : rotation5[i].X > 0.5 ? 5
                : 0
            ))
            .ToList(), space);
    }
}