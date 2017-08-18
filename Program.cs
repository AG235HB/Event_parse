using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Linq;

namespace Event_parse
{
    class Program
    {
        static void Main(string[] args)
        {
            List<Expression> _Xpressions = new List<Expression>();
            List<Event> _events = new List<Event>();
            string outPath = String.Empty;

            try
            {
                XmlReaderSettings _settings = new XmlReaderSettings();
                _settings.IgnoreWhitespace = true;

                XmlDocument _config = new XmlDocument();
                _config.Load("config.xml");

                //считывание конфига
                XmlNode root = _config.FirstChild;
                if (root.HasChildNodes)
                    for (int i = 0; i < root.ChildNodes.Count; i++)
                        ReadConfig(root.ChildNodes[i], _Xpressions, outPath);

                //вывод регулярных выражений на экран
                foreach (Expression expr in _Xpressions)
                    expr.Print();
                
                Console.Write("Config: ");
                string _path = Console.ReadLine();
                using (StreamReader _sr = new StreamReader(_path))
                {
                    //обработка лога
                    string readerString = String.Empty;
                    while ((readerString = _sr.ReadLine()) != null)
                    {
                        Event _event = new Event();

                        foreach (Expression expr in _Xpressions)
                            RunExpressions(expr, _event, readerString);
                        _events.Add(_event);
                    }

                    if (outPath.Substring(outPath.Length-3).Equals("txt"))
                    {
                        StreamWriter writer = new StreamWriter(outPath, true);
                        foreach(Event evnt in _events)
                        {
                            writer.WriteLine("Event :");
                            evnt.WriteValues(writer);
                        }
                        writer.Flush();
                    }
                    else if (outPath.Substring(outPath.Length - 3).Equals("csv"))
                    {
                        var outData = new StringBuilder();
                        string line = String.Empty;

                        //заголовки для таблицы
                        WriteHeaders(line, _Xpressions);

                        outData.AppendLine(line);
                        line = String.Empty;

                        //наполнение таблицы
                        foreach (Event evnt in _events)
                        {
                            line = String.Empty;
                            WriteEvents(evnt, _Xpressions, line, outData);
                            outData.AppendLine(line);
                        }

                        File.WriteAllText("out.csv", outData.ToString());
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            Console.Write("Done!");
            Console.Read();
        }

        static private void ReadConfig(XmlNode node, List<Expression> expressions, string path)
        {
            if ((node.ParentNode.Name == "expressions")||(node.ParentNode.Name=="regex"))
            {
                Expression expr = new Expression();
                expr.name = node.Attributes[0].Value;
                expr.pattern = node.Attributes[1].Value;

                if (node.ParentNode.Name == "regex")
                    expressions[expressions.Count - 1].AddChild(expr);
                else
                    expressions.Add(expr);
            }

            if (node.Name == "output")
                for(int i=0;i<node.Attributes.Count;i++)
                {
                    switch (node.Attributes[i].Name)
                    {
                        //case "table":
                        //    table = bool.Parse(node.Attributes[i].Value);
                        //    break;
                        case "path":
                            path = node.Attributes[i].Value;
                            break;
                    }
                }

            if (node.HasChildNodes)
                for (int i = 0; i < node.ChildNodes.Count; i++)
                    ReadConfig(node.ChildNodes[i], expressions, path);
        }

        static private void RunExpressions(Expression expr, Event evnt, string rString)
        {
            string pattern = expr.pattern;
            if (Regex.IsMatch(rString, pattern))
            {
                string match = Regex.Match(rString, pattern).ToString().Trim(' ').Trim(':');
                evnt.AddValue(expr.name, match);

                if (expr.HasChildren())
                    foreach (Expression chExpr in expr.children)
                        RunExpressions(chExpr, evnt, rString);

                if (Regex.Match(rString, pattern).Length != rString.Length)
                    rString = rString.Remove(0, Regex.Match(rString, pattern).Length);
                else
                    return;
            }
        }

        static private void WriteHeaders(string line, List<Expression> expressions)
        {
            foreach (Expression expr in expressions)
            {
                if (!expr.Equals(expressions[expressions.Count - 1]))
                {
                    line += expr.name + ";";
                    if (expr.HasChildren())
                        WriteHeaders(line, expr.children);
                }
                else
                {
                    line += expr.name;
                    if (expr.HasChildren())
                    {
                        line += ";";
                        WriteHeaders(line, expr.children);
                    }
                }
            }
        }

        static private void WriteEvents(Event evnt, List<Expression> expressions, string line, StringBuilder outData)
        {
            foreach (Expression expr in expressions)
                if (evnt._values[expr.name] != null)
                    if (!expr.Equals(expressions[expressions.Count - 1]))
                    {
                        line += evnt._values[expr.name] + ";";
                        if (expr.HasChildren())
                            WriteEvents(evnt, expr.children, line, outData);
                    }
                    else
                    {
                        line += evnt._values[expr.name];
                        if (expr.HasChildren())
                        {
                            line += ";";
                            WriteEvents(evnt, expr.children, line, outData);
                        }
                    }
                else
                    line += ";";
            
        }

        static private List<Expression> GetExpressions(List<Expression> expressions, XmlReader reader)
        {
            string regexField = String.Empty;

            while (reader.Read())
            {
                if (reader.NodeType == XmlNodeType.Element && reader.Name.Equals("regex"))
                {
                    Expression expr = new Expression();

                    do
                    {
                        reader.Read();
                        switch (reader.NodeType)
                        {
                            case XmlNodeType.Element:
                                if (reader.Name.Equals("name") || reader.Name.Equals("pattern"))
                                    regexField = reader.Name;
                                else if (reader.Name.Equals("regex"))
                                    expr.children = GetExpressions(expr.children, reader);
                                continue;

                            case XmlNodeType.Text:
                                switch (regexField)
                                {
                                    case "name":
                                        expr.name = reader.Value;
                                        break;
                                    case "pattern":
                                        expr.pattern = reader.Value;
                                        break;
                                }
                                break;
                        }
                    }
                    while (!reader.Name.Equals("regex")||!reader.Name.Equals("None"));

                    expressions.Add(expr);

                    continue;
                }
            }

            return expressions;
        }
    }
}
