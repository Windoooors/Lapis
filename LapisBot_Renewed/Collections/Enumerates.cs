using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Newtonsoft.Json.Serialization;

namespace LapisBot_Renewed.Collections
{
    internal static class Enumerates
    {
        public static IEnumerable<TResult> Cast<TResult>(this IEnumerable enumerable)
        {
            if (enumerable == null)
            {
                throw new ArgumentNullException("enumerable");
            }

            var result = enumerable as IEnumerable<TResult>;

            if (result != null)
            {
                return result;
            }

            return CastIterator<TResult>(enumerable);
        }

        public static int Count<T>(this IEnumerable<T> enumerable)
        {
            var collection = enumerable as ICollection<T>;

            if (collection != null)
            {
                return collection.Count;
            }

            int count = 0;

            using (var enumerator = enumerable.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    ++count;
                }
            }

            return count;
        }

        public static IEnumerable<TResult> Select<TSource, TResult>(this IEnumerable<TSource> enumerable, Func<TSource, TResult> func)
        {
            if (enumerable == null)
            {
                throw new ArgumentNullException("enumerable");
            }

            if (func == null)
            {
                throw new ArgumentNullException("func");
            }

            foreach (var item in enumerable)
            {
                yield return func.Invoke(item);
            }
        }

        public static IEnumerable<T> Where<T>(this IEnumerable<T> enumerable, Func<T, bool> func)
        {
            if (enumerable == null)
            {
                throw new ArgumentNullException("enumerable");
            }

            if (func == null)
            {
                throw new ArgumentNullException("func");
            }

            using (var enumerator = enumerable.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    if (func.Invoke(enumerator.Current))
                    {
                        yield return enumerator.Current;
                    }
                }
            }
        }

        public static T[] ToArray<T>(this IEnumerable<T> enumerable)
        {
            if (enumerable == null)
            {
                throw new ArgumentNullException("enumerable");
            }

            var collection = enumerable as ICollection<T>;

            if (collection != null)
            {
                int count = collection.Count;

                if (count == 0)
                {
                    return Arrays.Empty<T>();
                }

                var array = new T[count];
                collection.CopyTo(array, 0);
                return array;
            }

            var builder = new ArrayBuilder<T>();
            var enumerator = enumerable.GetEnumerator();

            while (enumerator.MoveNext())
            {
                builder.Add(enumerator.Current);
            }

            return builder.ToArray();
        }

        public static List<T> ToList<T>(this IEnumerable<T> enumerable)
        {
            if (enumerable == null)
            {
                throw new ArgumentNullException("enumerable");
            }

            return new List<T>(enumerable);
        }

        public static IEnumerable<T> Skip<T>(this IEnumerable<T> enumerable, int count)
        {
            using (var enumerator = enumerable.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    if (--count < 0)
                    {
                        yield return enumerator.Current;
                    }
                }
            }
        }

        public static IEnumerable<T> Take<T>(this IEnumerable<T> enumerable, int count)
        {
            using (var enumerator = enumerable.GetEnumerator())
            {
                return TakeIterator(enumerator, count);
            }
        }

        public static IEnumerable<IEnumerable<T>> Slice<T>(this IEnumerable<T> enumerable, int size)
        {
            int count = enumerable.Count();
            int sliceCount = (count / size) + (count % size == 0 ? 0 : 1);

            using (var enumerator = enumerable.GetEnumerator())
            {
                while (sliceCount-- > 0)
                {
                    yield return TakeIterator(enumerator, size);
                }
            }
        }

        private static IEnumerable<T> TakeIterator<T>(IEnumerator<T> enumerator, int count)
        {
            while (enumerator.MoveNext())
            {
                yield return enumerator.Current;

                if (--count == 0)
                {
                    yield break;
                }
            }
        }

        private static IEnumerable<TResult> CastIterator<TResult>(IEnumerable enumerable)
        {
            foreach (var item in enumerable)
            {
                yield return (TResult)item;
            }
        }

        private struct ArrayBuilder<T>
        {
            private const int DefaultCapacity = 4;

            private T[] _array;
            private int _count;

            public ArrayBuilder(int capacity) : this()
            {
                if (capacity > 0)
                {
                    _array = new T[capacity];
                }
            }

            public T[] Buffer { get { return _array; } }

            public int Capacity { get { return _array == null ? 0 : _array.Length; } }

            public int Count { get { return _count; } }

            public void Add(T item)
            {
                EnsureCapacity(_count + 1);
                _array[_count++] = item;
            }

            public T[] ToArray()
            {
                if (Count == 0)
                {
                    return Arrays.Empty<T>();
                }

                var result = _array;

                if (_count < result.Length)
                {
                    result = new T[_count];
                    System.Array.Copy(_array, result, _count);
                }

                return result;
            }

            private void EnsureCapacity(int minimum)
            {
                int capacity = Capacity;

                if (minimum > capacity)
                {
                    if (capacity > 0)
                    {
                        int newCapacity = capacity * 2;
                        newCapacity = Math.Max(newCapacity, minimum);

                        if ((uint)newCapacity > (uint)Arrays.MaxLength)
                        {
                            newCapacity = Arrays.MaxLength;
                        }

                        var array = new T[newCapacity];
                        System.Array.Copy(_array, array, _count);

                        _array = array;
                    }
                    else
                    {
                        int newCapacity = Math.Max(DefaultCapacity, minimum);
                        _array = new T[newCapacity];
                    }
                }
            }
        }
    }
}
