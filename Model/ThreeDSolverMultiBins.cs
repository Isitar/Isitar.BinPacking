using Gurobi;

namespace Model;

public class ThreeDSolverMultiBins
{
    public record Product(int Width, int Height, int Depth);

    public record BinType(int Width, int Height, int Depth, int Cost);

    public record Scenario(IReadOnlyList<Product> Products, IReadOnlyList<BinType> BinTypes, string? Name = null);

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

    public record UsedBin(BinType BinType, IReadOnlyList<SolvedProduct> SolvedProducts);

    public record Solution(IReadOnlyList<UsedBin> UsedBins);

    public Solution Solve(Scenario scenario)
    {
        var (products, binTypes, _) = scenario;

        var env = new GRBEnv(true);
        env.Start();
        env.TimeLimit = 120;
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
                productXIsLeftOfY[p, p2] = model.AddVar(0, 1, 0, GRB.BINARY, $"product_{p}_left_of_{p2}");
                productXIsBelowOfY[p, p2] = model.AddVar(0, 1, 0, GRB.BINARY, $"product_{p}_belof_of_{p2}");
                productXIsBehindOfY[p, p2] = model.AddVar(0, 1, 0, GRB.BINARY, $"product_{p}_behind_of_{p2}");
            }
        }


        // worst case: every product needs separate bin
        var numBins = products.Count;
        var binXIsUsed = model.AddVars(numBins, GRB.BINARY);
        var binXIsBinTypeY = new GRBVar[numBins, binTypes.Count];
        for (var b = 0; b < numBins; b++)
        {
            for (var binType = 0; binType < binTypes.Count; binType++)
            {
                binXIsBinTypeY[b, binType] = model.AddVar(0, 1, 0, GRB.BINARY, $"bin_{b}_is_type_{binType}");
            }
        }

        // symmetry breaking, bin1 is used before bin2 will be considered
        for (var b = 1; b < numBins; b++)
        {
            model.AddConstr(binXIsUsed[b] <= binXIsUsed[b - 1], $"bin{b - 1}_used_before_bin_{b}");
        }

        var productXInBinY = new GRBVar[products.Count, numBins];

        for (var p = 0; p < products.Count; p++)
        {
            for (var b = 0; b < numBins; b++)
            {
                productXInBinY[p, b] = model.AddVar(0, 1, 0, GRB.BINARY, $"product_{p}_is_in_bin_{b}");
            }
        }

        // every product is placed in one bin
        for (var p = 0; p < products.Count; p++)
        {
            var sumProductInBin = new GRBLinExpr();
            for (int b = 0; b < numBins; b++)
            {
                sumProductInBin += productXInBinY[p, b];
            }

            model.AddConstr(sumProductInBin == 1, $"product_{p}_is_used");
        }

        // only used bins can contain products
        for (var p = 0; p < products.Count; p++)
        {
            for (int b = 0; b < numBins; b++)
            {
                model.AddConstr(productXInBinY[p, b] <= binXIsUsed[b], $"product_{p}_is_in_used_bin_{b}");
            }
        }

        // every bin has a type
        for (int b = 0; b < numBins; b++)
        {
            var binTypeUsed = new GRBLinExpr();
            for (var binType = 0; binType < binTypes.Count; binType++)
            {
                binTypeUsed += binXIsBinTypeY[b, binType];
            }

            model.AddConstr(binTypeUsed == 1, $"bin_{b}_has_a_type");
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


            for (var b = 0; b < numBins; b++)
            {
                for (var binType = 0; binType < binTypes.Count; binType++)
                {
                    var binIsUsedByProductAndType = model.AddVar(0, 1, 0, GRB.BINARY, $"bin_{b}_is_used_{binType}");
                    model.AddGenConstrAnd(binIsUsedByProductAndType,
                        [binXIsUsed[b], binXIsBinTypeY[b, binType], productXInBinY[p, b]],
                        "bin_is_used_and_type_and_contains_product");

                    model.AddGenConstrIndicator(binIsUsedByProductAndType, 1,
                        productPositionsX[p] + width <= binTypes[binType].Width,
                        $"product_{p}_within_x_space");
                    model.AddGenConstrIndicator(binIsUsedByProductAndType, 1,
                        productPositionsY[p] + depth <= binTypes[binType].Depth,
                        $"product_{p}_within_y_space");
                    model.AddGenConstrIndicator(binIsUsedByProductAndType, 1, productPositionsZ[p] + height <= binTypes[binType].Height,
                        $"product_{p}_within_z_space");
                }
            }


            for (var p2 = 0; p2 < products.Count; p2++)
            {
                if (p >= p2) continue;

                model.AddGenConstrIndicator(productXIsLeftOfY[p, p2], 1,
                    productPositionsX[p] + width <= productPositionsX[p2], $"product_{p}_left_of_{p2}");


                model.AddGenConstrIndicator(productXIsBehindOfY[p, p2], 1,
                    productPositionsY[p] + depth <= productPositionsY[p2], $"product_{p}_behind_of_{p2}");

                model.AddGenConstrIndicator(productXIsBelowOfY[p, p2], 1,
                    productPositionsZ[p] + height <= productPositionsZ[p2], $"product_{p}_below_of_{p2}");
                
                for (var b = 0; b < numBins; b++)
                {
                    var productsAreInSameBin =
                        model.AddVar(0, 1, 0, GRB.BINARY, $"product_{p}_and_{p2}_are_in_bin_{b}");
                    model.AddGenConstrAnd(productsAreInSameBin, [productXInBinY[p, b], productXInBinY[p2, b]],
                        $"product_{p}_and_{p2}_are_in_same_bin_{b}");

                    model.AddGenConstrIndicator(productsAreInSameBin, 1,
                        productXIsLeftOfY[p, p2] + productXIsBehindOfY[p, p2] + productXIsBelowOfY[p, p2] >= 1,
                        $"no_overlap_between_{p}_{p2}");
                }
            }
        }

        // minimize bins used
        var sumBinCost = new GRBLinExpr();
        for (var b = 0; b < numBins; b++)
        {
            for (var bt = 0; bt < binTypes.Count; bt++)
            {
                var binIsUsedAndIsType = model.AddVar(0,1,0, GRB.BINARY, $"bin_{b}_used_{bt}");
                model.AddGenConstrAnd(binIsUsedAndIsType, [binXIsUsed[b], binXIsBinTypeY[b, bt]], $"bin_{b}_used_{bt}");
                sumBinCost += binIsUsedAndIsType * binTypes[bt].Cost;    
            }
        }

        model.SetObjective(sumBinCost, GRB.MINIMIZE);

        model.Optimize();


        var usedBins = new List<UsedBin>();
        for (var b = 0; b < numBins; b++)
        {
            if (binXIsUsed[b].X > 0.5)
            {
                BinType? binType = null;
                for (var bt = 0; bt < binTypes.Count; bt++)
                {
                    if (binXIsBinTypeY[b, bt].X > 0.5)
                    {
                        binType = binTypes[bt];
                    }
                }

                if (binType is null)
                {
                    throw new InvalidOperationException("something is wrong with the model");
                }

                Console.WriteLine($"Bin {b} is used with bintype: {binType}");

                var solvedProducts = new List<SolvedProduct>();
                for (var p = 0; p < products.Count; p++)
                {
                    if (productXInBinY[p, b].X > 0.5)
                    {
                        var rotation = rotation1[p].X > 0.5 ? 1
                            : rotation2[p].X > 0.5 ? 2
                            : rotation3[p].X > 0.5 ? 3
                            : rotation4[p].X > 0.5 ? 4
                            : rotation5[p].X > 0.5 ? 5
                            : 0;
                        Console.WriteLine(
                            $"Product {p} at ({productPositionsX[p].X}, {productPositionsY[p].X}, {productPositionsZ[p].X}), rotation: {rotation}");

                        solvedProducts.Add(new SolvedProduct(products[p],
                            new Point(productPositionsX[p].X, productPositionsY[p].X, productPositionsZ[p].X),
                            rotation
                        ));
                    }
                }


                usedBins.Add(new UsedBin(binType!, solvedProducts));
            }
        }

        return new Solution(usedBins);
    }
}