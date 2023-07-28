using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace PdaFromCfg
{
	public class Grammer<TokenType>
	{
		private readonly IDictionary<Symbol, TokenType?> _terminalSymbols;
		private readonly IDictionary<Symbol, IList<SymbolList>> _productionRules;
		private readonly SymbolPool _symbolPool;
		private const string AvailableCharactors = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz";

		public Grammer(SymbolPool symbolPool)
		{
			_terminalSymbols = new Dictionary<Symbol, TokenType?>();
			_productionRules = new Dictionary<Symbol, IList<SymbolList>>();
			_symbolPool = symbolPool;
		}

		public Symbol? StartSymbol { get; private set; }

		public void AddTerminalSymbol(Symbol terminalSymbol, TokenType tokenType)
		{
			terminalSymbol.IsTerminal = true;
			_terminalSymbols.Add(terminalSymbol, tokenType);
		}

		public void AddRule(Symbol lhs, IEnumerable<Symbol> rhs)
		{
			lhs.IsTerminal = false;
			SymbolList list = new(rhs);
			bool found = _productionRules.TryGetValue(lhs, out IList<SymbolList>? after);
			if (!found || after == null)
			{
				_productionRules.Add(lhs, new List<SymbolList>() { list });
			}
			else
			{
				if (!after.Any(it => it.SequenceEqual(list)))
				{
					after.Add(list);
				}
			}
		}

		public void SetStartSymbol(Symbol symbol)
		{
			HashSet<Symbol> vocab = GetVocab();
			if (vocab.Any(it => it == symbol))
			{
				StartSymbol = symbol;
				StartSymbol.Rank = 0;
				return;
			}
		}

		public void ToChomskyStandardForm()
		{
			if (StartSymbol is null)
			{
				const string ErrMsg = "start symbol is not specified.";
				Console.WriteLine(ErrMsg);
				throw new Exception(ErrMsg);
			}

			SetSymbolRank();

			RemoveAllStartSymbolInRhs();
			IsolateTerminalSymbol();
			SeparateSymbolList();
			RemoveEmptyProductionRules();
			RemoveUnitProductionRules();
			RemoveUnnecessarydRules(CalcDeadSymbol(), CalcUnreachableSymbols());

			SetSymbolRank();
		}

		private void RemoveAllStartSymbolInRhs()
		{
			bool exist = _productionRules.Values.Any(it => it.Any(it2 => it2.Any(it3 => it3 == StartSymbol)));
			if (exist)
			{
				// random name
				string randomName;
				do
				{
					randomName = $"Start_{GenerateRandomName(4, AvailableCharactors)}";
				} while (GetVocab().Any(it => it.Name == randomName));

				Symbol newStart = _symbolPool.GetSymbol(randomName);
				AddRule(newStart, new Symbol[] { StartSymbol! });

				SetStartSymbol(newStart);
			}
		}

		private void IsolateTerminalSymbol()
		{
			HashSet<Symbol> vocab = GetVocab();

			// create substitution map
			IDictionary<Symbol, Symbol> map = new Dictionary<Symbol, Symbol>();
			foreach (var pair in _productionRules)
			{
				foreach (var rhs in pair.Value)
				{
					if (rhs.HasUnIsolatedTerminalSymbols())
					{
						foreach (Symbol s in rhs.Where(it => it.IsTerminal))
						{
							if (!map.ContainsKey(s))
							{
								// s should be isolated

								string randomName;
								do
								{
									randomName = $"Trm_{s.Name}_{GenerateRandomName(4, AvailableCharactors)}";
								} while (vocab.Any(it => it.Name == randomName));

								Symbol l = _symbolPool.GetSymbol(randomName);
								vocab.Add(l);
								map.Add(s, l);
							}
						}
					}
				}
			}

			// add new rules
			foreach (var pair in map)
			{
				AddRule(pair.Value, new Symbol[] { pair.Key });
			}

			// replace target symbols
			foreach (var pair in _productionRules)
			{
				foreach (var rhs in pair.Value)
				{
					if (rhs.HasUnIsolatedTerminalSymbols())
					{
						foreach (var pairsDone in map)
						{
							Symbol before = pairsDone.Key;
							Symbol after = pairsDone.Value;

							rhs.ReplaceAll(before, after);
						}
					}
				}
			}
		}

		private void SeparateSymbolList()
		{
			HashSet<Symbol> vocab = GetVocab();

			IDictionary<SymbolList, Symbol> map = new Dictionary<SymbolList, Symbol>();
			int n = 0;
			do
			{
				n = 0;
				foreach (var pair in _productionRules)
				{
					foreach (SymbolList list in pair.Value)
					{
						if (3 <= list.Count)
						{
							SymbolList oldSymbols = new(list.TakeLast(2));

							if (map.TryGetValue(oldSymbols, out Symbol? newSymbol) && newSymbol is not null)
							{
								list.ReplaceAll(oldSymbols, new SymbolList(newSymbol));
							}
							else
							{
								string randomName;
								do
								{
									randomName = $"Sep_{GenerateRandomName(4, AvailableCharactors)}";
								} while (vocab.Any(it => it.Name == randomName));

								newSymbol = _symbolPool.GetSymbol(randomName);

								list.ReplaceAll(oldSymbols, new SymbolList(newSymbol));

								map.Add(oldSymbols, newSymbol);
								vocab.Add(newSymbol);
							}

							n++;
						}
					}
				}
			} while (0 < n);

			// add rules
			foreach (var pair in map)
			{
				AddRule(pair.Value, pair.Key);
			}
		}

		private void RemoveEmptyProductionRules()
		{
			while (true)
			{
				var e = _productionRules.Where(it => it.Key != StartSymbol! && it.Value.Any(it2 => it2.IsEmptyProductionRule()));
				if (!e.Any())
				{
					break;
				}

				// select empty rule
				Symbol lhs = e.First().Key;

				IDictionary<Symbol, IList<SymbolList>> ext = new Dictionary<Symbol, IList<SymbolList>>();
				foreach (var pair in _productionRules)
				{
					foreach (var rhs in pair.Value)
					{
						var positions = rhs.SearchAll(new Symbol[] { lhs });
						if (positions.Any())
						{
							IEnumerable<SymbolList> newRules = MakeNewRules(lhs, rhs, positions);
							foreach (SymbolList l in newRules)
							{
								if (ext.ContainsKey(pair.Key))
								{
									ext[pair.Key].Add(l);
								}
								else
								{
									ext.Add(pair.Key, new List<SymbolList> { l });
								}
							}
						}
					}
				}

				// remove empty rule
				e.First().Value.Remove(new SymbolList(SymbolPool.Empty));

				// add rules
				foreach (var pair1 in ext)
				{
					foreach (SymbolList list in pair1.Value)
					{
						if (!_productionRules[pair1.Key].Contains(list))
						{
							_productionRules[pair1.Key].Add(list);
						}
					}
				}

			}
		}

		private static IEnumerable<SymbolList> MakeNewRules(Symbol lhs, SymbolList rhs, IEnumerable<int> positions)
		{
			if (!positions.Any())
			{
				throw new Exception();
			}
			if (positions.Any(it => it < 0))
			{
				throw new Exception();
			}

			var directProduct =
				EnumerateDirectProduct(
					Enumerable.Repeat(new bool[] { false, true }, positions.Count())
				);

			foreach (var dp in directProduct)
			{
				SymbolList l = new();
				var itor = dp.GetEnumerator();
				for (int i = 0; i < rhs.Count; i++)
				{
					if (positions.Contains(i))
					{
						itor.MoveNext();
						if (itor.Current)
						{
							l.Add(lhs);
						}
					}
					else
					{
						l.Add(rhs[i]);
					}
				}

				if (l.Count == 0)
				{
					l.Add(SymbolPool.Empty);
				}

				yield return l;
			}
		}

		private void RemoveUnitProductionRules()
		{
			while (true)
			{
				var e = _productionRules.Where(it => it.Value.Any(it2 => it2.IsUnitProductionRule()));
				if (!e.Any())
				{
					break;
				}

				Symbol lhs = e.First().Key;
				Symbol rhs = e.First().Value.First(it => it.IsUnitProductionRule()).First();

				foreach (var l in _productionRules[rhs])
				{
					_productionRules[lhs].Add(l);
				}

				_productionRules[lhs].Remove(new SymbolList(rhs));
			}
		}

		private IEnumerable<Symbol> CalcDeadSymbol()
		{
			HashSet<Symbol> aliveOld = new();
			foreach (var pair in _productionRules)
			{
				bool found = false;
				foreach(var list in pair.Value)
				{
					bool isAlive = list.All(it => aliveOld.Union(_terminalSymbols.Keys).Contains(it));
					if (isAlive)
					{
						found = true;
						break;
					}
				}

				if (found)
				{
					aliveOld.Add(pair.Key);
					continue;
				}
			}


			while (true)
			{
				HashSet<Symbol> aliveNew = new(aliveOld);
				foreach (var pair in _productionRules)
				{
					bool found = false;
					foreach (var list in pair.Value)
					{
						bool isAlive = list.All(it => aliveNew.Union(_terminalSymbols.Keys).Contains(it));
						if (isAlive)
						{
							found = true;
							break;
						}
					}

					if (found)
					{
						aliveNew.Add(pair.Key);
						continue;
					}
				}

				if (IsEqualSet(aliveOld, aliveNew))
				{
					break;
				}
				aliveOld = aliveNew;
			}

			HashSet<Symbol> entire = GetVocab();
			var result = entire.Except(_terminalSymbols.Keys).Except(aliveOld);
			return result;
		}

		private IEnumerable<Symbol> CalcUnreachableSymbols()
		{
			HashSet<Symbol> reachableOld = new() { StartSymbol! };
			foreach (var pair in _productionRules)
			{
				if (reachableOld.Contains(pair.Key))
				{
					foreach (SymbolList rhs in pair.Value)
					{
						foreach(Symbol s in rhs)
						{
							if (!s.IsTerminal && !s.IsEmpty && !s.IsEos)
							{
								reachableOld.Add(s);
							}
						}
					}
				}
			}

			while(true)
			{
				HashSet<Symbol> reachableNew = new(reachableOld);
				foreach (var pair in _productionRules)
				{
					if (reachableNew.Contains(pair.Key))
					{
						foreach (SymbolList rhs in pair.Value)
						{
							foreach (Symbol s in rhs)
							{
								if (!s.IsTerminal && !s.IsEmpty && !s.IsEos)
								{
									reachableNew.Add(s);
								}
							}
						}
					}
				}

				if (IsEqualSet(reachableOld, reachableNew))
				{
					break;
				}
				reachableOld = reachableNew;
			}

			HashSet<Symbol> entire = GetVocab();
			var result = entire.Except(_terminalSymbols.Keys).Except(reachableOld);
			return result;
		}

		private void RemoveUnnecessarydRules(IEnumerable<Symbol> deadSymbols, IEnumerable<Symbol> unreachableSymbols)
		{
			HashSet<Symbol> targets = new(deadSymbols.Union(unreachableSymbols));

			// lhs
			foreach (Symbol target in targets)
			{
				_productionRules.Remove(target);
			}

			// rhs
			foreach (Symbol target in targets)
			{
				foreach (var pair in _productionRules)
				{
					HashSet<SymbolList> l = new();
					foreach (SymbolList rhs in pair.Value)
					{
						int pos = rhs.SearchFirst(target);
						if (0 < pos)
						{
							l.Add(rhs);
						}
					}

					foreach (SymbolList rhs in l)
					{
						pair.Value.Remove(rhs);
					}
				}
			}

			HashSet<Symbol> set = new();
			foreach (var pair in _productionRules)
			{
				if(_productionRules[pair.Key].Count == 0)
				{
					set.Add(pair.Key);
				}
			}

			foreach(Symbol key in set)
			{
				_productionRules.Remove(key);
			}
		}

		private static IEnumerable<IEnumerable<T>> EnumerateDirectProduct<T>(IEnumerable<IEnumerable<T>> sets)
		{
			return
				sets.Aggregate(
					Enumerable.Repeat(Enumerable.Empty<T>(), 1),
					(prod, list) => from x in prod from y in list select x.Concat(Enumerable.Repeat(y, 1))
				);
		}

		private static bool IsEqualSet(IEnumerable<Symbol> a, IEnumerable<Symbol> b)
		{
			if (a is null && b is null)
			{
				return true;
			}
			else if (a is null || b is null)
			{
				return false;
			}
			else
			{
				return a.All(it => b.Contains(it)) && b.All(it => a.Contains(it));
			}
		}

		private static string GenerateRandomName(int length, string availableCharacters)
		{
			if (length < 3) { throw new Exception(); }

			int size = availableCharacters.Length;
			Random rand = new();
			StringBuilder sb = new();
			for (int i = 0; i < length; i++)
			{
				int pos = rand.Next(0, size);
				sb.Append(availableCharacters[pos]);
			}

			return sb.ToString();
		}

		private void SetSymbolRank()
		{
			IEnumerable<Symbol> vocab = GetVocab();

			// reset rank
			foreach (Symbol s in vocab)
			{
				if (s != StartSymbol)
				{
					s.Rank = -1;
				}
			}

			int rank = 0;

			while (true)
			{
				IEnumerable<Symbol> from = vocab.Where(it => it.Rank == rank);
				IEnumerable<Symbol> rest = vocab.Where(it => it.Rank == -1);

				if (!rest.Any())
				{
					break;
				}

				foreach (Symbol symbol in from)
				{
					if (symbol.IsTerminal || symbol.IsEmpty || symbol.IsEos)
					{
						continue;
					}

					IList<SymbolList> rules = _productionRules[symbol];
					foreach (SymbolList list in rules)
					{
						foreach (Symbol s in list)
						{
							if (s.Rank < 0)
							{
								s.Rank = rank + 1;
							}
						}
					}
				}

				rank++;
			}
		}

		public void DisplayGrammer()
		{
			Console.WriteLine("----------------------------------------");
			Console.WriteLine(" Grammer");
			Console.WriteLine("----------------------------------------");

			Console.WriteLine("[nonterminal symbols]");
			foreach (Symbol s in _productionRules.Keys)
			{
				Console.WriteLine($"  {s}");
			}

			Console.WriteLine("[terminal symbols]");
			foreach (Symbol s in _terminalSymbols.Keys)
			{
				Console.WriteLine($"  {s}");
			}

			Console.WriteLine("[production]");
			foreach (var entry1 in _productionRules)
			{
				Symbol lhs = entry1.Key;
				foreach (SymbolList rhs in entry1.Value)
				{
					Console.WriteLine($"  {lhs} -> {rhs}");
				}
			}
			Console.WriteLine($"[start symbol]");
			Console.WriteLine($"  {StartSymbol}");
			Console.WriteLine();
		}

		public HashSet<Symbol> GetVocab()
		{
			HashSet<Symbol> result = new(_terminalSymbols.Keys);
			foreach (var entry in _productionRules)
			{
				result.Add(entry.Key);
				foreach (IEnumerable<Symbol> list in entry.Value)
				{
					foreach (Symbol item in list)
					{
						result.Add(item);
					}
				}
			}
			return result;
		}

		public HashSet<Symbol> GetTerminalSymbols()
		{
			HashSet<Symbol> result = new();
			foreach (var item in _terminalSymbols.Keys)
			{
				result.Add(item);
			}
			return result;
		}

		public IDictionary<Symbol, IEnumerable<SymbolList>> GetNonTerminalRules()
		{
			IDictionary<Symbol, IEnumerable<SymbolList>> result =
				new Dictionary<Symbol, IEnumerable<SymbolList>>();

			foreach (var entry in _productionRules)
			{
				IList<SymbolList> values = new List<SymbolList>();
				foreach (IEnumerable<Symbol> list in entry.Value)
				{
					SymbolList listClone = new(list);
					values.Add(listClone);
				}
				result.Add(entry.Key, values);
			}
			return result;
		}
	}
}
