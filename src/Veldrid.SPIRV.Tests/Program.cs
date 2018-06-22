using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Veldrid;
using Veldrid.SPIRV;
using Veldrid.SPIRV.Tests;
using Xunit;

class Program
{
    static int Main(string[] args)
    {
        List<string> newArgs = new List<string>(args);
        newArgs.Insert(0, typeof(Program).Assembly.Location);
        int returnCode = Xunit.ConsoleClient.Program.Main(newArgs.ToArray());
        Console.WriteLine("Tests finished. Press any key to exit.");
        if (!Console.IsInputRedirected)
        {
            Console.ReadKey(true);
        }
        return returnCode;
    }
}
