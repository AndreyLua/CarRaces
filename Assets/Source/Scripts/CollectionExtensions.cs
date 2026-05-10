using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.Profiling;

public static class CollectionExtensions
{
    public static void Do<TElement>(this IEnumerable<TElement> collection, Action<TElement> action)
    {
        if (collection is null)
            throw new ArgumentNullException("Input collection cannot be null");

        if (action is null)
            throw new NullReferenceException("Action cannot be null");

        foreach (TElement element in collection)
            action(element);
    }

    public static void DoWithIndex<TElement>(this IEnumerable<TElement> collection, Action<TElement, int> action)
    {
        if (collection is null)
            throw new ArgumentNullException("Input collection cannot be null");

        if (action is null)
            throw new NullReferenceException("Action cannot be null");

        int index = 0;

        foreach (TElement e in collection)
        {
            action(e, index);
            index++;
        }
    }

    public static TElement Find<TElement>(this IEnumerable<TElement> collection, Predicate<TElement> comparator)
    {
        if (collection is null)
            throw new ArgumentNullException("Input collection cannot be null");

        if (comparator is null)
            throw new NullReferenceException("Comparator cannot be null");

        foreach (TElement element in collection)
            if (comparator(element))
                return element;

        return default;
    }

    public static bool Contains<TElement>(this IEnumerable<TElement> collection, Predicate<TElement> comparator)
    {
        if (collection is null)
            throw new ArgumentNullException("Input collection cannot be null");

        if (comparator is null)
            throw new NullReferenceException("Comparator cannot be null");

        foreach (TElement element in collection)
            if (comparator(element))
                return true;

        return false;
    }

    public static bool ElementAt<TElement>(this IEnumerable<TElement> collection, int index, Predicate<TElement> comparator)
    {
        if (collection is null)
            throw new ArgumentNullException("Input collection cannot be null");

        if (comparator is null)
            throw new NullReferenceException("Comparator cannot be null");

        int i = 0;

        foreach (TElement element in collection)
            if (comparator(element))
            {
                if (i == index)
                    return true;

                index++;
            }

        return false;
    }

    public static TElement Random<TElement>(this IEnumerable<TElement> collection, Predicate<TElement> filter = null)
    {
        if (collection is null)
            throw new ArgumentNullException("Input collection cannot be null");

        if (filter != null && collection.All(p => !filter(p)))
            return default;

        int length = collection.Count();

        if (length is 0)
            return default;

        int value = UnityEngine.Random.Range(0, length);
        int index = 0;

        int iterationsCount = 0;

        while (true)
        {
            if (iterationsCount > length * 100)
                throw new StackOverflowException();

            iterationsCount++;

            if (filter != null && !filter(collection.ElementAt(index)))
                goto End;

            if (value == 0)
                return collection.ElementAt(index);
            else
                value--;

            End:

            if (index < length - 1)
                index++;
            else
                index = 0;
        }
    }

    public static int IndexOf<TElement>(this IEnumerable<TElement> collection, Predicate<TElement> comparison)
    {
        int index = 0;

        foreach (TElement e in collection)
        {
            if (comparison(e))
                return index;

            index++;
        }

        return -1;
    }

    public static TElement FindMinOrMax<TElement, TValue>(this IEnumerable<TElement> collection,
        Func<TElement, TValue> valueGetter, Func<TValue, TValue, bool> nextElementMoreSuitable, Func<TElement, bool> filter = null)
    {
        if (collection is null || collection.Count() < 1)
            return default;

        bool inited = false;
        TElement last = default;
        TValue valueLast = default;

        foreach (TElement e in collection)
        {
            if (filter != null && !filter(e))
                continue;

            if (!inited)
            {
                last = e;
                valueLast = valueGetter(e);
                inited = true;
            }
            else
            {
                TValue newValue = valueGetter(e);

                if (nextElementMoreSuitable(valueLast, newValue))
                {
                    last = e;
                    valueLast = newValue;
                }
            }
        }

        return last;
    }

    public static TElement FindMinOrMax<TElement>(this IEnumerable<TElement> collection,
        Func<TElement, TElement, bool> nextElementMoreSuitable, Func<TElement, bool> filter = null)
    {
        if (collection is null || collection.Count() < 1)
            return default;

        bool inited = false;
        TElement last = default;

        foreach (TElement e in collection)
        {
            if (filter != null && !filter(e))
                continue;

            if (!inited)
            {
                last = e;
                inited = true;
            }
            else
            {
                if (nextElementMoreSuitable(last, e))
                    last = e;
            }
        }

        return last;
    }
}
