using System;

namespace SoapClientLibrary
{
    public class OmaEvent
    {
        public string Job { get; set; }
        public string LeftDesign { get; set; }
        public string RightDesign { get; set; }
        public string Result { get; set; }
        public bool PreCalculation { get; set; }
        public DateTime StartDateTime { get; set; }
        public bool HasErrors { get; set; }
        public OmaEvent(string Job, string LeftDesign, string RightDesign, bool PreCalculation, DateTime StartDateTime)
        {
            this.Job = Job;
            this.LeftDesign = LeftDesign;
            this.RightDesign = RightDesign;
            this.PreCalculation = PreCalculation;
            this.StartDateTime = StartDateTime;
        }
    }
}
