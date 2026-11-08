using System;
using System.Net;
using System.Reflection.Metadata.Ecma335;

internal static class Program
{
    private static void Main()
    {
        int n = int.Parse(Console.ReadLine()!);
        string input = Console.ReadLine()!;

        int CurrentCount = 0;
        int maxCount = 0;

        for (int i = 0; i < n; i++)
        {
            if (input[i] != 'R')
            {
                CurrentCount = 0;
                continue;
            }
            CurrentCount++;
            if (CurrentCount > maxCount)
            {
                maxCount = CurrentCount;
            }
        }
        Console.WriteLine(maxCount);
    }
}
