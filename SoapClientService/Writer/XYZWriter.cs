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
            points.ToList().ForEach(point => streamWriter.WriteLine(string.Join(Separator, new[] { point.x, point.y, point.z }.Select(value => value.ToString(CultureInfo.InvariantCulture)))));
        }

    }
}
