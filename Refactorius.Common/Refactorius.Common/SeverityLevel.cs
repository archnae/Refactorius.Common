namespace Refactorius
{
    /// <summary>Validation error or logging message severity level.</summary>
    public enum SeverityLevel
    {
        /// <summary>No error; message completely ignored.</summary>
        None = 0,

        /// <summary>A debug message describes the internal working of the application at the <b>very</b> verbose fine-grained
        /// level.</summary>
        /// <remarks>Displayed only in extended debug/trace mode (developer view).</remarks>
        Verbose,

        /// <summary>A debug message describes the internal working of the application at the fine-grained level.</summary>
        /// <remarks>Displayed only in debug trace mode (developer view).</remarks>
        Trace,

        /// <summary>A debug message describes the internal working of the application.</summary>
        /// <remarks>Displayed only in debug mode (developer view).</remarks>
        Debug,

        /// <summary>Informational message describes the progress of the application.</summary>
        /// <remarks>Can be useful for the end user (user view).</remarks>
        Info,

        /// <summary>A warning indicates some abnormal condition that must be displayed to the user but doesn't interrupt the
        /// operation.</summary>
        /// <remarks>Must be displayed to the user (user view).</remarks>
        Warning,

        /// <summary>An error indicates some abnormal condition that usually terminates the current operation.</summary>
        /// <remarks>Must be displayed to the user and the administrator (admin view).</remarks>
        Error,

        /// <summary>A severe error terminates the current operation, application is assumed to be broken or misconfigured.</summary>
        /// <remarks>Must be displayed to the user and the administrator (admin view).</remarks>
        Severe,

        /// <summary>A fatal error means that application is unusable and must be terminated.</summary>
        /// <remarks>Must be displayed to the administrator (admin view). It may be impossible to display the error to the user.</remarks>
        Fatal,

        /// <summary>A synonim for <see cref="Warning"/>.</summary>
        /// <remarks>Useful to keep compatability with log4net which uses <b>WARN</b> instead of <b>Warning</b>.</remarks>
        Warn = Warning
    }
}