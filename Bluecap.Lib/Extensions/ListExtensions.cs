using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bluecap.Lib.Extensions
{
    public static class ListExtensions
    {
		public static int BinarySearch2<T>(this List<T> list, T q) where T : IComparable<T>
		{
			int res = BinarySearchFunc(list, 0, 0, list.Count - 1, q);
			return res;

		}
		private static int BinarySearchFunc<T>(List<T> list, int m, int i, int j, T q) where T : IComparable<T>
		{

			if (i > j)
				return m;

			m = (i + j) / 2;
			if (Equals(list[m], q))
				return m;
			if (q.CompareTo(list[m]) > 0)
				return BinarySearchFunc(list, m, m + 1, j, q);
			else
				return BinarySearchFunc(list, m, i, m - 1, q);

		}
	}
}
