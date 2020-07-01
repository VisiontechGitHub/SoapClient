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
                double row = -point.y;
                if (!map.ContainsKey(row))
                {
                    map.Add(row, new SortedDictionary<double, threeDimensionalPointDTO>());
                }
                map[row].Add(point.x, point);
            });
            map.Values.ToList().ForEach(row => streamWriter.WriteLine(string.Join(Separator, row.Values.Select(point => point.z.ToString(CultureInfo.InvariantCulture)))));
        }

    }
}
