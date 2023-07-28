namespace PdaFromCfg
{
	class Program
	{
		static void Main()
		{
			Calc();
		}

		private static void Calc()
		{
			/*
			 * Grammer
			 * 
			 * <E0>  ::= <T0><E1>
			 * <E1>  ::= +<T0><E1> | -<T0><E1> | ε
			 * <T0>  ::= <F><T1>
			 * <T1>  ::= *<F><T1> | /<F><T1> | ε
			 * <F>   ::= intnum | (<E0>)
			*/

			SymbolPool symbolPool = new();
			Grammer<TokenTypeCalc> grammer = new(symbolPool);

			//////////////////////////////
			// terminal symbols
			//////////////////////////////
			Symbol symbolLparen = symbolPool.GetSymbol(@"(");
			grammer.AddTerminalSymbol(symbolLparen, TokenTypeCalc.LeftParen);
			Symbol symbolRparen = symbolPool.GetSymbol(@")");
			grammer.AddTerminalSymbol(symbolRparen, TokenTypeCalc.RightParen);
			Symbol symbolPlus = symbolPool.GetSymbol(@"+");
			grammer.AddTerminalSymbol(symbolPlus, TokenTypeCalc.Plus);
			Symbol symbolMinus = symbolPool.GetSymbol(@"-");
			grammer.AddTerminalSymbol(symbolMinus, TokenTypeCalc.Minus);
			Symbol symbolAsterisk = symbolPool.GetSymbol(@"*");
			grammer.AddTerminalSymbol(symbolAsterisk, TokenTypeCalc.Asterisk);
			Symbol symbolSlash = symbolPool.GetSymbol(@"/");
			grammer.AddTerminalSymbol(symbolSlash, TokenTypeCalc.Slash);
			Symbol symbolIntnum = symbolPool.GetSymbol(@"intnum");
			grammer.AddTerminalSymbol(symbolIntnum, TokenTypeCalc.LiteralInteger);

			//////////////////////////////
			// non-terminal
			//////////////////////////////
			Symbol symbolE0 = symbolPool.GetSymbol(@"E0");
			Symbol symbolE1 = symbolPool.GetSymbol(@"E1");
			Symbol symbolT0 = symbolPool.GetSymbol(@"T0");
			Symbol symbolT1 = symbolPool.GetSymbol(@"T1");
			Symbol symbolF = symbolPool.GetSymbol(@"F");

			grammer.AddRule(symbolE0, new Symbol[] { symbolT0, symbolE1 });
			grammer.AddRule(symbolE1, new Symbol[] { symbolPlus, symbolT0, symbolE1 });
			grammer.AddRule(symbolE1, new Symbol[] { symbolMinus, symbolT0, symbolE1 });
			grammer.AddRule(symbolE1, new Symbol[] { SymbolPool.Empty });

			grammer.AddRule(symbolT0, new Symbol[] { symbolF, symbolT1 });
			grammer.AddRule(symbolT1, new Symbol[] { symbolAsterisk, symbolF, symbolT1 });
			grammer.AddRule(symbolT1, new Symbol[] { symbolSlash, symbolF, symbolT1 });
			grammer.AddRule(symbolT1, new Symbol[] { SymbolPool.Empty });

			grammer.AddRule(symbolF, new Symbol[] { symbolIntnum });
			grammer.AddRule(symbolF, new Symbol[] { symbolLparen, symbolE0, symbolRparen });

			grammer.SetStartSymbol(symbolE0);

			grammer.ToChomskyStandardForm();
			grammer.DisplayGrammer();
		}
	}
}
