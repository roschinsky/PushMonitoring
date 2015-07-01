using System;

namespace TRoschinsky.Lib.PushMonitoring
{
    /// <summary>
    /// The class CheckByState is a specialized sub type of checks that can be 
    /// used by specific check types that will obtain a boolean value to determine 
    /// wether the check result is positive or negative.
    /// </summary>
    public abstract class CheckByState : Check
    {
        public bool CheckSuccessful { get; set; }
        
        public override bool NotifyRequired { get { return !CheckSuccessful; } }


        public CheckByState(object input)
            : base(input)
        {
            this.input = input;
        }

        public override string ToString()
        {
            if (String.IsNullOrWhiteSpace(Output))
            {
                return String.Format("Check '{0}' was {1}.", Name, (NotifyRequired ? "not successful" : "okay"));
            }
            else
            {
                return String.Format("Check '{0}' was {1}: {2}", Name, (NotifyRequired ? "not successful" : "okay"), Output);
            }
        }
    }
}
