using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace SemanticLibrary
{
	public class Title
	{
		public string Text { get; set; }
		public int Count { get; set; }
	}

	public static class TitleExtractor
	{
		public static IEnumerable<Title> Extract(string content)
		{
			Dictionary<string, int> titles = new Dictionary<string, int>();
			MatchCollection mc = regtitle.Matches(content);
			foreach (Match m in mc)
			{
				if (!titles.ContainsKey(m.Value)) titles.Add(m.Value, 0);
				titles[m.Value]++;
			}
			IEnumerable<Title> list = from n in titles select new Title { Text = n.Key, Count = n.Value };
			return list;
		}

		private static Regex regtitle = new Regex(@"(?<=(\s|^))"
															 + @"[A-Z\.0-9][A-Za-z0-9]*?[\.\-]*[A-Za-z0-9]+?"
															 + @"((\s[a-z]{1,3}){0,2}\s[A-Z\.0-9][A-Za-z0-9]*?[\.\-]*[A-Za-z0-9]+?){1,4}"
															 + @"(?=(\.|\?|!|\s|$))", (RegexOptions.Compiled | RegexOptions.Multiline));
	}
}
