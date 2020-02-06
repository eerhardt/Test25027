#nullable enable
using ClassLibrary1;
using System;
using System.IO;
using System.IO.Compression;
using System.Runtime.InteropServices;

namespace ZipProblemRepro
{
    class Program
    {
        public static void Main(string[] args)
        {
            new Class1().ConfigurationManager();
        }
    }
}