using System;
using System.Runtime.InteropServices;
using Xunit;

namespace Refactorius.Common.Tests
{
    public class RegistryHelperTests
    {
        private Data.RegistryHelper _rh;

        [Fact]
        public void CtorOkWin32()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                _rh = new Data.RegistryHelper(Microsoft.Win32.Registry.CurrentUser);
            }
        }

        [Fact]
        public void CtorThrowsLinux()
        {
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                Assert.Throws<Exception>(() =>
                {
                    _rh = new Data.RegistryHelper(Microsoft.Win32.Registry.CurrentUser);
                });
            }
        }
    }
}