namespace PdaFromCfg
{
	class Program
	{
		static void Main()
		{
			//Calc();
			Rec();
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

		private static void Rec()
		{
			SymbolPool symbolPool = new();
			Grammer<TokenRec> grammer = new(symbolPool);

			Symbol a = symbolPool.GetSymbol("a");
			grammer.AddTerminalSymbol(a, TokenRec.a);
			Symbol b = symbolPool.GetSymbol("b");
			grammer.AddTerminalSymbol(b, TokenRec.b);
			Symbol c = symbolPool.GetSymbol("c");
			grammer.AddTerminalSymbol(c, TokenRec.c);
			Symbol d = symbolPool.GetSymbol("d");
			grammer.AddTerminalSymbol(d, TokenRec.d);

			Symbol A1 = symbolPool.GetSymbol("A1");
			Symbol A2 = symbolPool.GetSymbol("A2");
			Symbol A3 = symbolPool.GetSymbol("A3");
			Symbol A4 = symbolPool.GetSymbol("A4");

			grammer.AddRule(A1, new Symbol[] { A2, a });
			grammer.AddRule(A1, new Symbol[] { a });
			grammer.AddRule(A2, new Symbol[] { A3, b });
			grammer.AddRule(A2, new Symbol[] { b });
			grammer.AddRule(A3, new Symbol[] { A4, c });
			grammer.AddRule(A3, new Symbol[] { c });
			grammer.AddRule(A4, new Symbol[] { A1, d });
			grammer.AddRule(A4, new Symbol[] { d });

			grammer.SetStartSymbol(A1);

			grammer.ToGreibachStandardForm();
			grammer.DisplayGrammer();
		}
	}
}
