using System;
using System.Collections.Generic;

public static class ThreadSafeRandom
{
    [ThreadStatic] private static System.Random Local;

    public static System.Random ThisThreadsRandom
    {
        get { return Local ?? (Local = new System.Random(unchecked(System.Environment.TickCount * 31 + System.Threading.Thread.CurrentThread.ManagedThreadId))); }
    }
}