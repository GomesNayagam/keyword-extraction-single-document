using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using SemanticLibrary;

namespace kw
{
	class Program
	{
		static void Main(string[] args)
		{
			//Note: you will have to supply your own text files
			string gettys = File.ReadAllText(@"c:\temp\gettys.txt");
			string gu = File.ReadAllText(@"c:\temp\scott.txt");

			KeywordAnalyzer ka = new KeywordAnalyzer();

			var g = ka.Analyze(gettys);

			var s = ka.Analyze(gu);

			Console.WriteLine("gettys");
			foreach (var key in g.Keywords)
			{
				Console.WriteLine("   key: {0}, rank: {1}", key.Word, key.Rank);
			}

			Console.WriteLine("gu");
			foreach (var key in s.Keywords)
			{
				Console.WriteLine("   key: {0}, rank: {1}", key.Word, key.Rank);
			}

			Console.WriteLine("gettys");
			var gty = (from n in g.Keywords select n).Take(10);
			foreach (var key in gty)
			{
				Console.WriteLine("   {0}", key.Word);
			}

			Console.WriteLine("gu");
			var gus = (from n in s.Keywords select n).Take(10);
			foreach (var key in gus)
			{
				Console.WriteLine("   {0}", key.Word);
			}
			Console.ReadLine();
		}
	}
}
