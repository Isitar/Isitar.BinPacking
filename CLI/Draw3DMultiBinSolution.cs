using System.Text;
using Model;

namespace CLI;

public class Draw3DMultiBinSolution
{
    public static void Draw(ThreeDSolverMultiBins.Solution solution, string fileName = "solution.obj")
    {
        var objContent = new StringBuilder();

        var offset = 0;
        var vertexCount = 0;

        var binIndex = 0;
        foreach (var usedBin in solution.UsedBins)
        {
            vertexCount = AddCube(objContent, offset, 0, 0, usedBin.BinType.Width, usedBin.BinType.Height, usedBin.BinType.Depth,
                vertexCount, $"bin_{binIndex}");

            var i = 0;
            foreach (var solvedProduct in usedBin.SolvedProducts)
            {
                vertexCount = AddCube(objContent, solvedProduct.Point.X + offset, solvedProduct.Point.Y, solvedProduct.Point.Z,
                    solvedProduct.AdjustedWidth, solvedProduct.AdjustedHeight, solvedProduct.AdjustedDepth,
                    vertexCount, $"b_{binIndex}_p_{i++}");
            }

            binIndex++;
            offset += usedBin.BinType.Width + 10;
        }
        



        // Write the OBJ content to a file
        File.WriteAllText(fileName, objContent.ToString());

        // Notify the user
        Console.WriteLine($"3D file {fileName} created");
    }

    private static int AddCube(StringBuilder objContent, double x, double y, double z, double width, double height, double depth,
        int lastVertexCount, string name)
    {
        objContent.AppendLine($"o {name}");
        // top
        objContent.AppendLine($"v {x} {y} {z}");
        objContent.AppendLine($"v {x + width} {y} {z}");
        objContent.AppendLine($"v {x + width} {y + depth} {z}");
        objContent.AppendLine($"v {x} {y + depth} {z}");

        // bottom
        objContent.AppendLine($"v {x} {y} {z + height}");
        objContent.AppendLine($"v {x + width} {y} {z + height}");
        objContent.AppendLine($"v {x + width} {y + depth} {z + height}");
        objContent.AppendLine($"v {x} {y + depth} {z + height}");

        // faces
        // top
        objContent.AppendLine(
            $"f {lastVertexCount + 1} {lastVertexCount + 2} {lastVertexCount + 3} {lastVertexCount + 4}");
        // bottom
        objContent.AppendLine(
            $"f {lastVertexCount + 5} {lastVertexCount + 6} {lastVertexCount + 7} {lastVertexCount + 8}");
        // front
        objContent.AppendLine(
            $"f {lastVertexCount + 1} {lastVertexCount + 2} {lastVertexCount + 6} {lastVertexCount + 5}");
        // right
        objContent.AppendLine(
            $"f {lastVertexCount + 2} {lastVertexCount + 3} {lastVertexCount + 7} {lastVertexCount + 6}");
        // behind
        objContent.AppendLine(
            $"f {lastVertexCount + 3} {lastVertexCount + 4} {lastVertexCount + 8} {lastVertexCount + 7}");
        // left
        objContent.AppendLine(
            $"f {lastVertexCount + 1} {lastVertexCount + 4} {lastVertexCount + 8} {lastVertexCount + 5}");
        return lastVertexCount + 8;
    }
}