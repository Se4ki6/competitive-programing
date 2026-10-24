using System;
using System.Linq;

internal static class Program
{
    private static void Main()
    {
        // 1行目から、整数列の要素数 N を読み取る。
        int listCount = int.Parse(Console.ReadLine()!);

        // 2行目の整数列を、まだ文字列のまま読み取る。
        string rawIntList = Console.ReadLine()!;

        // 空白で分割し、各要素を int に変換して List<int> にする。
        // RemoveEmptyEntries により、連続した空白があっても空要素は無視される。
        List<int> intList = rawIntList
            .Split(' ', StringSplitOptions.RemoveEmptyEntries)
            .Select(int.Parse)
            .ToList();

        var oddList = new List<int>();

        for (int i = 0; i < listCount; i++)
        {
            if (i % 2 == 1) { continue; }

            oddList.Add(intList[i]);
        }

        int sum = oddList.Sum();

        Console.WriteLine(sum);
    }
}
