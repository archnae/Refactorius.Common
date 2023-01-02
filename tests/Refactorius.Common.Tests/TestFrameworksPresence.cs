using System;
using Xunit;
using System.Text.RegularExpressions;

namespace Refactorius.Common.Tests
{
    public class TestFrameworksPresence
    {
        [SkippableFact]
        public void SkipIfOldFramework4()
        {
            Skip.If(frameworkDescription.StartsWith(".NET Framework"));

            if (!Regex.IsMatch(frameworkDescription, ".NET (Core|5|6|7)"))
                throw new Exception();
        }

        [SkippableFact]
        public void SkipIfNewFramework()
        {
            Skip.If(Regex.IsMatch(frameworkDescription, ".NET (5|6)"));

            if (!Regex.IsMatch(frameworkDescription, ".NET (Framework|Core)"))
                throw new Exception();
        }

        [SkippableFact]
        public void SkipIfDotNetCore()
        {
            Skip.If(frameworkDescription.StartsWith(".NET Core"));

            if (!Regex.IsMatch(frameworkDescription, ".NET (Framework|5|6|7)"))
                throw new Exception();
        }

        private static string frameworkDescription = System.Runtime.InteropServices.RuntimeInformation.FrameworkDescription;
    }
}