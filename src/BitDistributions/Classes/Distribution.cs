using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Threetwosevensixseven.BitDistributions.Classes
{
     public class Distribution
     {
          #region Public

          public int Variance;
          public int MatrixVariance;
          public int BestMatrixVariance;
          public int Minimum;
          public int BestMinimum;
          public int Maximum;
          public int BestMaximum;
          public bool Improvement;
          public bool Abandon;
          public bool FinishedStatistics;

          public Distribution(bool Random = true, int ZeroBit = 6)
          {
               zeroBit = ZeroBit;
               zeroBitAssigned = false;
               BestMatrixVariance = int.MaxValue;
               BestMaximum = int.MaxValue;
               Improvement = false;
               if (Random)
               {
                    table = new byte[LEN];
                    populated = 0;
                    bitBuckets = new Dictionary<int, Dictionary<int, bool>>();
                    for (int j = 0; j < BITS; j++)
                         bitBuckets.Add(j, new Dictionary<int, bool>());
                    for (int i = 0; i < LEN; i++)
                         AddToBitBuckets(i);
               }
               else
               {
                    table = INIT;
                    populated = LEN;
               }
               bitTotals = new int[BITS];
               matrix = new int[BITS, BITS];
               matrixTotals = new int[BITS];
               matrixVariances = new int[BITS];
               matrixAbsVariances = new int[BITS];
               statsHits = new double[BITS, BITS];
               statsCounts = new double[BITS, BITS];
               powers = new Dictionary<int, int>();
               for (int i = 0; i < BITS; i++)
                    powers.Add(Convert.ToInt32(Math.Pow(2, i)), i);
               Recalculate();
          }

          public bool Swap()
          {
               if (populated < LEN)
               {
                    Populate();
                    Recalculate();
                    return false;
               }
               int var = Variance;
               int sourceRow = 0, targetRow = 0;
               byte sourceVal = 0, targetVal = 0;
               bool match;
               do
               {
                    sourceRow = rng.Next(256);
                    sourceVal = table[sourceRow];
                    match = GetTarget(sourceRow, sourceVal, ref targetRow, ref targetVal);
               }
               while (!match);
               SwapValues(sourceRow, targetRow);
               Recalculate();
               if (Variance <= var)
                    return true;
               SwapValues(sourceRow, targetRow);
               Recalculate();
               return false;
          }

          public override string ToString()
          {
               var sb = new StringBuilder();
               sb.AppendLine("                       Bit7  Bit6  Bit5  Bit4  Bit3  Bit2  Bit1  Bit0      TOTAL");
               sb.AppendLine();
               sb.Append("Bit Totals:          ");
               for (int i = BITS - 1; i >= 0; i--)
                    sb.Append(bitTotals[i].ToString().PadLeft(6));
               sb.Append("     ");
               sb.AppendLine(bitTotals.Sum().ToString().PadLeft(6));
               sb.AppendLine();
               for (int i = BITS - 1; i >= 0; i--)
               {
                    sb.Append("Matrix (");
                    sb.Append(i);
                    sb.Append("):          ");
                    int bTotal = 0;
                    for (int j = BITS - 1; j >= 0; j--)
                    {
                         sb.Append(matrix[i, j].ToString().PadLeft(6));
                         bTotal += matrix[i, j];
                    }
                    sb.Append("     ");
                    sb.AppendLine(bTotal.ToString().PadLeft(6));
               }
               sb.AppendLine("                     ─────────────────────────────────────────────────────────────");
               sb.Append("Matrix Totals:       ");
               int mTotal = 0;
               for (int i = BITS - 1; i >= 0; i--)
               {
                    sb.Append(matrixTotals[i].ToString().PadLeft(6));
                    mTotal += matrixTotals[i];
               }
               sb.Append("     ");
               sb.AppendLine(mTotal.ToString().PadLeft(6));
               sb.AppendLine();
               sb.Append("Matrix Variances:    ");
               for (int i = BITS - 1; i >= 0; i--)
                    sb.Append(matrixVariances[i].ToString().PadLeft(6));
               sb.AppendLine();
               sb.AppendLine("                                                                         ╔═══════╗");
               sb.Append("Matrix Abs Variances:");
               for (int i = BITS - 1; i >= 0; i--)
                    sb.Append(matrixAbsVariances[i].ToString().PadLeft(6));
               sb.Append("    ║");
               sb.Append(Variance.ToString().PadLeft(6));
               sb.AppendLine(" ║");
               sb.AppendLine("                                                                         ╚═══════╝");
               {
                    sb.AppendLine();
                    for (int i = BITS - 1; i >= 0; i--)
                    {
                         sb.Append("Statistics (");
                         sb.Append(i);
                         sb.Append("):          ");
                         for (int j = BITS - 1; j >= 0; j--)
                         {
                              if (populated == LEN && Variance == 0 && !Abandon)
                              {
                                   double pct = statsCounts[i, j] == 0 ? 0 : (100 * statsHits[i, j] / statsCounts[i, j]);
                                   sb.Append(pct.ToString("##0.0").PadLeft(7));
                              }
                              else
                              {
                                   sb.Append("       ");
                              }
                         }
                         sb.AppendLine("    ");
                    }
               }
               FinishedStatistics = (statsCount > 5000);
               return sb.ToString();
          }

          public string RenderTable(bool PowersOfTwo)
          {
               var sb = new StringBuilder();
               int cols = BITS * 2;
               int rows = LEN / cols;
               int index = 0;
               for (int i = 0; i < rows; i++)
               {
                    string join = "";
                    sb.Append("     db ");
                    for (int j = 0; j < cols; j++)
                    {
                         sb.Append(join);
                         if (PowersOfTwo)
                         {
                              sb.Append("$");
                              sb.Append(table[index++].ToString("X2"));
                         }
                         else
                         {
                              sb.Append(powers[table[index++]]);
                         }
                         join = ", ";
                    }
                    sb.AppendLine();
               }
               return sb.ToString();
          }

          public void Statistics()
          {
               int index = rng.Next(255) + 1;
               int val = table[index];
               int bits = 0;
               for (int i = 0; i < BITS; i++)
                    if (Bit(i, index) == 1)
                         bits++;
               bits--;
               for (int i = 0; i < BITS; i++)
               {
                    if (Bit(i, val) == 1)
                         statsHits[bits, i]++;
                    if (Bit(i, index) == 1)
                         statsCounts[bits, i]++;
               }
               statsCount++;
          }

          #endregion

          #region Private

          private byte[] INIT = new byte[] { 2, 1, 2, 1, 4, 4, 2, 1, 8, 8, 2, 2, 4, 1, 8, 1, 16, 1, 16, 16, 4, 16, 16, 4,
               8, 16, 8, 2, 16, 8, 16, 16, 32, 32, 32, 2, 4, 4, 2, 2, 32, 8, 32, 1, 4, 8, 32, 32, 16, 32, 2, 32, 16, 4,
               32, 16, 16, 32, 16, 16, 8, 1, 2, 16, 64, 1, 64, 64, 64, 1, 2, 2, 64, 8, 64, 8, 8, 8, 64, 4, 16, 1, 2, 1,
               64, 1, 64, 64, 16, 64, 64, 8, 4, 8, 16, 8, 64, 32, 2, 1, 4, 32, 4, 1, 8, 32, 8, 8, 4, 1, 64, 64, 16, 1, 64,
               64, 64, 32, 32, 2, 16, 64, 2, 2, 4, 64, 2, 8, 128, 1, 128, 128, 128, 128, 2, 4, 8, 128, 128, 8, 4, 1, 2, 1,
               16, 16, 2, 1, 4, 128, 4, 16, 8, 128, 16, 8, 128, 4, 128, 2, 32, 1, 32, 32, 4, 32, 2, 128, 128, 32, 32, 8,
               4, 4, 128, 32, 32, 16, 128, 32, 32, 128, 4, 4, 128, 32, 128, 8, 4, 1, 8, 16, 128, 128, 128, 64, 64, 64, 4,
               2, 8, 128, 8, 64, 8, 4, 2, 4, 64, 1, 2, 16, 128, 64, 128, 64, 64, 16, 8, 2, 16, 128, 64, 4, 128, 1, 2, 2,
               64, 128, 128, 32, 128, 1, 64, 32, 128, 1, 4, 1, 32, 1, 2, 1, 4, 1, 128, 16, 64, 1, 32, 2, 8, 16, 32, 128 };
          private const int LEN = 256;
          private const int BITS = 8;
          private readonly byte[] table;
          private readonly int[] bitTotals;
          private readonly int[,] matrix;
          private readonly int[] matrixTotals;
          private readonly int[] matrixVariances;
          private readonly int[] matrixAbsVariances;
          private readonly double[,] statsCounts;
          private readonly double[,] statsHits;
          private readonly Dictionary<int, Dictionary<int, bool>> bitBuckets;
          private readonly Dictionary<int, int> powers;
          private readonly int zeroBit;
          private bool zeroBitAssigned;
          private int populated;
          private int stuckCount;
          private int fixCount;
          private int statsCount;
          private RNG rng = new RNG();

          private void Recalculate()
          {
               for (int i = 0; i < BITS; i++)
                    bitTotals[i] = table.Sum(v => Bit(i, v));
               for (int i = 0; i < BITS; i++)
               {
                    for (int j = 0; j < BITS; j++)
                    {
                         matrix[i, j] = 0;
                         for (int k = 0; k < LEN; k++)
                              matrix[i, j] += Bit(j, table[k]) & Bit(i, k);
                    }
               }
               int mTotal = 0;
               for (int i = 0; i < BITS; i++)
               {
                    matrixTotals[i] = 0;
                    for (int j = 0; j < BITS; j++)
                         matrixTotals[i] += matrix[j, i];
                    mTotal += matrixTotals[i];
               }
               int mAvg = mTotal / BITS;
               Variance = 0;
               for (int i = 0; i < BITS; i++)
               {
                    matrixVariances[i] = matrixTotals[i] - mAvg;
                    matrixAbsVariances[i] = Math.Abs(matrixVariances[i]);
                    Variance += matrixAbsVariances[i];
               }
               if (populated == LEN && Variance == 0)
               {
                    Minimum = matrix.Cast<int>().Min();
                    Maximum = matrix.Cast<int>().Max(m => m >= 31 ? int.MinValue : m);
                    MatrixVariance = matrix.Cast<int>().Sum(m => Math.Abs(m >= 31 ? 0 : (m - 16)));
                    Improvement = (Minimum > BestMinimum) || (Maximum < BestMaximum) || (MatrixVariance < BestMatrixVariance);
                    BestMinimum = BestMinimum > Minimum ? BestMinimum : Minimum;
                    BestMaximum = BestMaximum < Maximum ? BestMaximum : Maximum;
                    BestMatrixVariance = BestMatrixVariance > MatrixVariance ? BestMatrixVariance : MatrixVariance;
               }
               else Improvement = false;
          }

          private int Bit(int BitNo, Byte Value)
          {
               return (Value & (1 << BitNo)) != 0 ? 1 : 0;
          }

          private int Bit(int BitNo, int Value)
          {
               return (Value & (1 << BitNo)) != 0 ? 1 : 0;
          }

          private bool GetTarget(int SourceRow, int SourceVal, ref int TargetRow, ref byte TargetVal)
          {
               do
               {
                    TargetRow = rng.Next(256);
               }
               while (TargetRow == SourceRow);
               TargetVal = table[TargetRow];
               return ((SourceRow & TargetVal) == TargetVal)
                    && ((TargetRow & SourceVal) == SourceVal);
          }

          private void SwapValues(int SourceRow, int TargetRow)
          {
               byte temp = table[TargetRow];
               table[TargetRow] = table[SourceRow];
               table[SourceRow] = temp;
          }

          private void Populate()
          {
               if (populated >= LEN)
                    return;

               int count = 0;
               do
               {
                    for (int i = 0; i < BITS; i++)
                    {
                         byte val = Convert.ToByte(Math.Pow(2, i));
                         if (populated > count++)
                              continue;
                         if (bitTotals[i] >= 32)
                              continue;
                         if (!zeroBitAssigned && i == zeroBit)
                         {
                              table[0] = val;
                              populated++;
                              zeroBitAssigned = true;
                              continue;
                         }
                         var skip = rng.Next(bitBuckets[i].Count);
                         int index = bitBuckets[i].Keys.Skip(skip).FirstOrDefault();
                         if (index == 0)
                         {

                              stuckCount++;
                              if (stuckCount >= 100)
                              {
                                   Recalculate();
                                   List<int> candidates = new List<int>();
                                   do
                                   {
                                        int minBit = GetRandomMinMaxBit(false);
                                        int maxBit = GetRandomMinMaxBit(true, minBit);
                                        byte minVal = Convert.ToByte(Math.Pow(2, minBit));
                                        byte maxVal = Convert.ToByte(Math.Pow(2, maxBit));
                                        for (int j = 0; j < LEN; j++)
                                             if (((j & minVal) == minVal) && ((j & maxVal) == maxVal))
                                                  candidates.Add(j);
                                   }
                                   while (candidates.Count == 0);
                                   var remove = candidates.Skip(rng.Next(candidates.Count)).First();
                                   byte rVal = table[remove];
                                   table[remove] = 0;
                                   AddToBitBuckets(remove);
                                   populated--;
                                   stuckCount = 0;
                                   fixCount++;
                                   Recalculate();
                                   if (fixCount > 1000)
                                        Abandon = true;
                                   return;
                              }
                              continue;
                         }
                         table[index] = val;
                         RemoveFromBitBuckets(index);
                         populated++;
                         stuckCount = 0;
                         Recalculate();
                         return;
                    }
               }
               while (true);
          }

          private void AddToBitBuckets(int Value)
          {
               for (int j = 0; j < BITS; j++)
               {
                    int pow = Convert.ToInt32(Math.Pow(2, j));
                    if ((Value & pow) == pow && !bitBuckets[j].ContainsKey(Value))
                         bitBuckets[j].Add(Value, false);
               }
          }

          private void RemoveFromBitBuckets(int Value)
          {
               for (int k = 0; k < BITS; k++)
                    if (bitBuckets[k].ContainsKey(Value))
                         bitBuckets[k].Remove(Value);
          }

          private int GetRandomMinMaxBit(bool Max, int Except = -1)
          {
               int comp = Max ? bitTotals.Max() : bitTotals.Min();
               List<int> maxs = new List<int>();
               for (int j = 0; j < BITS; j++)
                    if (bitTotals[j] == comp)
                         maxs.Add(j);
               int max;
               do
               {
                    max = maxs.Skip(rng.Next(maxs.Count)).First();
               }
               while (max == Except);
               return max;
          }

          #endregion
     }
}
