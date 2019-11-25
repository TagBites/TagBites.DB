using System;
using System.Linq;
using TagBites.DB.Postgres;
using TagBites.DB.Tests.DB.Core;
using Xunit;

namespace TagBites.DB.Tests.DB.PostgreSql
{
    public class CustomTypesTest : DbTestBase
    {
        [Fact]
        public void SupportsArrayTypeTest()
        {
            using var link = NpgsqlProvider.CreateLink();

            // {ala,NULL," ma ","NULL","null"}
            Compare(new PgSqlArray("ala", null, " ma ", "NULL", "null"));

            // {1,NULL,3}
            CompareT<int>(new PgSqlArray<int>(1, null, 3));

            // [3:5]={True,NULL,False}
            CompareT<bool>(new PgSqlArray<bool>(true, null, false) { StartIndex = 3 });

            void Compare(PgSqlArray array)
            {
                var value = link.ExecuteScalar("SELECT {0}::text[]", array.ToString());

                switch (value)
                {
                    case string s:
                        {
                            var valueArray = PgSqlArray.TryParseDefault(s);
                            Assert.Equal<string>(array, valueArray);
                            break;
                        }

                    case string[] strings:
                        Assert.Equal<string>(array, strings);
                        break;
                }
            }
            void CompareT<T>(PgSqlArray<T> array) where T : struct
            {
                var name = typeof(T) == typeof(int)
                    ? "int"
                    : typeof(T).Name.ToLower();

                var value = link.ExecuteScalar($"SELECT {{0}}::{name}[]", array.ToString());

                switch (value)
                {
                    case string s:
                        {
                            var valueArray = PgSqlArray<T>.TryParseDefault(s);
                            Assert.Equal<T?>(array, valueArray);
                            break;
                        }

                    case T?[] nullable:
                        Assert.Equal<T?>(array, nullable);
                        break;

                    case T[] nonNullable:
                        {
                            var items = array.ToArray().Select(x => x.GetValueOrDefault()).ToArray();
                            Assert.Equal<T>(items, nonNullable);
                            break;
                        }

                    default:
                        throw new NotSupportedException();
                }
            }
        }

        //[Fact]
        //public void SupportsUnknownTypeTest()
        //{
        //    using var link = NpgsqlProvider.CreateLink();
        //    //var connection = link.ConnectionContext.GetOpenConnection() as NpgsqlConnection;
        //    //RegisterTypes(connection);

        //    var mpq = link.ExecuteScalar("SELECT '1/2'::mpq");
        //    Assert.Equal("1/2", mpq);
        //}



        //static void RegisterTypes(NpgsqlConnection connection)
        //{
        //    try
        //    {
        //        connection.TypeMapper.AddMapping(new NpgsqlTypeMappingBuilder
        //        {
        //            PgTypeName = "mpq",
        //            ClrTypes = new[] { typeof(Fraction) },
        //            TypeHandlerFactory = new FractionHandlerFactory()
        //        }.Build());
        //    }
        //    catch
        //    {

        //    }
        //}

        //class FractionHandlerFactory : NpgsqlTypeHandlerFactory<Fraction>
        //{
        //    public override NpgsqlTypeHandler<Fraction> Create(PostgresType pgType, NpgsqlConnection conn)
        //    {
        //        return new FractionHandler(pgType);
        //    }
        //}

        ////  Tylko przykład - nie działa
        //class FractionHandler : NpgsqlSimpleTypeHandler<Fraction>
        //{
        //    public override Fraction Read(NpgsqlReadBuffer buf, int len, FieldDescription fieldDescription = null)
        //    {
        //        var str = buf.ReadString(len - 1);
        //        buf.ReadByte();

        //        var ret = Fraction.Parse(str);
        //        return ret;
        //    }

        //    public override int ValidateAndGetLength(Fraction value, NpgsqlParameter parameter)
        //    {
        //        return value.ToString().Length + 1;
        //    }
        //    public override void Write(Fraction value, NpgsqlWriteBuffer buf, NpgsqlParameter parameter)
        //    {
        //        buf.WriteString(value.ToString());
        //        buf.WriteByte(0);
        //    }
        //    public FractionHandler(PostgresType postgresType)
        //        : base(postgresType)
        //    { }
        //}

        //[TypeConverter(typeof(FractionConverter))]
        //public struct Fraction : IEquatable<Fraction>, IConvertible
        //{
        //    private long _numerator;
        //    public long Numerator
        //    {
        //        get { return _numerator; }
        //        private set { _numerator = value; }
        //    }

        //    private long _denominator;
        //    public long Denominator
        //    {
        //        get { return _denominator == 0 ? 1 : _denominator; }
        //        private set
        //        {
        //            if (value == 0)
        //                throw new InvalidOperationException("Denominator cannot be assigned a 0 Value.");

        //            _denominator = value;
        //        }
        //    }

        //    public Fraction(long value)
        //    {
        //        _numerator = value;
        //        _denominator = 1;
        //        Reduce();
        //    }
        //    public Fraction(long numerator, long denominator)
        //    {
        //        if (denominator == 0)
        //            throw new InvalidOperationException("Denominator cannot be assigned a 0 Value.");

        //        _numerator = numerator;
        //        _denominator = denominator;
        //        Reduce();
        //    }


        //    private void Reduce()
        //    {
        //        try
        //        {
        //            if (Numerator == 0)
        //            {
        //                Denominator = 1;
        //                return;
        //            }

        //            var iGCD = GCD(Numerator, Denominator);
        //            Numerator /= iGCD;
        //            Denominator /= iGCD;

        //            if (Denominator < 0)
        //            {
        //                Numerator *= -1;
        //                Denominator *= -1;
        //            }
        //        }
        //        catch (Exception exp)
        //        {
        //            throw new InvalidOperationException("Cannot reduce Fraction: " + exp.Message);
        //        }
        //    }

        //    public bool Equals(Fraction other)
        //    {
        //        if (other == null)
        //            return false;

        //        return (Numerator == other.Numerator && Denominator == other.Denominator);
        //    }
        //    public override bool Equals(object obj)
        //    {
        //        if (obj == null || !(obj is Fraction))
        //            return false;

        //        return Equals((Fraction)obj);
        //    }
        //    public override int GetHashCode()
        //    {
        //        return Convert.ToInt32((Numerator ^ Denominator) & 0xFFFFFFFF);
        //    }
        //    public override string ToString()
        //    {
        //        if (Denominator == 1)
        //            return Numerator.ToString();

        //        var sb = new System.Text.StringBuilder();
        //        sb.Append(Numerator);
        //        sb.Append('/');
        //        sb.Append(Denominator);

        //        return sb.ToString();
        //    }

        //    public static Fraction Parse(string strValue)
        //    {
        //        var i = strValue.IndexOf('/');
        //        if (i == -1)
        //            return DecimalToFraction(Convert.ToDecimal(strValue));

        //        var iNumerator = Convert.ToInt64(strValue.Substring(0, i));
        //        var iDenominator = Convert.ToInt64(strValue.Substring(i + 1));
        //        return new Fraction(iNumerator, iDenominator);
        //    }
        //    public static bool TryParse(string strValue, out Fraction fraction)
        //    {
        //        if (!string.IsNullOrWhiteSpace(strValue))
        //        {
        //            try
        //            {
        //                var i = strValue.IndexOf('/');
        //                if (i == -1)
        //                {
        //                    decimal dValue;
        //                    if (decimal.TryParse(strValue, out dValue))
        //                    {
        //                        fraction = DecimalToFraction(dValue);
        //                        return true;
        //                    }
        //                }
        //                else
        //                {
        //                    long iNumerator, iDenominator;
        //                    if (long.TryParse(strValue.Substring(0, i), out iNumerator) && long.TryParse(strValue.Substring(i + 1), out iDenominator))
        //                    {
        //                        fraction = new Fraction(iNumerator, iDenominator);
        //                        return true;
        //                    }
        //                }
        //            }
        //            catch { }
        //        }

        //        fraction = new Fraction();
        //        return false;
        //    }

        //    private static Fraction DoubleToFraction(double dValue)
        //    {
        //        var separator = System.Globalization.CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator[0];

        //        try
        //        {
        //            checked
        //            {
        //                Fraction frac;
        //                if (dValue % 1 == 0)    // if whole number
        //                {
        //                    frac = new Fraction((long)dValue);
        //                }
        //                else
        //                {
        //                    var dTemp = dValue;
        //                    long iMultiple = 1;
        //                    var strTemp = dValue.ToString();
        //                    while (strTemp.IndexOf("E") > 0)    // if in the form like 12E-9
        //                    {
        //                        dTemp *= 10;
        //                        iMultiple *= 10;
        //                        strTemp = dTemp.ToString();
        //                    }
        //                    var i = 0;
        //                    while (strTemp[i] != separator)
        //                        i++;
        //                    var iDigitsAfterDecimal = strTemp.Length - i - 1;
        //                    while (iDigitsAfterDecimal > 0)
        //                    {
        //                        dTemp *= 10;
        //                        iMultiple *= 10;
        //                        iDigitsAfterDecimal--;
        //                    }
        //                    frac = new Fraction((int)Math.Round(dTemp), iMultiple);
        //                }
        //                return frac;
        //            }
        //        }
        //        catch (OverflowException e)
        //        {
        //            throw new InvalidCastException("Conversion to Fraction in no possible due to overflow.", e);
        //        }
        //        catch (Exception e)
        //        {
        //            throw new InvalidCastException("Conversion to Fraction in not possible.", e);
        //        }
        //    }
        //    private static Fraction DecimalToFraction(decimal dValue)
        //    {
        //        var separator = System.Globalization.CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator[0];

        //        try
        //        {
        //            checked
        //            {
        //                Fraction frac;
        //                if (dValue % 1 == 0)    // if whole number
        //                {
        //                    frac = new Fraction((long)dValue);
        //                }
        //                else
        //                {
        //                    var dTemp = dValue;
        //                    long iMultiple = 1;
        //                    var strTemp = dValue.ToString();
        //                    while (strTemp.IndexOf("E") > 0)    // if in the form like 12E-9
        //                    {
        //                        dTemp *= 10;
        //                        iMultiple *= 10;
        //                        strTemp = dTemp.ToString();
        //                    }
        //                    var i = 0;
        //                    while (strTemp[i] != separator)
        //                        i++;
        //                    var iDigitsAfterDecimal = strTemp.Length - i - 1;
        //                    while (iDigitsAfterDecimal > 0)
        //                    {
        //                        dTemp *= 10;
        //                        iMultiple *= 10;
        //                        iDigitsAfterDecimal--;
        //                    }
        //                    frac = new Fraction((int)Math.Round(dTemp), iMultiple);
        //                }
        //                return frac;
        //            }
        //        }
        //        catch (OverflowException e)
        //        {
        //            throw new InvalidCastException("Conversion to Fraction in no possible due to overflow.", e);
        //        }
        //        catch (Exception e)
        //        {
        //            throw new InvalidCastException("Conversion to Fraction in not possible.", e);
        //        }
        //    }

        //    private static Fraction Inverse(Fraction frac1)
        //    {
        //        if (frac1.Numerator == 0)
        //            throw new InvalidOperationException("Operation not possible (Denominator cannot be assigned a ZERO Value)");

        //        var iNumerator = frac1.Denominator;
        //        var iDenominator = frac1.Numerator;
        //        return new Fraction(iNumerator, iDenominator);
        //    }
        //    private static Fraction Negate(Fraction frac1)
        //    {
        //        var iNumerator = -frac1.Numerator;
        //        var iDenominator = frac1.Denominator;
        //        return new Fraction(iNumerator, iDenominator);

        //    }
        //    private static Fraction Add(Fraction frac1, Fraction frac2)
        //    {
        //        try
        //        {
        //            checked
        //            {
        //                var iNumerator = frac1.Numerator * frac2.Denominator + frac2.Numerator * frac1.Denominator;
        //                var iDenominator = frac1.Denominator * frac2.Denominator;
        //                return new Fraction(iNumerator, iDenominator);
        //            }
        //        }
        //        catch (OverflowException e)
        //        {
        //            throw new OverflowException("Overflow occurred while performing arithemetic operation on Fraction.", e);
        //        }
        //        catch (Exception e)
        //        {
        //            throw new Exception("An error occurred while performing arithemetic operation on Fraction.", e);
        //        }
        //    }
        //    private static Fraction Multiply(Fraction frac1, Fraction frac2)
        //    {
        //        try
        //        {
        //            checked
        //            {
        //                var iNumerator = frac1.Numerator * frac2.Numerator;
        //                var iDenominator = frac1.Denominator * frac2.Denominator;
        //                return new Fraction(iNumerator, iDenominator);
        //            }
        //        }
        //        catch (OverflowException e)
        //        {
        //            throw new OverflowException("Overflow occurred while performing arithemetic operation on Fraction.", e);
        //        }
        //        catch (Exception e)
        //        {
        //            throw new Exception("An error occurred while performing arithemetic operation on Fraction.", e);
        //        }
        //    }

        //    private static long GCD(long iNo1, long iNo2)
        //    {
        //        if (iNo1 < 0) iNo1 = -iNo1;
        //        if (iNo2 < 0) iNo2 = -iNo2;

        //        do
        //        {
        //            if (iNo1 < iNo2)
        //            {
        //                var tmp = iNo1;
        //                iNo1 = iNo2;
        //                iNo2 = tmp;
        //            }
        //            iNo1 = iNo1 % iNo2;
        //        }
        //        while (iNo1 != 0);

        //        return iNo2;
        //    }

        //    public static Fraction operator -(Fraction frac1) { return (Negate(frac1)); }
        //    public static Fraction operator +(Fraction frac1, Fraction frac2) { return (Add(frac1, frac2)); }
        //    public static Fraction operator -(Fraction frac1, Fraction frac2) { return (Add(frac1, -frac2)); }
        //    public static Fraction operator *(Fraction frac1, Fraction frac2) { return (Multiply(frac1, frac2)); }
        //    public static Fraction operator /(Fraction frac1, Fraction frac2) { return (Multiply(frac1, Inverse(frac2))); }

        //    public static bool operator ==(Fraction frac1, Fraction frac2) { return frac1.Equals(frac2); }
        //    public static bool operator !=(Fraction frac1, Fraction frac2) { return (!frac1.Equals(frac2)); }
        //    public static bool operator <(Fraction frac1, Fraction frac2) { return frac1.Numerator * frac2.Denominator < frac2.Numerator * frac1.Denominator; }
        //    public static bool operator >(Fraction frac1, Fraction frac2) { return frac1.Numerator * frac2.Denominator > frac2.Numerator * frac1.Denominator; }
        //    public static bool operator <=(Fraction frac1, Fraction frac2) { return frac1.Numerator * frac2.Denominator <= frac2.Numerator * frac1.Denominator; }
        //    public static bool operator >=(Fraction frac1, Fraction frac2) { return frac1.Numerator * frac2.Denominator >= frac2.Numerator * frac1.Denominator; }

        //    public static implicit operator Fraction(long value) { return new Fraction(value); }
        //    public static implicit operator Fraction(double value) { return DoubleToFraction(value); }
        //    public static implicit operator Fraction(decimal value) { return DecimalToFraction(value); }

        //    public static explicit operator double(Fraction frac)
        //    {
        //        return ((double)frac.Numerator / frac.Denominator);
        //    }
        //    public static explicit operator decimal(Fraction frac)
        //    {
        //        return ((decimal)frac.Numerator / (decimal)frac.Denominator);
        //    }

        //    TypeCode IConvertible.GetTypeCode() => TypeCode.Object;

        //    decimal IConvertible.ToDecimal(IFormatProvider provider) => (decimal)this;
        //    double IConvertible.ToDouble(IFormatProvider provider) => (double)this;

        //    bool IConvertible.ToBoolean(IFormatProvider provider) => Convert.ToBoolean((double)this, provider);
        //    sbyte IConvertible.ToSByte(IFormatProvider provider) => (sbyte)(double)this;
        //    byte IConvertible.ToByte(IFormatProvider provider) => (byte)(double)this;
        //    short IConvertible.ToInt16(IFormatProvider provider) => (short)(double)this;
        //    ushort IConvertible.ToUInt16(IFormatProvider provider) => (ushort)(double)this;
        //    int IConvertible.ToInt32(IFormatProvider provider) => (int)(double)this;
        //    uint IConvertible.ToUInt32(IFormatProvider provider) => (uint)(double)this;
        //    long IConvertible.ToInt64(IFormatProvider provider) => (long)(double)this;
        //    ulong IConvertible.ToUInt64(IFormatProvider provider) => (ulong)(double)this;
        //    float IConvertible.ToSingle(IFormatProvider provider) => (float)(double)this;
        //    object IConvertible.ToType(Type conversionType, IFormatProvider provider) => Convert.ChangeType((double)this, conversionType);
        //    string IConvertible.ToString(IFormatProvider provider) => ToString();

        //    char IConvertible.ToChar(IFormatProvider provider) => throw new InvalidCastException();
        //    DateTime IConvertible.ToDateTime(IFormatProvider provider) => throw new InvalidCastException();
        //}

        //public class FractionConverter : TypeConverter
        //{
        //    public override bool CanConvertFrom(ITypeDescriptorContext context, Type type)
        //    {
        //        if (type == typeof(string) || IsNumericType(type))
        //            return true;

        //        return base.CanConvertFrom(context, type);
        //    }

        //    public override bool CanConvertTo(ITypeDescriptorContext context, Type type)
        //    {
        //        if (type == typeof(string) || IsNumericType(type))
        //            return true;

        //        return base.CanConvertTo(context, type);
        //    }

        //    public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo cultureInfo, object value)
        //    {
        //        if (value != null)
        //        {
        //            if (value is string s)
        //                return Fraction.Parse(s);

        //            if (value is double d)
        //                return (Fraction)d;

        //            if (value is float f)
        //                return (Fraction)f;

        //            if (IsNumericType(value.GetType()))
        //                return new Fraction(Convert.ToInt64(value));
        //        }

        //        return base.ConvertFrom(context, cultureInfo, value);
        //    }

        //    public override object ConvertTo(ITypeDescriptorContext context, CultureInfo cultureInfo, object value, Type destinationType)
        //    {
        //        if (destinationType == typeof(string) || IsNumericType(destinationType))
        //            return Convert.ChangeType(value, destinationType);

        //        return base.ConvertTo(context, cultureInfo, value, destinationType);
        //    }

        //    private static bool IsNumericType(Type type)
        //    {
        //        var code = Type.GetTypeCode(type);
        //        return code >= TypeCode.SByte && code <= TypeCode.Decimal;
        //    }
        //}
    }
}
