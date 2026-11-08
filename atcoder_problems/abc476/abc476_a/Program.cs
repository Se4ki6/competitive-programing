using System;
using System.Runtime.CompilerServices;
using System.Security.Cryptography.X509Certificates;

internal static class Program
{
    private static void Main()
    {
        string input = Console.ReadLine();

        // var inputReverse = new char[input.Length()]();

        // for(int i=input.Length(); i > 0; i--)
        // {
        //     inputReverse.Append[i];
        // }
        // if (inputReverse[0] == "e")
        // {
        //     Console.WriteLine(input+"r");
        // }
        // else
        // {
        //     Console.WriteLine(input+"er");
        // }
        // もっとシンプルにかけたのでコメントアウト

        char lastChar = input[input.Length()];
        if (lastChar == "e")
        {
            Console.WriteLine(input + "r");
        }
        else
        {
            Console.WriteLine(input + "er");
        }
    }
    // eがあったら削除、なかったらerそのまま（pythonのsplit構文）
}
