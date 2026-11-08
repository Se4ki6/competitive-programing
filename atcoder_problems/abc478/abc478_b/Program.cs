using System;
using System.Globalization;

internal static class Program
{
    private static void Main()
    {
        var[] input = Console.ReadLine()!.Split();
        int n = int.Parse(input[0]);
        int v = int.Parse(input[1]);

        int[] input2 = Console.ReadLine().Split();
        int[] wArr;
        foreach (int i in input2)
        {
            wArr.Append(input2[i]);
        }

        int wMax = 0;
        for (int i = 1; i <= v; i++)
        {
            for (int j = i + 1; 1 + j <= v; j++)
            {
                for (int k = j + 1; i + j + k <= v; i++)
                {

                }
            }
        }
    }
}
