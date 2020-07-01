using Org.Visiontech.Compute;
using SoapClientService.Reader;

namespace SoapClientService.Writer
{
    public interface IDoubleResultOmaWriter
    {

        void Write(OmaReaderDoubleResult OmaReaderDoubleResult, computeLensResponseDTO leftComputeLensResponseDTO, computeLensResponseDTO rightComputeLensResponseDTO, string leftSurfaceFilePath, string rightSurfaceFilePath, IFilePathOmaBuilder filePathBuilder);

    }
}
