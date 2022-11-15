using System;
using System.Runtime.InteropServices;
using System.Security;
using Refactorius.Data;

namespace Refactorius
{
    /// <summary>The replacement for GuidComb that is more like what MS SQL Server does.</summary>
    /// <remarks>Cheers to
    /// https://blogs.msdn.microsoft.com/dbrowne/2012/07/03/how-to-generate-sequential-guids-for-sql-server-in-net/ .</remarks>
    public static class GuidSeq
    {
#if NET6_0_OR_GREATER
        [DllImport("rpcrt4.dll", SetLastError = true)]
        private static extern int UuidCreateSequential(out Guid guid);

        private static bool _sandboxed;

        /// <summary>Creates a new sequential <see cref="Guid"/>.</summary>
        /// <returns>A new sequential <see cref="Guid"/>.</returns>
        /// <remarks>Version by David Browne,
        /// <see
        ///     href="https://blogs.msdn.microsoft.com/dbrowne/2012/07/03/how-to-generate-sequential-guids-for-sql-server-in-net/"/>
        /// . Uses UuidCreateSequential to generate GUIDs that are sequential according to SQL Server’s GUID sort order.</remarks>
        [SecuritySafeCritical]
        public static Guid NewGuid()
        {
            if (_sandboxed)
#pragma warning disable 618
                return GuidComb.NewGuid();
#pragma warning restore 618

            Guid guid;
            try
            {
                // will fail in a partial trust domain
                UuidCreateSequential(out guid);
            }
            catch (SecurityException)
            {
                _sandboxed = true;
#pragma warning disable 618
                return GuidComb.NewGuid();
#pragma warning restore 618
            }

            var s = guid.ToByteArray();
            var t = new byte[16];
            t[3] = s[0];
            t[2] = s[1];
            t[1] = s[2];
            t[0] = s[3];
            t[5] = s[4];
            t[4] = s[5];
            t[7] = s[6];
            t[6] = s[7];
            t[8] = s[8];
            t[9] = s[9];
            t[10] = s[10];
            t[11] = s[11];
            t[12] = s[12];
            t[13] = s[13];
            t[14] = s[14];
            t[15] = s[15];

            return new Guid(t);
        }
#else
        /// <summary>Creates a new sequential <see cref="Guid"/>.</summary>
        /// <returns>A new sequential <see cref="Guid"/>.</returns>
        public static Guid NewGuid()
        {
#pragma warning disable 618
        return GuidComb.NewGuid();
#pragma warning restore 618
        }
#endif
    }
}