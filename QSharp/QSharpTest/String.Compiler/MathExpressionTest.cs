using QSharp.String.Compiler;
using QSharp.String.Stream;

namespace QSharpTest.String.Compiler
{
    public class MathExpressionTest
    {
        private readonly string[] _bnf =
        {
            @"Lang -> AExpVal", 
            
            @"AExpBool-> ExpBool | QExpBool",                   // any boolean expression, quoted or not
            @"QExpBool-> '(' ExpBool ')' | '(' QExpBool ')'",   // quoted boolean expression
            @"ExpBool -> ExpComp | ExpLogc | ExpNot",           // boolean expression
            
            @"AExpVal -> ExpVal | QExpVal",                     // value expresion, quoted or not
            @"QExpVal -> '(' ExpVal ')' | '(' QExpVal ')'",     // quoted value expression
            @"ExpVal -> ExpMul | ExpAdd | ExpNeg | Val",        // unquoted value expression
            
            @"AExp -> Exp | QExp",                              // any expression, quoted or not
            @"QExp -> '(' Exp ')' | '(' QExp ')'",              // quoted expression
            @"Exp-> ExpBool | ExpVal",                          // any expression, unquoted

            @"AExpMul -> QExpMul | ExpMul",
            @"QExpMul -> '(' QExpMul ')' | '(' ExpMul ')'",
            @"ExpMul -> OpdMul1 OpMul OpdMul",
            @"OpdMul1 -> Val | QExpVal | ExpMul",
            @"OpdMul -> Val | QExpVal",
            @"OpMul -> '*' | '/'",

            @"AExpAdd -> QExpAdd | ExpAdd", 
            @"QExpAdd -> '(' QExpAdd ')' | '(' ExpAdd ')'",
            @"ExpAdd -> OpdAdd1 OpAdd OpdAdd",
            @"OpdAdd1 -> OpdAdd | ExpAdd",
            @"OpdAdd -> OpdMul | QExpVal",
            @"OpAdd -> '+' | '-'",

            @"AExpNeg -> QExpNeg | ExpNeg ",
            @"QExpNeg -> '(' ExpNeg ')' | '(' QExpNeg ')'",
            @"ExpNeg -> OpMin OpdNeg",
            @"OpdNeg -> AVal | QExpVal",

            @"AExpComp -> QExpComp | ExpComp", 
            @"QExpComp -> '(' QExpComp ')' | '(' ExpComp ')'",
            @"ExpComp -> OpdComp1 OpComp OpdComp",
            @"OpdComp1 -> OpdComp | ExpComp",
            @"OpdComp -> AExpVal",
            @"OpComp -> '<' | '=' | '>' | '<>'",

            @"ExpLogc -> OpdLogc1 OpLogc OpdLogc",
            @"OpdLogc1 -> OpdComp | ExpLogc",
            @"OpdLogc -> ",
            @"OpLogc -> '&&' | '\|\|'",

            @"AExpNot -> QExpNot | ExpNot",
            @"QExpNot -> '(' QExpNot ')' | '(' ExpNot ')'",
            @"ExpNot -> OpNot QExpBool",            // inverse expression (qualified by 'NOT')
            
            @"AVal-> Val | QVal ",                  // value, quoted or not
            @"QVal-> '(' Val ')' | '(' QVal ')'",   // quoted value
            @"Val -> Var | Const",                    // unquoted value
        
            @"Var -> LetterUl ANUl",
            // TODO this cannot contain LetterUl ANUl any more (need to figure out why)
            @"ANUl -> Digit ANUl | Letter ANUl | '_' ANUl | ",   // alphanumeric string that can have underline characters and can be empty
            @"LetterUl -> Letter | '_'",                         // letter or underline
            @"Letter -> 'A'|'B'|'C'|'D'|'E'|'F'|'G'|'H'|'I'|'J'|'K'|'L'|'M'|'N'|'O'|'P'|'Q'|'R'|'S'|'T'|'U'|'V'|'W'|'X'|'Y'|'Z'|'a'|'b'|'c'|'d'|'e'|'f'|'g'|'h'|'i'|'j'|'k'|'l'|'m'|'n'|'o'|'p'|'q'|'r'|'s'|'t'|'u'|'v'|'w'|'x'|'y'|'z'",
            @"Const -> Real | Int",
            @"Real -> Int | Int '.' Int | '.' Int | Int '.'",
            @"Int -> Digit Int | Digit",
            @"Digit -> '0'|'1'|'2'|'3'|'4'|'5'|'6'|'7'|'8'|'9'",
            
            @"OpNot -> '!'",    // Operator 'NOT'
            @"OpMin-> '-'",     // minus sign
            @"OpDot-> '.'"
        };

        public void Test()
        {
            var sss = new StringsStream(_bnf);

            
            Bnf bnf;
            ITerminalSelector ts;
            var bnfct = new BnfCreator_Textual();
            var bOk = bnfct.Create(out bnf, out ts, sss);

            BnfAnalysis.VtTokenSet[] firstSets = BnfAnalysis.DeriveFirstSets(bnf);

            var dfa = new Dfa_LR1();
            dfa.Create_LR1(bnf, firstSets);
            

            const string sInput = "_a3ad_1+1";
            var ssInput = new StringStream(sInput);
            var sswInput = new StreamSwitcher(ts, ssInput);

            var table = new LRTable();

            table.Create_LR1(dfa);

            var parser = new SyntaxParser_LRTable { Table = table };

            var bRes = parser.Parse(sswInput);

            System.Console.WriteLine("Parser result = {0}", bRes);
        }
    }
}
