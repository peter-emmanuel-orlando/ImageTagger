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
}