using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Search
{
    class SortedArray<T>
    {
        public T[] Array { get; private set; }
        public bool Not { get; private set; }

        public static Func<T, long> Key;
        public static Func<T, T, T> ForEquals;

        public T this[int index]
        {
            get
            {
                return Array[index];
            }
        }

        public int Length
        {
            get
            {
                return Array.Length;
            }
        }

        public SortedArray(T[] array, bool not)
        {
            Array = array;
            Not = not;
        }

        public SortedArray(T[] array)
        {
            Array = array;
            Not = false;
        }

        public void No()
        {
            Not = !Not;
        }

        public SortedArray<T> Union(SortedArray<T> array)
        {
            if (Not && array.Not)
            {
                No();
                array.No();
                var resul = Intersect(array);
                resul.No();
                return resul;
            }
            else if (Not)
                return new SortedArray<T>(Except(array.Array), true);
            else if (array.Not)
                return new SortedArray<T>(array.Except(Array), true);

            var res = new LinkedList<T>();
            int i, j;

            for (i = 0, j = 0; i < Array.Length && j < array.Length;)
            {
                long key0 = Key(Array[i]), key1 = Key(array[j]);

                if (key0 < key1)
                    res.AddLast(Array[i++]);
                else if (key1 < key0)
                    res.AddLast(array[j++]);
                else
                {
                    res.AddLast(ForEquals(Array[i++], array[j++]));
                }
            }

            for (; i < Array.Length; i++) res.AddLast(Array[i]);
            for (; j < array.Length; j++) res.AddLast(array[j]);

            return new SortedArray<T>(res.ToArray());
        }

        public SortedArray<T> Intersect(SortedArray<T> array)
        {
            if (Not && array.Not)
            {
                No();
                array.No();
                var resul = Union(array);
                resul.No();
                return resul;
            }
            else if (array.Not)
                return new SortedArray<T>(Except(array.Array), false);
            else if (Not)
                return new SortedArray<T>(array.Except(Array), false);

            var res = new LinkedList<T>();
            int i, j;

            for (i = 0, j = 0; i < Length && j < array.Length;)
            {
                long key0 = Key(Array[i]), key1 = Key(array[j]);

                if (key0 < key1) i++;
                else if (key1 < key0) j++;
                else
                {
                    res.AddLast(ForEquals(Array[i++], array[j++]));
                }
            }

            return new SortedArray<T>(res.ToArray());
        }

        public T[] Except(T[] array)
        {
            var res = new LinkedList<T>();

            int i, j;

            for (i = 0, j = 0; i < Array.Length && j < array.Length;)
            {
                long key0 = Key(Array[i]), key1 = Key(array[j]);

                if (key0 < key1) res.AddLast(Array[i++]);
                else if (key1 < key0) j++;
                else
                {
                    i++;
                    j++;
                }
            }

            for (; i < Array.Length; i++) res.AddLast(Array[i]);

            return res.ToArray();
        }

        public override string ToString()
        {
            return string.Join(" ", Array);
        }
    }
}
