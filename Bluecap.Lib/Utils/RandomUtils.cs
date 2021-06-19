using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bluecap.Lib.Utils
{
    public static class RandomUtils
    {
        // Function to return the next random number
        static int GetNum(ArrayList v)
        {

            // Size of the vector
            int n = v.Count;

            Random rand = new Random();

            // Make sure the number is within
            // the index range
            int index = (rand.Next() % n);

            // Get random number from the vector
            int num = (int)v[index];

            // Remove the number from the vector
            v[index] = (int)v[n - 1];
            v.Remove(v[n - 1]);

            // Return the removed number
            return num;
        }

        // Function to generate n
        // non-repeating random numbers
        public static IEnumerable<int> GenerateRandomPermutation(int n)
        {
            ArrayList v = new ArrayList(n);
            IEnumerable<int> permutation = new List<int>();
            // Fill the vector with the values
            // 1, 2, 3, ..., n
            for (int i = 0; i < n; i++)
                v.Add(i + 1);

            // While vector has elements get a
            // random number from the vector
            // and print it
            while (v.Count > 0)
            {
                //Console.Write(getNum(v) + " ");
                permutation = permutation.Append(GetNum(v));
            }
            return permutation;
        }
    }
}
