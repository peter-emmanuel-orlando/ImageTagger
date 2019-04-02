using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;

public static class MyExtensions
{
    public static void Add<T>(this ObservableCollection<T> col, IEnumerable<T> enumerable)
    {
        foreach( var item in enumerable)
        {
            col.Add(item);
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
}