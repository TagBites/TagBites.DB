//using System;
//using System.Collections.Generic;
//using System.Data.Objects.DataClasses;
//using System.Linq;
//using System.Linq.Expressions;
//using System.Reflection;
//using System.Text;
//using TBS.DB.Entity;

//namespace TBS.Data.DB.Entity
//{
//    public static class EntityFunctions
//    {
//        /// <summary>
//        /// A LINQ to Entities operator that ensures the input string is treated as a unicode string. This method only applies to LINQ to Entities queries.
//        /// </summary>
//        /// 
//        /// <returns>
//        /// A unicode string.
//        /// </returns>
//        /// <param name="value">The input string.</param>
//        public static string AsUnicode(string value)
//        {
//            return value;
//        }

//        /// <summary>
//        /// A LINQ to Entities operator that treats the input string as a non-unicode string. This method only applies to LINQ to Entities queries.
//        /// </summary>
//        /// 
//        /// <returns>
//        /// A non-unicode string.
//        /// </returns>
//        /// <param name="value">The input string.</param>
//        public static string AsNonUnicode(string value)
//        {
//            return value;
//        }

//        /// <summary>
//        /// Invokes the canonical StDev function. For information about the canonical StDev function, see Canonical Functions.
//        /// </summary>
//        /// 
//        /// <returns>
//        /// The statistical standard deviation of the input collection.
//        /// </returns>
//        /// <param name="collection">A set of numeric values.</param>
//        [EdmFunction("Edm", "StDev")]
//        public static double? StandardDeviation(IEnumerable<Decimal> collection)
//        {
//            EntityQuery<Decimal> EntityQueryable = collection as EntityQuery<Decimal>;
//            if (EntityQueryable != null)
//                return EntityQueryable.Provider.Execute<double?>((Expression)Expression.Call((MethodInfo)MethodBase.GetCurrentMethod(), (Expression)Expression.Constant((object)collection)));
//            throw new NotSupportedException("Cannot direct call methods");
//        }

//        /// <summary>
//        /// Invokes the canonical StDev function. For information about the canonical StDev function, see Canonical Functions.
//        /// </summary>
//        /// 
//        /// <returns>
//        /// The statistical standard deviation of the input collection.
//        /// </returns>
//        /// <param name="collection">A set of numeric values.</param>
//        [EdmFunction("Edm", "StDev")]
//        public static double? StandardDeviation(IEnumerable<Decimal?> collection)
//        {
//            EntityQuery<Decimal?> EntityQueryable = collection as EntityQuery<Decimal?>;
//            if (EntityQueryable != null)
//                return EntityQueryable.Provider.Execute<double?>((Expression)Expression.Call((MethodInfo)MethodBase.GetCurrentMethod(), (Expression)Expression.Constant((object)collection)));
//            throw new NotSupportedException("Cannot direct call methods");
//        }

//        /// <summary>
//        /// Invokes the canonical StDev function. For information about the canonical StDev function, see Canonical Functions.
//        /// </summary>
//        /// 
//        /// <returns>
//        /// The statistical standard deviation of the input collection.
//        /// </returns>
//        /// <param name="collection">A set of numeric values.</param>
//        [EdmFunction("Edm", "StDev")]
//        public static double? StandardDeviation(IEnumerable<double> collection)
//        {
//            EntityQuery<double> EntityQueryable = collection as EntityQuery<double>;
//            if (EntityQueryable != null)
//                return EntityQueryable.Provider.Execute<double?>((Expression)Expression.Call((MethodInfo)MethodBase.GetCurrentMethod(), (Expression)Expression.Constant((object)collection)));
//            throw new NotSupportedException("Cannot direct call methods");
//        }

//        /// <summary>
//        /// Invokes the canonical StDev function. For information about the canonical StDev function, see Canonical Functions.
//        /// </summary>
//        /// 
//        /// <returns>
//        /// The statistical standard deviation of the input collection.
//        /// </returns>
//        /// <param name="collection">A set of numeric values.</param>
//        [EdmFunction("Edm", "StDev")]
//        public static double? StandardDeviation(IEnumerable<double?> collection)
//        {
//            EntityQuery<double?> EntityQueryable = collection as EntityQuery<double?>;
//            if (EntityQueryable != null)
//                return EntityQueryable.Provider.Execute<double?>((Expression)Expression.Call((MethodInfo)MethodBase.GetCurrentMethod(), (Expression)Expression.Constant((object)collection)));
//            throw new NotSupportedException("Cannot direct call methods");
//        }

//        /// <summary>
//        /// Invokes the canonical StDev function. For information about the canonical StDev function, see Canonical Functions.
//        /// </summary>
//        /// 
//        /// <returns>
//        /// The statistical standard deviation of the input collection.
//        /// </returns>
//        /// <param name="collection">A set of numeric values.</param>
//        [EdmFunction("Edm", "StDev")]
//        public static double? StandardDeviation(IEnumerable<int> collection)
//        {
//            EntityQuery<int> EntityQueryable = collection as EntityQuery<int>;
//            if (EntityQueryable != null)
//                return EntityQueryable.Provider.Execute<double?>((Expression)Expression.Call((MethodInfo)MethodBase.GetCurrentMethod(), (Expression)Expression.Constant((object)collection)));
//            throw new NotSupportedException("Cannot direct call methods");
//        }

//        /// <summary>
//        /// Invokes the canonical StDev function. For information about the canonical StDev function, see Canonical Functions.
//        /// </summary>
//        /// 
//        /// <returns>
//        /// The statistical standard deviation of the input collection.
//        /// </returns>
//        /// <param name="collection">A set of numeric values.</param>
//        [EdmFunction("Edm", "StDev")]
//        public static double? StandardDeviation(IEnumerable<int?> collection)
//        {
//            EntityQuery<int?> EntityQueryable = collection as EntityQuery<int?>;
//            if (EntityQueryable != null)
//                return EntityQueryable.Provider.Execute<double?>((Expression)Expression.Call((MethodInfo)MethodBase.GetCurrentMethod(), (Expression)Expression.Constant((object)collection)));
//            throw new NotSupportedException("Cannot direct call methods");
//        }

//        /// <summary>
//        /// Invokes the canonical StDev function. For information about the canonical StDev function, see Canonical Functions.
//        /// </summary>
//        /// 
//        /// <returns>
//        /// The statistical standard deviation of the input collection.
//        /// </returns>
//        /// <param name="collection">A set of numeric values.</param>
//        [EdmFunction("Edm", "StDev")]
//        public static double? StandardDeviation(IEnumerable<long> collection)
//        {
//            EntityQuery<long> EntityQueryable = collection as EntityQuery<long>;
//            if (EntityQueryable != null)
//                return EntityQueryable.Provider.Execute<double?>((Expression)Expression.Call((MethodInfo)MethodBase.GetCurrentMethod(), (Expression)Expression.Constant((object)collection)));
//            throw new NotSupportedException("Cannot direct call methods");
//        }

//        /// <summary>
//        /// Invokes the canonical StDev function. For information about the canonical StDev function, see Canonical Functions.
//        /// </summary>
//        /// 
//        /// <returns>
//        /// The statistical standard deviation of the input collection.
//        /// </returns>
//        /// <param name="collection">A set of numeric values.</param>
//        [EdmFunction("Edm", "StDev")]
//        public static double? StandardDeviation(IEnumerable<long?> collection)
//        {
//            EntityQuery<long?> EntityQueryable = collection as EntityQuery<long?>;
//            if (EntityQueryable != null)
//                return EntityQueryable.Provider.Execute<double?>((Expression)Expression.Call((MethodInfo)MethodBase.GetCurrentMethod(), (Expression)Expression.Constant((object)collection)));
//            throw new NotSupportedException("Cannot direct call methods");
//        }

//        /// <summary>
//        /// Invokes the canonical StDevP function. For information about the canonical StDevP function, see Canonical Functions.
//        /// </summary>
//        /// 
//        /// <returns>
//        /// The statistical standard deviation of the population in the input collection.
//        /// </returns>
//        /// <param name="collection">A set of numeric values.</param>
//        [EdmFunction("Edm", "StDevP")]
//        public static double? StandardDeviationP(IEnumerable<Decimal> collection)
//        {
//            EntityQuery<Decimal> EntityQueryable = collection as EntityQuery<Decimal>;
//            if (EntityQueryable != null)
//                return EntityQueryable.Provider.Execute<double?>((Expression)Expression.Call((MethodInfo)MethodBase.GetCurrentMethod(), (Expression)Expression.Constant((object)collection)));
//            throw new NotSupportedException("Cannot direct call methods");
//        }

//        /// <summary>
//        /// Invokes the canonical StDevP function. For information about the canonical StDevP function, see Canonical Functions.
//        /// </summary>
//        /// 
//        /// <returns>
//        /// The statistical standard deviation of the population in the input collection.
//        /// </returns>
//        /// <param name="collection">A set of numeric values.</param>
//        [EdmFunction("Edm", "StDevP")]
//        public static double? StandardDeviationP(IEnumerable<Decimal?> collection)
//        {
//            EntityQuery<Decimal?> EntityQueryable = collection as EntityQuery<Decimal?>;
//            if (EntityQueryable != null)
//                return EntityQueryable.Provider.Execute<double?>((Expression)Expression.Call((MethodInfo)MethodBase.GetCurrentMethod(), (Expression)Expression.Constant((object)collection)));
//            throw new NotSupportedException("Cannot direct call methods");
//        }

//        /// <summary>
//        /// Invokes the canonical StDevP function. For information about the canonical StDevP function, see Canonical Functions.
//        /// </summary>
//        /// 
//        /// <returns>
//        /// The statistical standard deviation of the population in the input collection.
//        /// </returns>
//        /// <param name="collection">A set of numeric values.</param>
//        [EdmFunction("Edm", "StDevP")]
//        public static double? StandardDeviationP(IEnumerable<double> collection)
//        {
//            EntityQuery<double> EntityQueryable = collection as EntityQuery<double>;
//            if (EntityQueryable != null)
//                return EntityQueryable.Provider.Execute<double?>((Expression)Expression.Call((MethodInfo)MethodBase.GetCurrentMethod(), (Expression)Expression.Constant((object)collection)));
//            throw new NotSupportedException("Cannot direct call methods");
//        }

//        /// <summary>
//        /// Invokes the canonical StDevP function. For information about the canonical StDevP function, see Canonical Functions.
//        /// </summary>
//        /// 
//        /// <returns>
//        /// The statistical standard deviation of the population in the input collection.
//        /// </returns>
//        /// <param name="collection">A set of numeric values.</param>
//        [EdmFunction("Edm", "StDevP")]
//        public static double? StandardDeviationP(IEnumerable<double?> collection)
//        {
//            EntityQuery<double?> EntityQueryable = collection as EntityQuery<double?>;
//            if (EntityQueryable != null)
//                return EntityQueryable.Provider.Execute<double?>((Expression)Expression.Call((MethodInfo)MethodBase.GetCurrentMethod(), (Expression)Expression.Constant((object)collection)));
//            throw new NotSupportedException("Cannot direct call methods");
//        }

//        /// <summary>
//        /// Invokes the canonical StDevP function. For information about the canonical StDevP function, see Canonical Functions.
//        /// </summary>
//        /// 
//        /// <returns>
//        /// The statistical standard deviation of the population in the input collection.
//        /// </returns>
//        /// <param name="collection">A set of numeric values.</param>
//        [EdmFunction("Edm", "StDevP")]
//        public static double? StandardDeviationP(IEnumerable<int> collection)
//        {
//            EntityQuery<int> EntityQueryable = collection as EntityQuery<int>;
//            if (EntityQueryable != null)
//                return EntityQueryable.Provider.Execute<double?>((Expression)Expression.Call((MethodInfo)MethodBase.GetCurrentMethod(), (Expression)Expression.Constant((object)collection)));
//            throw new NotSupportedException("Cannot direct call methods");
//        }

//        /// <summary>
//        /// Invokes the canonical StDevP function. For information about the canonical StDevP function, see Canonical Functions.
//        /// </summary>
//        /// 
//        /// <returns>
//        /// The statistical standard deviation of the population in the input collection.
//        /// </returns>
//        /// <param name="collection">A set of numeric values.</param>
//        [EdmFunction("Edm", "StDevP")]
//        public static double? StandardDeviationP(IEnumerable<int?> collection)
//        {
//            EntityQuery<int?> EntityQueryable = collection as EntityQuery<int?>;
//            if (EntityQueryable != null)
//                return EntityQueryable.Provider.Execute<double?>((Expression)Expression.Call((MethodInfo)MethodBase.GetCurrentMethod(), (Expression)Expression.Constant((object)collection)));
//            throw new NotSupportedException("Cannot direct call methods");
//        }

//        /// <summary>
//        /// Invokes the canonical StDevP function. For information about the canonical StDevP function, see Canonical Functions.
//        /// </summary>
//        /// 
//        /// <returns>
//        /// The statistical standard deviation of the population in the input collection.
//        /// </returns>
//        /// <param name="collection">A set of numeric values.</param>
//        [EdmFunction("Edm", "StDevP")]
//        public static double? StandardDeviationP(IEnumerable<long> collection)
//        {
//            EntityQuery<long> EntityQueryable = collection as EntityQuery<long>;
//            if (EntityQueryable != null)
//                return EntityQueryable.Provider.Execute<double?>((Expression)Expression.Call((MethodInfo)MethodBase.GetCurrentMethod(), (Expression)Expression.Constant((object)collection)));
//            throw new NotSupportedException("Cannot direct call methods");
//        }

//        /// <summary>
//        /// Invokes the canonical StDevP function. For information about the canonical StDevP function, see Canonical Functions.
//        /// </summary>
//        /// 
//        /// <returns>
//        /// The statistical standard deviation of the population in the input collection.
//        /// </returns>
//        /// <param name="collection">A set of numeric values.</param>
//        [EdmFunction("Edm", "StDevP")]
//        public static double? StandardDeviationP(IEnumerable<long?> collection)
//        {
//            EntityQuery<long?> EntityQueryable = collection as EntityQuery<long?>;
//            if (EntityQueryable != null)
//                return EntityQueryable.Provider.Execute<double?>((Expression)Expression.Call((MethodInfo)MethodBase.GetCurrentMethod(), (Expression)Expression.Constant((object)collection)));
//            throw new NotSupportedException("Cannot direct call methods");
//        }

//        /// <summary>
//        /// Invokes the canonical Var function. For information about the canonical Var function, see Canonical Functions.
//        /// </summary>
//        /// 
//        /// <returns>
//        /// The statistical variance of all values in the specified collection.
//        /// </returns>
//        /// <param name="collection">The set of values for which the variance will be calculated.</param>
//        [EdmFunction("Edm", "Var")]
//        public static double? Var(IEnumerable<Decimal> collection)
//        {
//            EntityQuery<Decimal> EntityQueryable = collection as EntityQuery<Decimal>;
//            if (EntityQueryable != null)
//                return EntityQueryable.Provider.Execute<double?>((Expression)Expression.Call((MethodInfo)MethodBase.GetCurrentMethod(), (Expression)Expression.Constant((object)collection)));
//            throw new NotSupportedException("Cannot direct call methods");
//        }

//        /// <summary>
//        /// Invokes the canonical Var function. For information about the canonical Var function, see Canonical Functions.
//        /// </summary>
//        /// 
//        /// <returns>
//        /// The statistical variance of all values in the specified collection.
//        /// </returns>
//        /// <param name="collection">The set of values for which the variance will be calculated.</param>
//        [EdmFunction("Edm", "Var")]
//        public static double? Var(IEnumerable<Decimal?> collection)
//        {
//            EntityQuery<Decimal?> EntityQueryable = collection as EntityQuery<Decimal?>;
//            if (EntityQueryable != null)
//                return EntityQueryable.Provider.Execute<double?>((Expression)Expression.Call((MethodInfo)MethodBase.GetCurrentMethod(), (Expression)Expression.Constant((object)collection)));
//            throw new NotSupportedException("Cannot direct call methods");
//        }

//        /// <summary>
//        /// Invokes the canonical Var function. For information about the canonical Var function, see Canonical Functions.
//        /// </summary>
//        /// 
//        /// <returns>
//        /// The statistical variance of all values in the specified collection.
//        /// </returns>
//        /// <param name="collection">The set of values for which the variance will be calculated.</param>
//        [EdmFunction("Edm", "Var")]
//        public static double? Var(IEnumerable<double> collection)
//        {
//            EntityQuery<double> EntityQueryable = collection as EntityQuery<double>;
//            if (EntityQueryable != null)
//                return EntityQueryable.Provider.Execute<double?>((Expression)Expression.Call((MethodInfo)MethodBase.GetCurrentMethod(), (Expression)Expression.Constant((object)collection)));
//            throw new NotSupportedException("Cannot direct call methods");
//        }

//        /// <summary>
//        /// Invokes the canonical Var function. For information about the canonical Var function, see Canonical Functions.
//        /// </summary>
//        /// 
//        /// <returns>
//        /// The statistical variance of all values in the specified collection.
//        /// </returns>
//        /// <param name="collection">The set of values for which the variance will be calculated.</param>
//        [EdmFunction("Edm", "Var")]
//        public static double? Var(IEnumerable<double?> collection)
//        {
//            EntityQuery<double?> EntityQueryable = collection as EntityQuery<double?>;
//            if (EntityQueryable != null)
//                return EntityQueryable.Provider.Execute<double?>((Expression)Expression.Call((MethodInfo)MethodBase.GetCurrentMethod(), (Expression)Expression.Constant((object)collection)));
//            throw new NotSupportedException("Cannot direct call methods");
//        }

//        /// <summary>
//        /// Invokes the canonical Var function. For information about the canonical Var function, see Canonical Functions.
//        /// </summary>
//        /// 
//        /// <returns>
//        /// The statistical variance of all values in the specified collection.
//        /// </returns>
//        /// <param name="collection">The set of values for which the variance will be calculated.</param>
//        [EdmFunction("Edm", "Var")]
//        public static double? Var(IEnumerable<int> collection)
//        {
//            EntityQuery<int> EntityQueryable = collection as EntityQuery<int>;
//            if (EntityQueryable != null)
//                return EntityQueryable.Provider.Execute<double?>((Expression)Expression.Call((MethodInfo)MethodBase.GetCurrentMethod(), (Expression)Expression.Constant((object)collection)));
//            throw new NotSupportedException("Cannot direct call methods");
//        }

//        /// <summary>
//        /// Invokes the canonical Var function. For information about the canonical Var function, see Canonical Functions.
//        /// </summary>
//        /// 
//        /// <returns>
//        /// The statistical variance of all values in the specified collection.
//        /// </returns>
//        /// <param name="collection">The set of values for which the variance will be calculated.</param>
//        [EdmFunction("Edm", "Var")]
//        public static double? Var(IEnumerable<int?> collection)
//        {
//            EntityQuery<int?> EntityQueryable = collection as EntityQuery<int?>;
//            if (EntityQueryable != null)
//                return EntityQueryable.Provider.Execute<double?>((Expression)Expression.Call((MethodInfo)MethodBase.GetCurrentMethod(), (Expression)Expression.Constant((object)collection)));
//            throw new NotSupportedException("Cannot direct call methods");
//        }

//        /// <summary>
//        /// Invokes the canonical Var function. For information about the canonical Var function, see Canonical Functions.
//        /// </summary>
//        /// 
//        /// <returns>
//        /// The statistical variance of all values in the specified collection.
//        /// </returns>
//        /// <param name="collection">The set of values for which the variance will be calculated.</param>
//        [EdmFunction("Edm", "Var")]
//        public static double? Var(IEnumerable<long> collection)
//        {
//            EntityQuery<long> EntityQueryable = collection as EntityQuery<long>;
//            if (EntityQueryable != null)
//                return EntityQueryable.Provider.Execute<double?>((Expression)Expression.Call((MethodInfo)MethodBase.GetCurrentMethod(), (Expression)Expression.Constant((object)collection)));
//            throw new NotSupportedException("Cannot direct call methods");
//        }

//        /// <summary>
//        /// Invokes the canonical Var function. For information about the canonical Var function, see Canonical Functions.
//        /// </summary>
//        /// 
//        /// <returns>
//        /// The statistical variance of all values in the specified collection.
//        /// </returns>
//        /// <param name="collection">The set of values for which the variance will be calculated.</param>
//        [EdmFunction("Edm", "Var")]
//        public static double? Var(IEnumerable<long?> collection)
//        {
//            EntityQuery<long?> EntityQueryable = collection as EntityQuery<long?>;
//            if (EntityQueryable != null)
//                return EntityQueryable.Provider.Execute<double?>((Expression)Expression.Call((MethodInfo)MethodBase.GetCurrentMethod(), (Expression)Expression.Constant((object)collection)));
//            throw new NotSupportedException("Cannot direct call methods");
//        }

//        /// <summary>
//        /// Invokes the canonical VarP function. For information about the canonical VarP function, see Canonical Functions.
//        /// </summary>
//        /// 
//        /// <returns>
//        /// The statistical variance of the population in the specified collection.
//        /// </returns>
//        /// <param name="collection">The set of values for which the variance will be calculated.</param>
//        [EdmFunction("Edm", "VarP")]
//        public static double? VarP(IEnumerable<Decimal> collection)
//        {
//            EntityQuery<Decimal> EntityQueryable = collection as EntityQuery<Decimal>;
//            if (EntityQueryable != null)
//                return EntityQueryable.Provider.Execute<double?>((Expression)Expression.Call((MethodInfo)MethodBase.GetCurrentMethod(), (Expression)Expression.Constant((object)collection)));
//            throw new NotSupportedException("Cannot direct call methods");
//        }

//        /// <summary>
//        /// Invokes the canonical VarP function. For information about the canonical VarP function, see Canonical Functions.
//        /// </summary>
//        /// 
//        /// <returns>
//        /// The statistical variance of the population in the specified collection.
//        /// </returns>
//        /// <param name="collection">The set of values for which the variance will be calculated.</param>
//        [EdmFunction("Edm", "VarP")]
//        public static double? VarP(IEnumerable<Decimal?> collection)
//        {
//            EntityQuery<Decimal?> EntityQueryable = collection as EntityQuery<Decimal?>;
//            if (EntityQueryable != null)
//                return EntityQueryable.Provider.Execute<double?>((Expression)Expression.Call((MethodInfo)MethodBase.GetCurrentMethod(), (Expression)Expression.Constant((object)collection)));
//            throw new NotSupportedException("Cannot direct call methods");
//        }

//        /// <summary>
//        /// Invokes the canonical VarP function. For information about the canonical VarP function, see Canonical Functions.
//        /// </summary>
//        /// 
//        /// <returns>
//        /// The statistical variance of the population in the specified collection.
//        /// </returns>
//        /// <param name="collection">The set of values for which the variance will be calculated.</param>
//        [EdmFunction("Edm", "VarP")]
//        public static double? VarP(IEnumerable<double> collection)
//        {
//            EntityQuery<double> EntityQueryable = collection as EntityQuery<double>;
//            if (EntityQueryable != null)
//                return EntityQueryable.Provider.Execute<double?>((Expression)Expression.Call((MethodInfo)MethodBase.GetCurrentMethod(), (Expression)Expression.Constant((object)collection)));
//            throw new NotSupportedException("Cannot direct call methods");
//        }

//        /// <summary>
//        /// Invokes the canonical VarP function. For information about the canonical VarP function, see Canonical Functions.
//        /// </summary>
//        /// 
//        /// <returns>
//        /// The statistical variance of the population in the specified collection.
//        /// </returns>
//        /// <param name="collection">The set of values for which the variance will be calculated.</param>
//        [EdmFunction("Edm", "VarP")]
//        public static double? VarP(IEnumerable<double?> collection)
//        {
//            EntityQuery<double?> EntityQueryable = collection as EntityQuery<double?>;
//            if (EntityQueryable != null)
//                return EntityQueryable.Provider.Execute<double?>((Expression)Expression.Call((MethodInfo)MethodBase.GetCurrentMethod(), (Expression)Expression.Constant((object)collection)));
//            throw new NotSupportedException("Cannot direct call methods");
//        }

//        /// <summary>
//        /// Invokes the canonical VarP function. For information about the canonical VarP function, see Canonical Functions.
//        /// </summary>
//        /// 
//        /// <returns>
//        /// The statistical variance of the population in the specified collection.
//        /// </returns>
//        /// <param name="collection">The set of values for which the variance will be calculated.</param>
//        [EdmFunction("Edm", "VarP")]
//        public static double? VarP(IEnumerable<int> collection)
//        {
//            EntityQuery<int> EntityQueryable = collection as EntityQuery<int>;
//            if (EntityQueryable != null)
//                return EntityQueryable.Provider.Execute<double?>((Expression)Expression.Call((MethodInfo)MethodBase.GetCurrentMethod(), (Expression)Expression.Constant((object)collection)));
//            throw new NotSupportedException("Cannot direct call methods");
//        }

//        /// <summary>
//        /// Invokes the canonical VarP function. For information about the canonical VarP function, see Canonical Functions.
//        /// </summary>
//        /// 
//        /// <returns>
//        /// The statistical variance of the population in the specified collection.
//        /// </returns>
//        /// <param name="collection">The set of values for which the variance will be calculated.</param>
//        [EdmFunction("Edm", "VarP")]
//        public static double? VarP(IEnumerable<int?> collection)
//        {
//            EntityQuery<int?> EntityQueryable = collection as EntityQuery<int?>;
//            if (EntityQueryable != null)
//                return EntityQueryable.Provider.Execute<double?>((Expression)Expression.Call((MethodInfo)MethodBase.GetCurrentMethod(), (Expression)Expression.Constant((object)collection)));
//            throw new NotSupportedException("Cannot direct call methods");
//        }

//        /// <summary>
//        /// Invokes the canonical VarP function. For information about the canonical VarP function, see Canonical Functions.
//        /// </summary>
//        /// 
//        /// <returns>
//        /// The statistical variance of the population in the specifed collection.
//        /// </returns>
//        /// <param name="collection">The set of values for which the variance will be calculated.</param>
//        [EdmFunction("Edm", "VarP")]
//        public static double? VarP(IEnumerable<long> collection)
//        {
//            EntityQuery<long> EntityQueryable = collection as EntityQuery<long>;
//            if (EntityQueryable != null)
//                return EntityQueryable.Provider.Execute<double?>((Expression)Expression.Call((MethodInfo)MethodBase.GetCurrentMethod(), (Expression)Expression.Constant((object)collection)));
//            throw new NotSupportedException("Cannot direct call methods");
//        }

//        /// <summary>
//        /// Invokes the canonical VarP function. For information about the canonical VarP function, see Canonical Functions.
//        /// </summary>
//        /// 
//        /// <returns>
//        /// The statistical variance of the population in the specified collection.
//        /// </returns>
//        /// <param name="collection">The set of values for which the variance will be calculated.</param>
//        [EdmFunction("Edm", "VarP")]
//        public static double? VarP(IEnumerable<long?> collection)
//        {
//            EntityQuery<long?> EntityQueryable = collection as EntityQuery<long?>;
//            if (EntityQueryable != null)
//                return EntityQueryable.Provider.Execute<double?>((Expression)Expression.Call((MethodInfo)MethodBase.GetCurrentMethod(), (Expression)Expression.Constant((object)collection)));
//            throw new NotSupportedException("Cannot direct call methods");
//        }

//        /// <summary>
//        /// Invokes the canonical Left function. For information about the canonical Left function, see String Canonical Functions.
//        /// </summary>
//        /// 
//        /// <returns>
//        /// The leftmost <paramref name="length"/> number of characters of <paramref name="stringArgument"/>.
//        /// </returns>
//        /// <param name="stringArgument">A valid string expression.</param><param name="length">The number of characters to return.</param>
//        [EdmFunction("Edm", "Left")]
//        public static string Left(string stringArgument, long? length)
//        {
//            throw new NotSupportedException("Cannot direct call methods");
//        }

//        /// <summary>
//        /// Invokes the canonical Right function. For information about the canonical Right function, see String Canonical Functions.
//        /// </summary>
//        /// 
//        /// <returns>
//        /// The rightmost <paramref name="length"/> number of characters of <paramref name="stringArgument"/>.
//        /// </returns>
//        /// <param name="stringArgument">A valid string expression.</param><param name="length">The number of characters to return.</param>
//        [EdmFunction("Edm", "Right")]
//        public static string Right(string stringArgument, long? length)
//        {
//            throw new NotSupportedException("Cannot direct call methods");
//        }

//        /// <summary>
//        /// Invokes the canonical Reverse function. For information about the canonical Reverse function, see String Canonical Functions.
//        /// </summary>
//        /// 
//        /// <returns>
//        /// The input string with the order of the characters reversed.
//        /// </returns>
//        /// <param name="stringArgument">A valid string.</param>
//        [EdmFunction("Edm", "Reverse")]
//        public static string Reverse(string stringArgument)
//        {
//            throw new NotSupportedException("Cannot direct call methods");
//        }

//        /// <summary>
//        /// Invokes the canonical GetTotalOffsetMinutes function. For information about the canonical GetTotalOffsetMinutes function, see Date and Time Canonical Functions.
//        /// </summary>
//        /// 
//        /// <returns>
//        /// The number of minutes that the <paramref name="dateTimeOffsetArgument"/> is offset from GMT. This is generally between +780 and -780 (+ or - 13 hrs).
//        /// </returns>
//        /// <param name="dateTimeOffsetArgument">A valid date time offset.</param>
//        [EdmFunction("Edm", "GetTotalOffsetMinutes")]
//        public static int? GetTotalOffsetMinutes(DateTimeOffset? dateTimeOffsetArgument)
//        {
//            throw new NotSupportedException("Cannot direct call methods");
//        }

//        /// <summary>
//        /// Invokes the canonical TruncateTime function. For information about the canonical TruncateTime function, see Date and Time Canonical Functions.
//        /// </summary>
//        /// 
//        /// <returns>
//        /// The input date with the time portion cleared.
//        /// </returns>
//        /// <param name="dateValue">The date time offset to truncate.</param>
//        [EdmFunction("Edm", "TruncateTime")]
//        public static DateTimeOffset? TruncateTime(DateTimeOffset? dateValue)
//        {
//            throw new NotSupportedException("Cannot direct call methods");
//        }

//        /// <summary>
//        /// Invokes the canonical TruncateTime function. For information about the canonical TruncateTime function, see Date and Time Canonical Functions.
//        /// </summary>
//        /// 
//        /// <returns>
//        /// The input date with the time portion cleared.
//        /// </returns>
//        /// <param name="dateValue">The date to truncate.</param>
//        [EdmFunction("Edm", "TruncateTime")]
//        public static DateTime? TruncateTime(DateTime? dateValue)
//        {
//            throw new NotSupportedException("Cannot direct call methods");
//        }

//        /// <summary>
//        /// Invokes the canonical CreateDateTime function. For information about the canonical CreateDateTime function, see Date and Time Canonical Functions.
//        /// </summary>
//        /// 
//        /// <returns>
//        /// The new date.
//        /// </returns>
//        /// <param name="year">The year part of the new date.</param><param name="month">The month part of the new date.</param><param name="day">The day part of the new date.</param><param name="hour">The hour part of the new date.</param><param name="minute">The minutes part of the new date.</param><param name="second">The seconds part of the new date. Note that you can specify fractions of a second with this parameter.</param>
//        [EdmFunction("Edm", "CreateDateTime")]
//        public static DateTime? CreateDateTime(int? year, int? month, int? day, int? hour, int? minute, double? second)
//        {
//            throw new NotSupportedException("Cannot direct call methods");
//        }

//        /// <summary>
//        /// Invokes the canonical CreateDateTimeOffset function. For information about the canonical CreateDateTimeOffset function, see Date and Time Canonical Functions.
//        /// </summary>
//        /// 
//        /// <returns>
//        /// The new date.
//        /// </returns>
//        /// <param name="year">The year part of the new date.</param><param name="month">The month part of the new date.</param><param name="day">The day part of the new date.</param><param name="hour">The hour part of the new date.</param><param name="minute">The minute part of the new date.</param><param name="second">The seconds part of the new date. Note that you can specify fractions of a second with this parameter.</param><param name="timeZoneOffset">The time zone offset part of the new date.</param>
//        [EdmFunction("Edm", "CreateDateTimeOffset")]
//        public static DateTimeOffset? CreateDateTimeOffset(int? year, int? month, int? day, int? hour, int? minute, double? second, int? timeZoneOffset)
//        {
//            throw new NotSupportedException("Cannot direct call methods");
//        }

//        /// <summary>
//        /// Invokes the canonical CreateTime function. For information about the canonical CreateTime function, see Date and Time Canonical Functions.
//        /// </summary>
//        /// 
//        /// <returns>
//        /// The new time span.
//        /// </returns>
//        /// <param name="hour">The hours part of the new time span.</param><param name="minute">The minutes part of the new time span.</param><param name="second">The seconds part of the new time span. Note that you can specify fractions of a second with this parameter.</param>
//        [EdmFunction("Edm", "CreateTime")]
//        public static TimeSpan? CreateTime(int? hour, int? minute, double? second)
//        {
//            throw new NotSupportedException("Cannot direct call methods");
//        }

//        /// <summary>
//        /// Invokes the canonical AddYears function. For information about the canonical AddYears function, see Date and Time Canonical Functions.
//        /// </summary>
//        /// 
//        /// <returns>
//        /// The <paramref name="dateValue"/> incremented by <paramref name="addValue"/>.
//        /// </returns>
//        /// <param name="dateValue">A valid date time offset.</param><param name="addValue">The number of years to add to <paramref name="dateValue"/>.</param>
//        [EdmFunction("Edm", "AddYears")]
//        public static DateTimeOffset? AddYears(DateTimeOffset? dateValue, int? addValue)
//        {
//            throw new NotSupportedException("Cannot direct call methods");
//        }

//        /// <summary>
//        /// Invokes the canonical AddYears function. For information about the canonical AddYears function, see Date and Time Canonical Functions.
//        /// </summary>
//        /// 
//        /// <returns>
//        /// The <paramref name="dateValue"/> incremented by <paramref name="addValue"/>.
//        /// </returns>
//        /// <param name="dateValue">A valid date.</param><param name="addValue">The number of years to add to <paramref name="dateValue"/>.</param>
//        [EdmFunction("Edm", "AddYears")]
//        public static DateTime? AddYears(DateTime? dateValue, int? addValue)
//        {
//            throw new NotSupportedException("Cannot direct call methods");
//        }

//        /// <summary>
//        /// Invokes the canonical AddMonths function. For information about the canonical AddMonths function, see Date and Time Canonical Functions.
//        /// </summary>
//        /// 
//        /// <returns>
//        /// The <paramref name="dateValue"/> incremented by <paramref name="addValue"/>.
//        /// </returns>
//        /// <param name="dateValue">A valid date time offset.</param><param name="addValue">The number of months to add to <paramref name="dateValue"/>.</param>
//        [EdmFunction("Edm", "AddMonths")]
//        public static DateTimeOffset? AddMonths(DateTimeOffset? dateValue, int? addValue)
//        {
//            throw new NotSupportedException("Cannot direct call methods");
//        }

//        /// <summary>
//        /// Invokes the canonical AddMonths function. For information about the canonical AddMonths function, see Date and Time Canonical Functions.
//        /// </summary>
//        /// 
//        /// <returns>
//        /// The <paramref name="dateValue"/> incremented by <paramref name="addValue"/>.
//        /// </returns>
//        /// <param name="dateValue">A valid date.</param><param name="addValue">The number of months to add to <paramref name="dateValue"/>.</param>
//        [EdmFunction("Edm", "AddMonths")]
//        public static DateTime? AddMonths(DateTime? dateValue, int? addValue)
//        {
//            throw new NotSupportedException("Cannot direct call methods");
//        }

//        /// <summary>
//        /// Invokes the canonical AddDays function. For information about the canonical AddDays function, see Date and Time Canonical Functions.
//        /// </summary>
//        /// 
//        /// <returns>
//        /// The <paramref name="dateValue"/> incremented by <paramref name="addValue"/>.
//        /// </returns>
//        /// <param name="dateValue">A valid date time offset.</param><param name="addValue">The number of days to add to <paramref name="dateValue"/>.</param>
//        [EdmFunction("Edm", "AddDays")]
//        public static DateTimeOffset? AddDays(DateTimeOffset? dateValue, int? addValue)
//        {
//            throw new NotSupportedException("Cannot direct call methods");
//        }

//        /// <summary>
//        /// Invokes the canonical AddDays function. For information about the canonical AddDays function, see Date and Time Canonical Functions.
//        /// </summary>
//        /// 
//        /// <returns>
//        /// The <paramref name="dateValue"/> incremented by <paramref name="addValue"/>.
//        /// </returns>
//        /// <param name="dateValue">A valid date.</param><param name="addValue">The number of days to add to <paramref name="dateValue"/>.</param>
//        [EdmFunction("Edm", "AddDays")]
//        public static DateTime? AddDays(DateTime? dateValue, int? addValue)
//        {
//            throw new NotSupportedException("Cannot direct call methods");
//        }

//        /// <summary>
//        /// Invokes the canonical AddHours function. For information about the canonical AddHours function, see Date and Time Canonical Functions.
//        /// </summary>
//        /// 
//        /// <returns>
//        /// The <paramref name="timeValue"/> incremented by <paramref name="addValue"/>.
//        /// </returns>
//        /// <param name="timeValue">A valid date time offset.</param><param name="addValue">The number of hours to add to <paramref name="timeValue"/>.</param>
//        [EdmFunction("Edm", "AddHours")]
//        public static DateTimeOffset? AddHours(DateTimeOffset? timeValue, int? addValue)
//        {
//            throw new NotSupportedException("Cannot direct call methods");
//        }

//        /// <summary>
//        /// Invokes the canonical AddHours function. For information about the canonical AddHours function, see Date and Time Canonical Functions.
//        /// </summary>
//        /// 
//        /// <returns>
//        /// The <paramref name="timeValue"/> incremented by <paramref name="addValue"/>.
//        /// </returns>
//        /// <param name="timeValue">A valid date.</param><param name="addValue">The number of hours to add to <paramref name="timeValue"/>.</param>
//        [EdmFunction("Edm", "AddHours")]
//        public static DateTime? AddHours(DateTime? timeValue, int? addValue)
//        {
//            throw new NotSupportedException("Cannot direct call methods");
//        }

//        /// <summary>
//        /// Invokes the canonical AddHours function. For information about the canonical AddHours function, see Date and Time Canonical Functions.
//        /// </summary>
//        /// 
//        /// <returns>
//        /// The <paramref name="timeValue"/> incremented by <paramref name="addValue"/>.
//        /// </returns>
//        /// <param name="timeValue">A valid time span.</param><param name="addValue">The number of hours to add to <paramref name="timeValue"/>.</param>
//        [EdmFunction("Edm", "AddHours")]
//        public static TimeSpan? AddHours(TimeSpan? timeValue, int? addValue)
//        {
//            throw new NotSupportedException("Cannot direct call methods");
//        }

//        /// <summary>
//        /// Invokes the canonical AddMinutes function. For information about the canonical AddMinutes function, see Date and Time Canonical Functions.
//        /// </summary>
//        /// 
//        /// <returns>
//        /// The <paramref name="timeValue"/> incremented by <paramref name="addValue"/>.
//        /// </returns>
//        /// <param name="timeValue">A valid date time offset.</param><param name="addValue">The number of minutes to add to <paramref name="timeValue"/>.</param>
//        [EdmFunction("Edm", "AddMinutes")]
//        public static DateTimeOffset? AddMinutes(DateTimeOffset? timeValue, int? addValue)
//        {
//            throw new NotSupportedException("Cannot direct call methods");
//        }

//        /// <summary>
//        /// Invokes the canonical AddMinutes function. For information about the canonical AddMinutes function, see Date and Time Canonical Functions.
//        /// </summary>
//        /// 
//        /// <returns>
//        /// The <paramref name="timeValue"/> incremented by <paramref name="addValue"/>.
//        /// </returns>
//        /// <param name="timeValue">A valid date.</param><param name="addValue">The number of minutes to add to <paramref name="timeValue"/>.</param>
//        [EdmFunction("Edm", "AddMinutes")]
//        public static DateTime? AddMinutes(DateTime? timeValue, int? addValue)
//        {
//            throw new NotSupportedException("Cannot direct call methods");
//        }

//        /// <summary>
//        /// Invokes the canonical AddMinutes function. For information about the canonical AddMinutes function, see Date and Time Canonical Functions.
//        /// </summary>
//        /// 
//        /// <returns>
//        /// The <paramref name="timeValue"/> incremented by <paramref name="addValue"/>.
//        /// </returns>
//        /// <param name="timeValue">A valid time span.</param><param name="addValue">The number of minutes to add to <paramref name="timeValue"/>.</param>
//        [EdmFunction("Edm", "AddMinutes")]
//        public static TimeSpan? AddMinutes(TimeSpan? timeValue, int? addValue)
//        {
//            throw new NotSupportedException("Cannot direct call methods");
//        }

//        /// <summary>
//        /// Invokes the canonical AddSeconds function. For information about the canonical AddSeconds function, see Date and Time Canonical Functions.
//        /// </summary>
//        /// 
//        /// <returns>
//        /// The <paramref name="timeValue"/> incremented by <paramref name="addValue"/>.
//        /// </returns>
//        /// <param name="timeValue">A valid date time offset.</param><param name="addValue">The number of seconds to add to <paramref name="timeValue"/>.</param>
//        [EdmFunction("Edm", "AddSeconds")]
//        public static DateTimeOffset? AddSeconds(DateTimeOffset? timeValue, int? addValue)
//        {
//            throw new NotSupportedException("Cannot direct call methods");
//        }

//        /// <summary>
//        /// Invokes the canonical AddSeconds function. For information about the canonical AddSeconds function, see Date and Time Canonical Functions.
//        /// </summary>
//        /// 
//        /// <returns>
//        /// The <paramref name="timeValue"/> incremented by <paramref name="addValue"/>.
//        /// </returns>
//        /// <param name="timeValue">A valid date.</param><param name="addValue">The number of seconds to add to <paramref name="timeValue"/>.</param>
//        [EdmFunction("Edm", "AddSeconds")]
//        public static DateTime? AddSeconds(DateTime? timeValue, int? addValue)
//        {
//            throw new NotSupportedException("Cannot direct call methods");
//        }

//        /// <summary>
//        /// Invokes the canonical AddSeconds function. For information about the canonical AddSeconds function, see Date and Time Canonical Functions.
//        /// </summary>
//        /// 
//        /// <returns>
//        /// The <paramref name="timeValue"/> incremented by <paramref name="addValue"/>.
//        /// </returns>
//        /// <param name="timeValue">A valid time span.</param><param name="addValue">The number of seconds to add to <paramref name="timeValue"/>.</param>
//        [EdmFunction("Edm", "AddSeconds")]
//        public static TimeSpan? AddSeconds(TimeSpan? timeValue, int? addValue)
//        {
//            throw new NotSupportedException("Cannot direct call methods");
//        }

//        /// <summary>
//        /// Invokes the canonical AddMilliseconds function. For information about the canonical AddMilliseconds function, see Date and Time Canonical Functions.
//        /// </summary>
//        /// 
//        /// <returns>
//        /// The <paramref name="timeValue"/> incremented by <paramref name="addValue"/>.
//        /// </returns>
//        /// <param name="timeValue">A valid date time offset.</param><param name="addValue">The number of milliseconds to add to <paramref name="timeValue"/>.</param>
//        [EdmFunction("Edm", "AddMilliseconds")]
//        public static DateTimeOffset? AddMilliseconds(DateTimeOffset? timeValue, int? addValue)
//        {
//            throw new NotSupportedException("Cannot direct call methods");
//        }

//        /// <summary>
//        /// Invokes the canonical AddMilliseconds function. For information about the canonical AddMilliseconds function, see Date and Time Canonical Functions.
//        /// </summary>
//        /// 
//        /// <returns>
//        /// The <paramref name="timeValue"/> incremented by <paramref name="addValue"/>.
//        /// </returns>
//        /// <param name="timeValue">A valid date.</param><param name="addValue">The number of milliseconds to add to <paramref name="timeValue"/>.</param>
//        [EdmFunction("Edm", "AddMilliseconds")]
//        public static DateTime? AddMilliseconds(DateTime? timeValue, int? addValue)
//        {
//            throw new NotSupportedException("Cannot direct call methods");
//        }

//        /// <summary>
//        /// Invokes the canonical AddMilliseconds function. For information about the canonical AddMilliseconds function, see Date and Time Canonical Functions.
//        /// </summary>
//        /// 
//        /// <returns>
//        /// The <paramref name="timeValue"/> incremented by <paramref name="addValue"/>.
//        /// </returns>
//        /// <param name="timeValue">A valid time span.</param><param name="addValue">The number of milliseconds to add to <paramref name="timeValue"/>.</param>
//        [EdmFunction("Edm", "AddMilliseconds")]
//        public static TimeSpan? AddMilliseconds(TimeSpan? timeValue, int? addValue)
//        {
//            throw new NotSupportedException("Cannot direct call methods");
//        }

//        /// <summary>
//        /// Invokes the canonical AddMicroseconds function. For information about the canonical AddMicroseconds function, see Date and Time Canonical Functions.
//        /// </summary>
//        /// 
//        /// <returns>
//        /// The <paramref name="timeValue"/> incremented by <paramref name="addValue"/>.
//        /// </returns>
//        /// <param name="timeValue">A valid date time offset.</param><param name="addValue">The number of microseconds to add to <paramref name="timeValue"/>.</param>
//        [EdmFunction("Edm", "AddMicroseconds")]
//        public static DateTimeOffset? AddMicroseconds(DateTimeOffset? timeValue, int? addValue)
//        {
//            throw new NotSupportedException("Cannot direct call methods");
//        }

//        /// <summary>
//        /// Invokes the canonical AddMicroseconds function. For information about the canonical AddMicroseconds function, see Date and Time Canonical Functions.
//        /// </summary>
//        /// 
//        /// <returns>
//        /// The <paramref name="timeValue"/> incremented by <paramref name="addValue"/>.
//        /// </returns>
//        /// <param name="timeValue">A valid date.</param><param name="addValue">The number of microseconds to add to <paramref name="timeValue"/>.</param>
//        [EdmFunction("Edm", "AddMicroseconds")]
//        public static DateTime? AddMicroseconds(DateTime? timeValue, int? addValue)
//        {
//            throw new NotSupportedException("Cannot direct call methods");
//        }

//        /// <summary>
//        /// Invokes the canonical AddMicroseconds function. For information about the canonical AddMicroseconds function, see Date and Time Canonical Functions.
//        /// </summary>
//        /// 
//        /// <returns>
//        /// The <paramref name="timeValue"/> incremented by <paramref name="addValue"/>.
//        /// </returns>
//        /// <param name="timeValue">A valid time span.</param><param name="addValue">The number of microseconds to add to <paramref name="timeValue"/>.</param>
//        [EdmFunction("Edm", "AddMicroseconds")]
//        public static TimeSpan? AddMicroseconds(TimeSpan? timeValue, int? addValue)
//        {
//            throw new NotSupportedException("Cannot direct call methods");
//        }

//        /// <summary>
//        /// Invokes the canonical AddNanoseconds function. For information about the canonical AddNanoseconds function, see Date and Time Canonical Functions.
//        /// </summary>
//        /// 
//        /// <returns>
//        /// The <paramref name="timeValue"/> incremented by <paramref name="addValue"/>.
//        /// </returns>
//        /// <param name="timeValue">A valid date time offset.</param><param name="addValue">The number of nanoseconds to add to <paramref name="timeValue"/>.</param>
//        [EdmFunction("Edm", "AddNanoseconds")]
//        public static DateTimeOffset? AddNanoseconds(DateTimeOffset? timeValue, int? addValue)
//        {
//            throw new NotSupportedException("Cannot direct call methods");
//        }

//        /// <summary>
//        /// Invokes the canonical AddNanoseconds function. For information about the canonical AddNanoseconds function, see Date and Time Canonical Functions.
//        /// </summary>
//        /// 
//        /// <returns>
//        /// The <paramref name="timeValue"/> incremented by <paramref name="addValue"/>.
//        /// </returns>
//        /// <param name="timeValue">A valid date.</param><param name="addValue">The number of nanoseconds to add to <paramref name="timeValue"/>.</param>
//        [EdmFunction("Edm", "AddNanoseconds")]
//        public static DateTime? AddNanoseconds(DateTime? timeValue, int? addValue)
//        {
//            throw new NotSupportedException("Cannot direct call methods");
//        }

//        /// <summary>
//        /// Invokes the canonical AddNanoseconds function. For information about the canonical AddNanoseconds function, see Date and Time Canonical Functions.
//        /// </summary>
//        /// 
//        /// <returns>
//        /// The <paramref name="timeValue"/> incremented by <paramref name="addValue"/>.
//        /// </returns>
//        /// <param name="timeValue">A valid time span.</param><param name="addValue">The number of nanoseconds to add to <paramref name="timeValue"/>.</param>
//        [EdmFunction("Edm", "AddNanoseconds")]
//        public static TimeSpan? AddNanoseconds(TimeSpan? timeValue, int? addValue)
//        {
//            throw new NotSupportedException("Cannot direct call methods");
//        }

//        /// <summary>
//        /// Invokes the canonical DiffYears function. For information about the canonical DiffYears function, see Date and Time Canonical Functions.
//        /// </summary>
//        /// 
//        /// <returns>
//        /// The number of years between <paramref name="dateValue1"/> and <paramref name="dateValue2"/>.
//        /// </returns>
//        /// <param name="dateValue1">A valid date time offset.</param><param name="dateValue2">A valid date time offset.</param>
//        [EdmFunction("Edm", "DiffYears")]
//        public static int? DiffYears(DateTimeOffset? dateValue1, DateTimeOffset? dateValue2)
//        {
//            throw new NotSupportedException("Cannot direct call methods");
//        }

//        /// <summary>
//        /// Invokes the canonical DiffYears function. For information about the canonical DiffYears function, see Date and Time Canonical Functions.
//        /// </summary>
//        /// 
//        /// <returns>
//        /// The number of years between <paramref name="dateValue1"/> and <paramref name="dateValue2"/>.
//        /// </returns>
//        /// <param name="dateValue1">A valid date.</param><param name="dateValue2">A valid date.</param>
//        [EdmFunction("Edm", "DiffYears")]
//        public static int? DiffYears(DateTime? dateValue1, DateTime? dateValue2)
//        {
//            throw new NotSupportedException("Cannot direct call methods");
//        }

//        /// <summary>
//        /// Invokes the canonical DiffMonths function. For information about the canonical DiffMonths function, see Date and Time Canonical Functions.
//        /// </summary>
//        /// 
//        /// <returns>
//        /// The number of months between <paramref name="dateValue1"/> and <paramref name="dateValue2"/>.
//        /// </returns>
//        /// <param name="dateValue1">A valid date time offset.</param><param name="dateValue2">A valid date time offset.</param>
//        [EdmFunction("Edm", "DiffMonths")]
//        public static int? DiffMonths(DateTimeOffset? dateValue1, DateTimeOffset? dateValue2)
//        {
//            throw new NotSupportedException("Cannot direct call methods");
//        }

//        /// <summary>
//        /// Invokes the canonical DiffMonths function. For information about the canonical DiffMonths function, see Date and Time Canonical Functions.
//        /// </summary>
//        /// 
//        /// <returns>
//        /// The number of months between <paramref name="dateValue1"/> and <paramref name="dateValue2"/>.
//        /// </returns>
//        /// <param name="dateValue1">A valid date.</param><param name="dateValue2">A valid date.</param>
//        [EdmFunction("Edm", "DiffMonths")]
//        public static int? DiffMonths(DateTime? dateValue1, DateTime? dateValue2)
//        {
//            throw new NotSupportedException("Cannot direct call methods");
//        }

//        /// <summary>
//        /// Invokes the canonical DiffDays function. For information about the canonical DiffDays function, see Date and Time Canonical Functions.
//        /// </summary>
//        /// 
//        /// <returns>
//        /// The number of days between <paramref name="dateValue1"/> and <paramref name="dateValue2"/>.
//        /// </returns>
//        /// <param name="dateValue1">A valid date time offset.</param><param name="dateValue2">A valid date time offset.</param>
//        [EdmFunction("Edm", "DiffDays")]
//        public static int? DiffDays(DateTimeOffset? dateValue1, DateTimeOffset? dateValue2)
//        {
//            throw new NotSupportedException("Cannot direct call methods");
//        }

//        /// <summary>
//        /// Invokes the canonical DiffDays function. For information about the canonical DiffDays function, see Date and Time Canonical Functions.
//        /// </summary>
//        /// 
//        /// <returns>
//        /// The number of days between <paramref name="dateValue1"/> and <paramref name="dateValue2"/>.
//        /// </returns>
//        /// <param name="dateValue1">A valid date.</param><param name="dateValue2">A valid date.</param>
//        [EdmFunction("Edm", "DiffDays")]
//        public static int? DiffDays(DateTime? dateValue1, DateTime? dateValue2)
//        {
//            throw new NotSupportedException("Cannot direct call methods");
//        }

//        /// <summary>
//        /// Invokes the canonical DiffHours function. For information about the canonical DiffHours function, see Date and Time Canonical Functions.
//        /// </summary>
//        /// 
//        /// <returns>
//        /// The number of hours between <paramref name="timeValue1"/> and <paramref name="timeValue2"/>.
//        /// </returns>
//        /// <param name="timeValue1">A valid date time offset.</param><param name="timeValue2">A valid date time offset.</param>
//        [EdmFunction("Edm", "DiffHours")]
//        public static int? DiffHours(DateTimeOffset? timeValue1, DateTimeOffset? timeValue2)
//        {
//            throw new NotSupportedException("Cannot direct call methods");
//        }

//        /// <summary>
//        /// Invokes the canonical DiffHours function. For information about the canonical DiffHours function, see Date and Time Canonical Functions.
//        /// </summary>
//        /// 
//        /// <returns>
//        /// The number of hours between <paramref name="timeValue1"/> and <paramref name="timeValue2"/>.
//        /// </returns>
//        /// <param name="timeValue1">A valid date.</param><param name="timeValue2">A valid date.</param>
//        [EdmFunction("Edm", "DiffHours")]
//        public static int? DiffHours(DateTime? timeValue1, DateTime? timeValue2)
//        {
//            throw new NotSupportedException("Cannot direct call methods");
//        }

//        /// <summary>
//        /// Invokes the canonical DiffHours function. For information about the canonical DiffHours function, see Date and Time Canonical Functions.
//        /// </summary>
//        /// 
//        /// <returns>
//        /// The number of hours between <paramref name="timeValue1"/> and <paramref name="timeValue2"/>.
//        /// </returns>
//        /// <param name="timeValue1">A valid time span.</param><param name="timeValue2">A valid time span.</param>
//        [EdmFunction("Edm", "DiffHours")]
//        public static int? DiffHours(TimeSpan? timeValue1, TimeSpan? timeValue2)
//        {
//            throw new NotSupportedException("Cannot direct call methods");
//        }

//        /// <summary>
//        /// Invokes the canonical DiffMinutes function. For information about the canonical DiffMinutes function, see Date and Time Canonical Functions.
//        /// </summary>
//        /// 
//        /// <returns>
//        /// The number of minutes between <paramref name="timeValue1"/> and <paramref name="timeValue2"/>.
//        /// </returns>
//        /// <param name="timeValue1">A valid date time offset.</param><param name="timeValue2">A valid date time offset.</param>
//        [EdmFunction("Edm", "DiffMinutes")]
//        public static int? DiffMinutes(DateTimeOffset? timeValue1, DateTimeOffset? timeValue2)
//        {
//            throw new NotSupportedException("Cannot direct call methods");
//        }

//        /// <summary>
//        /// Invokes the canonical DiffMinutes function. For information about the canonical DiffMinutes function, see Date and Time Canonical Functions.
//        /// </summary>
//        /// 
//        /// <returns>
//        /// The number of minutes between <paramref name="timeValue1"/> and <paramref name="timeValue2"/>.
//        /// </returns>
//        /// <param name="timeValue1">A valid date.</param><param name="timeValue2">A valid date.</param>
//        [EdmFunction("Edm", "DiffMinutes")]
//        public static int? DiffMinutes(DateTime? timeValue1, DateTime? timeValue2)
//        {
//            throw new NotSupportedException("Cannot direct call methods");
//        }

//        /// <summary>
//        /// Invokes the canonical DiffMinutes function. For information about the canonical DiffMinutes function, see Date and Time Canonical Functions.
//        /// </summary>
//        /// 
//        /// <returns>
//        /// The number of minutes between <paramref name="timeValue1"/> and <paramref name="timeValue2"/>.
//        /// </returns>
//        /// <param name="timeValue1">A valid time span.</param><param name="timeValue2">A valid time span.</param>
//        [EdmFunction("Edm", "DiffMinutes")]
//        public static int? DiffMinutes(TimeSpan? timeValue1, TimeSpan? timeValue2)
//        {
//            throw new NotSupportedException("Cannot direct call methods");
//        }

//        /// <summary>
//        /// Invokes the canonical DiffSeconds function. For information about the canonical DiffSeconds function, see Date and Time Canonical Functions.
//        /// </summary>
//        /// 
//        /// <returns>
//        /// The number of seconds between <paramref name="timeValue1"/> and <paramref name="timeValue2"/>.
//        /// </returns>
//        /// <param name="timeValue1">A valid date time offset.</param><param name="timeValue2">A valid date time offset.</param>
//        [EdmFunction("Edm", "DiffSeconds")]
//        public static int? DiffSeconds(DateTimeOffset? timeValue1, DateTimeOffset? timeValue2)
//        {
//            throw new NotSupportedException("Cannot direct call methods");
//        }

//        /// <summary>
//        /// Invokes the canonical DiffSeconds function. For information about the canonical DiffSeconds function, see Date and Time Canonical Functions.
//        /// </summary>
//        /// 
//        /// <returns>
//        /// The number of seconds between <paramref name="timeValue1"/> and <paramref name="timeValue2"/>.
//        /// </returns>
//        /// <param name="timeValue1">A valid date.</param><param name="timeValue2">A valid date.</param>
//        [EdmFunction("Edm", "DiffSeconds")]
//        public static int? DiffSeconds(DateTime? timeValue1, DateTime? timeValue2)
//        {
//            throw new NotSupportedException("Cannot direct call methods");
//        }

//        /// <summary>
//        /// Invokes the canonical DiffSeconds function. For information about the canonical DiffSeconds function, see Date and Time Canonical Functions.
//        /// </summary>
//        /// 
//        /// <returns>
//        /// The number of seconds between <paramref name="timeValue1"/> and <paramref name="timeValue2"/>.
//        /// </returns>
//        /// <param name="timeValue1">A valid time span.</param><param name="timeValue2">A valid time span.</param>
//        [EdmFunction("Edm", "DiffSeconds")]
//        public static int? DiffSeconds(TimeSpan? timeValue1, TimeSpan? timeValue2)
//        {
//            throw new NotSupportedException("Cannot direct call methods");
//        }

//        /// <summary>
//        /// Invokes the canonical DiffMilliseconds function. For information about the canonical DiffMilliseconds function, see Date and Time Canonical Functions.
//        /// </summary>
//        /// 
//        /// <returns>
//        /// The number of milliseconds between <paramref name="timeValue1"/> and <paramref name="timeValue2"/>.
//        /// </returns>
//        /// <param name="timeValue1">A valid date time offset.</param><param name="timeValue2">A valid date time offset.</param>
//        [EdmFunction("Edm", "DiffMilliseconds")]
//        public static int? DiffMilliseconds(DateTimeOffset? timeValue1, DateTimeOffset? timeValue2)
//        {
//            throw new NotSupportedException("Cannot direct call methods");
//        }

//        /// <summary>
//        /// Invokes the canonical DiffMilliseconds function. For information about the canonical DiffMilliseconds function, see Date and Time Canonical Functions.
//        /// </summary>
//        /// 
//        /// <returns>
//        /// The number of milliseconds between <paramref name="timeValue1"/> and <paramref name="timeValue2"/>.
//        /// </returns>
//        /// <param name="timeValue1">A valid date.</param><param name="timeValue2">A valid date.</param>
//        [EdmFunction("Edm", "DiffMilliseconds")]
//        public static int? DiffMilliseconds(DateTime? timeValue1, DateTime? timeValue2)
//        {
//            throw new NotSupportedException("Cannot direct call methods");
//        }

//        /// <summary>
//        /// Invokes the canonical DiffMilliseconds function. For information about the canonical DiffMilliseconds function, see Date and Time Canonical Functions.
//        /// </summary>
//        /// 
//        /// <returns>
//        /// The number of milliseconds between <paramref name="timeValue1"/> and <paramref name="timeValue2"/>.
//        /// </returns>
//        /// <param name="timeValue1">A valid time span.</param><param name="timeValue2">A valid time span.</param>
//        [EdmFunction("Edm", "DiffMilliseconds")]
//        public static int? DiffMilliseconds(TimeSpan? timeValue1, TimeSpan? timeValue2)
//        {
//            throw new NotSupportedException("Cannot direct call methods");
//        }

//        /// <summary>
//        /// Invokes the canonical DiffMicroseconds function. For information about the canonical DiffMicroseconds function, see Date and Time Canonical Functions.
//        /// </summary>
//        /// 
//        /// <returns>
//        /// The number of microseconds between <paramref name="timeValue1"/> and <paramref name="timeValue2"/>.
//        /// </returns>
//        /// <param name="timeValue1">A valid date time offset.</param><param name="timeValue2">A valid date time offset.</param>
//        [EdmFunction("Edm", "DiffMicroseconds")]
//        public static int? DiffMicroseconds(DateTimeOffset? timeValue1, DateTimeOffset? timeValue2)
//        {
//            throw new NotSupportedException("Cannot direct call methods");
//        }

//        /// <summary>
//        /// Invokes the canonical DiffMicroseconds function. For information about the canonical DiffMicroseconds function, see Date and Time Canonical Functions.
//        /// </summary>
//        /// 
//        /// <returns>
//        /// The number of microseconds between <paramref name="timeValue1"/> and <paramref name="timeValue2"/>.
//        /// </returns>
//        /// <param name="timeValue1">A valid date.</param><param name="timeValue2">A valid date.</param>
//        [EdmFunction("Edm", "DiffMicroseconds")]
//        public static int? DiffMicroseconds(DateTime? timeValue1, DateTime? timeValue2)
//        {
//            throw new NotSupportedException("Cannot direct call methods");
//        }

//        /// <summary>
//        /// Invokes the canonical DiffMicroseconds function. For information about the canonical DiffMicroseconds function, see Date and Time Canonical Functions.
//        /// </summary>
//        /// 
//        /// <returns>
//        /// The number of microseconds between <paramref name="timeValue1"/> and <paramref name="timeValue2"/>.
//        /// </returns>
//        /// <param name="timeValue1">A valid time span.</param><param name="timeValue2">A valid time span.</param>
//        [EdmFunction("Edm", "DiffMicroseconds")]
//        public static int? DiffMicroseconds(TimeSpan? timeValue1, TimeSpan? timeValue2)
//        {
//            throw new NotSupportedException("Cannot direct call methods");
//        }

//        /// <summary>
//        /// Invokes the canonical DiffNanoseconds function. For information about the canonical DiffNanoseconds function, see Date and Time Canonical Functions.
//        /// </summary>
//        /// 
//        /// <returns>
//        /// The number of nanoseconds between <paramref name="timeValue1"/> and <paramref name="timeValue2"/>.
//        /// </returns>
//        /// <param name="timeValue1">A valid date time offset.</param><param name="timeValue2">A valid date time offset.</param>
//        [EdmFunction("Edm", "DiffNanoseconds")]
//        public static int? DiffNanoseconds(DateTimeOffset? timeValue1, DateTimeOffset? timeValue2)
//        {
//            throw new NotSupportedException("Cannot direct call methods");
//        }

//        /// <summary>
//        /// Invokes the canonical DiffNanoseconds function. For information about the canonical DiffNanoseconds function, see Date and Time Canonical Functions.
//        /// </summary>
//        /// 
//        /// <returns>
//        /// The number of nanoseconds between <paramref name="timeValue1"/> and <paramref name="timeValue2"/>.
//        /// </returns>
//        /// <param name="timeValue1">A valid date.</param><param name="timeValue2">A valid date.</param>
//        [EdmFunction("Edm", "DiffNanoseconds")]
//        public static int? DiffNanoseconds(DateTime? timeValue1, DateTime? timeValue2)
//        {
//            throw new NotSupportedException("Cannot direct call methods");
//        }

//        /// <summary>
//        /// Invokes the canonical DiffNanoseconds function. For information about the canonical DiffNanoseconds function, see Date and Time Canonical Functions.
//        /// </summary>
//        /// 
//        /// <returns>
//        /// The number of nanoseconds between <paramref name="timeValue1"/> and <paramref name="timeValue2"/>.
//        /// </returns>
//        /// <param name="timeValue1">A valid date.</param><param name="timeValue2">A valid date.</param>
//        [EdmFunction("Edm", "DiffNanoseconds")]
//        public static int? DiffNanoseconds(TimeSpan? timeValue1, TimeSpan? timeValue2)
//        {
//            throw new NotSupportedException("Cannot direct call methods");
//        }

//        /// <summary>
//        /// Invokes the canonical Truncate function. For information about the canonical Truncate function, seeMath Canonical Functions.
//        /// </summary>
//        /// 
//        /// <returns>
//        /// <paramref name="value"/> truncated to the length or precision specified by <paramref name="digits"/>.
//        /// </returns>
//        /// <param name="value">The number to truncate.</param><param name="digits">The length or precision to truncate to.</param>
//        [EdmFunction("Edm", "Truncate")]
//        public static double? Truncate(double? value, int? digits)
//        {
//            throw new NotSupportedException("Cannot direct call methods");
//        }

//        /// <summary>
//        /// Invokes the canonical Truncate function. For information about the canonical Truncate function, seeMath Canonical Functions.
//        /// </summary>
//        /// 
//        /// <returns>
//        /// <paramref name="value"/> truncated to the length or precision specified by <paramref name="digits"/>.
//        /// </returns>
//        /// <param name="value">The number to truncate.</param><param name="digits">The length or precision to truncate to.</param>
//        [EdmFunction("Edm", "Truncate")]
//        public static Decimal? Truncate(Decimal? value, int? digits)
//        {
//            throw new NotSupportedException("Cannot direct call methods");
//        }
//    }
//}
