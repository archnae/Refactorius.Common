using System;
using System.ComponentModel;
using System.Data;
using System.Globalization;
using JetBrains.Annotations;

namespace Refactorius.Data
{
    /// <summary>Convert a base data type to another base data type.</summary>
    public static class DbTypeConvert
    {
        [NotNull] private static readonly DbTypeMapEntry[] _knownDbTypes = InitializeMap();

        #region private methods

        [NotNull]
        private static DbTypeMapEntry[] InitializeMap()
        {
            // TODO: add new NH2.1 / SQL2008 datatypes
            return new[]
            {
                new DbTypeMapEntry(typeof(bool), DbType.Boolean, SqlDbType.Bit),
                new DbTypeMapEntry(typeof(byte), DbType.Double, SqlDbType.TinyInt),
                new DbTypeMapEntry(typeof(byte[]), DbType.Binary, SqlDbType.Image),
                new DbTypeMapEntry(typeof(DateTime), DbType.DateTime, SqlDbType.DateTime),
                new DbTypeMapEntry(typeof(decimal), DbType.Decimal, SqlDbType.Decimal),
                new DbTypeMapEntry(typeof(double), DbType.Double, SqlDbType.Float),
                new DbTypeMapEntry(typeof(Guid), DbType.Guid, SqlDbType.UniqueIdentifier),
                new DbTypeMapEntry(typeof(short), DbType.Int16, SqlDbType.SmallInt),
                new DbTypeMapEntry(typeof(int), DbType.Int32, SqlDbType.Int),
                new DbTypeMapEntry(typeof(long), DbType.Int64, SqlDbType.BigInt),
                new DbTypeMapEntry(typeof(object), DbType.Object, SqlDbType.Variant),
                new DbTypeMapEntry(typeof(string), DbType.String, SqlDbType.VarChar)
            };
        }

        #endregion

        #region nested types

        private struct DbTypeMapEntry
        {
            [NotNull] public readonly Type Type;
            public readonly DbType DbType;
            public readonly SqlDbType SqlDbType;

            public DbTypeMapEntry([NotNull] Type type, DbType dbType, SqlDbType sqlDbType)
            {
                Type = type;
                DbType = dbType;
                SqlDbType = sqlDbType;
            }
        }

        #endregion

        #region constructors

        #endregion

        #region public methods

        /// <summary>Convert db type to .Net data type.</summary>
        /// <param name="dbType">A source <see cref="DbType"/>.</param>
        /// <returns>The target .Net type.</returns>
        [NotNull]
        public static Type ToNetType(DbType dbType)
        {
            if (!Enum.IsDefined(typeof(DbType), dbType))
                throw new InvalidEnumArgumentException(nameof(dbType), (int) dbType, typeof(DbType));

            return Find(dbType).Type;
        }

        /// <summary>Convert TSQL type to .Net data type.</summary>
        /// <param name="sqlDbType">A source <see cref="SqlDbType"/>.</param>
        /// <returns>The target .Net type.</returns>
        [NotNull]
        public static Type ToNetType(SqlDbType sqlDbType)
        {
            if (!Enum.IsDefined(typeof(SqlDbType), sqlDbType))
                throw new InvalidEnumArgumentException(nameof(sqlDbType), (int) sqlDbType, typeof(SqlDbType));

            return Find(sqlDbType).Type;
        }

        /// <summary>Convert .Net type to Db type.</summary>
        /// <param name="type">A source .Net type.</param>
        /// <returns>The target <see cref="DbType"/>.</returns>
        public static DbType ToDbType([NotNull] Type type)
        {
            return Find(type).DbType;
        }

        /// <summary>Convert TSQL data type to DbType.</summary>
        /// <param name="sqlDbType">A source <see cref="SqlDbType"/>.</param>
        /// <returns>The target <see cref="DbType"/>.</returns>
        public static DbType ToDbType(SqlDbType sqlDbType)
        {
            return Find(sqlDbType).DbType;
        }

        /// <summary>Convert .Net type to TSQL data type.</summary>
        /// <param name="type">A source .Net type.</param>
        /// <returns>The target <see cref="SqlDbType"/>.</returns>
        public static SqlDbType ToSqlDbType([NotNull] Type type)
        {
            return Find(type).SqlDbType;
        }

        /// <summary>Convert DbType type to TSQL data type.</summary>
        /// <param name="dbType">A source <see cref="DbType"/>.</param>
        /// <returns>The target <see cref="SqlDbType"/>.</returns>
        public static SqlDbType ToSqlDbType(DbType dbType)
        {
            return Find(dbType).SqlDbType;
        }

        private static DbTypeMapEntry Find(Type type)
        {
            type.MustNotBeNull(nameof(type));

            var entry = Array.Find(_knownDbTypes, x => x.Type == type);

            if (entry.Type == null)
                throw new ArgumentException(
                    string.Format(CultureInfo.InvariantCulture, "Unsupported .Net type '{0}'.", type.FullName));

            return entry;
        }

        private static DbTypeMapEntry Find(DbType dbType)
        {
            var entry = Array.Find(_knownDbTypes, x => x.DbType == dbType);

            // TODO: default value of DbType?
            if (entry.Type == null)
                throw new ArgumentException(
                    string.Format(CultureInfo.InvariantCulture, "Unsupported DbType '{0}'.", dbType));

            return entry;
        }

        private static DbTypeMapEntry Find(SqlDbType sqlDbType)
        {
            var entry = Array.Find(_knownDbTypes, x => x.SqlDbType == sqlDbType);

            if (entry.Type == null)
                throw new ArgumentException(
                    string.Format(CultureInfo.InvariantCulture, "Unsupported SqlDbType '{0}'.", sqlDbType));

            return entry;
        }

        #endregion
    }
}