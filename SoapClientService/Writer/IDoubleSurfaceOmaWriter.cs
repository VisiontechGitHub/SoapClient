using Org.Visiontech.Compute;
using SoapClientService.Reader;

namespace SoapClientService.Writer
{
    public interface IDoubleSurfaceOmaWriter
    {

        void Write(OmaReaderDoubleResult OmaReaderDoubleResult, computeLensResponseDTO leftComputeLensResponseDTO, computeLensResponseDTO rightComputeLensResponseDTO);

    }
}
