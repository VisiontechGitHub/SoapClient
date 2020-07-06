using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using Org.Visiontech.Compute;

namespace SoapClientService.Writer
{
    public class SDFWriter : AbstractSurfaceOmaWriter<threeDimensionalPointDTO>
    {

        public SDFWriter(string SurfaceDirectory) : base(SurfaceDirectory, ".sdf", ";") { }
        
        protected override string GetFileName(IDictionary<OmaParameter, string> parameters, side side)
        {
            return parameters[OmaParameter.JOB];
        }

        protected override void Write(IEnumerable<threeDimensionalPointDTO> points, StreamWriter streamWriter, double dimension, side side)
        {
            double mass = dimension + 1;

            IDictionary<double, IDictionary<double, threeDimensionalPointDTO>> map = new SortedDictionary<double, IDictionary<double, threeDimensionalPointDTO>>();
            points.ToList().ForEach(point => {
                double row = point.y;
                if (!map.ContainsKey(row))
                {
                    map.Add(row, new SortedDictionary<double, threeDimensionalPointDTO>());
                }
                map[row].Add(point.x, point);
            });

            streamWriter.WriteLine("SURFMT=" + string.Join(Separator, "1", side.LEFT.Equals(side) ? "L" : "R", "B", mass.ToString(), mass.ToString(), dimension.ToString(), dimension.ToString(), "0"));
            map.Values.ToList().ForEach(row => streamWriter.WriteLine("ZZ=" + string.Join(Separator, row.Values.Select(point => point.z.ToString(CultureInfo.InvariantCulture)))));
        }
    }
}
