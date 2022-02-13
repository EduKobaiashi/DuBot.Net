using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DuBot.Modules
{
    public static class Shuffler
    {
        // Knuth Shuffle O(n)
        public static void Shuffle(this Random rng, List<string[]> vetor)
        {
            int n = vetor.Count;
            while (n > 1)
            {
                int k = rng.Next(n--);
                string[] temp = vetor[n];
                vetor[n] = vetor[k];
                vetor[k] = temp;
            }
        }
    }
}
