using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using TagBites.DB.Configuration;
using TagBites.Utils;

namespace TagBites.DB;

internal sealed class QueryObjectBinder
{
    private static readonly ConditionalWeakTable<Type, QueryObjectBinder> s_binders = new();

    public Func<object> Factory { get; }
    public Property[] Properties { get; }

    private QueryObjectBinder(Func<object> factory, Property[] properties)
    {
        Factory = factory;
        Properties = properties;
    }


    public static QueryObjectBinder Get(Type type) => s_binders.GetValue(type, Create);

    private static QueryObjectBinder Create(Type type)
    {
        var factory = Expression.Lambda<Func<object>>(Expression.Convert(CreateInstance(type), typeof(object))).Compile();

        var properties = TypeUtils.GetProperties(type)
            .Where(CanFill)
            .Select(x => new Property(x))
            .ToArray();

        return new QueryObjectBinder(factory, properties);
    }
    private static NewExpression CreateInstance(Type type)
    {
        var constructor = type.GetTypeInfo().DeclaredConstructors.FirstOrDefault(x => x.IsPublic && x.GetParameters().Length == 0);
        if (constructor != null)
            return Expression.New(constructor);

        if (type.IsValueType)
            return Expression.New(type);

        throw new ArgumentException($"Type {type} has no public parameterless constructor.", nameof(type));
    }
    private static bool CanFill(PropertyInfo property)
    {
        return property.SetMethod is { IsStatic: false }
               && property.GetIndexParameters().Length == 0;
    }

    internal sealed class Property
    {
        public PropertyInfo PropertyInfo { get; }
        public Type PropertyType { get; }
        public Action<object, object> Setter { get; }
        public bool CanShareNullValue { get; }
        public object NullValue { get; }

        public Property(PropertyInfo property)
        {
            PropertyInfo = property;
            PropertyType = property.PropertyType;
            Setter = CreateSetter(property);

            CanShareNullValue = PropertyType.IsValueType && PropertyType.GetConstructor(Type.EmptyTypes) == null;
            NullValue = CanShareNullValue ? DbLinkDataConverter.Default.ChangeType(null, PropertyType) : null;
        }


        private static Action<object, object> CreateSetter(PropertyInfo property)
        {
            var item = Expression.Parameter(typeof(object), "item");
            var value = Expression.Parameter(typeof(object), "value");

            var declaringType = property.DeclaringType!;

            var instance = declaringType.IsValueType
                ? Expression.Unbox(item, declaringType)
                : Expression.Convert(item, declaringType);

            var propertyType = property.PropertyType;
            var converted = propertyType.IsValueType && Nullable.GetUnderlyingType(propertyType) == null
                ? Expression.Condition(Expression.Equal(value, Expression.Constant(null)), Expression.Default(propertyType), Expression.Convert(value, propertyType))
                : (Expression)Expression.Convert(value, propertyType);

            var body = Expression.Call(instance, property.SetMethod!, converted);

            return Expression.Lambda<Action<object, object>>(body, item, value).Compile();
        }
    }
}
