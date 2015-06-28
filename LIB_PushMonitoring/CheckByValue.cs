using System;

namespace TRoschinsky.Lib.PushMonitoring
{
    /// <summary>
    /// The class CheckByValue is a specialized sub type of checks that can be 
    /// used by specific check types that will obtain ranged values to determine 
    /// wether the check result is positive or negative.
    /// </summary>
    public abstract class CheckByValue : Check
    {
        protected double minValue = double.MinValue;
        public double MinValue { get { return minValue; } set { minValue = value; } }
        protected double maxValue = double.MaxValue;
        public double MaxValue { get { return maxValue; } set { maxValue = value; } }
        protected double currentValue = 0;
        public double CurrentValue { get { return currentValue; } protected set { currentValue = value; } }
        protected string valueUnit = String.Empty;
        public string ValueUnit { get { return valueUnit; } }

        public override bool NotifyRequired { get { return (MinValue > double.MinValue && CurrentValue < MinValue) || (MaxValue < double.MaxValue && CurrentValue > MaxValue); } }


        public CheckByValue(object input, double minValue, double maxValue, string valueUnit)
            : base(input)
        {
            this.minValue = minValue;
            this.maxValue = maxValue;
            this.valueUnit = valueUnit;
        }

        public CheckByValue(object input, string valueUnit)
            : base(input)
        {
            this.valueUnit = valueUnit;
        }
        
        public override string ToString()
        {
            if (LastCheck != null && LastCheck > DateTime.Now.AddHours(-1))
            {
                if (String.IsNullOrWhiteSpace(Output))
                {
                    Output = String.Format("{0}{1}", CurrentValue, ValueUnit);
                }
                return String.Format("Check '{0}' {1} with {2}", Name, (NotifyRequired ? "is out of valid range" : "within expceted scope"), Output);
            }
            else
            {
                return String.Format("Check '{0}' was not executed recently", Name);
            }
        }
    }
}
