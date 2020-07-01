using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using Org.Visiontech.Compute;
using SoapClientService.Reader;
using SoapClientLibrary;

namespace SoapClientService.Writer
{
    public class OmaWriter : ISingleResultOmaWriter, IDoubleResultOmaWriter { 


        private readonly string OutputFolder;
        private readonly string Gax;
        private readonly string LapGax;

        public OmaWriter(string OutputFolder, string Gax, string LapGax)
        {
            this.OutputFolder = OutputFolder;
            this.Gax = Gax;
            this.LapGax = LapGax;
        }

        public void Write(OmaReaderDoubleResult OmaReaderDoubleResult, computeLensResponseDTO leftComputeLensResponse, computeLensResponseDTO rightComputeLensResponse, string leftSurfaceFilePath, string rightSurfaceFilePath, IFilePathOmaBuilder filePathBuilder)
        {
            Write(OmaReaderDoubleResult.Left, OmaReaderDoubleResult.Right, leftComputeLensResponse, rightComputeLensResponse, leftSurfaceFilePath, rightSurfaceFilePath, filePathBuilder);
        }

        public void Write(OmaReaderSingleResult OmaReaderSingleResult, computeLensResponseDTO computeLensResponse, string surfaceFile, IFilePathOmaBuilder filePathBuilder)
        {
            switch (OmaReaderSingleResult)
            {
                case OmaReaderLeftResult OmaReaderLeftResult:
                    Write(OmaReaderLeftResult.Result, null, computeLensResponse, null, surfaceFile, null, filePathBuilder);
                    break;
                case OmaReaderRightResult OmaReaderRightResult:
                    Write(null, OmaReaderRightResult.Result, null, computeLensResponse, null, surfaceFile, filePathBuilder);
                    break;
            }
        }

        private void Write(IDictionary<OmaParameter, string> leftParameters, IDictionary<OmaParameter, string> rightParameters, computeLensResponseDTO leftComputeLensResponse, computeLensResponseDTO rightComputeLensResponse, string leftSurfaceFilePath, string rightSurfaceFilePath, IFilePathOmaBuilder filePathBuilder)
        {
            string tmpPath = Path.Combine(OutputFolder, (leftParameters is object ? leftParameters : rightParameters)[OmaParameter.JOB]);

            using (StreamWriter streamWriter = File.AppendText(tmpPath))
            {

                streamWriter.WriteLine("REQ=LMS");
                streamWriter.WriteLine("JOB=" + (leftParameters is object ? leftParameters : rightParameters)[OmaParameter.JOB]);
                streamWriter.WriteLine("DO=" + (leftParameters is object ? leftParameters : rightParameters)[OmaParameter.DO]);

                double rightCentralThickness = rightComputeLensResponse is object ? rightComputeLensResponse.value.finishedCenterThickness : 0D;
                double leftCentralThickness = leftComputeLensResponse is object ? leftComputeLensResponse.value.finishedCenterThickness : 0D;
                streamWriter.WriteLine("CTHICK=" + (rightComputeLensResponse is object ? rightCentralThickness.ToString("0.00", CultureInfo.InvariantCulture) : "") + ";" + (leftComputeLensResponse is object ? leftCentralThickness.ToString("0.00", CultureInfo.InvariantCulture) : ""));
                streamWriter.WriteLine("GTHK=" + (rightComputeLensResponse is object ? rightCentralThickness.ToString("0.00", CultureInfo.InvariantCulture) : "") + ";" + (leftComputeLensResponse is object ? leftCentralThickness.ToString("0.00", CultureInfo.InvariantCulture) : ""));
                // streamWriter.WriteLine("GTHK=" + (rightComputeLensResponse is object ? rightComputeLensResponse.value.surfaceBlockCenterGeneratorThickness.ToString("0.00", CultureInfo.InvariantCulture) : "") + ";" + (leftComputeLensResponse is object ? leftComputeLensResponse.value.surfaceBlockCenterGeneratorThickness.ToString("0.00", CultureInfo.InvariantCulture) : ""));

                streamWriter.WriteLine("CTHNP=" + (rightComputeLensResponse is object ? rightComputeLensResponse.value.cribPerimeterThinnestPointThickness.ToString("0.00", CultureInfo.InvariantCulture) : "") + ";" + (leftComputeLensResponse is object ? leftComputeLensResponse.value.cribPerimeterThinnestPointThickness.ToString("0.00", CultureInfo.InvariantCulture) : ""));
                streamWriter.WriteLine("CTHNA=" + (rightComputeLensResponse is object ? rightComputeLensResponse.value.cribPerimeterThinnestPointAngle.ToString() : "") + ";" + (leftComputeLensResponse is object ? leftComputeLensResponse.value.cribPerimeterThinnestPointAngle.ToString() : ""));

                streamWriter.WriteLine("CTHKP=" + (rightComputeLensResponse is object ? rightComputeLensResponse.value.cribPerimeterThickestPointThickness.ToString("0.00", CultureInfo.InvariantCulture) : "") + ";" + (leftComputeLensResponse is object ? leftComputeLensResponse.value.cribPerimeterThickestPointThickness.ToString("0.00", CultureInfo.InvariantCulture) : ""));

                streamWriter.WriteLine("CTHKA=" + (rightComputeLensResponse is object ? rightComputeLensResponse.value.cribPerimeterThickestPointAngle.ToString() : "") + ";" + (leftComputeLensResponse is object ? leftComputeLensResponse.value.cribPerimeterThickestPointAngle.ToString() : ""));

                streamWriter.WriteLine("LAPBASX=" + (rightComputeLensResponse is object ? rightComputeLensResponse.value.lapBaseCurve.ToString("0.00", CultureInfo.InvariantCulture) : "") + ";" + (leftComputeLensResponse is object ? leftComputeLensResponse.value.lapBaseCurve.ToString("0.00", CultureInfo.InvariantCulture) : ""));
                streamWriter.WriteLine("LAPCRSX=" + (rightComputeLensResponse is object ? rightComputeLensResponse.value.lapCrossCurve.ToString("0.00", CultureInfo.InvariantCulture) : "") + ";" + (leftComputeLensResponse is object ? leftComputeLensResponse.value.lapCrossCurve.ToString("0.00", CultureInfo.InvariantCulture) : ""));

                streamWriter.WriteLine("CRIB=" + (rightParameters is object ? rightParameters[OmaParameter.CRIB] : "") + ";" + (leftParameters is object ? leftParameters[OmaParameter.CRIB] : ""));
                streamWriter.WriteLine("ELLH=" + (rightParameters is object ? rightParameters[OmaParameter.ELLH] : "") + ";" + (leftParameters is object ? leftParameters[OmaParameter.ELLH] : ""));

                streamWriter.WriteLine("LDDRSPH=" + (rightComputeLensResponse is object ? rightComputeLensResponse.value.designedDistanceReferencePointSpherePower.ToString("0.00", CultureInfo.InvariantCulture) : "") + ";" + (leftComputeLensResponse is object ? leftComputeLensResponse.value.designedDistanceReferencePointSpherePower.ToString("0.00", CultureInfo.InvariantCulture) : ""));

                streamWriter.WriteLine("LDDRCYL=" + (rightComputeLensResponse is object ? rightComputeLensResponse.value.designedDistanceReferencePointCylinderPower.ToString("0.00", CultureInfo.InvariantCulture) : "") + ";" + (leftComputeLensResponse is object ? leftComputeLensResponse.value.designedDistanceReferencePointCylinderPower.ToString("0.00", CultureInfo.InvariantCulture) : ""));

                streamWriter.WriteLine("LDDRAX=" + (rightComputeLensResponse is object ? rightComputeLensResponse.value.designedLayoutReferencePointCylinderAxis.ToString() : "") + ";" + (leftComputeLensResponse is object ? leftComputeLensResponse.value.designedLayoutReferencePointCylinderAxis.ToString() : ""));

                streamWriter.WriteLine("LDNRSPH=" + (rightComputeLensResponse is object ? rightComputeLensResponse.value.designedNearReferencePointSpherePower.ToString("0.00", CultureInfo.InvariantCulture) : "") + ";" + (leftComputeLensResponse is object ? leftComputeLensResponse.value.designedNearReferencePointSpherePower.ToString("0.00", CultureInfo.InvariantCulture) : ""));
                streamWriter.WriteLine("LDNRCYL=" + (rightComputeLensResponse is object ? rightComputeLensResponse.value.designedNearReferencePointCylinderPower.ToString("0.00", CultureInfo.InvariantCulture) : "") + ";" + (leftComputeLensResponse is object ? leftComputeLensResponse.value.designedNearReferencePointCylinderPower.ToString("0.00", CultureInfo.InvariantCulture) : ""));
                streamWriter.WriteLine("LDNRAX=" + (rightComputeLensResponse is object ? rightComputeLensResponse.value.designedNearReferencePointCylinderAxis.ToString() : "") + ";" + (leftComputeLensResponse is object ? leftComputeLensResponse.value.designedNearReferencePointCylinderAxis.ToString() : ""));

                streamWriter.WriteLine("LDADD=" + (rightComputeLensResponse is object ? rightComputeLensResponse.value.designedAdditionPower.ToString("0.00", CultureInfo.InvariantCulture) : "") + ";" + (leftComputeLensResponse is object ? leftComputeLensResponse.value.designedAdditionPower.ToString("0.00", CultureInfo.InvariantCulture) : ""));
                streamWriter.WriteLine("LDSGSPH=" + (rightComputeLensResponse is object ? rightComputeLensResponse.value.designedLayoutReferencePointSpherePower.ToString("0.00", CultureInfo.InvariantCulture) : "") + ";" + (leftComputeLensResponse is object ? leftComputeLensResponse.value.designedLayoutReferencePointSpherePower.ToString("0.00", CultureInfo.InvariantCulture) : ""));
                streamWriter.WriteLine("LDSGCYL=" + (rightComputeLensResponse is object ? rightComputeLensResponse.value.designedLayoutReferencePointCylinderPower.ToString("0.00", CultureInfo.InvariantCulture) : "") + ";" + (leftComputeLensResponse is object ? leftComputeLensResponse.value.designedLayoutReferencePointCylinderPower.ToString("0.00", CultureInfo.InvariantCulture) : ""));
                streamWriter.WriteLine("LDSGAX=" + (rightComputeLensResponse is object ? rightComputeLensResponse.value.designedNearReferencePointCylinderAxis.ToString() : "") + ";" + (leftComputeLensResponse is object ? leftComputeLensResponse.value.designedNearReferencePointCylinderAxis.ToString() : ""));

                streamWriter.WriteLine("LDPRVM=" + (rightComputeLensResponse is object ? rightComputeLensResponse.value.designedPrismMagnitude.ToString("0.00", CultureInfo.InvariantCulture) : "") + ";" + (leftComputeLensResponse is object ? leftComputeLensResponse.value.designedPrismMagnitude.ToString("0.00", CultureInfo.InvariantCulture) : ""));
                streamWriter.WriteLine("LDPRVA=" + (rightComputeLensResponse is object ? rightComputeLensResponse.value.designedPrismBaseSetting.ToString() : "") + ";" + (leftComputeLensResponse is object ? leftComputeLensResponse.value.designedPrismBaseSetting.ToString() : ""));

                string R_BCERIN = rightParameters is object ? rightParameters.ContainsKey(OmaParameter.SMOCIN) ? rightParameters[OmaParameter.SMOCIN] : rightParameters.ContainsKey(OmaParameter.BCERIN) ? rightParameters[OmaParameter.BCERIN] == "?" ? "0" : rightParameters[OmaParameter.BCERIN] : null : null;
                string L_BCERIN = leftParameters is object ? leftParameters.ContainsKey(OmaParameter.SMOCIN) ? leftParameters[OmaParameter.SMOCIN] : leftParameters.ContainsKey(OmaParameter.BCERIN) ? leftParameters[OmaParameter.BCERIN] == "?" ? "0" : leftParameters[OmaParameter.BCERIN] : null : null;
                string R_BCERUP = rightParameters is object ? rightParameters.ContainsKey(OmaParameter.SMOCUP) ? rightParameters[OmaParameter.SMOCUP] : rightParameters.ContainsKey(OmaParameter.BCERUP) ? rightParameters[OmaParameter.BCERUP] == "?" ? "0" : rightParameters[OmaParameter.BCERUP] : null : null;
                string L_BCERUP = leftParameters is object ? leftParameters.ContainsKey(OmaParameter.SMOCUP) ? leftParameters[OmaParameter.SMOCUP] : leftParameters.ContainsKey(OmaParameter.BCERUP) ? leftParameters[OmaParameter.BCERUP] == "?" ? "0" : leftParameters[OmaParameter.BCERUP] : null : null;

                if (R_BCERIN is object || L_BCERIN is object)
                {
                    streamWriter.WriteLine("BCERIN=" + R_BCERIN + ";" + L_BCERIN);
                }
                if (R_BCERUP is object || L_BCERUP is object)
                {
                    streamWriter.WriteLine("BCERUP=" + R_BCERUP + ";" + L_BCERUP);
                }
                if (!string.IsNullOrWhiteSpace(Gax))
                {
                    streamWriter.WriteLine("GAX=" + (
                    rightComputeLensResponse is object ?
                        Gax : ""
                ) + ';' + (
                    leftComputeLensResponse is object ? Gax : ""));
                }
                if (!string.IsNullOrWhiteSpace(LapGax))
                {
                    streamWriter.WriteLine("LAPGAX=" + (
                    rightComputeLensResponse is object ?
                        LapGax : ""
                ) + ';' + (
                    leftComputeLensResponse is object ? LapGax : ""));
                }

                streamWriter.WriteLine("KPRVM=" + "0" + ";" + "0");

                streamWriter.WriteLine("INSET=" + (rightComputeLensResponse is object ? rightComputeLensResponse.value.inset.ToString("0.00", CultureInfo.InvariantCulture) : "") + ";" + (leftComputeLensResponse is object ? leftComputeLensResponse.value.inset.ToString("0.00", CultureInfo.InvariantCulture) : ""));
                streamWriter.WriteLine("_DOWNSET=" + (rightComputeLensResponse is object ? rightComputeLensResponse.value.downset.ToString() : "") + ";" + (leftComputeLensResponse is object ? leftComputeLensResponse.value.downset.ToString() : ""));
                streamWriter.WriteLine("_FCRSSY=" + (rightComputeLensResponse is object ? rightComputeLensResponse.value.crossPosition.ToString() : "") + ";" + (leftComputeLensResponse is object ? leftComputeLensResponse.value.crossPosition.ToString() : ""));
                streamWriter.WriteLine("LDPATH=" + filePathBuilder.BuildFilePath());

                // RENDERE CONFIGURABILE/OPZIONALE LA RESTITUZIONE DI SVAL AVAL
                // streamWriter.WriteLine("SVAL=" + (rightParameters is object ? Sval(rightParameters, rightCentralThickness).ToString("0.00", CultureInfo.InvariantCulture) : "") + ";" + (leftParameters is object ? Sval(leftParameters, leftCentralThickness).ToString("0.00", CultureInfo.InvariantCulture) : ""));
                // streamWriter.WriteLine("AVAL=" + (rightParameters is object ? Aval(rightParameters, rightCentralThickness).ToString("0.00", CultureInfo.InvariantCulture) : "") + ";" + (leftParameters is object ? Aval(leftParameters, leftCentralThickness).ToString("0.00", CultureInfo.InvariantCulture) : ""));

                streamWriter.WriteLine("STATUS=" + "0");

            }

            string filePath = Path.ChangeExtension(tmpPath, ".lms");
            File.Delete(filePath);
            if (File.Exists(tmpPath)) File.Move(tmpPath, filePath);

        }

        private double Sval(IDictionary<OmaParameter, string> parameters, double centralThickness)
        {
            double radiusDx = 1000 * (StringConverter.ExtractDoubleValue(parameters, OmaParameter.LIND) - 1) / StringConverter.ExtractDoubleValue(parameters, OmaParameter.FRNT);
            double result = StringConverter.ExtractDoubleValue(parameters, OmaParameter.RNGH) - (radiusDx - Math.Sqrt(Math.Pow(radiusDx, 2) - Math.Pow(StringConverter.ExtractDoubleValue(parameters, OmaParameter.BLKD) / 2, 2)));
            result += centralThickness;
            return result;
        }

        private double Aval(IDictionary<OmaParameter, string> parameters, double centralThickness)
        {
            double result = Sval(parameters, centralThickness);
            if (parameters.ContainsKey(OmaParameter.BCTHK))
            {
                result += StringConverter.ExtractDoubleValue(parameters, OmaParameter.BCTHK);
            }
            else
            {
                result += 20;
            }
            return result;
        }

    }
}
