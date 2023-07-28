using System.Collections.Generic;

namespace PdaFromCfg
{
	public class SymbolPool
	{
		/// <summary>Empty</summary>
		public static Symbol Empty { get; private set; }

		/// <summary>End of source code</summary>
		public static Symbol Eos { get; private set; }

		public const string EmptyName = @"empty";
		public const int EmptyID = 0;
		public const string EosName = @"eos";
		public const int EosID = 1;

		private int _currentId;
		private readonly IDictionary<string, Symbol> _fromName;
		private readonly IDictionary<int, Symbol> _fromID;

		static SymbolPool()
		{
			Empty = new Symbol(EmptyName, EmptyID) { IsTerminal = false };
			Eos = new Symbol(EosName, EosID) { IsTerminal = false };
		}

		public SymbolPool()
		{
			_currentId = 1;
			_fromName = new Dictionary<string, Symbol> { { EmptyName, Empty } };
			_fromID = new Dictionary<int, Symbol> { { EmptyID, Empty } };
		}

		public Symbol GetSymbol(string name)
		{
			bool found = _fromName.TryGetValue(name, out Symbol? s);
			if (found && s is not null)
			{
				return s;
			}
			else
			{
				_currentId++;
				Symbol result = new(name, _currentId);
				_fromName.Add(name, result);
				_fromID.Add(_currentId, result);
				return result;
			}
		}
	}
}
