using System;
using Xunit;

namespace Refactorius.Common.Tests
{
    public class TestFrameworksPresence
    {
        [SkippableFact]
        public void SkipIfFullFramework()
        {
            Skip.If(frameworkDescription.StartsWith(".NET Framework") || frameworkDescription.StartsWith(".NET 5"));

            if (!frameworkDescription.StartsWith(".NET Core"))
                throw new Exception();
        }

        [SkippableFact]
        public void SkipIfDotNetCore()
        {
            Skip.If(frameworkDescription.StartsWith(".NET Core") || frameworkDescription.StartsWith(".NET 5"));

            if (!frameworkDescription.StartsWith(".NET Framework"))
                throw new Exception();
        }

        private static string frameworkDescription = System.Runtime.InteropServices.RuntimeInformation.FrameworkDescription;
    }
}