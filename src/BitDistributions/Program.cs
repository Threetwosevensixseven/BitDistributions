using Threetwosevensixseven.BitDistributions.Classes;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace Threetwosevensixseven.BitDistributions
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
                    while (d.Variance > 0 && !d.Abandon);
                    if (!d.Abandon)
                    {
                         do
                         {
                              d.Statistics();
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
                         while (!d.FinishedStatistics);
                         if (d.Improvement)
                         {
                              string file = d.MatrixVariance.ToString("D3") + "-" + d.Minimum.ToString("D2") + "-" + d.Maximum.ToString("D2") + ".txt";
                              string dir = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "tables");
                              string fn = Path.Combine(dir, file);
                              if (!File.Exists(fn))
                              {
                                   if (!Directory.Exists(dir))
                                        Directory.CreateDirectory(dir);
                                   var sb = new StringBuilder();
                                   sb.AppendLine(d.ToString());
                                   sb.AppendLine(d.RenderTable(true));
                                   sb.AppendLine(d.RenderTable(false));
                                   File.WriteAllText(Path.Combine(dir, file), sb.ToString());
                              }
                              d = new Distribution();
                              GC.Collect();
                              GC.Collect();
                         }
                    }
               }
               while (d.BestMatrixVariance > 0);
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
