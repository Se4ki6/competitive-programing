using System;

internal static class Program
{
    private static void Main()
    {
        // 入力の受け取り方はわからなかったのでAIに聞いた
        string[] first = Console.ReadLine()!.Split();
        int n = int.Parse(first[0]);
        int m = int.Parse(first[1]);

        string[] candidates = new string[n];
        for (int i = 0; i < n; i++)
        {
            candidates[i] = Console.ReadLine()!;
        }

        string[] votes = new string[m];
        for (int i = 0; i < m; i++)
        {
            votes[i] = Console.ReadLine()!;
        }


    }
}
