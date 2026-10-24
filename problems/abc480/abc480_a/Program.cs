using System;
using System.Linq;

internal static class Program
{
    private static void Main()
    {
        // この先AI記述
        List<char[]> grid = new()
        {
            Console.ReadLine()!.ToCharArray(),
            Console.ReadLine()!.ToCharArray()
        };

        (char ch, int row, int col) a = grid
            .SelectMany((row, r) =>
                row.Select((ch, c) => (ch: ch, row: r, col: c)))
            .First(x => x.ch == 'A');

        (char ch, int row, int col) d = grid
            .SelectMany((row, r) =>
                row.Select((ch, c) => (ch: ch, row: r, col: c)))
            .First(x => x.ch == 'D');

        // 行番号の差と列番号の差をそれぞれ絶対値にして足す。
        // 辺を共有するマス同士なら、この合計は 1 になる。
        // 例: A が (0, 0)、D が (1, 0) なら、|0 - 1| + |0 - 0| = 1 で隣接。
        // 斜めの (0, 0) と (1, 1) なら、|0 - 1| + |0 - 1| = 2 で隣接しない。
        int distance = Math.Abs(a.row - d.row) + Math.Abs(a.col - d.col);

        // distance が 1 なら "Yes"、それ以外なら "No" を出力する。
        Console.WriteLine(distance == 1 ? "Yes" : "No");
    }
}
