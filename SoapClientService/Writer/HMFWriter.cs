using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using Org.Visiontech.Compute;
using SoapClientService.Logging;

namespace SoapClientService.Writer
{
    public class HMFWriter : AbstractSurfaceOmaWriter<threeDimensionalPointDTO>
    {

        public HMFWriter(string SurfaceDirectory) : base(SurfaceDirectory, ".hmf", ",") { }
        
        protected override string GetFileName(IDictionary<OmaParameter, string> parameters, side side)
        {
            return parameters[OmaParameter.JOB] + (side.LEFT.Equals(side) ? "L" : "R");
        }

        protected override void Write(IEnumerable<threeDimensionalPointDTO> points, StreamWriter streamWriter, double dimension, side side)
        {
            streamWriter.WriteLine("File Version=1.2");
            streamWriter.WriteLine("[Properties]");
            streamWriter.WriteLine("Count=" + (dimension + 1));
            streamWriter.WriteLine("Interval=1");
            streamWriter.WriteLine("[Data]");
            IDictionary<double, IDictionary<double, threeDimensionalPointDTO>> map = new SortedDictionary<double, IDictionary<double, threeDimensionalPointDTO>>();
            points.ToList().ForEach(point => {
                double row = point.x;
                if (!map.ContainsKey(row))
                {
                    map.Add(row, new Dictionary<double, threeDimensionalPointDTO>());
                }
                map[row].Add(point.y, point);
            });
            for (double y = points.Select(point => point.y).Min(); y <= points.Select(point => point.y).Max(); y++)
            {
                ICollection<threeDimensionalPointDTO> row = new List<threeDimensionalPointDTO>();
                for (double x = points.Select(point => point.x).Min(); x <= points.Select(point => point.x).Max(); x++)
                {
                    row.Add(map[x][y]);
                }
                streamWriter.WriteLine(string.Join(Separator, row.Select(point => point.z.ToString(CultureInfo.InvariantCulture))));
            }
        }

    }
}
