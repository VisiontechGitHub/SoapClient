using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoapClientService.Writer
{
    public class SurfaceFiles
    {
        public string RightSurfaceFile
        {
            get; set;
        }
        public string LeftSurfaceFile
        {
            get; set;
        }

        public SurfaceFiles(string rightSurfaceFile, string leftSurfaceFile)
        {
            RightSurfaceFile = rightSurfaceFile;
            LeftSurfaceFile = leftSurfaceFile;
        }
    }
}
