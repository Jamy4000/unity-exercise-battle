using System;
using System.Collections.Generic;

public static class ListExtensions
{
    public static void Resize<T>(this List<T> list, int size, T element = default(T))
    {
        int count = list.Count;

        if (size < count)
        {
            list.RemoveRange(size, count - size);
        }
        else if (size > count)
        {
            if (size > list.Capacity)   // Optimization
                list.Capacity = size;

            for (int i = 0; i < size - count; i++)
                list.Add(element);
        }
    }

    public static int EnsureCapacity<T>(this List<T> list, int capacity)
    {
        if (list.Capacity >= capacity)
        {
            return list.Capacity;
        }

        list.Capacity = capacity;
        return capacity;
    }

    public static T GetRandomElement<T>(this List<T> list)
    {
        return list[UnityEngine.Random.Range(0, list.Count)];
    }

    public static T GetRandomElement<T>(this List<T> list, System.Random random)
    {
        return list[random.Next(0, list.Count)];
    }

    public static void Shuffle<T>(this IList<T> list)
    {
        int n = list.Count;
        while (n > 1)
        {
            n--;
            int k = UnityEngine.Random.Range(0, n + 1);
            T value = list[k];
            list[k] = list[n];
            list[n] = value;
        }
    }

    public static void Shuffle<T>(this IList<T> list, System.Random random)
    {
        int n = list.Count;
        while (n > 1)
        {
            n--;
            int k = random.Next(0, n + 1);
            (list[n], list[k]) = (list[k], list[n]);
        }
    }

    public static bool TryFindIndex<T>(this List<T> list, T item, out int foundIndex)
    {
        foundIndex = list.IndexOf(item);
        return foundIndex >= 0;
    }

    public static void EraseWithLastSwap<T>(this List<T> list, int index)
    {
        int end = list.Count - 1;
        list[index] = list[end];
        list.RemoveAt(end);
    }

    public static bool TryRemoveWithLastSwap<T>(this List<T> list, T item)
    {
        if (list.TryFindIndex(item, out int found))
        {
            list.EraseWithLastSwap(found);
            return true;
        }
        return false;
    }

    public static bool Any<T>(this List<T> list, Func<T, bool> predicate)
    {

        for (int i = 0; i < list.Count; i++)
        {
            if (predicate(list[i]))
                return true;
        }
        return false;
    }

    public static T OrderedFirst<T>(this List<T> source, IComparer<T> comparer)
    {
        List<T> _tempList = SortedList<T>(ref source, comparer);
        return _tempList[0];
    }

    public static T OrderedLast<T>(this List<T> source, IComparer<T> comparer)
    {
        List<T> _tempList = SortedList<T>(ref source, comparer);
        return _tempList[_tempList.Count - 1];
    }

    private static List<T> SortedList<T>(ref List<T> unorderedList, IComparer<T> comparer)
    {
        unorderedList.Sort(comparer);
        return unorderedList;
    }
}
