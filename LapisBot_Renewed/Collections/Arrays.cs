namespace LapisBot_Renewed.Collections
{
    internal static class Arrays
    {
        public static readonly int MaxLength = 0X7FFFFFC7;

        public static T[] Empty<T>()
        {
            return EmptyArray<T>.Value;
        }

        private static class EmptyArray<T>
        {
            public static readonly T[] Value = new T[0];
        }
    }
}
