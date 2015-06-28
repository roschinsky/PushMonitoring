using System;

namespace TRoschinsky.Lib.PushMonitoring
{
    /// <summary>
    /// The class Check represents the abstract base class of all check templates. 
    /// Every check type the application is able to host has its origin right here.
    /// </summary>
    public abstract class Check
    {
        protected string name;
        public string Name
        {
            get { return GetName(); }
            set { name = value; }
        }
        protected object input;
        public object Input { get { return input; } }
        public string Output { get; protected set; }

        public DateTime LastCheck { get; protected set; }
        public virtual bool NotifyRequired { get; protected set; }


        public Check(object input)
        {
            this.input = input;
        }

        public virtual void ExecuteCheck()
        {
            throw new NotImplementedException("No ExecuteCheck method implemented in abstract Check class; please use specific Check type class!");
        }

        protected virtual string GetName()
        {
            return name;
        }

        public override string ToString()
        {
            if (LastCheck != null && LastCheck > DateTime.Now.AddHours(-1))
            {
                return String.Format("Check '{0}' is {1}.", Name, (NotifyRequired ? "out of valid range" : "within expceted scope"));
            }
            else
            {
                return String.Format("Check '{0}' was not executed recently", Name);
            }
        }
    }
}
