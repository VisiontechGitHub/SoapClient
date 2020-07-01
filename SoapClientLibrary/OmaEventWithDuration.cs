using System;
using System.Collections.Generic;
using System.Text;

namespace SoapClientLibrary
{
    public class OmaEventWithDuration : OmaEvent
    {
        public OmaEventWithDuration(string Job, string LeftDesign, string RightDesign, bool PreCalculation, DateTime StartDateTime) :
            base(Job, LeftDesign, RightDesign, PreCalculation, StartDateTime)
        {}

        public TimeSpan Duration { get; set; }

        public OmaEvent ToOmaEvent()
        {
            OmaEvent equivalent = new OmaEvent(Job, LeftDesign, RightDesign, PreCalculation, StartDateTime);
            equivalent.Result = this.Result;
            equivalent.HasErrors = this.HasErrors;
            return equivalent;
        }

    }
}
