using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using TBS.Utils;

namespace TBS
{
    internal static class CollectionHelper
    {
        public static bool IsReadOnly(object collection)
        {
            if (collection == null)
                return true;

            if (collection.GetType().IsArray)
                return true;

            if (collection is IList)
                return ((IList)collection).IsReadOnly;

            return false;
        }
        public static bool AllowAdd(object collection)
        {
            if (collection == null)
                return false;

            if (collection.GetType().IsArray)
                return false;

            if (collection is IList)
                return !((IList)collection).IsReadOnly;

            return false;
        }

        public static void Add(object dataSource, object item)
        {
            Guard.ArgumentNotNull(dataSource, nameof(dataSource));

            if (dataSource is IList)
                ((IList)dataSource).Add(item);
            else
            {
                var mi = dataSource.GetType().GetTypeInfo().GetDeclaredMethod("Add");
                var prms = mi.GetParameters();
                if (prms.Length != 1 || !prms[0].ParameterType.GetTypeInfo().IsAssignableFrom(item.GetType().GetTypeInfo()))
                    throw new InvalidOperationException();

                mi.Invoke(dataSource, new[] { item });
            }
        }

        public static object FirstOrDefault(object collection)
        {
            if (collection is IList)
            {
                var list = (IList)collection;
                return list.Count > 0 ? list[0] : null;
            }
            else
            {
                var enumerable = collection is ICollection
                    ? (ICollection)collection
                    : collection as IEnumerable;

                if (enumerable != null)
                {
                    var enumerator = enumerable.GetEnumerator();
                    if (enumerator.MoveNext())
                        return enumerator.Current;
                }
            }

            return null;
        }

        /// <summary>
        /// Looking for on of interfaces: ICollection'T, IEnumerable'T, IEnumerable and returns first match (object for IEnumerable). 
        /// If not found returns null.
        /// </summary>
        public static Type GetItemType(object collection)
        {
            Guard.ArgumentNotNull(collection, "collection");

            return GetItemType(collection.GetType());
        }

        /// <summary>
        /// Looking for on of interfaces: ICollection'T, IEnumerable'T, IEnumerable and returns first match (object for IEnumerable).
        /// If not found returns null.
        /// </summary>
        public static Type GetItemType(Type collectionType)
        {
            Guard.ArgumentNotNull(collectionType, "collectionType");

            var args = TypeUtils.GetGenericArguments(collectionType, typeof(ICollection<>));
            if (args.Length == 1)
                return args[0];

            args = TypeUtils.GetGenericArguments(collectionType, typeof(IEnumerable<>));
            if (args.Length == 1)
                return args[0];

            return typeof(IEnumerable).GetTypeInfo().IsAssignableFrom(collectionType.GetTypeInfo())
                ? typeof(object)
                : null;
        }

        public static bool RemoveRange<T>(this ICollection<T> list, IEnumerable<T> range)
        {
            Guard.ArgumentNotNull(list, "list");

            var removed = false;

            foreach (var item in range)
                if (list.Remove(item))
                    removed = true;

            return removed;
        }
        public static bool RemoveRange<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, IEnumerable<TKey> range)
        {
            Guard.ArgumentNotNull(dictionary, "dictionary");

            var removed = false;

            foreach (var item in range)
                if (dictionary.Remove(item))
                    removed = true;

            return removed;
        }

        public static void SynchronizeWithClear<T>(ICollection<T> src, ICollection<T> des)
        {
            des.Clear();

            foreach (var item in src)
                des.Add(item);
        }
        public static void Synchronize<T>(IList<T> src, IList<T> des)
        {
            var srcH = new HashSet<T>(src);

            // Remove Old
            for (var i = des.Count - 1; i >= 0; i--)
                if (!srcH.Remove(des[i]))
                    des.RemoveAt(i);

            // Add New
            foreach (var item in srcH)
                des.Add(item);
        }
        public static void Synchronize<T, G>(IList<T> src, IList<G> des, Func<T, G, bool> equals, Func<T, G> convert)
        {
            var srcH = new List<T>(src);

            // Remove Old
            for (var i = des.Count - 1; i >= 0; i--)
            {
                var desItem = des[i];
                var index = -1;

                for (var j = srcH.Count - 1; j >= 0; j--)
                    if (equals(srcH[j], desItem))
                    {
                        index = j;
                        break;
                    }

                if (index != -1)
                    srcH.RemoveAt(index);
                else
                    des.RemoveAt(i);
            }

            // Add New
            foreach (var item in srcH)
                des.Add(convert(item));
        }
        public static void SynchronizeWithOrder<T>(IList<T> src, IList<T> des)
        {
            for (var i = 0; i < src.Count; i++)
            {
                var item = src[i];

                if (i == des.Count)
                    des.Add(item);
                else if (!EqualityComparer<T>.Default.Equals(item, des[i]))
                    des[i] = item;
            }

            while (des.Count > src.Count)
                des.RemoveAt(des.Count - 1);
        }
        public static void SynchronizeWithOrder<T, G>(IList<T> src, IList<G> des, Func<T, G, bool> equals, Func<T, G> convert)
        {
            for (var i = 0; i < src.Count; i++)
            {
                var item = src[i];

                if (i == des.Count)
                    des.Add(convert(item));
                else if (!equals(item, des[i]))
                    des[i] = convert(item);
            }

            while (des.Count > src.Count)
                des.RemoveAt(des.Count - 1);
        }

        public static bool SequenceEqual<T>(IEnumerable<T> first, IEnumerable<T> second)
        {
            if (first == second)
                return true;
            if (first == null)
                return !second.Any();
            if (second == null)
                return !first.Any();

            return first.SequenceEqual(second);
        }
        public static bool SequenceStartsWith<T>(IEnumerable<T> first, IEnumerable<T> second)
        {
            return SequenceStartsWith(first, second, null);
        }
        public static bool SequenceStartsWith<T>(IEnumerable<T> first, IEnumerable<T> second, IEqualityComparer<T> comparer)
        {
            if (first == second)
                return true;
            if (first == null)
                return !second.Any();
            if (second == null)
                return !first.Any();

            if (comparer == null)
                comparer = EqualityComparer<T>.Default;

            using (var enumerator1 = first.GetEnumerator())
            using (var enumerator2 = second.GetEnumerator())
            {
                while (enumerator2.MoveNext())
                {
                    if (!enumerator1.MoveNext() || !comparer.Equals(enumerator1.Current, enumerator2.Current))
                        return false;
                }
            }

            return true;
        }

        public static T[] ArrayCombine<T>(T[] array1, T[] array2)
        {
            if (array1 == null || array1.Length == 0)
                return array2;
            if (array2 == null || array2.Length == 0)
                return array1;

            var newArray = new T[array1.Length + array2.Length];

            Array.Copy(array1, 0, newArray, 0, array1.Length);
            Array.Copy(array2, 0, newArray, array1.Length, array2.Length);

            return newArray;
        }
        public static IEnumerable<T> Combine<T>(IEnumerable<T> collection, T element)
        {
            foreach (var item in collection)
                yield return item;

            yield return element;
        }
        public static IEnumerable<T> CombineNotNull<T>(IEnumerable<T> collection, T elementToCheck)
        {
            return elementToCheck == null
                ? collection
                : Combine(collection, elementToCheck);
        }
        public static IEnumerable<T> Combine<T>(params IEnumerable<T>[] collections)
        {
            foreach (var collection in collections)
                if (collection != null)
                    foreach (var item in collection)
                        yield return item;
        }

        public static IEnumerable<T> AsEnumerable<T>(params T[] items)
        {
            foreach (var item in items)
                yield return item;
        }
        public static IEnumerable<T> AsEnumerableWithoutNulls<T>(params T[] items)
        {
            foreach (var item in items)
                if (item != null)
                    yield return item;
        }
        public static IEnumerable<string> AsEnumerableWithoutNullsOrEmpty(params string[] items)
        {
            foreach (var item in items)
                if (!string.IsNullOrEmpty(item))
                    yield return item;
        }

        public static IEnumerable<T> GetRecursive<T>(T root, Func<T, T> getNextElement)
        {
            for (var i = root; i != null; i = getNextElement(i))
                yield return i;
        }
        public static IEnumerable<T> GetRecursive<T>(IEnumerable<T> roots, Func<T, T> getNextElement)
        {
            Guard.ArgumentNotNull(roots, "roots");

            foreach (var root in roots)
                for (var i = root; i != null; i = getNextElement(i))
                    yield return i;
        }
        public static IEnumerable<T> GetRecursive<CT, T>(CT root, Func<CT, CT> getNextElement, Func<CT, T> getItem)
        {
            for (var i = root; i != null; i = getNextElement(i))
                yield return getItem(i);
        }
        public static IEnumerable<T> GetRecursive<CT, T>(IEnumerable<CT> roots, Func<CT, CT> getNextElement, Func<CT, T> getItem)
        {
            Guard.ArgumentNotNull(roots, "roots");

            foreach (var root in roots)
                for (var i = root; i != null; i = getNextElement(i))
                    yield return getItem(i);
        }
        public static IEnumerable<T> GetRecursive<T>(T root, Func<T, IEnumerable<T>> getNextElements)
        {
            return root != null
                ? GetRecursive(new[] { root }, getNextElements)
                : Enumerable.Empty<T>();
        }
        public static IEnumerable<T> GetRecursive<T>(IEnumerable<T> roots, Func<T, IEnumerable<T>> getNextElements)
        {
            Guard.ArgumentNotNull(roots, "roots");

            var stack = new LinkedList<T>(roots);
            while (stack.Count > 0)
            {
                var node = stack.First;
                yield return node.Value;

                var childItems = getNextElements(node.Value);
                if (childItems != null)
                    foreach (var next in childItems)
                        node = stack.AddAfter(node, next);

                stack.RemoveFirst();
            }
        }
        public static IEnumerable<T> GetRecursive<CT, T>(CT root, Func<CT, IEnumerable<CT>> getNextElements, Func<CT, T> getItem)
        {
            return root != null
                ? GetRecursive(new[] { root }, getNextElements, getItem)
                : Enumerable.Empty<T>();
        }
        public static IEnumerable<T> GetRecursive<CT, T>(IEnumerable<CT> roots, Func<CT, IEnumerable<CT>> getNextElements, Func<CT, T> getItem)
        {
            Guard.ArgumentNotNull(roots, "roots");

            var queue = new Queue<CT>(roots);
            while (queue.Count > 0)
            {
                var element = queue.Dequeue();
                yield return getItem(element);

                var childItems = getNextElements(element);
                if (childItems != null)
                    foreach (var next in childItems)
                        queue.Enqueue(next);
            }
        }
        public static IEnumerable<T> GetRecursiveMany<CT, T>(CT root, Func<CT, IEnumerable<CT>> getNextElements, Func<CT, IEnumerable<T>> getItems)
        {
            return root != null
                ? GetRecursiveMany(new[] { root }, getNextElements, getItems)
                : Enumerable.Empty<T>();
        }
        public static IEnumerable<T> GetRecursiveMany<CT, T>(IEnumerable<CT> roots, Func<CT, IEnumerable<CT>> getNextElements, Func<CT, IEnumerable<T>> getItems)
        {
            Guard.ArgumentNotNull(roots, "roots");

            var queue = new Queue<CT>(roots);
            while (queue.Count > 0)
            {
                var element = queue.Dequeue();
                var items = getItems(element);

                if (items != null)
                    foreach (var item in items)
                        yield return item;

                var childItems = getNextElements(element);
                if (childItems != null)
                    foreach (var next in childItems)
                        queue.Enqueue(next);
            }
        }
        public static IEnumerable<T> GetRecursiveMany<CT, T>(CT root, Func<CT, CT> getNextElement, Func<CT, IEnumerable<T>> getItems)
        {
            for (var i = root; i != null; i = getNextElement(i))
                foreach (var item in getItems(i))
                    yield return item;
        }
        public static IEnumerable<T> GetRecursiveMany<CT, T>(IEnumerable<CT> roots, Func<CT, CT> getNextElement, Func<CT, IEnumerable<T>> getItems)
        {
            Guard.ArgumentNotNull(roots, "roots");

            foreach (var root in roots)
                for (var i = root; i != null; i = getNextElement(i))
                    foreach (var item in getItems(i))
                        yield return item;
        }
    }
}
