using DbTableEditor.Models;

namespace DbTableEditor.Services
{
    /// <summary>
    /// Служба для предоставления общих типов данных
    /// </summary>
    public class DataTypeProvider
    {
        public static IReadOnlyList<SqlDataType> CommonTypes { get; } =
        [
            SqlDataType.Int,
            SqlDataType.VarChar,
            SqlDataType.NVarChar,
            SqlDataType.DateTime,
            SqlDataType.Bit,
            SqlDataType.Decimal,
            SqlDataType.Float,
            SqlDataType.BigInt,
            SqlDataType.SmallInt,
            SqlDataType.TinyInt,
            SqlDataType.Real,
            SqlDataType.Date,
            SqlDataType.DateTime2,
            SqlDataType.SmallDateTime,
            SqlDataType.Time,
            SqlDataType.Char,
            SqlDataType.NChar,
            SqlDataType.Text,
            SqlDataType.NText,
            SqlDataType.UniqueIdentifier
        ];

        /// <summary>
        /// Преобразует строковое значение SQL типа в перечисление <see cref="SqlDataType"/>
        /// </summary>
        public static SqlDataType GetSqlDataType(string dataType)
        {
            switch (dataType.ToLowerInvariant())
            {
                case "int":
                    return SqlDataType.Int;
                case "varchar":
                    return SqlDataType.VarChar;
                case "nvarchar":
                    return SqlDataType.NVarChar;
                case "datetime":
                    return SqlDataType.DateTime;
                case "bit":
                    return SqlDataType.Bit;
                case "decimal":
                    return SqlDataType.Decimal;
                case "float":
                    return SqlDataType.Float;
                case "bigint":
                    return SqlDataType.BigInt;
                case "smallint":
                    return SqlDataType.SmallInt;
                case "tinyint":
                    return SqlDataType.TinyInt;
                case "real":
                    return SqlDataType.Real;
                case "date":
                    return SqlDataType.Date;
                case "datetime2":
                    return SqlDataType.DateTime2;
                case "smalldatetime":
                    return SqlDataType.SmallDateTime;
                case "time":
                    return SqlDataType.Time;
                case "char":
                    return SqlDataType.Char;
                case "nchar":
                    return SqlDataType.NChar;
                case "text":
                    return SqlDataType.Text;
                case "ntext":
                    return SqlDataType.NText;
                case "uniqueidentifier":
                    return SqlDataType.UniqueIdentifier;
                default:
                    throw new ArgumentOutOfRangeException(nameof(dataType), $"Неизвестный тип данных: {dataType}");
            }
        }

        /// <summary>
        /// Конвертирует значение перечисления <see cref="SqlDataType"/> в строковое представление SQL типа
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static string ToSqlTypeString(SqlDataType type)
        {
            return type switch
            {
                SqlDataType.NVarChar => "NVARCHAR(255)",
                SqlDataType.VarChar => "VARCHAR(255)",
                SqlDataType.Decimal => "DECIMAL(18,2)",
                SqlDataType.Char => "CHAR(10)",
                SqlDataType.NChar => "NCHAR(10)",
                SqlDataType.Text => "TEXT",
                SqlDataType.NText => "NTEXT",
                SqlDataType.UniqueIdentifier => "UNIQUEIDENTIFIER",
                SqlDataType.DateTime => "DATETIME",
                SqlDataType.DateTime2 => "DATETIME2",
                SqlDataType.SmallDateTime => "SMALLDATETIME",
                SqlDataType.Date => "DATE",
                SqlDataType.Time => "TIME",
                SqlDataType.Int => "INT",
                SqlDataType.BigInt => "BIGINT",
                SqlDataType.SmallInt => "SMALLINT",
                SqlDataType.TinyInt => "TINYINT",
                SqlDataType.Bit => "BIT",
                SqlDataType.Float => "FLOAT",
                SqlDataType.Real => "REAL",
                _ => type.ToString().ToUpperInvariant()
            };
        }

    }
}
