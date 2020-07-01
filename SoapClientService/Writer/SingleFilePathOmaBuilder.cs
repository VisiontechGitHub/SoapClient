using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoapClientService.Writer
{
    class SingleFilePathOmaBuilder : IFilePathOmaBuilder
    {
        private string surfaceFilePath;
        public SingleFilePathOmaBuilder(string surfaceFilePath)
        {
            this.surfaceFilePath = surfaceFilePath;
        }
        public string BuildFilePath()
        {
            return surfaceFilePath is object ? surfaceFilePath : "";
        }
    }
}
