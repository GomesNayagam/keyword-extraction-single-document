//The following algorithm is based on
//		Keyword Extraction from a Single Document
//		using Word Co-occurrence Statistical Information
//		(found at http://ymatsuo.com/papers/ijait04.pdf)


/*	The paragraphs KeyTerm collection is complete. Process extraction algorithm here.
* 
* The algorithm used here was found in a paper published by the
* International Journal of Artificial Intelligence Tools 
* Copyright 2003 (C) World Scientific Publishing Company
* Authored by Yutaka Matsuo of the National Institute of Advanced Industrial 
* Science and Technology in Tokyo, Japan and by Mitsuru Ishizuka of the University of Tokyo
* and published either in July or December of 2003 (unsure)
* 
* G = set of most frequent terms	  --> termsG = a subset of probabilityTerms where probability is above the average
* 
* Pg = sum of the total number of terms in sentences where             Dictionary<string, decimal> termPg
*      g appears divided by the total number of terms in the document
* 
* nw = total number of terms in sentences where w appears	 --> termNw
* 
* Fwg = sentence count where w and g occur divided by the total number of sentences
*       = termFwg is Dictionary<string, Dictionary<string, decimal>>
* 
* X2(w) is the rank for a give word w
* 
* X2(w) = sum of Z for each g in G (g = term in G or most frequent terms)
*         EXCEPT for the MAX g -- to create what the authors call robustness
* 
* D = nw * Pg
* 
* T = (Fwg - D)
* 
* Z = (T * T) / D
* 
* X2(w) = calculate Z for each g for w and sum the total
*         EXCEPT for the MAX g -- to create what the authors call robustness
* 
*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace SemanticLibrary
{
	public class KeywordAnalyzer
	{
		public KeywordAnalysis Analyze(string content)
		{
			KeywordAnalysis analysis = new KeywordAnalysis { Content = content };
			int wordCount = 0;
			var titles = TitleExtractor.Extract(content);
			var paragraphs = WordScraper.ScrapeToParagraphs(content, out wordCount);

			//flatten list of words
			List<Word> allWords = new List<Word>();
			paragraphs.ForEach(p => p.Sentences.ForEach(s => allWords.AddRange(s.Words)));

			analysis.WordCount = wordCount;
			analysis.Paragraphs = paragraphs;
			analysis.Titles = titles;

			int termTotal = 0;
			
			//run through each sentence and grab two and three word segments and add them to the termCount
			Dictionary<string, int> termOccurrenceCounts = GetWordTermOccurence(paragraphs);

			Dictionary<string, int> termNw = new Dictionary<string, int>();
			Dictionary<string, decimal> termsX2 = new Dictionary<string, decimal>();
			
			//this gets us termsG for frequent terms, and an initialized termsX2
			SortedDictionary<decimal, string> termsG = SortTermsIntoProbabilities(termOccurrenceCounts, ref termsX2, ref termTotal);

			//now we have to fill termPg and termNw with values
			Dictionary<string, decimal> termPg = FillTermPgNwCollections(paragraphs, termsG, ref termNw, ref termTotal);

			//now we have to fill the termFgw collection
			Dictionary<string, Dictionary<string, decimal>> termFwg = FillTermFwgCollection(paragraphs, termsG);		

			string[] terms = new string[termsG.Count];
			termsG.Values.CopyTo(terms, 0);  //gives terms array where last term is the MAX g in G
			foreach (string w in terms)
			{
				decimal sumZ = 0;
				for (int i = 0; i < terms.Length - 1; i++) //do calcs for all but MAX
				{
					string g = terms[i];
					if (w != g) //skip where on the diagonal
					{
						int nw = termNw[w];
						decimal Pg = termPg[g];
						decimal D = nw * Pg;
						if (D != 0.0m)
						{
							decimal Fwg = termFwg[w][terms[i]];
							decimal T = Fwg - D;
							decimal Z = (T * T) / D;
							sumZ += Z;
						}
					}
				}
				termsX2[w] = sumZ;
			}

			SortedDictionary<decimal, string> sortedX2 = new SortedDictionary<decimal, string>();
			foreach (KeyValuePair<string, decimal> pair in termsX2)
			{
				decimal x2 = pair.Value;
				while (sortedX2.ContainsKey(x2))
				{
					x2 = x2 - 0.00001m;
				}
				sortedX2.Add(x2, pair.Key);
			}

			//now get simple array of values as lowest to highest X2 terms
			string[] x2Terms = new string[sortedX2.Count];
			sortedX2.Values.CopyTo(x2Terms, 0);

			Dictionary<string, decimal> preres = new Dictionary<string, decimal>();
			for (int i = x2Terms.Length - 1; i > -1; i--)
			{
				string stemterm = x2Terms[i];
				string term = GetTermFromStemTerm(allWords, stemterm);
				if (!preres.ContainsKey(term))
					preres.Add(term, termsX2[x2Terms[i]]);
				else
					preres[term] = termsX2[x2Terms[i]];
			}

			//post process title case and caseSpecial words
			//titles = new Dictionary<string, int>();
			//caselist = new Dictionary<string, int>();
			//caseListWords -- so we don't have to regex slit the caselist words
			//for now, case list is going to be left alone since we split those and added them to the sentence end for ranking
			SortedDictionary<decimal, string> tsort = new SortedDictionary<decimal, string>();
			foreach (var title in titles)
			{
				decimal tscore = 0.0m;
				MatchCollection mc = WordScraper.WordReg.Matches(title.Text);
				foreach (Match m in mc)
				{
					if (preres.ContainsKey(m.Value))
					{
						tscore += preres[m.Value];
					}
				}
				while (tsort.ContainsKey(tscore))
				{
					tscore = tscore - 0.00001m;
				}
				tsort.Add(tscore, title.Text);
			}

			//mix tsort with preres and return the top 50
			foreach (KeyValuePair<string, decimal> pre in preres)
			{
				decimal x = pre.Value;
				while (tsort.ContainsKey(x))
				{
					x = x - 0.00001m;
				}
				tsort.Add(x, pre.Key);
			}

			Dictionary<string, decimal> result = new Dictionary<string, decimal>();
			string[] resultTerms = new string[tsort.Count];
			tsort.Values.CopyTo(resultTerms, 0);
			decimal[] resultValues = new decimal[tsort.Count];
			tsort.Keys.CopyTo(resultValues, 0);
			int max = 0;
			for (int i = resultTerms.Length - 1; i > -1; i--)
			{
				if (!result.ContainsKey(resultTerms[i]))
				{
					result.Add(resultTerms[i], resultValues[i]);
				}
				//if (max > 50) break;
				max++;
			}

			analysis.Keywords = from n in result select new Keyword { Word = n.Key, Rank = n.Value };
			return analysis;
		}

		private string GetTermFromStemTerm(List<Word> allWords, string term)
		{
			if (term.IndexOf(" ") > -1)
			{
				string[] terms = term.Split(' ');
				string[] words = new string[terms.Length];
				for (int i = 0; i < terms.Length; i++)
				{
					words[i] = GetTermFromStem(allWords, terms[i]);
				}
				string retval = string.Join(" ", words);
				return retval;
			}
			else
			{
				return GetTermFromStem(allWords, term);
			}
		}

		private string GetTermFromStem(List<Word> allWords, string stem)
		{
			var words = (from n in allWords where n.Stem == stem select n).ToList();
			if (words.Count > 0)
			{
				var w = from n in words
						  group n by n.Text into grp
						  select new { Text = grp.Key, Count = grp.Select(x => x.Text).Distinct().Count() };

				var top = (from n in w orderby n.Count descending select n).First();

				return top.Text;
			}
			else
				return string.Empty;

			//if (stems.ContainsKey(stem))
			//{
			//   Dictionary<string, int> words = stems[stem];
			//   string word = string.Empty;
			//   int count = 0;
			//   foreach (KeyValuePair<string, int> pair in words)
			//   {
			//      if (pair.Value > count)
			//      {
			//         word = pair.Key;
			//         count = pair.Value;
			//      }
			//   }
			//   return word;
			//}
			//else
			//   return string.Empty;
		}

		private Dictionary<string, Dictionary<string, decimal>> FillTermFwgCollection(List<Paragraph> paragraphs, 
			SortedDictionary<decimal, string> termsG)
		{
			//termFwg
			// * Fwg = sentence count where w and g occur divided by the total number of sentences (sentenceCount)
			// *       = termFwg is Dictionary<string, Dictionary<string, decimal>>
			Dictionary<string, Dictionary<string, decimal>> termFwg = new Dictionary<string, Dictionary<string, decimal>>();
			int sentenceCount = (from n in paragraphs select n.Sentences).Count();

			string[] terms = new string[termsG.Count];
			foreach (string w in termsG.Values.ToArray())
			{
				foreach (KeyValuePair<decimal, string> pair in termsG)
				{
					string g = pair.Value;
					if (g != w)
					{
						int sentCountWG = 0;
						foreach (var paragraph in paragraphs)
						{
							foreach (var sentence in paragraph.Sentences)
							{
								if (TermsCoOccur(sentence, w, g)) sentCountWG++;
							}
						}
						decimal Fwg = sentCountWG > 0 ? sentCountWG / (decimal)sentenceCount : 0.0m;
						if (!termFwg.ContainsKey(w))
							termFwg.Add(w, new Dictionary<string, decimal>()); //add if not there yet
						termFwg[w].Add(g, Fwg);
					}
				}
			}
			return termFwg;
		}

		private bool TermsCoOccur(Sentence sentence, string w, string g)
		{
			if (TermInSentence(sentence, w) && TermInSentence(sentence, g))
				return true;
			else
				return false;
		}

		private bool TermInSentence(Sentence sentence, string term)
		{
			bool found = false;
			//if term appears in this sentence, count the terms (words + 2 and 3 word terms)
			if (term.IndexOf(" ") > -1)
			{
				string[] termWords = term.Split(' ');
				for (int i = 0; i < sentence.Words.Count; i++)
				{
					var t = sentence.Words[i];
					if (termWords.Length == 2 && i > 2)
					{
						var t1 = sentence.Words[i - 1];
						if (termWords[0] == t1.Stem && termWords[1] == t.Stem)
						{
							found = true;
							break;
						}
					}
					else if (termWords.Length == 3 && i > 3)
					{
						var t1 = sentence.Words[i - 1];
						var t2 = sentence.Words[i - 2];
						if (termWords[0] == t2.Stem && termWords[1] == t1.Stem && termWords[2] == t.Stem)
						{
							found = true;
							break;
						}
					}
				}
			}
			else
			{
				for (int i = 0; i < sentence.Words.Count; i++)
				{
					var t = sentence.Words[i];
					if (t.Stem == term)
					{
						found = true;
						break;
					}
				}
			}
			return found;
		}

		private Dictionary<string, decimal> FillTermPgNwCollections(List<Paragraph> paragraphs,
			SortedDictionary<decimal, string> termsG, ref Dictionary<string, int> termNw, ref int termTotal)
		{
			//termPg
			// * Pg = sum of the total number of terms in sentences where  
			// *      g appears divided by the total number of terms in the document (termTotal)
			// total number of terms in sentence = word count + # of 2 and 3 word combos = termsInSentencesForTerm
			Dictionary<string, decimal> termPg = new Dictionary<string, decimal>();

			foreach (KeyValuePair<decimal, string> pair in termsG)
			{
				string term = pair.Value;
				int termsInSentencesForTerm = 0;
				foreach (var paragraph in paragraphs)
				{
					foreach (var sentence in paragraph.Sentences)
					{
						bool found = false;
						//if term appears in this sentence, count the terms (words + 2 and 3 word terms)
						if (term.IndexOf(" ") > -1)
						{
							string[] termWords = term.Split(' ');
							for (int i = 0; i < sentence.Words.Count; i++)
							{
								var t = sentence.Words[i];
								if (termWords.Length == 2 && i > 2)
								{
									var t1 = sentence.Words[i - 1];
									if (termWords[0] == t1.Stem && termWords[1] == t.Stem)
									{
										found = true;
										break;
									}
								}
								else if (termWords.Length == 3 && i > 3)
								{
									var t1 = sentence.Words[i - 1];
									var t2 = sentence.Words[i - 2];
									if (termWords[0] == t2.Stem && termWords[1] == t1.Stem && termWords[2] == t.Stem)
									{
										found = true;
										break;
									}
								}
							}
						}
						else
						{
							for (int i = 0; i < sentence.Words.Count; i++)
							{
								var t = sentence.Words[i];
								if (t.Stem == term)
								{
									found = true;
									break;
								}
							}
						}
						if (found)
						{
							//now get terms count (words + 2 and 3 word terms) and increment termsInSentencesForTerm
							termsInSentencesForTerm += sentence.Words.Count;
							if (sentence.Words.Count > 2) termsInSentencesForTerm += sentence.Words.Count - 2; //all three word terms
							if (sentence.Words.Count > 1) termsInSentencesForTerm += sentence.Words.Count - 1; //all two word terms
						}
					}
				}
				termNw.Add(term, termsInSentencesForTerm);
				decimal pg = termsInSentencesForTerm / (decimal)termTotal;
				termPg.Add(term, pg);
			} //end foreach in termsG
			return termPg;
		}

		private SortedDictionary<decimal, string> SortTermsIntoProbabilities(Dictionary<string, int> counts, 
			ref Dictionary<string, decimal> termsX2, ref int termTotal)
		{
			SortedDictionary<decimal, string> probabilityTerms = new SortedDictionary<decimal, string>();
			SortedDictionary<decimal, string> termsG = new SortedDictionary<decimal, string>();
			
			foreach (KeyValuePair<string, int> pair in counts)
			{
				termTotal += pair.Value;
			}
			decimal total = (decimal)termTotal;
			decimal probTotal = 0; //to be used for calculating the average probability
			foreach (KeyValuePair<string, int> pair in counts)
			{
				decimal prob = pair.Value / total;
				probTotal += prob;
				while (probabilityTerms.ContainsKey(prob))
				{
					prob = prob - 0.00001m; //offset by the slightest amount to get unique key
				}
				probabilityTerms.Add(prob, pair.Key);
			}
			decimal probAvg = counts.Count > 0 ? probTotal / counts.Count : 0;

			//only take the top 10% up to the top 30 terms and if top 10% is less than 10 then take up to 5
			int toptenCount = counts.Count / 10;
			if (toptenCount > 30)
				toptenCount = 30;
			else if (toptenCount < 10)
				toptenCount = 5;

			if (toptenCount > counts.Count) toptenCount = counts.Count; //just in case there are so few

			decimal[] ptkey = new decimal[probabilityTerms.Count];
			probabilityTerms.Keys.CopyTo(ptkey, 0);

			for (int i = ptkey.Length - 1; i > ptkey.Length - toptenCount - 1; i--)
			{
				decimal key = ptkey[i];
				string val = probabilityTerms[key];
				termsG.Add(key, val);
				termsX2.Add(val, 0); //initializes the list for storing X2 calculation results to be sorted later
			}
			return termsG;
		}

		private Dictionary<string, int> GetWordTermOccurence(List<Paragraph> paragraphs)
		{
			Dictionary<string, int> counts = new Dictionary<string, int>();
			foreach (var p in paragraphs)
			{
				foreach (var s in p.Sentences)
				{
					for (int i = 0; i < s.Words.Count; i++)
					{
						Word w = s.Words[i];
						CountTerm(counts, w.Stem);
						if (i > 0) //we can have a two word phrase
						{
							Word tm1 = s.Words[i - 1];
							string term = tm1.Stem + " " + w.Stem;
							CountTerm(counts, term);
						}
						if (i > 1) //we can have a three word phrase
						{
							Word tm1 = s.Words[i - 1];
							Word tm2 = s.Words[i - 2];
							string term = tm2.Stem + " " + tm1.Stem + " " + w.Stem;
							CountTerm(counts, term);
						}
					}
				}
			}
			return counts;
		}

		private void CountTerm(Dictionary<string, int> counts, string stem)
		{
			if (counts.ContainsKey(stem))
				counts[stem]++;
			else
				counts.Add(stem, 1);
		}
	}
}
