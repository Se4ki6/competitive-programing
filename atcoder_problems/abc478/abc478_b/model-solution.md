# 模範解答とフィードバック

## 参照元と実装の由来

- [ABC 478 B 公式解説](https://atcoder.jp/contests/abc478/editorial/27104)
- 以下の C# コードは、公式解説の全探索をもとに作成したものです。公式掲載のコードではありません。

## 解法の要点と正しさ

トッピングの番号を `i < j < k` として、3 種類の組み合わせをすべて調べます。`i + j + k <= v` なら、嬉しさの合計を暫定の最大値と比べて更新します。

異なる 3 個の番号は小さい順に並べると必ず `i < j < k` と一通りに表せるので、すべての組み合わせを漏れなく調べられます。条件を満たす組み合わせの嬉しさの最大値を記録するため、最後の値が答えです。

番号は 1 からですが、配列の添字は 0 からなので、`i` 番の嬉しさは `w[i - 1]` です。

## 計算量

3 重ループによる時間計算量は O(N³)、入力を格納する領域は O(N) です。N は最大 100 なので十分間に合います。

## C# の実装

```csharp
using System;

internal static class Program
{
    private static void Main()
    {
        string[] first = Console.ReadLine()!.Split();
        int n = int.Parse(first[0]);
        int v = int.Parse(first[1]);
        int[] w = Array.ConvertAll(Console.ReadLine()!.Split(), int.Parse);

        int answer = 0;
        for (int i = 1; i <= n; i++)
        {
            for (int j = i + 1; j <= n; j++)
            {
                for (int k = j + 1; k <= n; k++)
                {
                    if (i + j + k <= v)
                    {
                        int happiness = w[i - 1] + w[j - 1] + w[k - 1];
                        answer = Math.Max(answer, happiness);
                    }
                }
            }
        }

        Console.WriteLine(answer);
    }
}
```

## 自分の解答へのフィードバック

- `j = i + 1`、`k = j + 1` という開始位置は正しく、重複のない組み合わせを考えられています。
- 各番号は `n` 以下にし、予算 `v` は 3 番号の合計に対して判定します。内側のループでは `i++` ではなく `k++` で進めます。
- `Split()` は `string[]` を返すので `int[]` には直接代入できません。また、`var[]` は使えず、`var input` または `string[] input` と書きます。
- 嬉しさは 1 行に並んでいます。上の `Array.ConvertAll` で分割した各文字列を整数に変換できます。`Append` は元の配列を更新せず、新たな列を返すため、そのまま呼ぶだけでは配列に追加されません。
- 嬉しさの合計は最大 3,000,000 なので、今回は `int` で扱えます。

## 次に解くときの着眼点

制約を見て全探索が間に合うかを考え、要素の番号の範囲と、組み合わせ全体に対する条件を分けて整理しましょう。

## 別解：最大値の前計算で O(N²)

[具体例を使った詳しい解説はこちら](prefix-maximum-guide.md)。

[公式解説](https://atcoder.jp/contests/abc478/editorial/26499)は 3 種類を 2 種類と 1 種類に分ける O(N²) の方法も紹介しています。次のコードは同じ計算量を実現する別の考え方として、前からの最大値を使って作成した C# 実装です。

`best[x]` に、1 番から x 番までの嬉しさの最大値をあらかじめ保存します。2 番目と 3 番目の番号を `j < k` と固定したとき、残る番号 `i` は `i < j` と `i + j + k <= v` の両方を満たす必要があります。したがって、`i` の上限は `min(j - 1, v - j - k)` です。

上限が 1 以上なら、`best[上限]` を使って、条件を満たす `i` のうち嬉しさが最大のものを一度で選べます。どの 3 種類の組み合わせも一意に `i < j < k` と並べられ、各 `j, k` について最適な `i` を調べるので、答えを漏らしません。

前計算は O(N)、`j` と `k` の 2 重ループは O(N²) です。全体の時間計算量は **O(N²)**、配列に使う空間計算量は **O(N)** です。ただしこの問題の N ≤ 100 では、上の O(N³) の方法も十分高速で、考え方はそちらの方が簡単です。

```csharp
using System;

internal static class Program
{
    private static void Main()
    {
        string[] first = Console.ReadLine()!.Split();
        int n = int.Parse(first[0]);
        int v = int.Parse(first[1]);
        int[] w = Array.ConvertAll(Console.ReadLine()!.Split(), int.Parse);

        // best[x] は 1 番から x 番までの嬉しさの最大値。
        int[] best = new int[n + 1];
        for (int x = 1; x <= n; x++)
        {
            best[x] = Math.Max(best[x - 1], w[x - 1]);
        }

        int answer = 0;
        for (int j = 2; j <= n; j++)
        {
            for (int k = j + 1; k <= n; k++)
            {
                int limit = Math.Min(j - 1, v - j - k);
                if (limit >= 1)
                {
                    int happiness = best[limit] + w[j - 1] + w[k - 1];
                    answer = Math.Max(answer, happiness);
                }
            }
        }

        Console.WriteLine(answer);
    }
}
```
