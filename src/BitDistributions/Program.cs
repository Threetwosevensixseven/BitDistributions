using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace BitDistributions
{
     class Program
     {

          static void Main(string[] args)
          {
               var d = new Distribution();
               Console.CursorVisible = false;
               var versionInfo = FileVersionInfo.GetVersionInfo(Assembly.GetEntryAssembly().Location);
               Console.Clear();
               Console.WriteLine("BitDistributions v" + versionInfo.ProductVersion);
               Console.SetCursorPosition(0, 2);
               Console.WriteLine(d.ToString());
               Console.WriteLine("Calculating");
               int frame = 0;
               DateTime time = DateTime.Now;
               do
               {
                    d.Swap();
                    Console.SetCursorPosition(0, 2);
                    Console.WriteLine(d.ToString());
                    Console.WriteLine("Calculating" + new string('.', frame) + new string(' ', 10));
                    if ((DateTime.Now - time).TotalMilliseconds >= 500)
                    {
                         frame++;
                         if (frame > 3) frame = 0;
                         time = DateTime.Now;
                    }
               }
               while (d.Variance > 0);
               Console.Clear();
               Console.WriteLine("BitDistributions v" + versionInfo.ProductVersion);
               Console.SetCursorPosition(0, 2);
               Console.WriteLine(d.ToString());
               Console.WriteLine(d.RenderTable(true));
               Console.WriteLine(d.RenderTable(false));
               Console.WriteLine("Press ENTER to exit...");
               Console.CursorVisible = true;
               Console.ReadLine();
          }
     }
}
