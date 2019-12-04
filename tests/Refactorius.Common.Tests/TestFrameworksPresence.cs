using System;
using Xunit;

namespace Refactorius.Common.Tests
{
    public class TestFrameworksPresence
    {
        [SkippableFact]
        public void SkipIfFullFramework()
        {
            Skip.If(System.Runtime.InteropServices.RuntimeInformation.FrameworkDescription.StartsWith(".NET Framework"));

            if (!System.Runtime.InteropServices.RuntimeInformation.FrameworkDescription.StartsWith(".NET Core"))
                throw new Exception();
        }

        [SkippableFact]
        public void SkipIfDotNetCore()
        {
            Skip.If(System.Runtime.InteropServices.RuntimeInformation.FrameworkDescription.StartsWith(".NET Core"));

            if (!System.Runtime.InteropServices.RuntimeInformation.FrameworkDescription.StartsWith(".NET Framework"))
                throw new Exception();
        }
    }
}