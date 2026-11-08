# ABC476 A: Appender — 模範解答と比較

- 問題: https://atcoder.jp/contests/abc476/tasks/abc476_a
- 公式解説: https://atcoder.jp/contests/abc476/editorial/25815
- 以下は公式解説の方針をC#で実装したコードで、公式掲載のC#コードではない。

## 解法の要点

文字列の最後の1文字を確認する。`e` なら `r` を、それ以外なら `er` を末尾に追加して出力する。

文字列の長さを `N` とすると、文字列の連結に `O(N)` の時間と領域を使う。

## 模範解答

```csharp
using System;

internal static class Program
{
    private static void Main()
    {
        string input = Console.ReadLine()!;
        char lastChar = input[input.Length - 1];

        string suffix = lastChar == 'e' ? "r" : "er";
        Console.WriteLine(input + suffix);
    }
}
```

## 自分の解答との差

- 文字列を反転して末尾を調べようとした着想は、末尾の文字に注目する点では合っている。ただし、この問題は `input[input.Length - 1]` で末尾を直接取得できるため、反転用配列は不要。
- `input.Length` はプロパティなので、`input.Length()` ではなく `input.Length` と書く。
- 最後の有効なインデックスは `Length - 1`。`input[input.Length]` は範囲外になる。
- `lastChar` は `char` なので、文字との比較は文字列の `"e"` ではなく、文字リテラルの `'e'` を使う。
- `inputReverse` はコメントアウトした宣言の中にしかなく、実際のコードからは参照できない。
- `Console.ReadLine()` は `null` の可能性がある型なので、入力が存在する前提を `!` で示した。

## 次に解くときの着眼点

文字列の一部だけが必要なら、まずインデックスで直接取れるか考える。C#の文字列は `s[s.Length - 1]` で末尾の1文字を参照できる。
## 次に解くときの着眼点
