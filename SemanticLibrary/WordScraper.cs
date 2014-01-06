using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace SemanticLibrary
{
	public static class WordScraper
	{
		private static char[] p = new char[1] { (char)247 };
		private static string ps = null;

		/// <summary>
		/// Removes non-meaningful words and breaks text into paragraphs.
		/// </summary>
		/// <param name="text"></param>
		/// <param name="wordCount"></param>
		/// <returns></returns>
		public static List<Paragraph> ScrapeToParagraphs(string text, out int wordCount)
		{
			wordCount = 0;
			ps = new string(p);
			List<string> rawParagraphs = new List<string>();
			try
			{
				MatchCollection mc = RegWordCount.Matches(text);
				wordCount = mc.Count;
				text = para.Replace(text, ps); //replace decimal CRLF with a acsii 01 for later splitting
				text = crlftab.Replace(text, " "); //replace remaining line breaks with simple space
				//remove all right ' for finding it's 
				text = rsquote.Replace(text, "'");
				//pronouns, helper verbs (to be forms, prepositions, a, an, the, conjunctions

				//text = nonwords.Replace(text, "");
				text = non1.Replace(text, "");
				text = non2.Replace(text, "");
				text = non3.Replace(text, "");
				text = non4.Replace(text, "");
				text = non5.Replace(text, "");
				text = non6.Replace(text, "");
				text = non7.Replace(text, "");
				text = non8.Replace(text, "");
				text = non9.Replace(text, "");
				text = non10.Replace(text, "");
				text = non11.Replace(text, "");
				text = non12.Replace(text, "");
				text = non13.Replace(text, "");
				text = non14.Replace(text, "");
				text = non15.Replace(text, "");
				text = non16.Replace(text, "");
				text = non17.Replace(text, "");
				text = non18.Replace(text, "");
				text = non19.Replace(text, "");
				//remove large pockets of whitespace and replace with single space
				//LabLogger.Instance.Write("StripToParagraphs white called text = " + text, 411, 01, LoggingCategory.All);
				text = white.Replace(text, " ");

				//LabLogger.Instance.Write("StripToParagraphs split called text = " + text, 411, 01, LoggingCategory.All);
				string[] paras = text.Split(p);

				rawParagraphs = new List<string>(paras);
			}
			catch (Exception e)
			{
				throw e;
			}
			
			/* now process rawParagraphs into dense paragraphs
			 * 
			 * remove all non-essential words: pronouns, helper verbs (to be forms, propositions, a, an, the, conjunctions
			 * 
			 * split text into sentences ( . ? ! ) and into words stemming each and adding
			 * to sentences, stems, and termCount (total occurence)
			*/

			List<Paragraph> paragraphs = new List<Paragraph>();
			Stemmer stemmer = new Stemmer();
			foreach (string rawpara in rawParagraphs)
			{
				if (rawpara.Trim(trim).Length > 2) //ignore empty paragraphs
				{
					List<Sentence> sentlist = new List<Sentence>();

					MatchCollection mcsent = sentdiv.Matches(rawpara);
					string[] sents = new string[mcsent.Count];
					int i = 0;
					foreach (Match ms in mcsent)
					{
						sents[i] = ms.Value;
						i++;
					}

					foreach (string s in sents)
					{
						if (s.Trim(trim).Length > 2)
						{
							//look for title case phrase and add to titles collection???
							string fxs = ProcessSpecialCase(s);

							//add individual words from this sentence
							List<Word> words = new List<Word>();
							MatchCollection mc = WordReg.Matches(fxs);
							foreach (Match m in mc)
							{
								string word = m.Value.Trim(trim);
								if (word.Length > 2 || WordIsUncommon(word))	 //all two and one letter words are ignored
								{
									string stem = (word.Length > 2) ? stemmer.Porter.stemTerm(word) : word; //only stem if more than 2 characters
									Word term = new Word { Text = word, Stem = stem };
									words.Add(term);
								}
							}
							if (words.Count > 0) //only add if we have words in the sentence
							{
								sentlist.Add(new Sentence { Words = words });
							}
						}
					}
					if (sentlist.Count > 0) //only add paragraph if there are sentences
					{
						paragraphs.Add(new Paragraph { Sentences = sentlist });
					}
				}
			}
			return paragraphs;
		}

		private static string ProcessSpecialCase(string s)
		{
			//check each word for camelCase and TitleCase and add to special caselist split them into individual words
			//if so, add the split words to the sentence just after the combined word 
			MatchCollection mcc = regcase.Matches(s);
			foreach (Match m in mcc)
			{
				//split into individual words and add to sentence
				MatchCollection smc = splitcase.Matches(m.Value);
				foreach (Match inm in smc)
				{
					s += " " + inm.Value;
				}
			}
			return s;
		}

		private static bool WordIsUncommon(string word)
		{
			word = word.Trim();
			if (word.Length < 2) return false;
			return (regtwo.IsMatch(word) == false); //a match means it is common
		}

		public static Regex RegWordCount = new Regex(@"\b\S+?\b", RegexOptions.Compiled);
		private static Regex regtwo = new Regex(@"am|an|as|at|ax|be|by|do|go|he|if|in|is|it|me|my|no|of|on|or|ox|so|to|up|us|we|a|i", (RegexOptions.IgnoreCase | RegexOptions.Compiled));
		private static Regex sentdiv = new Regex(@"(\.{0,1}[a-z0-9].*?(?=(\.|\?|!|$)(\]|\)|\s|$)))", (RegexOptions.IgnoreCase | RegexOptions.Compiled));
		private static Regex para = new Regex("\r\n\r\n|\n\n", (RegexOptions.IgnoreCase | RegexOptions.Compiled));
		private static Regex crlftab = new Regex("(\r\n|\t)|(\n|\t)", (RegexOptions.IgnoreCase | RegexOptions.Compiled));
		private static Regex lsquote = new Regex(@"\u2018", (RegexOptions.IgnoreCase | RegexOptions.Compiled));
		private static Regex rsquote = new Regex(@"\u2019", (RegexOptions.IgnoreCase | RegexOptions.Compiled));
		private static Regex ldquote = new Regex(@"\u201C", (RegexOptions.IgnoreCase | RegexOptions.Compiled));
		private static Regex rdquote = new Regex(@"\u201D", (RegexOptions.IgnoreCase | RegexOptions.Compiled));

		private static Regex non1 = new Regex(@"\b(a|aboard|about|above|absent|according\sto|across|after|against|ago|ahead\sof|ain't|all|along|alongside)\b", (RegexOptions.IgnoreCase | RegexOptions.Compiled));
		private static Regex non2 = new Regex(@"\b(also|although|am|amid|amidst|among|amongst|an|and|anti|anybody|anyone|anything|apart|apart\sfrom|are|been)\b", (RegexOptions.IgnoreCase | RegexOptions.Compiled));
		private static Regex non3 = new Regex(@"\b(aren't|around|as|as\sfar\sas|as\ssoon\sas|as\swell\sas|aside|at|atop|away|be|because|because\sof|before)\b", (RegexOptions.IgnoreCase | RegexOptions.Compiled));
		private static Regex non4 = new Regex(@"\b(behind|below|beneath|beside|besides|between|betwixt|beyond|but|by|by\smeans\sof|by\sthe\stime|can|cannot)\b", (RegexOptions.IgnoreCase | RegexOptions.Compiled));
		private static Regex non5 = new Regex(@"\b(circa|close\sto|com|concerning|considering|could|couldn't|cum|'d|despite|did|didn't|do|does|doesn't|don't)\b", (RegexOptions.IgnoreCase | RegexOptions.Compiled));
		private static Regex non6 = new Regex(@"\b(down|due\sto|during|each_other|'em|even\sif|even\sthough|ever|every|every\stime|everybody|everyone)\b", (RegexOptions.IgnoreCase | RegexOptions.Compiled));
		private static Regex non7 = new Regex(@"\b(everything|except|far\sfrom|few|first\stime|following|for|from|get|got|had|hadn't|has|hasn't|have)\b", (RegexOptions.IgnoreCase | RegexOptions.Compiled));
		private static Regex non8 = new Regex(@"\b(haven't|he|hence|her|here|hers|herself|him|himself|his|how|i|if|in|in\saccordance\swith|in\saddition\sto|in\scase)\b", (RegexOptions.IgnoreCase | RegexOptions.Compiled));
		private static Regex non9 = new Regex(@"\b(in\sfront\sof|in\slieu\sof|in\splace\sof|in\sspite\sof|in\sthe\sevent\sthat|in\sto|inside|inside\sof)\b", (RegexOptions.IgnoreCase | RegexOptions.Compiled));
		private static Regex non10 = new Regex(@"\b(instead\sof|into|is|isn't|it|itself|just\sin\scase|like|'ll|lots|may|me|mid|might|mightn't|mine|more|most)\b", (RegexOptions.IgnoreCase | RegexOptions.Compiled));
		private static Regex non11 = new Regex(@"\b(must|mustn't|myself|near|near\sto|nearest|new|no|no\sone|nobody|none|not|nothing|notwithstanding|now\sthat|of)\b", (RegexOptions.IgnoreCase | RegexOptions.Compiled));
		private static Regex non12 = new Regex(@"\b(off|on|on\sbehalf\sof|on\sto|on\stop\sof|once|one|one\sanother|only\sif|onto|opposite|or|org|other|our|any)\b", (RegexOptions.IgnoreCase | RegexOptions.Compiled));
		private static Regex non13 = new Regex(@"\b(ours|ourselves|out|out\sof|outside|outside\sof|over|past|per|plenty|plus|prior\sto|qua|re|'re|really|set)\b", (RegexOptions.IgnoreCase | RegexOptions.Compiled));
		private static Regex non14 = new Regex(@"\b(regarding|round|'s|said|sans|save|say|says|shall|shan't|she|should|shouldn't|since|so|somebody|its|only)\b", (RegexOptions.IgnoreCase | RegexOptions.Compiled));
		private static Regex non15 = new Regex(@"\b(someone|something|than|that|the|thee|their|theirs|them|themselves|there|these|they|thine|this|thou)\b", (RegexOptions.IgnoreCase | RegexOptions.Compiled));
		private static Regex non16 = new Regex(@"\b(though|through|throughout|till|to|toward|towards|under|underneath|unless|unlike|until|unto|up|upon|using|even)\b", (RegexOptions.IgnoreCase | RegexOptions.Compiled));
		private static Regex non17 = new Regex(@"\b(us|'ve|versus|via|was|wasn't|we|were|weren't|what|when|whenever|where|whereas|whether\sor\snot|things)\b", (RegexOptions.IgnoreCase | RegexOptions.Compiled));
		private static Regex non18 = new Regex(@"\b(which|while|who|whoever|whom|why|will|with|with\sregard\sto|withal|within|without|won't|would|wouldn't|mere)\b", (RegexOptions.IgnoreCase | RegexOptions.Compiled));
		private static Regex non19 = new Regex(@"\b(ya|ye|yes|you|your|yours|yourself)\b", (RegexOptions.IgnoreCase | RegexOptions.Compiled));

		private static Regex white = new Regex(@"\s+", RegexOptions.Compiled);

		public static Regex WordReg = new Regex(@"(?<=(\s|^))[A-Z0-9\.\-]+", (RegexOptions.Compiled | RegexOptions.IgnoreCase));
		private static Regex regcase = new Regex(@"(?<=(\s|^))"
															+ @"[A-Za-z]{1}[a-z\.0-9]+?[A-Z]{1}[A-Za-z\.0-9]+?"
															+ @"(?=(\s|$))", (RegexOptions.Compiled));

		private static Regex splitcase = new Regex(@"(^|[A-Z\.])[a-z0-9]+?(?=(\.|[A-Z]|$))", (RegexOptions.Compiled));

		private static char[] trim = new char[11] { (char)247, ' ', '\t', '\n', '\r', '[', ']', '(', ')', '{', '}' };	//trim word

	}
}
