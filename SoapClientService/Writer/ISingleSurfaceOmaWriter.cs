using Org.Visiontech.Compute;
using SoapClientService.Reader;

namespace SoapClientService.Writer
{
    public interface ISingleSurfaceOmaWriter
    {

        void Write(OmaReaderSingleResult OmaReaderSingleResult, computeLensResponseDTO computeLensResponseDTO);

    }
}
