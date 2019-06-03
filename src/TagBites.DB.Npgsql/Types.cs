//using Npgsql;
//using Npgsql.BackendMessages;
//using Npgsql.PostgresTypes;
//using Npgsql.TypeHandling;
//using System.Data;

//namespace TagBites.DB.Npgsql
//{
//    public struct Fraction
//    {
//        public decimal A { get; set; }
//        public decimal B { get; set; }
//    }

//    class FractionHandlerFactory : NpgsqlTypeHandlerFactory<Fraction>
//    {
//        public override NpgsqlTypeHandler<Fraction> Create(PostgresType pgType, NpgsqlConnection conn)
//        {
//            return new FractionHandler(pgType);
//        }
//    }

//    class FractionHandler : NpgsqlSimpleTypeHandler<Fraction>
//    {
//        public override Fraction Read(NpgsqlReadBuffer buf, int len, FieldDescription fieldDescription = null)
//        {
//            var id = (ulong)buf.ReadInt64();
//            var labId = (ushort)(id >> (32 + 16));
//            var locId = id & 0x0000ffffffffffff;
//            return new Fraction { A = locId, B = labId };
//        }
//        public override int ValidateAndGetLength(Fraction value, NpgsqlParameter parameter)
//        {
//            if (!(value is Fraction))
//                throw new StrongTypingException();
//            return 8;
//        }
//        public override void Write(Fraction value, NpgsqlWriteBuffer buf, NpgsqlParameter parameter)
//        {
//            var v = (Fraction)value;
//            var graphid = (((ulong)(v.A)) << (32 + 16)) |
//                          (((ulong)(v.B)) & 0x0000ffffffffffff);
//            buf.WriteInt64((long)graphid);
//        }
//        public FractionHandler(PostgresType postgresType)
//            : base(postgresType)
//        { }
//    }
//}
