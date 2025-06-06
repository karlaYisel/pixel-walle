using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core.Lexer.pw;
using Core.Lexer;
using Core.Utils.TokenSystem;
using System.Drawing;

namespace Core.Parser.pw
{
    public class ParserAnalyser : IParserAnalyser
    {
        private static Parser? __Parser;
        public IParser Parser
        {
            get
            {
                if (__Parser == null)
                {
                    __Parser = new Parser();

                    __Parser.RegisterColor("red", Color.FromArgb(255, 0, 0));
                    __Parser.RegisterColor("rojo", Color.FromArgb(255, 0, 0));

                    __Parser.RegisterColor("blue", Color.FromArgb(0, 0, 255));
                    __Parser.RegisterColor("azul", Color.FromArgb(0, 0, 255));

                    __Parser.RegisterColor("green", Color.FromArgb(0, 255, 0));
                    __Parser.RegisterColor("verde", Color.FromArgb(0, 255, 0));

                    __Parser.RegisterColor("yellow", Color.FromArgb(255, 255, 0));
                    __Parser.RegisterColor("amarillo", Color.FromArgb(255, 255, 0));

                    __Parser.RegisterColor("orange", Color.FromArgb(255, 165, 0));
                    __Parser.RegisterColor("naranja", Color.FromArgb(255, 165, 0));

                    __Parser.RegisterColor("purple", Color.FromArgb(128, 0, 128));
                    __Parser.RegisterColor("morado", Color.FromArgb(128, 0, 128));

                    __Parser.RegisterColor("black", Color.FromArgb(0, 0, 0));
                    __Parser.RegisterColor("negro", Color.FromArgb(0, 0, 0));

                    __Parser.RegisterColor("white", Color.FromArgb(255, 255, 255));
                    __Parser.RegisterColor("blanco", Color.FromArgb(255, 255, 255));

                    __Parser.RegisterColor("transparent", Color.FromArgb(0));
                    __Parser.RegisterColor("transparente", Color.FromArgb(0));

                    __Parser.RegisterType("void", typeof(Core.Utils.SystemClass.Void));
                    __Parser.RegisterType("int", typeof(int));
                    __Parser.RegisterType("bool", typeof(bool));
                    __Parser.RegisterType("color", typeof(System.Drawing.Color));
                }

                return __Parser;
            }
        }
    }
}
