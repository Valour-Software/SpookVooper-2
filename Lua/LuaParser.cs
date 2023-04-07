using SV2.Scripting.Tokens;
using System.Xml.Linq;

namespace SV2.Scripting.Parser;

public class LuaParser
{
    public static Tokenizer<LuaTokenType> tokenizer { get; set; }
    public string test { get; set; }
    public LuaTable Objects { get; set; }
    public LuaTable CurrentParent { get; set; }
    public string FileName { get; set; }
    public int CurrentLineNumber { get; set; }
    public void LoadTokenizer()
    {
        tokenizer = new Tokenizer<LuaTokenType>(LuaTokenType.WildCard)
            .Token(LuaTokenType.Comment, @"(?<=^([^""\r\n]|""[^""\r\n]*"")*)--[^\r\n]*")
            .Token(LuaTokenType.String, "true")
            .Token(LuaTokenType.String, "false")
            .Token(LuaTokenType.Quote, "\"")
            .Token(LuaTokenType.String, "\\^")
            .Token(LuaTokenType.String, "\\*")
            .Token(LuaTokenType.String, "\\+")
            .Token(LuaTokenType.String, "/")
            .Token(LuaTokenType.Variable, @"\$\([a-zA-Z0-9_-]+\)")
            .Token(LuaTokenType.NewLine, @"[\r\n]")
            .Token(LuaTokenType.Whitespace, @"[ \t]+")
            .Token(LuaTokenType.Equals, "=")
            //.Token(LuaTokenType.Dot, @"\.")
            .Token(LuaTokenType.OpenSquareBracket, @"\[")
            .Token(LuaTokenType.CloseSquareBracket, @"\]")
            .Token(LuaTokenType.OpenCurlyBracket, "{")
            .Token(LuaTokenType.CloseCurlyBracket, "}")
            .Token(LuaTokenType.Comma, ",")
            .Token(LuaTokenType.OpenRoundBracket, @"\(")
            .Token(LuaTokenType.CloseRoundBracket, @"\)")
            .Token(LuaTokenType.Item, "^[a-zA-Z_$][a-zA-Z_$0-9.]*$")
            .Token(LuaTokenType.Number, @"^[+-]?(\d*|\d{1,3}(,\d{3})*)(\.\d+)?\b$")
            .Token(LuaTokenType.WildCard, @"\*")
            .Token(LuaTokenType.String, "\\-");
    }

    public int AddTable(List<Token<LuaTokenType>> tokens, int i, string name)
    {
        var obj = new LuaTable()
        {
            Name = name,
            Parent = CurrentParent,
            FileName = FileName,
            LineNumber = CurrentLineNumber,
        };
        CurrentParent.Items.Add(obj);
        CurrentParent = obj;

        while (true)
        {
            var token = tokens[i];
            if (token.Type == LuaTokenType.NewLine)
            {
                CurrentLineNumber += 1;
            }

            if (token.Type == LuaTokenType.Equals)
                i = AddAssign(tokens, i);
            if (token.Type == LuaTokenType.CloseCurlyBracket)
            {
                CurrentParent = CurrentParent.Parent;
                break;
            }
            i += 1;
        }

        return i;
    }

    public int AddList(List<Token<LuaTokenType>> tokens, int i, string name)
    {
        var obj = new LuaTable()
        {
            Name = name,
            Parent = CurrentParent,
            FileName = FileName,
            LineNumber = CurrentLineNumber,
        };
        CurrentParent.Items.Add(obj);
        CurrentParent = obj;

        while (true)
        {
            var token = tokens[i];
            if (token.Type == LuaTokenType.NewLine)
            {
                LuaObject _obj = new()
                {
                    Name = name,
                    FileName = FileName,
                    Parent = CurrentParent,
                    LineNumber = CurrentLineNumber,
                    type = ObjType.String,
                    Value = tokens[i-1].Value
                };
                CurrentParent.Items.Add(_obj);
                CurrentLineNumber += 1;
            }

            if (token.Type == LuaTokenType.CloseSquareBracket)
            {
                CurrentParent = CurrentParent.Parent;
                break;
            }
            i += 1;
        }

        return i;
    }

    public (string, int) GetStringValue(List<Token<LuaTokenType>> tokens, int i, bool startedwithquote = true)
    {
        string value = "";
        while (true)
        {
            var token = tokens[i];
            if ((token.Type == LuaTokenType.Quote && startedwithquote) || token.Type == LuaTokenType.NewLine || token.Type == LuaTokenType.CloseRoundBracket
                || token.Type == LuaTokenType.CloseCurlyBracket)
            {
                if (token.Type == LuaTokenType.NewLine)
                    CurrentLineNumber += 1;
                i -= 1;
                break;
            }
            value += tokens[i].Value;
            i += 1;
        }
        return new(value, i);
    }

    public (int returni, LuaObject obj) HandleSinglePartofExpression(List<Token<LuaTokenType>> tokens, int i)
    {
        // we want
        // 3000 -> LuaObject(type: StringForNumber, value: 3000)
        // building.level -> LuaObject(type: String, value: "building.level")
        // get_local("cost_increase") -> LuaObject(type: String, value: "cost_increase")
        LuaObject obj = null;

        var oldi = i;
        while (tokens[i].Type == LuaTokenType.Whitespace)
            i += 1;

        if (tokens[i].Type == LuaTokenType.OpenRoundBracket)
        {
            i = HandleExpression(tokens, i + 1, "");
            obj = CurrentParent.Items.Last();
            //CurrentParent = CurrentParent.Parent;
        }

        else if (tokens[i + 1].Type == LuaTokenType.OpenSquareBracket)
        {
            string value = tokens[i].Value + ".";
            if (tokens[i + 2].Type == LuaTokenType.Quote)
            {
                value += tokens[i + 3].Value;
                i += 6;
            }
            else { 
                value += tokens[i + 2].Value;
                i += 4;
            }

            obj = new()
            {
                Name = "",
                FileName = FileName,
                Parent = CurrentParent,
                LineNumber = CurrentLineNumber,
                type = ObjType.String,
                Value = value,
                IPosition = i
            };
        }
        else if (tokens[i+1].Type == LuaTokenType.OpenRoundBracket)
        {
            obj = new()
            {
                Name = tokens[i].Value,
                FileName = FileName,
                Parent = CurrentParent,
                LineNumber = CurrentLineNumber,
                type = ObjType.String,
                Value = tokens[i + 3].Value,
                IPosition = i
            };
            i += 6;
        }
        else
        {
            var token = tokens[i];
            i += 1;
            if (token.Type == LuaTokenType.Number)
            {
                obj = new()
                {
                    Name = "",
                    FileName = FileName,
                    Parent = CurrentParent,
                    LineNumber = CurrentLineNumber,
                    type = ObjType.StringForNumber,
                    Value = token.Value,
                    IPosition = i
                };
            }
            else if (token.Type == LuaTokenType.Item)
            {
                obj = new()
                {
                    Name = "",
                    FileName = FileName,
                    Parent = CurrentParent,
                    LineNumber = CurrentLineNumber,
                    type = ObjType.String,
                    Value = token.Value,
                    IPosition = i
                };
            }
        }

        return new(i, obj);
    }

    public int HandleExpression(List<Token<LuaTokenType>> tokens, int i, string name)
    {
        var table = new LuaTable()
        {
            Name = name,
            Parent = CurrentParent,
            FileName = FileName,
            LineNumber = CurrentLineNumber,
            IPosition = i
        };
        CurrentParent.Items.Add(table);
        CurrentParent = table;

        int bracketdepth = 0;
        var valueofi_beforechanged = i;
        i -= 1;
        while (true)
        {
            i += 1;
            var token = tokens[i];
            if (token.Type == LuaTokenType.Whitespace)
                continue;

            if (token.Type == LuaTokenType.OpenRoundBracket)
                bracketdepth += 1;

            if (token.Type == LuaTokenType.CloseRoundBracket)
            {
                if (bracketdepth == 0)
                {
                    CurrentParent = CurrentParent.Parent;
                    break;
                }
                bracketdepth -= 1;
            }

            if (bracketdepth > 0)
                continue;

            if (token.Type == LuaTokenType.NewLine)
            {
                CurrentLineNumber += 1;
                CurrentParent = CurrentParent.Parent;
                break;
            }

            if (token.Value == "*" || token.Value == "+" || token.Value == "-" || token.Value == "/" || token.Value == "^")
            {
                if (CurrentParent.Items.Count == 0)
                {
                    // we need to handle the first item
                    var data = HandleSinglePartofExpression(tokens, valueofi_beforechanged);
                    data.obj.Name = "base";
                    if (!CurrentParent.Items.Any(x => x.IPosition == data.obj.IPosition))
                        CurrentParent.Items.Add(data.obj);
                }

                var rightsidedata = HandleSinglePartofExpression(tokens, i + 1);
                i = rightsidedata.returni-1;
                rightsidedata.obj.Name = token.Value switch
                {
                    "*" => "factor",
                    "+" => "add",
                    "-" => "subtract",
                    "^" => "raiseto",
                    "/" => "divide"
                };
                if (!CurrentParent.Items.Any(x => x.IPosition == rightsidedata.obj.IPosition))
                    CurrentParent.Items.Add(rightsidedata.obj);
            }
        }

        return i;
    }

    public int AddAssign(List<Token<LuaTokenType>> tokens, int i)
    {
        string name = tokens[i - 2].Value;
        LuaObject obj = new()
        {
            Name = name,
            FileName = FileName,
            Parent = CurrentParent,
            LineNumber = CurrentLineNumber,
        };

        i += 2;
        var nexttoken = tokens[i];

        if (nexttoken.Type == LuaTokenType.Quote)
        {
            var returndata = GetStringValue(tokens, i+1);
            obj.Value = returndata.Item1;
            i = returndata.Item2;
            obj.type = ObjType.String;
            CurrentParent.Items.Add(obj);
        }

        else if (nexttoken.Type == LuaTokenType.Item || nexttoken.Type == LuaTokenType.String || nexttoken.Type == LuaTokenType.Number || nexttoken.Type == LuaTokenType.OpenRoundBracket)
        {
            // check if this is a single line expression
            
            // steel = 3000\n
            // steel = 3000 }
            if (tokens[i + 1].Type == LuaTokenType.OpenSquareBracket)
            {
                var returndata = HandleSinglePartofExpression(tokens, i);
                obj.Value = returndata.obj.Value;
                i = returndata.returni;
                obj.type = ObjType.String;
                CurrentParent.Items.Add(obj);
            }
            else if (tokens[i + 1].Type == LuaTokenType.NewLine || tokens[i + 2].Type == LuaTokenType.CloseCurlyBracket)
            {
                obj.type = ObjType.StringForNumber;
                obj.Value = nexttoken.Value;
                CurrentParent.Items.Add(obj);
            }
            else
            {
                // this is a single line expression :sob:
                i = HandleExpression(tokens, i, name);
            }
        }

        else if (nexttoken.Type == LuaTokenType.OpenCurlyBracket)
        {
            i = AddTable(tokens, i+1, name);
        }

        else if (nexttoken.Type == LuaTokenType.OpenSquareBracket)
        {
            i = AddList(tokens, i + 2, name);
        }

        return i;
    }

    public LuaTable Parse(string content, string filename)
    {
        Objects = new();
        CurrentParent = Objects;

        FileName = filename;
        CurrentLineNumber = 1;

        var tokens = tokenizer.Tokenize(content.Replace("\r", ""));
        Console.WriteLine("");
        int i = -1;
        if (false)
        {
            foreach (var token in tokens)
            {
                i += 1;
                if (token.Type == LuaTokenType.Whitespace || token.Type == LuaTokenType.Comment)
                    continue;

                var value = token.Type == LuaTokenType.NewLine ? "" : token.Value;
                Console.Write($"[{i}]{token.Type} ({value}) ");

                if (token.Type == LuaTokenType.NewLine)
                {
                    Console.WriteLine("");
                    CurrentLineNumber += 1;
                }
            }
        }
        i = -1;
        CurrentLineNumber = 1;
        while (i < tokens.Count-1)
        {
            i += 1;
            var token = tokens[i];

            if (token.Type == LuaTokenType.Whitespace || token.Type == LuaTokenType.Comment)
                continue;

            var value = token.Type == LuaTokenType.NewLine ? "" : token.Value;
            //Console.Write($"{token.Type} ({value}) ");

            if (token.Type == LuaTokenType.NewLine)
            {
                CurrentLineNumber += 1;
            }

            if (token.Type == LuaTokenType.Equals)
                i = AddAssign(tokens, i);
            
        }

        return Objects;
    }
}