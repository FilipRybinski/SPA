using System.Text.RegularExpressions;
using QueryProcessor.Helper;
using QueryProcessor.Utils;
using Utils.Enums;
using Utils.Helper;

namespace QueryProcessor
{
    public class QueryProcessor
    {
        private static Dictionary<string, EntityType> variables = null;
        private static Dictionary<string, List<string>> queryComponents = null;

        private static void Initialize()
        {
            variables = new Dictionary<string, EntityType>();
            queryComponents = new Dictionary<string, List<string>>();

        }
        public static List<string> ProcessQuery(String query, bool testing = false)
        {
            Initialize();
            query = Regex.Replace(query, @"\t|\n|\r", "");


            var queryParts = query.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);

            for (var i = 0; i < queryParts.Length - 1; i++)
            {
                DecodeVarDefinitionAndInsertToDict(queryParts[i].Trim()); 
            }

            var selectPart = queryParts[queryParts.Length - 1];
            var errors = CheckQuery(selectPart.ToLower());
            if (errors.Count > 0)
                return errors;
            ProcessSelectPart(selectPart.Trim()); 
            try
            {
                return QueryParser.GetData(testing);
            }
            catch (ArgumentException e)
            {
                errors = new List<string> { e.Message };
                return errors;
            }

        }

        public static List<string> CheckQuery(string query)
        {
            var errors = new List<string>();
            if (query.Contains(SyntaxDirectory.boolean))
                errors.Add(ErrorDirectory.BooleanError);
            else if (query.Contains(SyntaxDirectory.affects))
                errors.Add(ErrorDirectory.AffectsError);
            else if (query.Contains(SyntaxDirectory.pattern))
                errors.Add(ErrorDirectory.PatternError);

            if (errors.Count > 0)
                return errors;

            String[] spearator = { SyntaxDirectory.SuchThat, SyntaxDirectory.With };
            var partsList = query.Split(spearator, StringSplitOptions.RemoveEmptyEntries);
            if (partsList[0].Contains(","))
                errors.Add(SyntaxDirectory.ERROR);

            return errors;
        }

        private static void DecodeVarDefinitionAndInsertToDict(String variableDefinition)
        {
            var variableParts = variableDefinition.Replace(" ", ",").Split(',');
            var varTypeAsString = variableParts[0]; //Typ jako string (statement, assign, wgile albo procedure)
            EntityType entityType;

            switch (varTypeAsString.ToLower())
            {
                case SyntaxDirectory.Stmt:
                    entityType = EntityType.Statement;
                    break;
                case SyntaxDirectory.Assign:
                    entityType = EntityType.Assign;
                    break;
                case SyntaxDirectory.While:
                    entityType = EntityType.While;
                    break;
                case SyntaxDirectory.Procedure:
                    entityType = EntityType.Procedure;
                    break;
                case SyntaxDirectory.Variable:
                    entityType = EntityType.Variable;
                    break;
                case SyntaxDirectory.Constant:
                    entityType = EntityType.Constant;
                    break;
                case SyntaxDirectory.ProgLine:
                    entityType = EntityType.Prog_line;
                    break;
                case SyntaxDirectory.If:
                    entityType = EntityType.If;
                    break;
                case SyntaxDirectory.Call:
                    entityType = EntityType.Call;
                    break;
                default:
                    throw new Exception(SyntaxDirectory.ERROR);
            }

            for (var i = 1; i < variableParts.Length; i++)
            {
                if (variableParts[i] != "")
                    variables.Add(variableParts[i], entityType);
            }
        }

        private static void ProcessSelectPart(string selectPart)
        {
            var splitSelectParts = Regex.Split(selectPart.ToLower(), $"({SyntaxDirectory.SuchThat})");
            var splitSelectPartsArrays = new List<string[]>();
            var mergedSelectParts = new List<string>();
            var finalSelectParts = new List<string>();
            queryComponents.Add(SyntaxDirectory.SELECT, new List<string>());
            queryComponents.Add(SyntaxDirectory.SUCHTHAT, new List<string>());
            queryComponents.Add(SyntaxDirectory.WITH, new List<string>());


            foreach (var part in splitSelectParts)
                splitSelectPartsArrays.Add(Regex.Split(part, $"({SyntaxDirectory.With})"));

            foreach (var parts in splitSelectPartsArrays)
                foreach (var part in parts)
                    mergedSelectParts.Add(part);

            finalSelectParts.Add(mergedSelectParts[0]);
            for (var i = 1; i < mergedSelectParts.Count; i += 2)
                finalSelectParts.Add(mergedSelectParts[i] + mergedSelectParts[i + 1]);


            foreach (var part in finalSelectParts)
            {
                var index = selectPart.ToLower().IndexOf(part);

                var substring = "";
                var substrings = Array.Empty<string>();
                var separator = $" {SyntaxDirectory.And} ";
                if (part.StartsWith(SyntaxDirectory.SuchThat))
                {
                    substring = selectPart.Substring(index, part.Length).Substring(9).Trim();
                    string pattern = @"(?<!\w)and(?!\w)";
                    substrings = Regex.Split(substring.ToLower(), pattern, RegexOptions.IgnoreCase);
                    foreach (var sbs in substrings)
                        queryComponents[SyntaxDirectory.SUCHTHAT].Add(sbs.Trim());
                }
                else if (part.StartsWith(SyntaxDirectory.With))
                {
                    substring = selectPart.Substring(index, part.Length).Substring(4).Trim();
                    substrings = substring.Split(separator, StringSplitOptions.RemoveEmptyEntries);
                    foreach (var sbs in substrings)
                        queryComponents[SyntaxDirectory.WITH].Add(sbs.Trim());
                }
                else if (part.StartsWith(SyntaxDirectory.SELECT.ToLower()))
                {
                    substring = selectPart.Substring(index, part.Length).Substring(6).Trim();
                    substrings = substring.Split(',');
                    foreach (var sbs in substrings)
                        queryComponents[SyntaxDirectory.SELECT].Add(sbs.Trim().Trim(new Char[] { '<', '>' }));
                }
            }


        }
      

        private static void PrintParsingResults()
        {
            foreach (var oneVar in variables)
            {
                Console.WriteLine("\t{0} - {1}", oneVar.Key, oneVar.Value);
            }

            foreach (var oneDetail in queryComponents)
            {
                Console.WriteLine("{0}:", oneDetail.Key);
                foreach (var word in oneDetail.Value)
                {
                    Console.WriteLine("\t\"{0}\"", word);
                }

            }
        }

        public static Dictionary<string, EntityType> GetQueryVariables()
        {
            return variables;
        }

        public static Dictionary<string, List<string>> GetQueryDetails()
        {
            return queryComponents;
        }

        public static Dictionary<string, List<string>> GetVariableAttributes()
        {
            var variableAttributes = new Dictionary<string, List<string>>();
            if (queryComponents.ContainsKey(SyntaxDirectory.WITH))
            {
                foreach (var attribute in queryComponents[SyntaxDirectory.WITH])
                {
                    var attribtueWithValue = attribute.Split('=');
                    if (!variableAttributes.ContainsKey(attribtueWithValue[0].Trim()))
                    {
                        variableAttributes[attribtueWithValue[0].Trim()] = new List<string>();
                    }
                    variableAttributes[attribtueWithValue[0].Trim()].Add(attribtueWithValue[1].Trim());
                }
            }
            return variableAttributes;
        }

        public static List<string> GetVariableToSelect()
        {
            return queryComponents[SyntaxDirectory.SELECT];
        }

        public static EntityType GetVariableEnumType(string var)
        {
            try
            {
                return variables[var];
            }
            catch (Exception e)
            {
                throw new Exception(SyntaxDirectory.ERROR);
            }
        }
    }
}
