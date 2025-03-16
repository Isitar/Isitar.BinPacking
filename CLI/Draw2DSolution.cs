using System.Text;
using Model;

namespace CLI;

public class Draw2DSolution
{
    public static void Draw(TwoDSolver.Solution solution)
    {
        var svgContent = new StringBuilder();
        svgContent.AppendLine($"<svg width=\"{solution.Space.Width}\" height=\"{solution.Space.Height}\" xmlns=\"http://www.w3.org/2000/svg\">\n");
        svgContent.AppendLine("<rect width=\"100%\" height=\"100%\" fill=\"white\" />");

        foreach (var solvedProduct in solution.SolvedProducts)
        {
            var color = $"rgb({Random.Shared.Next(256)},{Random.Shared.Next(256)},{Random.Shared.Next(256)})";
            svgContent.AppendLine($"<rect x=\"{solvedProduct.Point.X}\" y=\"{solvedProduct.Point.Y}\" width=\"{solvedProduct.Product.Width}\" height=\"{solvedProduct.Product.Height}\" fill=\"{color}\" stroke=\"black\" stroke-width=\"1\" />");
        }

        svgContent.AppendLine("</svg>");
        File.WriteAllText("result.svg", svgContent.ToString());
    }
}