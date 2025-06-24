using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core.Parser;
using Core.Lexer;
using Core.PixelWallE;
using Core.Utils.ImageEditor;
using Core.Lexer.pw;
using Core.Parser.pw;
using Core.Utils.Error;
using Core.Utils.TokenSystem;
using Core.Utils;
using Core.AST;
using Core.Semantic;
using Core.Executor;
using static System.Runtime.InteropServices.JavaScript.JSType;
using System.IO;

namespace Core.Controller
{
    public delegate Task CanvasChanged();
    public class Controller
    {
        private LanguageServiceSelector lexerSelector;
        private ImageEditorSelector editorSelector;
        private CanvasChanged? change;

        public Controller()
        {
            LexerAnalyser lex = new LexerAnalyser();
            var lexers = new Dictionary<string, ILexerAnalyser>
            {
                { "pw", lex },
                { "pwscript", lex }
            };

            ParserAnalyser par = new ParserAnalyser();
            var parsers = new Dictionary<string, IParserAnalyser>
            {
                { "pw", par },
                { "pwscript", par }
            };

            ExecutionError? error;
            var editors = new Dictionary<string, IPixelWallE>
            {
                { "png", new Core.PixelWallE.png.PixelWallE(out error) }
            };

            lexerSelector = new LanguageServiceSelector(lexers, parsers);
            editorSelector = new ImageEditorSelector(editors);
        }

        public async Task CanvasHasChanged()
        {
            if (change != null)
                await change.Invoke();
        }

        public void AddCanvasChangedListener(CanvasChanged listener)
        {
            change += listener;
        }

        public Program Compile(string CodePath, string CodeContent, string[] ScriptsPath, string[] ScriptsContent, out List<CompilingError> errors)
        {
            errors = new List<CompilingError>();
            bool stopAtParser = false;

            IEnumerable<Token> code;
            string? path = GetCleanExtension(CodePath);
            if (path is null)
            {
                errors.Add(new CompilingError(new Utils.TokenSystem.CodeLocation { File = CodePath }, ErrorCode.ExtensionError, "Extension not suported"));
                code = new List<Token>();
                stopAtParser = true;
            }
            else switch (path)
            {
                case "pw":
                    code = lexerSelector.GetLexer(path).Lexical.GetTokens(CodePath, CodeContent, errors);
                    break;
                case "pwscript":
                    errors.Add(new CompilingError(new Utils.TokenSystem.CodeLocation { File = CodePath }, ErrorCode.ExtensionError, "The main code can not be a script"));
                    code = lexerSelector.GetLexer(path).Lexical.GetTokens(CodePath, CodeContent, errors);
                    break;
                default:
                    errors.Add(new CompilingError(new Utils.TokenSystem.CodeLocation{ File = CodePath}, ErrorCode.ExtensionError, "Extension not suported"));
                    code = new List<Token>();
                    stopAtParser = true;
                    break;
            }

            IEnumerable<Token>[] scripts = new IEnumerable<Token>[ScriptsPath.Length];
            for (int i = 0; i < ScriptsPath.Length; i++)
            {
                path = GetCleanExtension(ScriptsPath[i]);
                if (path is null)
                {
                    errors.Add(new CompilingError(new Utils.TokenSystem.CodeLocation { File = ScriptsPath[i] }, ErrorCode.ExtensionError, "Extension not suported"));
                    scripts[i] = new List<Token>();
                    stopAtParser = true;
                }
                else switch (path)
                {
                    case "pw":
                        errors.Add(new CompilingError(new Utils.TokenSystem.CodeLocation { File = ScriptsPath[i] }, ErrorCode.ExtensionError, "A script code extension must end in script"));
                        scripts[i] = lexerSelector.GetLexer(path).Lexical.GetTokens(ScriptsPath[i], ScriptsContent[i], errors);
                        break;
                    case "pwscript":
                        scripts[i] = lexerSelector.GetLexer(path).Lexical.GetTokens(ScriptsPath[i], ScriptsContent[i], errors);
                        break;
                    default:
                        errors.Add(new CompilingError(new Utils.TokenSystem.CodeLocation { File = ScriptsPath[i] }, ErrorCode.ExtensionError, "Extension not suported"));
                        scripts[i] = new List<Token>();
                        stopAtParser = true;
                        break;
                }
            }

            if (stopAtParser)
            {
                return new Program(new AST.ProgramAST("", errors));
            }
            TokenStream sCode = new TokenStream(code);
            TokenStream[] sScript = new TokenStream[scripts.Length];
            for (int i = 0; i < scripts.Length; i++)
            {
                sScript[i] = new TokenStream(scripts[i]);
            }

            ProgramAST pCode;
            Script[] pScripts = new Script[sScript.Length];
            path = GetCleanExtension(CodePath);
            if (path is null)
            {
                pCode = new ProgramAST("", errors);
            }
            else switch (path)
            {
                case "pw" or "pwscript":
                    pCode = lexerSelector.GetParser(path).Parser.ParseProgram(CodePath, sCode, errors);
                    break;
                default:
                    pCode = new ProgramAST("", errors);
                    break;
            }
            for (int i = 0; i < ScriptsPath.Length; i++)
            {
                path = GetCleanExtension(ScriptsPath[i]);
                if (path is null)
                {
                    pScripts[i] = new Script("", errors);
                }
                else switch (path)
                {
                    case "pw" or "pwscript":
                        pScripts[i] = lexerSelector.GetParser(path).Parser.ParseScript(ScriptsPath[i], sScript[i], errors);
                        break;
                    default:
                        pScripts[i] = new Script("", errors);
                        break;
                }
            }
            ISemantic sem = new Semantic.Semantic();

            Program program = sem.CheckSemantic(pCode, pScripts);

            return program;
        }

        private async Task<ExecutionError?> Execute(Program program, ExecutionError? error, ColorType color, BrushType brush, AnimationType animation, CancellationToken cancel, string path = "img.png")
        {
            error = null;
            IExecutorAnalyser exAny = new Executor.ExecutorAnalyser();
            IExecutor ex = exAny.Executor;
            ex.AddCanvasChangedListener(CanvasHasChanged);

            string? pathe = GetCleanExtension(path);
            switch (pathe)
            {
                case "png":
                    ex.SetWallE(editorSelector.GetEditor(pathe));
                    break;
                default:
                    error = new ExecutionError(ErrorCode.ExtensionError, "Extension not suported");
                    break;
            }

            ex.SetColorType(color);
            ex.SetBrushType(brush);
            ex.SetAnimationType(animation);
            ex.SetProgram(program);

            error = await ex.ExecuteCode(error, cancel);
            return error;
        }

        public async Task <ExecutionError?>Resize(ExecutionError? error, int Width, int Height)
        {
            error = null;
            IPixelWallE pw = new PixelWallE.png.PixelWallE(out error);

            pw = editorSelector.GetEditor("png");

            error = await pw.ImageLoad(error, Width, Height);
            return error;
        }

        public byte[] GetImage()
        {
            IPixelWallE pw = editorSelector.GetEditor("png");
            return pw.GetImage();
        }

        public async Task SetImage(byte[] img)
        {
            IPixelWallE pw = editorSelector.GetEditor("png");
            await pw.SetImage(img);
        }

        public async Task<(ExecutionError? error, List<CompilingError> errors)> Run(string CodePath, string CodeContent, string[] ScriptsPath, string[] ScriptsContent, List<CompilingError> errors, ExecutionError? error, ColorType color, BrushType brush, AnimationType animation, CancellationToken cancel, string path = "img.png")
        {
            error = null;
            Program program = Compile(CodePath, CodeContent, ScriptsPath, ScriptsContent, out errors);

            if (errors.Count() > 0) return (error, errors);

            error = await Execute(program, error, color, brush, animation, cancel, path);
            return (error, errors);
        }

        public static string? GetCleanExtension(string? path)
        {
            if (string.IsNullOrWhiteSpace(path))
                return null;

            try
            {
                string ext = Path.GetExtension(path);
                if (string.IsNullOrWhiteSpace(ext) || ext == ".")
                    return null;
                return ext.TrimStart('.').ToLowerInvariant();
            }
            catch
            {
                return null;
            }
        }
    }
}
