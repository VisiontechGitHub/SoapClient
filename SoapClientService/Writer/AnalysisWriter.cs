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
            points.ToList().ForEach(point => streamWriter.WriteLine(string.Join(Separator, new[] { point.x, point.y, point.z, point.cylinderMap, point.powerMap, point.cylinderAxis, point.thickness }.Select(value => value.ToString(CultureInfo.InvariantCulture)))));
        }

    }
}
