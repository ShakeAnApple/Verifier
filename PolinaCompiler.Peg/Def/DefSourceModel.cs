using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PolinaCompiler.Peg;

namespace PolinaCompiler.Peg.Def.TextModel
{
	
	class identifier
	{
		public string @string;
	}
	
	class regex
	{
		public string @string;
	}
	
	class chars
	{
		public string @string;
	}
	
	class check
	{
		public string @string;
		public trivial trivial;
	}
	
	class not
	{
		public string @string;
		public trivial trivial;
	}
	
	class group
	{
		public string[] @strings;
		public exprsSeq exprsSeq;
	}
	
	class num
	{
		public string @string;
	}
	
	class quantorSpec
	{
		public num[] nums;
		public string[] @strings;
	}
	
	class quantor
	{
		public string[] @strings;
		public quantorSpec quantorSpec;
	}
	
	class number
	{
		public trivial trivial;
		public quantor quantor;
	}
	
	class alternatives
	{
		public altItem[] altItems;
		public string[] @strings;
	}
	
	class altItem
	{
		public number number;
		public trivial trivial;
	}
	
	class trivial
	{
		public identifier identifier;
		public regex regex;
		public chars chars;
		public check check;
		public not not;
		public group group;
	}
	
	class exprsSeq
	{
		public exprItem[] exprItems;
	}
	
	class exprItem
	{
		public alternatives alternatives;
		public number number;
		public trivial trivial;
	}
	
	class nsName
	{
		public identifier[] identifiers;
		public string[] @strings;
	}
	
	class header
	{
		public string[] @strings;
		public nsName nsName;
		public identifier[] identifiers;
	}
	
	class rule
	{
		public identifier identifier;
		public string[] @strings;
		public exprsSeq exprsSeq;
	}
	
	class grammar
	{
		public header header;
		public rule[] rules;
	}
	
	class omitPattern
	{
		public string @string;
	}
	
	static class SourceGrammarMapping
	{
		static identifier MapIdentifier(StringTreeNode node)
		{
			return new identifier() {
				@string = node.Childs.Where(n => n.Childs.Count == 0).Select(n => n.Fragment.Content).FirstOrDefault(),
			};
		}
		
		static regex MapRegex(StringTreeNode node)
		{
			return new regex() {
				@string = node.Childs.Where(n => n.Childs.Count == 0).Select(n => n.Fragment.Content).FirstOrDefault(),
			};
		}
		
		static chars MapChars(StringTreeNode node)
		{
			return new chars() {
				@string = node.Childs.Where(n => n.Childs.Count == 0).Select(n => n.Fragment.Content).FirstOrDefault(),
			};
		}
		
		static check MapCheck(StringTreeNode node)
		{
			return new check() {
				@string = node.Childs.Where(n => n.Childs.Count == 0).Select(n => n.Fragment.Content).FirstOrDefault(),
				trivial = node.Childs.Where(n => n.Rule.Name == "trivial").Select(n => MapTrivial(n)).FirstOrDefault(),
			};
		}
		
		static not MapNot(StringTreeNode node)
		{
			return new not() {
				@string = node.Childs.Where(n => n.Childs.Count == 0).Select(n => n.Fragment.Content).FirstOrDefault(),
				trivial = node.Childs.Where(n => n.Rule.Name == "trivial").Select(n => MapTrivial(n)).FirstOrDefault(),
			};
		}
		
		static group MapGroup(StringTreeNode node)
		{
			return new group() {
				@strings = node.Childs.Where(n => n.Childs.Count == 0).Select(n => n.Fragment.Content).ToArray(),
				exprsSeq = node.Childs.Where(n => n.Rule.Name == "exprsSeq").Select(n => MapExprsSeq(n)).FirstOrDefault(),
			};
		}
		
		static num MapNum(StringTreeNode node)
		{
			return new num() {
				@string = node.Childs.Where(n => n.Childs.Count == 0).Select(n => n.Fragment.Content).FirstOrDefault(),
			};
		}
		
		static quantorSpec MapQuantorSpec(StringTreeNode node)
		{
			return new quantorSpec() {
				nums = node.Childs.Where(n => n.Rule.Name == "num").Select(n => MapNum(n)).ToArray(),
				@strings = node.Childs.Where(n => n.Childs.Count == 0).Select(n => n.Fragment.Content).ToArray(),
			};
		}
		
		static quantor MapQuantor(StringTreeNode node)
		{
			return new quantor() {
				@strings = node.Childs.Where(n => n.Childs.Count == 0).Select(n => n.Fragment.Content).ToArray(),
				quantorSpec = node.Childs.Where(n => n.Rule.Name == "quantorSpec").Select(n => MapQuantorSpec(n)).FirstOrDefault(),
			};
		}
		
		static number MapNumber(StringTreeNode node)
		{
			return new number() {
				trivial = node.Childs.Where(n => n.Rule.Name == "trivial").Select(n => MapTrivial(n)).FirstOrDefault(),
				quantor = node.Childs.Where(n => n.Rule.Name == "quantor").Select(n => MapQuantor(n)).FirstOrDefault(),
			};
		}
		
		static alternatives MapAlternatives(StringTreeNode node)
		{
			return new alternatives() {
				altItems = node.Childs.Where(n => n.Rule.Name == "altItem").Select(n => MapAltItem(n)).ToArray(),
				@strings = node.Childs.Where(n => n.Childs.Count == 0).Select(n => n.Fragment.Content).ToArray(),
			};
		}
		
		static altItem MapAltItem(StringTreeNode node)
		{
			return new altItem() {
				number = node.Childs.Where(n => n.Rule.Name == "number").Select(n => MapNumber(n)).FirstOrDefault(),
				trivial = node.Childs.Where(n => n.Rule.Name == "trivial").Select(n => MapTrivial(n)).FirstOrDefault(),
			};
		}
		
		static trivial MapTrivial(StringTreeNode node)
		{
			return new trivial() {
				identifier = node.Childs.Where(n => n.Rule.Name == "identifier").Select(n => MapIdentifier(n)).FirstOrDefault(),
				regex = node.Childs.Where(n => n.Rule.Name == "regex").Select(n => MapRegex(n)).FirstOrDefault(),
				chars = node.Childs.Where(n => n.Rule.Name == "chars").Select(n => MapChars(n)).FirstOrDefault(),
				check = node.Childs.Where(n => n.Rule.Name == "check").Select(n => MapCheck(n)).FirstOrDefault(),
				not = node.Childs.Where(n => n.Rule.Name == "not").Select(n => MapNot(n)).FirstOrDefault(),
				group = node.Childs.Where(n => n.Rule.Name == "group").Select(n => MapGroup(n)).FirstOrDefault(),
			};
		}
		
		static exprsSeq MapExprsSeq(StringTreeNode node)
		{
			return new exprsSeq() {
				exprItems = node.Childs.Where(n => n.Rule.Name == "exprItem").Select(n => MapExprItem(n)).ToArray(),
			};
		}
		
		static exprItem MapExprItem(StringTreeNode node)
		{
			return new exprItem() {
				alternatives = node.Childs.Where(n => n.Rule.Name == "alternatives").Select(n => MapAlternatives(n)).FirstOrDefault(),
				number = node.Childs.Where(n => n.Rule.Name == "number").Select(n => MapNumber(n)).FirstOrDefault(),
				trivial = node.Childs.Where(n => n.Rule.Name == "trivial").Select(n => MapTrivial(n)).FirstOrDefault(),
			};
		}
		
		static nsName MapNsName(StringTreeNode node)
		{
			return new nsName() {
				identifiers = node.Childs.Where(n => n.Rule.Name == "identifier").Select(n => MapIdentifier(n)).ToArray(),
				@strings = node.Childs.Where(n => n.Childs.Count == 0).Select(n => n.Fragment.Content).ToArray(),
			};
		}
		
		static header MapHeader(StringTreeNode node)
		{
			return new header() {
				@strings = node.Childs.Where(n => n.Childs.Count == 0).Select(n => n.Fragment.Content).ToArray(),
				nsName = node.Childs.Where(n => n.Rule.Name == "nsName").Select(n => MapNsName(n)).FirstOrDefault(),
				identifiers = node.Childs.Where(n => n.Rule.Name == "identifier").Select(n => MapIdentifier(n)).ToArray(),
			};
		}
		
		static rule MapRule(StringTreeNode node)
		{
			return new rule() {
				identifier = node.Childs.Where(n => n.Rule.Name == "identifier").Select(n => MapIdentifier(n)).FirstOrDefault(),
				@strings = node.Childs.Where(n => n.Childs.Count == 0).Select(n => n.Fragment.Content).ToArray(),
				exprsSeq = node.Childs.Where(n => n.Rule.Name == "exprsSeq").Select(n => MapExprsSeq(n)).FirstOrDefault(),
			};
		}
		
		public static grammar MapGrammar(StringTreeNode node)
		{
			return new grammar() {
				header = node.Childs.Where(n => n.Rule.Name == "header").Select(n => MapHeader(n)).FirstOrDefault(),
				rules = node.Childs.Where(n => n.Rule.Name == "rule").Select(n => MapRule(n)).ToArray(),
			};
		}
		
		static omitPattern MapOmitPattern(StringTreeNode node)
		{
			return new omitPattern() {
				@string = node.Childs.Where(n => n.Childs.Count == 0).Select(n => n.Fragment.Content).FirstOrDefault(),
			};
		}
		
	}
	
	
}
