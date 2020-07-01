using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoapClientService.Writer
{
    class DoubleFilePathOmaBuilder : IFilePathOmaBuilder
    {
        private readonly string rightSurfaceFilePath;
        private readonly string leftSurfaceFilePath;
        public DoubleFilePathOmaBuilder(string rightSurfaceFilePath, string leftSurfaceFilePath)
        {
            this.rightSurfaceFilePath = rightSurfaceFilePath;
            this.leftSurfaceFilePath = leftSurfaceFilePath;
        }
        public string BuildFilePath()
        {
            return (rightSurfaceFilePath is object ? rightSurfaceFilePath : "") + ";" + (leftSurfaceFilePath is object ? leftSurfaceFilePath : "");
        }
    }
}
