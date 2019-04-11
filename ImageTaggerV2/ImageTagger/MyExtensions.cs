using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Xml;
using System.Xml.Serialization;

public static class MyExtensions
{
    public static void Add<T>(this ObservableCollection<T> col, IEnumerable<T> enumerable)
    {
        if(enumerable != null)
        {
            foreach (var item in enumerable)
            {
                col.Add(item);
            }
        }
    }

    public static void AddRange<T>(this HashSet<T> col, IEnumerable<T> enumerable)
    {
        if (enumerable != null)
        {
            foreach (var item in enumerable)
            {
                if (!col.Contains(item))
                    col.Add(item);
            }
        }
    }
    
//-----------------------------------------------------

public static void Shuffle<T>(this IList<T> list)
    {
        int n = list.Count;
        while (n > 1)
        {
            n--;
            int k = ThreadSafeRandom.ThisThreadsRandom.Next(n + 1);
            T value = list[k];
            list[k] = list[n];
            list[n] = value;
        }
    }
    public static T Random<T>(this IList<T> list)
    {
        int k = ThreadSafeRandom.ThisThreadsRandom.Next(list.Count - 1);
        return list[k];
    }

    //-----------------------------------------------
    /// <summary>
    /// both min and max inclusive
    /// </summary>
    public static T Clamp<T>(this T val, T min, T max) where T : System.IComparable<T>
    {
        if (val.CompareTo(min) < 0) return min;
        else if (val.CompareTo(max) > 0) return max;
        else return val;
    }
}