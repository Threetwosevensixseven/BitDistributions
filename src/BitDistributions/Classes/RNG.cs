using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using freakcode.Cryptography;

namespace Threetwosevensixseven.BitDistributions.Classes
{
     /// <summary>
     /// Abstract wrapper class for <see cref="Random">System.Random</see> 
     /// and <see cref="RNGCryptoServiceProvider">System.Security.Cryptography.RNGCryptoServiceProvider</see>.
     /// Makes use of <see href="https://twitter.com/niik">Markus Olsson</see>'s 
     /// <see cref="CryptoRandom">freakcode.Cryptography.CryptoRandom</see>, 
     /// which is based on RNGCryptoServiceProvider but API-compatible (and a subclass of) Random.
     /// See <see href="https://gist.github.com/niik/1017834">GitHub</see> for more details.
     /// </summary>
     public class RNG
     {
          private readonly Random rng;

          public RNG(Engines Engine = Engines.CryptoRandom)
          {
               if (Engine == Engines.Random)
                    rng = new Random();
               else if (Engine == Engines.CryptoRandom)
                    rng = new CryptoRandom();
               else throw new NotImplementedException();
          }

          public int Next()
          {
               return rng.Next();
          }

          public int Next(int maxValue)
          {
               return rng.Next(maxValue);
          }

          public int Next(int minValue, int maxValue)
          {
               return rng.Next(maxValue, maxValue);
          }

          public enum Engines
          {
               Random,
               CryptoRandom
          }
     }
}
