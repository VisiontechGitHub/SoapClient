using Org.Visiontech.Compute;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

namespace SoapClientService.Writer
{
    public class AnalysisWriter : AbstractSurfaceOmaWriter<analyzedPointDTO>
    {

        public AnalysisWriter (string AnalysisDirectory) : base(AnalysisDirectory, ".txt", " ") { }

        protected override string GetFileName(IDictionary<OmaParameter, string> parameters, side side)
        {
            return "analyze_" + parameters[OmaParameter.JOB] + (side.LEFT.Equals(side) ? "L" : "R");
        }

        protected override void Write(IEnumerable<analyzedPointDTO> points, StreamWriter streamWriter, double dimension, side side)
        {
            IDictionary<double, IDictionary<double, analyzedPointDTO>> map = new Dictionary<double, IDictionary<double, analyzedPointDTO>>();
            points.ToList().ForEach(point => {
                double row = point.x;
                if (!map.ContainsKey(row))
                {
                    map.Add(row, new Dictionary<double, analyzedPointDTO>());
                }
                map[row].Add(point.y, point);
            });
            for (double x = points.Select(point => point.x).Min(); x <= points.Select(point => point.x).Max(); x++)
            {
                for (double y = points.Select(point => point.y).Min(); y <= points.Select(point => point.y).Max(); y++)
                {
                    analyzedPointDTO point = map[x][y];
                    streamWriter.WriteLine(string.Join(Separator, new[] { point.x, point.y, point.z, point.cylinderMap, point.powerMap, point.cylinderAxis, point.thickness }.Select(value => value.ToString(CultureInfo.InvariantCulture))));
                }
            }
        }

    }
}
