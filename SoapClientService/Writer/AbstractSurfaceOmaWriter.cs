using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Org.Visiontech.Compute;
using SoapClientService.Reader;
using System.Text;

namespace SoapClientService.Writer
{
    public abstract class AbstractSurfaceOmaWriter<T> : IDoubleSurfaceOmaWriter, ISingleSurfaceOmaWriter where T : threeDimensionalPointDTO
    {

        protected readonly string SurfaceDirectory;
        protected readonly string Extension;
        protected readonly string Separator;

        public AbstractSurfaceOmaWriter(string SurfaceDirectory, string Extension, string Separator)
        {
            this.SurfaceDirectory = SurfaceDirectory;
            this.Extension = Extension;
            this.Separator = Separator;
        }

        public void Write(OmaReaderSingleResult OmaReaderSingleResult, computeLensResponseDTO computeLensResponseDTO)
        {
            side side = OmaReaderSingleResult is OmaReaderLeftResult ? side.LEFT : side.RIGHT;
            string tmpPath = TempSurfaceFilePath(OmaReaderSingleResult);
            File.Delete(tmpPath);
            Write(tmpPath, computeLensResponseDTO, side);
            string filePath = SurfaceFilePath(OmaReaderSingleResult);
            File.Delete(filePath);
            if (File.Exists(tmpPath)) File.Move(tmpPath, filePath);
        }

        public void Write(OmaReaderDoubleResult OmaReaderDoubleResult, computeLensResponseDTO leftComputeLensResponseDTO, computeLensResponseDTO rightComputeLensResponseDTO)
        {
            SurfaceFiles tmpSurfaceFiles = TempSurfaceFilesPath(OmaReaderDoubleResult);
            string leftTmpPath = tmpSurfaceFiles.LeftSurfaceFile;
            string rightTmpPath = tmpSurfaceFiles.RightSurfaceFile;
            File.Delete(leftTmpPath);
            File.Delete(rightTmpPath);
            Write(rightTmpPath, rightComputeLensResponseDTO, side.RIGHT);
            Write(leftTmpPath, leftComputeLensResponseDTO, side.LEFT);
            SurfaceFiles surfaceFiles = SurfaceFilesPath(OmaReaderDoubleResult);
            string leftFilePath = surfaceFiles.LeftSurfaceFile;
            string rightFilePath = surfaceFiles.RightSurfaceFile;
            File.Delete(leftFilePath);
            File.Delete(rightFilePath);
            if (File.Exists(leftTmpPath)) File.Move(leftTmpPath, leftFilePath);
            if (File.Exists(rightTmpPath)) File.Move(rightTmpPath, rightFilePath);
        }

        protected void Write(string filePath, computeLensResponseDTO computeLensResponseDTO, side side)
        {
            if (computeLensResponseDTO is object && computeLensResponseDTO.value is object && computeLensResponseDTO.value.points is object)
            {
                IEnumerable<T> points = computeLensResponseDTO.value.points.Select(point => point as T);
                double dimension = Math.Abs(points.First().x) * 2;

                using (StreamWriter streamWriter = new StreamWriter(filePath, true, Encoding.ASCII))
                {
                    Write(points, streamWriter, dimension, side);
                }
            }
        }

        public string TempSurfaceFilePath(OmaReaderSingleResult OmaReaderSingleResult)
        {
            side side = OmaReaderSingleResult is OmaReaderLeftResult ? side.LEFT : side.RIGHT;
            string tmpPath = Path.Combine(SurfaceDirectory, GetFileName(OmaReaderSingleResult.Result, side));
            return tmpPath;
        }

        public string SurfaceFilePath(OmaReaderSingleResult OmaReaderSingleResult)
        {
            string tmpPath = TempSurfaceFilePath(OmaReaderSingleResult);
            string filePath = Path.ChangeExtension(tmpPath, Extension);
            return filePath;
        }

        public SurfaceFiles TempSurfaceFilesPath(OmaReaderDoubleResult OmaReaderDoubleResult)
        {
            string leftTmpPath = Path.Combine(SurfaceDirectory, GetFileName(OmaReaderDoubleResult.Left, side.LEFT));
            string rightTmpPath = Path.Combine(SurfaceDirectory, GetFileName(OmaReaderDoubleResult.Right, side.RIGHT));
            SurfaceFiles tmpSurfaceFiles = new SurfaceFiles(rightTmpPath, leftTmpPath);
            return tmpSurfaceFiles;
        }

        public SurfaceFiles SurfaceFilesPath(OmaReaderDoubleResult OmaReaderDoubleResult)
        {
            SurfaceFiles tmpSurfaceFiles = TempSurfaceFilesPath(OmaReaderDoubleResult);
            SurfaceFiles surfaceFiles = new SurfaceFiles(Path.ChangeExtension(tmpSurfaceFiles.RightSurfaceFile, Extension), Path.ChangeExtension(tmpSurfaceFiles.LeftSurfaceFile, Extension));
            return surfaceFiles;
        }

        abstract protected string GetFileName(IDictionary<OmaParameter, string> parameters, side side);
        abstract protected void Write(IEnumerable<T> points, StreamWriter streamWriter, double dimension, side side);

    }
}
