using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PolinaCompiler.Peg;

namespace Verifier.LtlAutomatonParser.TextModel
{
	
	class automaton
	{
		public StringTreeNode _rawTreeNode;
		public string[] @strings;
		public state[] states;
	}
	
	class state
	{
		public StringTreeNode _rawTreeNode;
		public stateName stateName;
		public string[] @strings;
		public transition[] transitions;
		public errTransition errTransition;
	}
	
	class stateName
	{
		public StringTreeNode _rawTreeNode;
		public identifier identifier;
	}
	
	class errTransition
	{
		public StringTreeNode _rawTreeNode;
		public string @string;
	}
	
	class transition
	{
		public StringTreeNode _rawTreeNode;
		public string[] @strings;
		public condition condition;
		public stateName stateName;
	}
	
	class condition
	{
		public StringTreeNode _rawTreeNode;
		public exprSeq exprSeq;
	}
	
	class not
	{
		public StringTreeNode _rawTreeNode;
		public string @string;
	}
	
	class boolOperator
	{
		public StringTreeNode _rawTreeNode;
		public string[] @strings;
	}
	
	class exprSeq
	{
		public StringTreeNode _rawTreeNode;
		public exprItem[] exprItems;
		public boolOperator[] boolOperators;
	}
	
	class exprItem
	{
		public StringTreeNode _rawTreeNode;
		public not not;
		public literal literal;
		public identifier identifier;
		public exprGroup exprGroup;
	}
	
	class exprGroup
	{
		public StringTreeNode _rawTreeNode;
		public string[] @strings;
		public exprSeq exprSeq;
	}
	
	class literal
	{
		public StringTreeNode _rawTreeNode;
		public string @string;
	}
	
	class identifier
	{
		public StringTreeNode _rawTreeNode;
		public string @string;
	}
	
	class keyword
	{
		public StringTreeNode _rawTreeNode;
		public string[] @strings;
	}
	
	class sep
	{
		public StringTreeNode _rawTreeNode;
		public string @string;
	}
	
	class omitPattern
	{
		public StringTreeNode _rawTreeNode;
		public string @string;
	}
	
	static class SourceAutomatonMapping
	{
		public static automaton MapAutomaton(StringTreeNode node)
		{
			return new automaton() {
				_rawTreeNode = node,
				@strings = node.Childs.Where(n => n.Childs.Count == 0).Select(n => n.Fragment.Content).ToArray(),
				states = node.Childs.Where(n => n.Rule.Name == "state").Select(n => MapState(n)).ToArray(),
			};
		}
		
		static state MapState(StringTreeNode node)
		{
			return new state() {
				_rawTreeNode = node,
				stateName = node.Childs.Where(n => n.Rule.Name == "stateName").Select(n => MapStateName(n)).FirstOrDefault(),
				@strings = node.Childs.Where(n => n.Childs.Count == 0).Select(n => n.Fragment.Content).ToArray(),
				transitions = node.Childs.Where(n => n.Rule.Name == "transition").Select(n => MapTransition(n)).ToArray(),
				errTransition = node.Childs.Where(n => n.Rule.Name == "errTransition").Select(n => MapErrTransition(n)).FirstOrDefault(),
			};
		}
		
		static stateName MapStateName(StringTreeNode node)
		{
			return new stateName() {
				_rawTreeNode = node,
				identifier = node.Childs.Where(n => n.Rule.Name == "identifier").Select(n => MapIdentifier(n)).FirstOrDefault(),
			};
		}
		
		static errTransition MapErrTransition(StringTreeNode node)
		{
			return new errTransition() {
				_rawTreeNode = node,
				@string = node.Childs.Where(n => n.Childs.Count == 0).Select(n => n.Fragment.Content).FirstOrDefault(),
			};
		}
		
		static transition MapTransition(StringTreeNode node)
		{
			return new transition() {
				_rawTreeNode = node,
				@strings = node.Childs.Where(n => n.Childs.Count == 0).Select(n => n.Fragment.Content).ToArray(),
				condition = node.Childs.Where(n => n.Rule.Name == "condition").Select(n => MapCondition(n)).FirstOrDefault(),
				stateName = node.Childs.Where(n => n.Rule.Name == "stateName").Select(n => MapStateName(n)).FirstOrDefault(),
			};
		}
		
		static condition MapCondition(StringTreeNode node)
		{
			return new condition() {
				_rawTreeNode = node,
				exprSeq = node.Childs.Where(n => n.Rule.Name == "exprSeq").Select(n => MapExprSeq(n)).FirstOrDefault(),
			};
		}
		
		static not MapNot(StringTreeNode node)
		{
			return new not() {
				_rawTreeNode = node,
				@string = node.Childs.Where(n => n.Childs.Count == 0).Select(n => n.Fragment.Content).FirstOrDefault(),
			};
		}
		
		static boolOperator MapBoolOperator(StringTreeNode node)
		{
			return new boolOperator() {
				_rawTreeNode = node,
				@strings = node.Childs.Where(n => n.Childs.Count == 0).Select(n => n.Fragment.Content).ToArray(),
			};
		}
		
		static exprSeq MapExprSeq(StringTreeNode node)
		{
			return new exprSeq() {
				_rawTreeNode = node,
				exprItems = node.Childs.Where(n => n.Rule.Name == "exprItem").Select(n => MapExprItem(n)).ToArray(),
				boolOperators = node.Childs.Where(n => n.Rule.Name == "boolOperator").Select(n => MapBoolOperator(n)).ToArray(),
			};
		}
		
		static exprItem MapExprItem(StringTreeNode node)
		{
			return new exprItem() {
				_rawTreeNode = node,
				not = node.Childs.Where(n => n.Rule.Name == "not").Select(n => MapNot(n)).FirstOrDefault(),
				literal = node.Childs.Where(n => n.Rule.Name == "literal").Select(n => MapLiteral(n)).FirstOrDefault(),
				identifier = node.Childs.Where(n => n.Rule.Name == "identifier").Select(n => MapIdentifier(n)).FirstOrDefault(),
				exprGroup = node.Childs.Where(n => n.Rule.Name == "exprGroup").Select(n => MapExprGroup(n)).FirstOrDefault(),
			};
		}
		
		static exprGroup MapExprGroup(StringTreeNode node)
		{
			return new exprGroup() {
				_rawTreeNode = node,
				@strings = node.Childs.Where(n => n.Childs.Count == 0).Select(n => n.Fragment.Content).ToArray(),
				exprSeq = node.Childs.Where(n => n.Rule.Name == "exprSeq").Select(n => MapExprSeq(n)).FirstOrDefault(),
			};
		}
		
		static literal MapLiteral(StringTreeNode node)
		{
			return new literal() {
				_rawTreeNode = node,
				@string = node.Childs.Where(n => n.Childs.Count == 0).Select(n => n.Fragment.Content).FirstOrDefault(),
			};
		}
		
		static identifier MapIdentifier(StringTreeNode node)
		{
			return new identifier() {
				_rawTreeNode = node,
				@string = node.Childs.Where(n => n.Childs.Count == 0).Select(n => n.Fragment.Content).FirstOrDefault(),
			};
		}
		
		static keyword MapKeyword(StringTreeNode node)
		{
			return new keyword() {
				_rawTreeNode = node,
				@strings = node.Childs.Where(n => n.Childs.Count == 0).Select(n => n.Fragment.Content).ToArray(),
			};
		}
		
		static sep MapSep(StringTreeNode node)
		{
			return new sep() {
				_rawTreeNode = node,
				@string = node.Childs.Where(n => n.Childs.Count == 0).Select(n => n.Fragment.Content).FirstOrDefault(),
			};
		}
		
		static omitPattern MapOmitPattern(StringTreeNode node)
		{
			return new omitPattern() {
				_rawTreeNode = node,
				@string = node.Childs.Where(n => n.Childs.Count == 0).Select(n => n.Fragment.Content).FirstOrDefault(),
			};
		}
		
	}
	
	
}
