using System;
using System.Collections;
using System.Collections.Generic;

namespace LapisBot.Collections;

internal static class Enumerates
{
    public static IEnumerable<TResult> Cast<TResult>(this IEnumerable enumerable)
    {
        if (enumerable == null) throw new ArgumentNullException("enumerable");

        var result = enumerable as IEnumerable<TResult>;

        if (result != null) return result;

        return CastIterator<TResult>(enumerable);
    }

    public static int Count<T>(this IEnumerable<T> enumerable)
    {
        var collection = enumerable as ICollection<T>;

        if (collection != null) return collection.Count;

        var count = 0;

        using (var enumerator = enumerable.GetEnumerator())
        {
            while (enumerator.MoveNext()) ++count;
        }

        return count;
    }

    public static IEnumerable<TResult> Select<TSource, TResult>(this IEnumerable<TSource> enumerable,
        Func<TSource, TResult> func)
    {
        if (enumerable == null) throw new ArgumentNullException("enumerable");

        if (func == null) throw new ArgumentNullException("func");

        foreach (var item in enumerable) yield return func.Invoke(item);
    }

    public static IEnumerable<T> Where<T>(this IEnumerable<T> enumerable, Func<T, bool> func)
    {
        if (enumerable == null) throw new ArgumentNullException("enumerable");

        if (func == null) throw new ArgumentNullException("func");

        using (var enumerator = enumerable.GetEnumerator())
        {
            while (enumerator.MoveNext())
                if (func.Invoke(enumerator.Current))
                    yield return enumerator.Current;
        }
    }

    public static T[] ToArray<T>(this IEnumerable<T> enumerable)
    {
        if (enumerable == null) throw new ArgumentNullException("enumerable");

        var collection = enumerable as ICollection<T>;

        if (collection != null)
        {
            var count = collection.Count;

            if (count == 0) return Arrays.Empty<T>();

            var array = new T[count];
            collection.CopyTo(array, 0);
            return array;
        }

        var builder = new ArrayBuilder<T>();
        var enumerator = enumerable.GetEnumerator();

        while (enumerator.MoveNext()) builder.Add(enumerator.Current);

        return builder.ToArray();
    }

    public static List<T> ToList<T>(this IEnumerable<T> enumerable)
    {
        if (enumerable == null) throw new ArgumentNullException("enumerable");

        return new List<T>(enumerable);
    }

    public static IEnumerable<T> Skip<T>(this IEnumerable<T> enumerable, int count)
    {
        using (var enumerator = enumerable.GetEnumerator())
        {
            while (enumerator.MoveNext())
                if (--count < 0)
                    yield return enumerator.Current;
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
        var count = enumerable.Count();
        var sliceCount = count / size + (count % size == 0 ? 0 : 1);

        using (var enumerator = enumerable.GetEnumerator())
        {
            while (sliceCount-- > 0) yield return TakeIterator(enumerator, size);
        }
    }

    private static IEnumerable<T> TakeIterator<T>(IEnumerator<T> enumerator, int count)
    {
        while (enumerator.MoveNext())
        {
            yield return enumerator.Current;

            if (--count == 0) yield break;
        }
    }

    private static IEnumerable<TResult> CastIterator<TResult>(IEnumerable enumerable)
    {
        foreach (var item in enumerable) yield return (TResult)item;
    }

    private struct ArrayBuilder<T>
    {
        private const int DefaultCapacity = 4;

        public ArrayBuilder(int capacity) : this()
        {
            if (capacity > 0) Buffer = new T[capacity];
        }

        public T[] Buffer { get; private set; }

        public int Capacity => Buffer == null ? 0 : Buffer.Length;

        public int Count { get; private set; }

        public void Add(T item)
        {
            EnsureCapacity(Count + 1);
            Buffer[Count++] = item;
        }

        public T[] ToArray()
        {
            if (Count == 0) return Arrays.Empty<T>();

            var result = Buffer;

            if (Count < result.Length)
            {
                result = new T[Count];
                Array.Copy(Buffer, result, Count);
            }

            return result;
        }

        private void EnsureCapacity(int minimum)
        {
            var capacity = Capacity;

            if (minimum > capacity)
            {
                if (capacity > 0)
                {
                    var newCapacity = capacity * 2;
                    newCapacity = Math.Max(newCapacity, minimum);

                    if ((uint)newCapacity > (uint)Arrays.MaxLength) newCapacity = Arrays.MaxLength;

                    var array = new T[newCapacity];
                    Array.Copy(Buffer, array, Count);

                    Buffer = array;
                }
                else
                {
                    var newCapacity = Math.Max(DefaultCapacity, minimum);
                    Buffer = new T[newCapacity];
                }
            }
        }
    }
}