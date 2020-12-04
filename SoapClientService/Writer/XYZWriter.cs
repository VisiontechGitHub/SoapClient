using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using Org.Visiontech.Compute;

namespace SoapClientService.Writer
{
    public class XYZWriter : AbstractSurfaceOmaWriter<threeDimensionalPointDTO>
    {

        public XYZWriter(string SurfaceDirectory) : base(SurfaceDirectory, ".txt", " ") { }

        protected override string GetFileName(IDictionary<OmaParameter, string> parameters, side side)
        {
            return parameters[OmaParameter.JOB] + (side.LEFT.Equals(side) ? "L" : "R");
        }

        protected override void Write(IEnumerable<threeDimensionalPointDTO> points, StreamWriter streamWriter, double dimension, side side)
        {
            IDictionary<double, IDictionary<double, threeDimensionalPointDTO>> map = new Dictionary<double, IDictionary<double, threeDimensionalPointDTO>>();
            points.ToList().ForEach(point => {
                double row = point.x;
                if (!map.ContainsKey(row))
                {
                    map.Add(row, new Dictionary<double, threeDimensionalPointDTO>());
                }
                map[row].Add(point.y, point);
            });
            for (double x = points.Select(point => point.x).Min(); x <= points.Select(point => point.x).Max(); x++)
            {
                for (double y = points.Select(point => point.y).Min(); y <= points.Select(point => point.y).Max(); y++)
                {
                    threeDimensionalPointDTO point = map[x][y];
                    streamWriter.WriteLine(string.Join(Separator, new[] { point.x, point.y, point.z }.Select(value => value.ToString(CultureInfo.InvariantCulture))));
                }
            }
        }

    }
}
