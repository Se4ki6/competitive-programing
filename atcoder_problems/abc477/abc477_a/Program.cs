using System;

internal static class Program
{
    private static void Main()
    {
        var nextColor = new Dictionary<char, char>
        {
            ['B'] = 'Y',
            ['Y'] = 'R',
            ['R'] = 'B'
        };

        string color = Console.ReadLine()!;

        char Next(char currentColor)
        {
            return nextColor[currentColor];
        }

        Console.WriteLine(Next(color[0]));
    }
}

/*
Before: 色を数値に変換してから、次の色を求める版

using System;
using System.Collections.Generic;

internal static class Program
{
    private static void Main()
    {
        var colorToNumber = new Dictionary<char, int>
        {
            ['B'] = 1,
            ['Y'] = 2,
            ['R'] = 3
        };

        var numberToColor = new Dictionary<int, char>
        {
            [1] = 'B',
            [2] = 'Y',
            [3] = 'R'
        };

        char Next(char color)
        {
            int nextNumber = colorToNumber[color] % 3 + 1;
            return numberToColor[nextNumber];
        }

        char currentColor = Console.ReadLine()![0];
        Console.WriteLine(Next(currentColor));
    }
}
*/
