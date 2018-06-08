using Xunit;

namespace Refactorius.Common.Tests
{
    public class SqueezeTests
    {
        [Fact]
        public void StripsWhitespaceAndTrims()
        {
            var s = " \t \r \n aaa \t \r\n bbb \t \r \n ";
            var s1 = s.Squeeze();
            Assert.Equal("aaa bbb", s1);
        }

        [Fact]
        public void SqueezesToEmpty()
        {
            var s = "  \t \r\n  \t\t \r\n  ";
            var s1 = s.Squeeze();
            Assert.Equal(string.Empty, s1);
        }
    }
}