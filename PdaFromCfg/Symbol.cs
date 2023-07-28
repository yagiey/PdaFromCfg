using System;

namespace PdaFromCfg
{
	public class Symbol
		: IEquatable<Symbol>
		, IComparable<Symbol>
	{
		public Symbol(string name, int id)
		{
			Name = name;
			ID = id;
			Rank = -1;
			IsTerminal = false;
		}

		public int ID { get; private set; }

		public string Name { get; private set; }

		public int Rank { get; set; }

		public bool IsTerminal { get; set; }

		public bool IsEmpty
		{
			get
			{
				return Name == SymbolPool.EmptyName && ID == SymbolPool.EmptyID;
			}
		}

		public bool IsEos
		{
			get
			{
				return Name == SymbolPool.EosName && ID == SymbolPool.EosID;
			}
		}

		public override bool Equals(object? obj)
		{
			if (obj is null)
			{
				return false;
			}
			else if (obj is Symbol s)
			{
				return Equals(s);
			}
			return false;
		}

		public override int GetHashCode()
		{
			return ID;
		}

		public override string ToString()
		{
			if (IsTerminal || IsEmpty || IsEos) { return Name; }
			else { return $"<{Name}>"; }
		}

		public bool Equals(Symbol? other)
		{
			if (other is null)
			{
				return false;
			}
			return ID == other.ID && Name == other.Name;
		}

		public int CompareTo(Symbol? other)
		{
			if (other is null)
			{
				return -1;
			}
			return other.ID.CompareTo(ID);
		}

		public static bool operator ==(Symbol? lhs, Symbol? rhs)
		{
			if (lhs is null && rhs is null)
			{
				return true;
			}
			else if (lhs is null || rhs is null)
			{
				return false;
			}
			else
			{
				return lhs.Equals(rhs);
			}
		}

		public static bool operator !=(Symbol? lhs, Symbol? rhs)
		{
			bool isEq = (lhs == rhs);
			return !isEq;
		}
	}
}
