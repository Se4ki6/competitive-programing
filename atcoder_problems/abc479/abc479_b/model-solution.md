# 模範解答とフィードバック

- 問題: https://atcoder.jp/contests/abc479/tasks/abc479_b
- 公式解説: https://atcoder.jp/contests/abc479/editorial/27194

## 参照元と実装の由来

公式解説の1文字ずつ調べる方法を基にしたC#実装です。公式掲載のサンプルコードはC++です。

## 解法の要点と正しさ

`currentCount` に、現在位置まで続いている雨の日数を記録します。文字が `R` なら1増やし、`S` なら連続が途切れるので0に戻します。各文字を処理した後に `maxCount` を更新します。すべての連続区間はその末尾の文字を処理した時点で `currentCount` に数えられるため、最大値を取り続ければ最長の連続日数が得られます。

## 計算量

時間計算量は O(N)、追加領域は O(1) です。

## C#の実装

```csharp
using System;

internal static class Program
{
    private static void Main()
    {
        int n = int.Parse(Console.ReadLine()!);
        string input = Console.ReadLine()!;

        int currentCount = 0;
        int maxCount = 0;

        for (int i = 0; i < n; i++)
        {
            if (input[i] == 'R')
            {
                currentCount++;
            }
            else
            {
                currentCount = 0;
            }

            if (currentCount > maxCount)
            {
                maxCount = currentCount;
            }
        }

        Console.WriteLine(maxCount);
    }
}
```

## 自分の解答へのフィードバック

`CurrentCount` を現在の連続数として使い、`maxCount` と分けて管理できています。`R` 以外の文字で現在の連続数を0に戻し、最大値を更新する方針も正しいです。コンパイルするには、`Console.ReadLine()` の結果を整数に変換するため `int.Parse` が必要です。

## 次に解くときの着眼点

「今の連続数」と「ここまでの最大値」を別の変数にすると、連続が途切れても最大値を保てます。
