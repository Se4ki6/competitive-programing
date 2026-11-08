using System;

internal static class Program
{
    private static void Main()
    {
        // 入力の受け取り方はわからなかったのでAIに聞いた
        string[] first = Console.ReadLine()!.Split();
        int n = int.Parse(first[0]);
        int m = int.Parse(first[1]);

        // 候補者リストの作成
        string[] candidates = new string[n];
        for (int i = 0; i < n; i++)
        {
            candidates[i] = Console.ReadLine()!;
        }

        // 投票リストの作成
        string[] votes = new string[m];
        for (int i = 0; i < m; i++)
        {
            votes[i] = Console.ReadLine()!;
        }

        // 辞書詰め
        var candidateDict = new Dictionary<string, int>();
        for (int i = 0; i < candidates.Count() - 1; i++)
        {
            string candidate = candidates[i];
            candidateDict[candidate] = i;
        }

        // 候補者ごとの票数
        int[] candidateNumList = new int[n];

        // 投票リストから一致する候補者をカウント
        for (int i = 0; i < votes.Count() - 1; i++)
        {
            int candidateIndex = candidateDict[votes[i]];
            candidateNumList.Append(candidateIndex);
        }

        //ギブアップ
        //foreach



    }
}
