using System;

internal static class Program
{
    private static void Main()
    {
        string[] input = Console.ReadLine()
            .Sprit(' ', StringSplitOptions.RemoveEmptyEntries);
        int load = input[0];
        int brick = input[1];

        int maxLoad = Math.Abs(load / brick);
        Console.WriteLine(maxLoad);
    }
}
