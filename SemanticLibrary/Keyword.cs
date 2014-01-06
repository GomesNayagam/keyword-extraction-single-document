using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SemanticLibrary
{
	public class KeywordAnalysis
	{
		public string Content { get; set; }
		public int WordCount { get; set; }
		public IEnumerable<Keyword> Keywords { get; set; }
		public List<Paragraph> Paragraphs { get; set; }
		public IEnumerable<Title> Titles { get; set; }
	}

	public class Keyword
	{
		public string Word { get; set; }
		public decimal Rank { get; set; }
	}

	public class Word
	{
		public string Text { get; set; }
		public string Stem { get; set; }
	}

	public class Sentence
	{
		public List<Word> Words { get; set; }

		public Sentence() { Words = new List<Word>(); }
	}

	public class Paragraph
	{
		public List<Sentence> Sentences { get; set; }

		public Paragraph() { Sentences = new List<Sentence>(); }
	}
}
