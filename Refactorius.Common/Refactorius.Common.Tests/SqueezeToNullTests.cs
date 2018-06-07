using Xunit;

namespace Refactorius.Common.Tests
{
    public class SqueezeToNullTests
    {
        [Fact]
        public void StripsWhitespaceAndTrims()
        {
            var s = " \t \r \n aaa \t \r\n bbb \t \r \n ";
            var s1 = s.SqueezeToNull();
            Assert.Equal("aaa bbb", s1);
        }

        [Fact]
        public void SqueezesToNull()
        {
            var s = "  \t \r\n  \t\t \r\n  ";
            var s1 = s.SqueezeToNull();
            Assert.Null(s1);
        }
    }
}