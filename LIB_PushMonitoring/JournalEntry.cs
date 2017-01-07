using System;

namespace TRoschinsky.Common
{
    /// <summary>
    /// This class is a very simple log-entry class that can be used to keep errors and important 
    /// events whithin multiple objects of different types in a structured and consistent format.
    /// You may use it with an array or a typed List to persist application logs at runtime.
    /// </summary>
    public class JournalEntry
    {
        private DateTime timeStamp = DateTime.Now;
        public DateTime TimeStamp { get { return timeStamp; } }
        public string Message { get; private set; }
        public string Origin { get; private set; } = String.Empty;
        public Exception Error { get; private set; }
        public bool IsWarning { get; private set; } = false;
        public string EntryType { get { return IsWarning ? (Error != null ? "ERR" : "WRN") : "INF"; } }

        /// <summary>
        /// Constructor for logging an INFormation
        /// </summary>
        /// <param name="message">Just the message you like to log</param>
        public JournalEntry(string message)
        {
            this.Message = message;
            this.IsWarning = false;
        }

        /// <summary>
        /// Constructor for logging an INFormation
        /// </summary>
        /// <param name="message">Just the message you like to log</param>
        /// <param name="origin">Keep in mind where it's comming from! Maybe a method, class or component name.</param>
        public JournalEntry(string message, string origin)
        {
            this.Message = message;
            this.Origin = origin;
            this.IsWarning = false;
        }

        /// <summary>
        /// Constructor for logging an INFormation or a WaRNing
        /// </summary>
        /// <param name="message">Just the message you like to log</param>
        /// <param name="isWarning">True if it should be a warning; otherwise it's just a information</param>
        public JournalEntry(string message, bool isWarning)
        {
            this.Message = message;
            this.IsWarning = isWarning;
        }

        /// <summary>
        /// Constructor for logging an INFormation or a WaRNing
        /// </summary>
        /// <param name="message">Just the message you like to log</param>
        /// <param name="origin">Keep in mind where it's comming from! Maybe a method, class or component name.</param>
        /// <param name="isWarning">True if it should be a warning; otherwise it's just a information</param>
        public JournalEntry(string message, string origin, bool isWarning)
        {
            this.Message = message;
            this.Origin = origin;
            this.IsWarning = isWarning;
        }

        /// <summary>
        /// Constructor for logging an ERRor
        /// </summary>
        /// <param name="message">Just the message you like to log</param>
        /// <param name="ex">The error as an exception</param>
        public JournalEntry(string message, Exception ex)
        {
            this.Message = message;
            this.IsWarning = (ex != null);
            this.Error = ex;
        }

        /// <summary>
        /// Constructor for logging an ERRor
        /// </summary>
        /// <param name="message">Just the message you like to log</param>
        /// <param name="origin">Keep in mind where it's comming from! Maybe a method, class or component name.</param>
        /// <param name="ex">The error as an exception</param>
        public JournalEntry(string message, string origin, Exception ex)
        {
            this.Message = message;
            this.Origin = origin;
            this.IsWarning = (ex != null);
            this.Error = ex;
        }

        /// <summary>
        /// Constructs a "outputable" string for generating consistent log files
        /// </summary>
        /// <returns>A log line</returns>
        public override string ToString()
        {
            string returnMessage = (Error != null ? Message + " (" + Error.GetType().Name + ": " + Error.Message + ")" : Message);
            return String.Format("{0} [{1}]: {2}", TimeStamp, EntryType, returnMessage);
        }
    }
}
