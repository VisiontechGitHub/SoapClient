using Org.Visiontech.Compute;
using SoapClientService.Reader;

namespace SoapClientService.Writer
{
    public interface ISingleResultOmaWriter
    {

        void Write(OmaReaderSingleResult OmaReaderSingleResult, computeLensResponseDTO computeLensResponseDTO, string surfaceFilePath, IFilePathOmaBuilder filePathBuilder);

    }
}
