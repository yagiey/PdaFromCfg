using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace PdaFromCfg
{
	public class SymbolList
		: IList<Symbol>
		, IEquatable<SymbolList>
		, IComparable<SymbolList>
	{
		private readonly List<Symbol> _innerList;

		public SymbolList()
		{
			_innerList = new List<Symbol>();
		}

		public SymbolList(Symbol symbol)
		{
			_innerList = new List<Symbol> { symbol };
		}

		public SymbolList(IEnumerable<Symbol> e)
		{
			_innerList = e.ToList();
		}

		public Symbol this[int index]
		{
			get => _innerList[index];
			set => _innerList[index] = value;
		}

		public int Count => _innerList.Count;

		public bool IsReadOnly => false;

		public void Add(Symbol item)
		{
			_innerList.Add(item);
		}

		public void Clear()
		{
			_innerList.Clear();
		}

		public bool Contains(Symbol item)
		{
			return _innerList.Contains(item);
		}

		public void CopyTo(Symbol[] array, int arrayIndex)
		{
			_innerList.CopyTo(array, arrayIndex);
		}

		public IEnumerator<Symbol> GetEnumerator()
		{
			return _innerList.GetEnumerator();
		}

		public int IndexOf(Symbol item)
		{
			return _innerList.IndexOf(item);
		}

		public void Insert(int index, Symbol item)
		{
			_innerList.Insert(index, item);
		}

		public bool Remove(Symbol item)
		{
			return _innerList.Remove(item);
		}

		public void RemoveAt(int index)
		{
			_innerList.RemoveAt(index);
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return ((IEnumerable)_innerList).GetEnumerator();
		}

		private static IDictionary<Symbol, int> MakeTable1(IList<Symbol> pattern)
		{
			IDictionary<Symbol, int> result = new Dictionary<Symbol, int>();
			for (int pos = 0; pos < pattern.Count; pos++)
			{
				if (result.ContainsKey(pattern[pos]))
				{
					result.Add(pattern[pos], pattern.Count - 1 - pos);
				}
				else
				{
					result[pattern[pos]] = pattern.Count - 1 - pos;
				}
			}
			return result;
		}

		private static IDictionary<int, int> MakeTable2(IList<Symbol> pattern)
		{
			IDictionary<int, int> result = new Dictionary<int, int>();

			for (int pattern_pos = 0; pattern_pos < pattern.Count; pattern_pos++)
			{
				result.Add(pattern_pos, -1);
			}

			{
				for (int tail_pos = 0; tail_pos < pattern.Count - 1; tail_pos++)
				{
					int eq_len = 0;
					while (eq_len < tail_pos && pattern[tail_pos - eq_len].Equals(pattern[pattern.Count - 1 - eq_len]))
					{

						eq_len++;
					}

					if (eq_len == 0)
					{
						continue;
					}

					if (!pattern[tail_pos - eq_len].Equals(pattern[pattern.Count - 1 - eq_len]))
					{
						result[pattern.Count - 1 - eq_len] = pattern.Count - 1 - tail_pos + eq_len;
					}
				}
			}

			{
				int tail_pos = -1;
				for (int pattern_pos = pattern.Count - 2; pattern_pos >= 0; pattern_pos--)
				{
					int eq_len = pattern.Count - 1 - pattern_pos;

					int i = 0;
					while (i < eq_len && pattern[i].Equals(pattern[pattern_pos + 1 + i]))
					{
						i++;
					}

					if (eq_len == i)
					{
						tail_pos = eq_len - 1;
					}

					if (result[pattern_pos] == -1)
					{
						if (tail_pos != -1)
						{
							result[pattern_pos] = pattern.Count - 1 - tail_pos + pattern.Count - 1 - pattern_pos;
						}
					}
				}
			}

			for (int pattern_pos = pattern.Count - 2; pattern_pos >= 0; pattern_pos--)
			{
				if (result[pattern_pos] == -1)
				{
					result[pattern_pos] = pattern.Count + (pattern.Count - 1 - pattern_pos);
				}
			}

			return result;
		}

		public IEnumerable<int> SearchAll(IList<Symbol> pattern)
		{
			int text_pos;
			int pattern_pos;

			IDictionary<Symbol, int> table1 = MakeTable1(pattern);
			IDictionary<int, int> table2 = MakeTable2(pattern);

			pattern_pos = pattern.Count - 1;

			text_pos = pattern.Count - 1;

			while (text_pos < _innerList.Count)
			{
				if (_innerList[text_pos].Equals(pattern[pattern_pos]))
				{
					if (pattern_pos == 0)
					{
						yield return text_pos;

						text_pos += pattern.Count - pattern_pos;
						pattern_pos = pattern.Count - 1;
					}
					else
					{
						text_pos--;
						pattern_pos--;
					}
				}
				else
				{
					if (!table1.ContainsKey(_innerList[text_pos]))
					{
						text_pos += Math.Max(pattern.Count - pattern_pos, table2[pattern_pos]);
					}
					else
					{
						text_pos += Math.Max(table1[_innerList[text_pos]], table2[pattern_pos]);
					}

					pattern_pos = pattern.Count - 1;
				}
			}
			yield break;
		}

		public int SearchFirst(Symbol symbol)
		{
			return SearchFirst(new List<Symbol>() { symbol });
		}

		public int SearchFirst(IList<Symbol> pattern)
		{
			int text_pos;
			int pattern_pos;

			IDictionary<Symbol, int> table1 = MakeTable1(pattern);
			IDictionary<int, int> table2 = MakeTable2(pattern);

			pattern_pos = pattern.Count - 1;

			text_pos = pattern.Count - 1;

			while (text_pos < _innerList.Count)
			{
				if (_innerList[text_pos].Equals(pattern[pattern_pos]))
				{
					if (pattern_pos == 0)
					{
						return text_pos;
					}

					text_pos--;
					pattern_pos--;

				}
				else
				{
					if (!table1.ContainsKey(_innerList[text_pos]))
					{
						text_pos += Math.Max(pattern.Count - pattern_pos, table2[pattern_pos]);
					}
					else
					{
						text_pos += Math.Max(table1[_innerList[text_pos]], table2[pattern_pos]);
					}

					pattern_pos = pattern.Count - 1;
				}
			}
			return -1;
		}

		public void ReplaceAll(Symbol before, Symbol after)
		{
			IEnumerable<int> positions = SearchAll(new Symbol[] { before });
			foreach (int p in positions.Reverse())
			{
				_innerList.RemoveAt(p);
				_innerList.Insert(p, after);
			}
		}

		public void ReplaceAll(IList<Symbol> before, IList<Symbol> after)
		{
			IEnumerable<int> positions = SearchAll(before);
			foreach (int p in positions.Reverse())
			{
				for (int i = before.Count - 1; 0 <= i; i--)
				{
					_innerList.RemoveAt(p + i);
				}
				_innerList.InsertRange(p, after);
			}
		}

		public override string ToString()
		{
			return string.Join("", _innerList.Select(it => it.ToString()));
		}

		public bool Equals(SymbolList? other)
		{
			if (other is null)
			{
				return false;
			}
			return Equals(_innerList, other._innerList);
		}

		public override bool Equals(object? obj)
		{
			if (obj is IEnumerable<Symbol> o)
			{
				return Equals(o);
			}
			return false;
		}

		public override int GetHashCode()
		{
			string strIDs = string.Join(",", _innerList.Select(it => it.ID));
			return strIDs.GetHashCode();
		}

		public static bool operator ==(SymbolList? lhs, SymbolList? rhs)
		{
			if (lhs is null && rhs is null)
			{
				return true;
			}
			else if (lhs is null || rhs is null)
			{
				return false;
			}
			else if (ReferenceEquals(lhs, rhs))
			{
				return true;
			}
			else
			{
				return lhs.Equals(rhs);
			}
		}

		public static bool operator !=(SymbolList? lhs, SymbolList? rhs)
		{
			bool isEq = lhs == rhs;
			return !isEq;
		}

		public int CompareTo(SymbolList? other)
		{
			if (other is null)
			{
				return 1;
			}
			return CompareTo(_innerList, other._innerList);
		}

		private static int CompareTo(IEnumerable<Symbol> lhs, IEnumerable<Symbol> rhs)
		{
			if (!lhs.Any() && !rhs.Any())
			{
				return 0;
			}
			else if (!lhs.Any())
			{
				return -1;
			}
			else if (!rhs.Any())
			{
				return 1;
			}
			else
			{
				Symbol x = lhs.First();
				Symbol y = rhs.First();
				int result = x.ID.CompareTo(y.ID);
				if (0 == result)
				{
					return CompareTo(lhs.Skip(1), rhs.Skip(1));
				}
				else
				{
					return result;
				}
			}
		}

		private static bool Equals(IEnumerable<Symbol> lhs, IEnumerable<Symbol> rhs)
		{
			if (!lhs.Any() && !rhs.Any())
			{
				return true;
			}
			else if (!lhs.Any() || !rhs.Any())
			{
				return false;
			}
			else
			{
				if (lhs.First() != rhs.First())
				{
					return false;
				}
				else
				{
					return Equals(lhs.Skip(1), rhs.Skip(1));
				}
			}
		}

		public bool HasUnIsolatedTerminalSymbols()
		{
			if (Count == 1)
			{
				// single symbol
				return false;
			}

			foreach (Symbol s in _innerList)
			{
				if (s.IsTerminal)
				{
					return true;
				}
			}
			return false;
		}

		public bool IsUnitProductionRule()
		{
			return _innerList.Count == 1 && !_innerList[0].IsTerminal && !_innerList[0].IsEmpty && !_innerList[0].IsEos;
		}

		public bool IsEmptyProductionRule()
		{
			return _innerList.Count == 1 && _innerList[0].IsEmpty;
		}
	}
}
