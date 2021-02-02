using SoapClientService.Error;
using Org.Visiontech.Compute;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using SoapClientLibrary;
using System.Globalization;

namespace SoapClientService
{
    public class RequestBuilder
    {

        private readonly Func<ComputeSoap> ComputeSoapProvider;
        public static readonly Regex farRegex = new Regex("([^_]+)_([0-9]+)_([0-9]+)_([0-9]+)");
        public static readonly Regex nearRegex = new Regex("([^_]+)_([0-9]+)_([0-9]+)");
        public static readonly Regex channelRegex = new Regex("([^_]+)_([0-9]+)");
        public static readonly Regex officeRegex = new Regex("((EBL)?OFFICE)_([0-9]+(\\.[0-9]+)?)");

        public RequestBuilder(Func<ComputeSoap> ComputeSoapProvider)
        {
            this.ComputeSoapProvider = ComputeSoapProvider;
        }

        public Task<computeLensResponseDTO> BuildComputeTask(IDictionary<OmaParameter, string> parameters, side side)
        {
            ComputeSoap ComputeSoap = ComputeSoapProvider.Invoke();
            return
                parameters.ContainsKey(OmaParameter._PRECALC) && parameters[OmaParameter._PRECALC].Equals("1") ?
                ComputeSoap.previewLensAsync(
                    new previewLens
                    {
                        arg0 = BuildRequest(parameters, side)
                    }
                ).ContinueWith(Task => Task.Result.@return) :
                ComputeSoap.computeLensAsync(
                    new computeLens
                    {
                        arg0 = BuildRequest(parameters, side)
                    }
                ).ContinueWith(Task => Task.Result.@return);
        }

        public computeLensRequestDTO BuildRequest(IDictionary<OmaParameter, string> parameters, side side)
        {
            computeLensRequestDTO request;

            string design = null;
            int far = 80;
            int near = 24;
            int channel = 10;

            double distance = Double.MaxValue;
            double degressiveSphereValue = 0;
            double degressiveAdditionValue = 0;
            Boolean degressiveSphere = false;
            Boolean degressiveAddition = false;

            if ((parameters.ContainsKey(OmaParameter.LDNAM) ? parameters[OmaParameter.LDNAM] : parameters.ContainsKey(OmaParameter.LNAM) ? parameters[OmaParameter.LNAM] : null) is string ldnam)
            {
                if (officeRegex.IsMatch(ldnam) && officeRegex.Match(ldnam) is Match OfficeMatch) {
                    design = OfficeMatch.Groups[1].Value;
                    distance = double.Parse(OfficeMatch.Groups[3].Value, CultureInfo.InvariantCulture);
                    degressiveAddition = true;
                    degressiveAdditionValue = Math.Round(StringConverter.ExtractDoubleValue(parameters, OmaParameter.ADD) - 1D / distance, 2);
                    degressiveSphere = true;
                    degressiveSphereValue = StringConverter.ExtractDoubleValue(parameters, OmaParameter.SPH) + StringConverter.ExtractDoubleValue(parameters, OmaParameter.ADD) - degressiveAdditionValue;
                }
                if (farRegex.IsMatch(ldnam) && farRegex.Match(ldnam) is Match FarMatch)
                {
                    design = FarMatch.Groups[1].Value;
                    far = Convert.ToInt32(FarMatch.Groups[2].Value);
                    near = Convert.ToInt32(FarMatch.Groups[3].Value);
                    channel = Convert.ToInt32(FarMatch.Groups[4].Value);
                }
                else if(nearRegex.IsMatch(ldnam) && nearRegex.Match(ldnam) is Match NearMath)
                {
                    design = NearMath.Groups[1].Value;
                    near = Convert.ToInt32(NearMath.Groups[2].Value);
                    channel = Convert.ToInt32(NearMath.Groups[3].Value);
                }
                else if (channelRegex.IsMatch(ldnam) && channelRegex.Match(ldnam) is Match ChannelMatch)
                {
                    design = ChannelMatch.Groups[1].Value;
                    channel = Convert.ToInt32(ChannelMatch.Groups[2].Value);
                } else
                {
                    design = ldnam;
                }
            }

            switch (design.ToUpper())
            {
                case "MAGIC":
                    request = new computeMagicLensRequestDTO();
                    break;
                case "FANTASY":
                    request = new computeFantasyLensRequestDTO();
                    break;
                case "OFFICE":
                case "EBLOFFICE":
                    request = new computeOfficeLensRequestDTO();
                    break;
                case "ANTIFATIGUE":
                case "EBLANTIFATIGUE":
                    request = new computeAntifatigueLensRequestDTO();
                    break;
                case "BASIC":
                case "EBLPROGRESSIVE":
                    request = new computeBasicLensRequestDTO();
                    break;
                case "4K07":
                case "4KUS":
                    request = new compute4K07LensRequestDTO();
                    break;
                case "4K09":
                case "4KSX":
                    request = new compute4K09LensRequestDTO();
                    break;
                case "4K11":
                case "4KS":
                    request = new compute4K11LensRequestDTO();
                    break;
                case "4K13":
                case "4KR":
                    request = new compute4K13LensRequestDTO();
                    break;
                case "DIGIT":
                    request = new computeDigitLensRequestDTO();
                    break;
                case "VARIO":
                    request = new computeVarioLensRequestDTO();
                    break;
                case "IFLEX":
                    request = new computeIflexLensRequestDTO();
                    break;
                case "IPROG":
                    request = new computeIprogLensRequestDTO();
                    break;
                case "IPROF":
                    request = new computeIprofLensRequestDTO();
                    break;
                case "PUNCTUAL":
                    request = new computePunctualLensRequestDTO();
                    break;
                case "MONOF":
                case "EBLSINGLEVISION":
                    request = new computeMonofLensRequestDTO();
                    break;
                case "EBLPROGR":
                    request = new computeEblRegularLensRequestDTO();
                    break;
                case "EBLPROGS":
                    request = new computeEblShortLensRequestDTO();
                    break;
                case "EBLPROGSX":
                    request = new computeEblExtraShortLensRequestDTO();
                    break;
                case "EBLPROGUS":
                    request = new computeEblUltraShortLensRequestDTO();
                    break;
                default:
                    throw new OmaException("LDNAME format is not correct", null, side, OmaStatusCode.IncorrectLdname);
            }

            if (request is computeFarLensRequestDTO computeFarLensRequestDTO)
            {
                computeFarLensRequestDTO.far = far;
                computeFarLensRequestDTO.farSpecified = true;
            }
            if (request is computeNearLensRequestDTO computeNearLensRequestDTO)
            {
                computeNearLensRequestDTO.near = near;
                computeNearLensRequestDTO.nearSpecified = true;
            }
            if (request is computeNativeLensRequestDTO computeNativeLensRequestDTO)
            {
                computeNativeLensRequestDTO.channel = channel;
                computeNativeLensRequestDTO.channelSpecified = true;
            }

            request.verticalPrismThinning = StringConverter.ExtractBoolValue(parameters, OmaParameter.PTOK);
            request.verticalPrismThinningSpecified = true;
            request.horizontalPrismThinning = StringConverter.ExtractBoolValue(parameters, OmaParameter._PTOK);
            request.horizontalPrismThinningSpecified = true;
            request.horizontalPrismThinningHeight = StringConverter.ExtractDoubleValue(parameters, OmaParameter._PTOKHVAL, 6);
            request.horizontalPrismThinningHeightSpecified = true;
            request.prismThinningCompensation = false;
            request.prismThinningCompensationSpecified = true;
            request.maximumPrismThinningCompensation = 0;
            request.maximumPrismThinningCompensationSpecified = true;
            request.atoric = false;
            request.atoricSpecified = true;
            request.pantoscopicAngle = StringConverter.ExtractDoubleValue(parameters, OmaParameter.PANTO);
            request.pantoscopicAngleSpecified = true;
            request.wrappingAngle = StringConverter.ExtractDoubleValue(parameters, OmaParameter.ZTILT);
            request.wrappingAngleSpecified = true;
            request.backVertexDistance = StringConverter.ExtractDoubleValue(parameters, OmaParameter.BVD);
            request.backVertexDistanceSpecified = true;
            request.eyeDiameter = 24;
            request.eyeDiameterSpecified = true;
            request.readDistance = StringConverter.ExtractDoubleValue(parameters, OmaParameter.NWD);
            request.readDistanceSpecified = true;

            request.value = new computeLensRequestSideDTO
            {
                side = side,
                sideSpecified = true,
                sphere = (degressiveSphere ? degressiveSphereValue : StringConverter.ExtractDoubleValue(parameters, OmaParameter.SPH)) + StringConverter.ExtractDoubleValue(parameters, OmaParameter.ADJSPH),
                sphereSpecified = true,
                cylinder = StringConverter.ExtractDoubleValue(parameters, OmaParameter.CYL) + StringConverter.ExtractDoubleValue(parameters, OmaParameter.ADJCYL),
                cylinderSpecified = true,
                cylinderAxis = Convert.ToInt32(Math.Round(StringConverter.ExtractDoubleValue(parameters, OmaParameter.AX) + StringConverter.ExtractDoubleValue(parameters, OmaParameter.ADJAX))),
                cylinderAxisSpecified = true,
                addiction = (degressiveAddition ? degressiveAdditionValue : StringConverter.ExtractDoubleValue(parameters, OmaParameter.ADD)) + StringConverter.ExtractDoubleValue(parameters, OmaParameter.ADJADD),
                addictionSpecified = true,
                horizontalDiameter = Convert.ToInt32(Math.Round(StringConverter.ExtractDoubleValue(parameters, OmaParameter.CRIB))),
                horizontalDiameterSpecified = true,
                verticalDiameter = Convert.ToInt32(Math.Round(StringConverter.ExtractDoubleValue(parameters, OmaParameter.ELLH, StringConverter.ExtractDoubleValue(parameters, OmaParameter.CRIB)))),
                verticalDiameterSpecified = true,
                refractiveIndex = StringConverter.ExtractDoubleValue(parameters, OmaParameter.LIND),
                refractiveIndexSpecified = true,
                toolRealBase = (StringConverter.ExtractDoubleValue(parameters, OmaParameter.LIND) - 1) * 1000 / ((StringConverter.ExtractDoubleValue(parameters, OmaParameter.TIND) - 1) * 1000 / StringConverter.ExtractDoubleValue(parameters, OmaParameter.FRNT)),
                toolRealBaseSpecified = true,
                toolIndex = StringConverter.ExtractDoubleValue(parameters, OmaParameter.TIND),
                toolIndexSpecified = true,
                prescriptedPrism = StringConverter.ExtractDoubleValue(parameters, OmaParameter.PRVM),
                prescriptedPrismSpecified = true,
                prescriptedPrismBase = StringConverter.ExtractIntValue(parameters, OmaParameter.PRVA),
                prescriptedPrismBaseSpecified = true,
                secondaryPrism = StringConverter.ExtractDoubleValue(parameters, OmaParameter._PRVM),
                secondaryPrismSpecified = true,
                secondaryPrismBase = StringConverter.ExtractIntValue(parameters, OmaParameter._PRVA),
                secondaryPrismBaseSpecified = true,
                minimalCentralThickness = StringConverter.ExtractDoubleValue(parameters, OmaParameter.MINCTR),
                minimalCentralThicknessSpecified = true,
                minimalSideThickness = StringConverter.ExtractDoubleValue(parameters, OmaParameter.MINEDG, StringConverter.ExtractDoubleValue(parameters, OmaParameter.MINTHKCD, 2.0)),
                minimalSideThicknessSpecified = true,
                horizontalDecentralization = StringConverter.ExtractDoubleValue(parameters, OmaParameter.BCERIN, StringConverter.ExtractDoubleValue(parameters, OmaParameter.SMOCIN)),
                horizontalDecentralizationSpecified = true,
                verticalDecentralization = StringConverter.ExtractDoubleValue(parameters, OmaParameter.BCERUP, StringConverter.ExtractDoubleValue(parameters, OmaParameter.SMOCUP)),
                verticalDecentralizationSpecified = true,
                monocularCentrationDistance = StringConverter.ExtractDoubleValue(parameters, OmaParameter.IPD, 32),
                monocularCentrationDistanceSpecified = true,
                inset = StringConverter.ExtractDoubleValue(parameters, OmaParameter.ERNRIN, 2.2),
                insetSpecified = true
            };

            if (new[] { OmaParameter._EBL }.ToList().All(Key => parameters.ContainsKey(Key)) && StringConverter.ExtractBoolValue(parameters, OmaParameter._EBL))
            {
                request = new computeAestheticLensRequestDTO() {
                    componentLensRequest = request,
                    internalLensDiameter = StringConverter.ExtractIntValue(parameters, OmaParameter._EBLINTDIA, 40),
                    internalLensDiameterSpecified = true,
                    junctionDiameter = StringConverter.ExtractIntValue(parameters, OmaParameter._EBLJUNDIA, 60),
                    junctionDiameterSpecified = true,
                    sideAngle = StringConverter.ExtractIntValue(parameters, OmaParameter._EBLANG, 10),
                    sideAngleSpecified = true
                };
            }

            return request;
        }


    }
}
