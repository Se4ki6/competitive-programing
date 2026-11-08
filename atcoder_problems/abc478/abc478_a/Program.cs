using System;

internal static class Program
{
    private static void Main()
    {
        string[] tokens = Console.ReadLine()!
            .Split(' ', StringSplitOptions.RemoveEmptyEntries);
        int numPeople = int.Parse(tokens[0]);
        int numGrape = int.Parse(tokens[1]);

        int quotient = numGrape / numPeople;
        int remainder = numGrape % numPeople;

        for (int i = numPeople; i > 0; i--)
        {
            if (remainder > 0)
            {
                Console.WriteLine(quotient + 1);
                remainder--;
            }
            else
            {
                Console.WriteLine(quotient);
            }
        }
    }
}
