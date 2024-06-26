﻿using System.Text.RegularExpressions;
using QueryProcessor.Helper;
using QueryProcessor.Utils;
using Utils.Enums;
using Utils.Helper;

namespace QueryProcessor
{
    public class QueryProcessor
    {
        private static Dictionary<string, EntityType> _variables = null;
        private static Dictionary<string, List<string>> _queryComponents = null;

        private static void Initialize()
        {
            _variables = new Dictionary<string, EntityType>();
            _queryComponents = new Dictionary<string, List<string>>();
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

        private static List<string> CheckQuery(string query)
        {
            var errors = new List<string>();
            switch (query)
            {
                case var q when q.Contains(SyntaxDirectory.boolean):
                    errors.Add(ErrorDirectory.BooleanError);
                    break;
                case var q when q.Contains(SyntaxDirectory.affects):
                    errors.Add(ErrorDirectory.AffectsError);
                    break;
                case var q when q.Contains(SyntaxDirectory.pattern):
                    errors.Add(ErrorDirectory.PatternError);
                    break;
            }

            if (errors.Count > 0)
                return errors;

            string[] spearator = { SyntaxDirectory.SuchThat, SyntaxDirectory.With };
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
                    _variables.Add(variableParts[i], entityType);
            }
        }

        private static void ProcessSelectPart(string selectPart)
        {
            var splitSelectParts = Regex.Split(selectPart.ToLower(), $"({SyntaxDirectory.SuchThat})");
            var splitSelectPartsArrays = new List<string[]>();
            var mergedSelectParts = new List<string>();
            var finalSelectParts = new List<string>();
            _queryComponents.Add(SyntaxDirectory.SELECT, new List<string>());
            _queryComponents.Add(SyntaxDirectory.SUCHTHAT, new List<string>());
            _queryComponents.Add(SyntaxDirectory.WITH, new List<string>());


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
                        _queryComponents[SyntaxDirectory.SUCHTHAT].Add(sbs.Trim());
                }
                else if (part.StartsWith(SyntaxDirectory.With))
                {
                    substring = selectPart.Substring(index, part.Length).Substring(4).Trim();
                    substrings = substring.Split(separator, StringSplitOptions.RemoveEmptyEntries);
                    foreach (var sbs in substrings)
                        _queryComponents[SyntaxDirectory.WITH].Add(sbs.Trim());
                }
                else if (part.StartsWith(SyntaxDirectory.SELECT.ToLower()))
                {
                    substring = selectPart.Substring(index, part.Length).Substring(6).Trim();
                    substrings = substring.Split(',');
                    foreach (var sbs in substrings)
                        _queryComponents[SyntaxDirectory.SELECT].Add(sbs.Trim().Trim(new Char[] { '<', '>' }));
                }
            }
        }

        public static Dictionary<string, EntityType> GetQueryVariables()
        {
            return _variables;
        }

        public static Dictionary<string, List<string>> GetQueryDetails()
        {
            return _queryComponents;
        }

        public static Dictionary<string, List<string>> GetVariableAttributes()
        {
            var variableAttributes = new Dictionary<string, List<string>>();
            if (_queryComponents.ContainsKey(SyntaxDirectory.WITH))
            {
                foreach (var attribute in _queryComponents[SyntaxDirectory.WITH])
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
            return _queryComponents[SyntaxDirectory.SELECT];
        }

        public static EntityType GetVariableEnumType(string var)
        {
            try
            {
                return _variables[var];
            }
            catch (Exception e)
            {
                throw new Exception(SyntaxDirectory.ERROR);
            }
        }
    }
}