using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoRank
{
	public static class Error
	{
		public static string Group(string group)
		{
			return String.Format("[AutoRank] Error: Group \"{0}\" doesn't exist.",
				group ?? String.Empty);
		}
		public static string User(string user)
		{
			return String.Format("[AutoRank] Error: User doesn't exist.",
				user ?? String.Empty);
		}
	}
}
