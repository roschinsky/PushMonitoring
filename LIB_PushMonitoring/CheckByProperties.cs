using System;
using System.Security.Cryptography;
using System.Text;

namespace TRoschinsky.Lib.PushMonitoring
{
    /// <summary>
    /// The class CheckByProperties is a specialized sub type of checks that can be 
    /// used by specific check types that will obtain specific string-based values 
    /// to determine wether the check result is positive or negative.
    /// </summary>
    public abstract class CheckByProperties : Check
    {
        public enum Comparison
        {
            CompleteMatch,
            StartsWith,
            EndsWith,
            Contains,
            LengthOfValue,
            Length,
            HashOfValue,
            Hash
        }

        protected Tuple<object, object, object, object> searchFor;
        public Tuple<object, object, object, object> SearchFor { get { return searchFor; } set { searchFor = value; } }
        protected string expectedValue = String.Empty;
        public string ExpectedValue { get { return expectedValue; } set { expectedValue = value; } }
        protected string currentValue = String.Empty;
        public string CurrentValue { get { return currentValue; } protected set { currentValue = value; } }
        protected Comparison compareType = Comparison.CompleteMatch;
        public Comparison CompareType { get { return compareType; } set { compareType = value; } }
        protected bool isCaseSensitive = true;
        public bool IsCaseSensitive { get { return isCaseSensitive; } set { isCaseSensitive = value; } }

        public override string Output { get { return String.IsNullOrWhiteSpace(output) ? String.Format("{0}", CurrentValue) : output; } }

        public override bool NotifyRequired
        {
            get
            {
                switch (compareType)
                {
                    case Comparison.Hash:
                        return (MD5.Create().ComputeHash(Encoding.ASCII.GetBytes(currentValue)) == MD5.Create().ComputeHash(Encoding.ASCII.GetBytes(expectedValue)));
                    case Comparison.HashOfValue:
                        return (MD5.Create().ComputeHash(Encoding.ASCII.GetBytes(currentValue)) == Encoding.ASCII.GetBytes(expectedValue));
                    case Comparison.Length:
                        return (currentValue.Length == int.Parse(expectedValue));
                    case Comparison.LengthOfValue:
                        return (currentValue.Length == expectedValue.Length);
                    case Comparison.Contains:
                        return (currentValue.Contains(expectedValue));
                    case Comparison.StartsWith:
                        return (currentValue.StartsWith(expectedValue));
                    case Comparison.EndsWith:
                        return (currentValue.EndsWith(expectedValue));
                    default:
                        return (currentValue.Equals(expectedValue));
                }
            }
        }


        public CheckByProperties(object input, Tuple<object, object, object, object> searchFor, string expectedValue)
            : base(input)
        {
            this.searchFor = searchFor;
            this.expectedValue = expectedValue;
        }
    }
}
