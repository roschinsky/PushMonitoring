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
        public string output;
        public virtual string Output { get { return output; } set { output = value; } }

        public DateTime LastCheck { get; protected set; }
        public virtual bool NotifyRequired { get; protected set; }


        public Check(object input)
        {
            this.input = input;
        }

        public virtual void ExecuteCheck()
        {
            LastCheck = DateTime.Now;
            throw new NotImplementedException("No ExecuteCheck method implemented in abstract Check class; please use specific check type class!");
        }

        protected virtual string GetName()
        {
            return name;
        }

        public override string ToString()
        {
            if (LastCheck != null && LastCheck > DateTime.Now.AddHours(-1))
            {
                if (String.IsNullOrWhiteSpace(output))
                {
                    return String.Format("[{0}] Check '{1}' {2}.", (NotifyRequired ? "N" : "Y"), Name, (NotifyRequired ? "failed" : "was okay"));
                }
                else
                {
                    return String.Format("[{0}] Check '{1}' {2} with {3}.", (NotifyRequired ? "N" : "Y"), Name, (NotifyRequired ? "failed" : "was okay"), Output);
                }
            }
            else
            {
                return String.Format("[?] Check '{0}' was not executed recently.", Name);
            }
        }
    }
}
