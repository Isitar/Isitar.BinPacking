using System.Text;
using Model;

namespace CLI;

public class Draw3DSolution
{
    public static void Draw(ThreeDSolver.Solution solution)
    {
        // Define the output file name
        string fileName = "solution.obj";

        var objContent = new StringBuilder();

        var vertexCount = 0;

        objContent.AppendLine("#box");
        vertexCount = AddCube(objContent, 0, 0, 0, solution.Space.Width, solution.Space.Height, solution.Space.Depth,
            vertexCount, "space");

        var i = 0;
        foreach (var solvedProduct in solution.SolvedProducts)
        {
            vertexCount = AddCube(objContent, solvedProduct.Point.X, solvedProduct.Point.Y, solvedProduct.Point.Z,
                solvedProduct.AdjustedWidth, solvedProduct.AdjustedHeight, solvedProduct.AdjustedDepth,
                vertexCount, $"p_{i++}");
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