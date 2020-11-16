using System;

class Program
{
    static void ddd()
    {
        // Read the file as one string.
        string[] lines = System.IO.File.ReadAllLines(@"map_1.txt");
 
      
        // Display the file contents by using a foreach loop.
        System.Console.WriteLine("Lines of map_1.txt = ");
        foreach (string line in lines)
        {
            // Use a tab to indent each line of the file.
            Console.WriteLine("\t" + line);
        }
        // Keep the console window open in debug mode.
        Console.WriteLine("Press any key to exit.");
        System.Console.ReadKey();
    }
}