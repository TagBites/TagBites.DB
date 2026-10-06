using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
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
        var constructor = type.GetTypeInfo().DeclaredConstructors.FirstOrDefault(x => x.IsPublic && x.GetParameters().Length == 0);
        if (constructor == null)
            throw new ArgumentException($"Type {type} has no public parameterless constructor.", nameof(type));

        var factory = Expression.Lambda<Func<object>>(Expression.New(constructor)).Compile();

        var properties = TypeUtils.GetProperties(type)
            .Where(CanFill)
            .Select(x => new Property(x))
            .ToArray();

        return new QueryObjectBinder(factory, properties);
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

        public Property(PropertyInfo property)
        {
            PropertyInfo = property;
            PropertyType = property.PropertyType;
            Setter = CreateSetter(property);
        }


        private static Action<object, object> CreateSetter(PropertyInfo property)
        {
            var item = Expression.Parameter(typeof(object), "item");
            var value = Expression.Parameter(typeof(object), "value");

            var body = Expression.Call(
                Expression.Convert(item, property.DeclaringType!),
                property.SetMethod!,
                Expression.Convert(value, property.PropertyType));

            return Expression.Lambda<Action<object, object>>(body, item, value).Compile();
        }
    }
}
