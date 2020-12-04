using System;
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

            IDictionary<double, IDictionary<double, threeDimensionalPointDTO>> map = new Dictionary<double, IDictionary<double, threeDimensionalPointDTO>>();
            points.ToList().ForEach(point => {
                double row = point.x;
                if (!map.ContainsKey(row))
                {
                    map.Add(row, new Dictionary<double, threeDimensionalPointDTO>());
                }
                map[row].Add(point.y, point);
            });

            streamWriter.WriteLine("SURFMT=" + string.Join(Separator, "1", side.LEFT.Equals(side) ? "L" : "R", "B", mass.ToString(), mass.ToString(), dimension.ToString(), dimension.ToString(), "0"));
            for (double y = points.Select(point => point.y).Min(); y <= points.Select(point => point.y).Max(); y++)
            {
                ICollection<threeDimensionalPointDTO> row = new List<threeDimensionalPointDTO>();
                for (double x = points.Select(point => point.x).Min(); x <= points.Select(point => point.x).Max(); x++)
                {
                    row.Add(map[x][y]);
                }
                streamWriter.WriteLine("ZZ=" + string.Join(Separator, row.Select(point => point.z.ToString(CultureInfo.InvariantCulture))));
            }
        }
    }
}
