using System.Text.RegularExpressions;
using QueryProcessor.Utils;
using Utils.Enums;

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
            query = Regex.Replace(query, @"\t|\n|\r", ""); //usunięcie znaków przejścia do nowej linii i tabulatorów


            string[] queryParts = query.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);

            for (int i = 0; i < queryParts.Length - 1; i++)
            {
                DecodeVarDefinitionAndInsertToDict(queryParts[i].Trim()); //dekoduje np. assign a, a1;
            }

            String selectPart = queryParts[queryParts.Length - 1];
            List<string> errors = CheckQuery(selectPart.ToLower());
            if (errors.Count > 0)
                return errors;
            ProcessSelectPart(selectPart.Trim()); //dekoduje część "Select ... "
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
            List<string> errors = new List<string>();
            if (query.Contains("boolean"))
                errors.Add("BOOLEAN not supported");
            else if (query.Contains("affects"))
                errors.Add("Affects not supported");
            else if (query.Contains("pattern"))
                errors.Add("Pattern not supported");

            if (errors.Count > 0)
                return errors;

            String[] spearator = { "such that", "with" };
            String[] partsList = query.Split(spearator, StringSplitOptions.RemoveEmptyEntries);
            if (partsList[0].Contains(","))
                errors.Add("Tuple not supported");

            return errors;
        }

        private static void DecodeVarDefinitionAndInsertToDict(String variableDefinition)
        {
            string[] variableParts = variableDefinition.Replace(" ", ",").Split(',');
            string varTypeAsString = variableParts[0]; //Typ jako string (statement, assign, wgile albo procedure)
            EntityType entityType;

            switch (varTypeAsString.ToLower())
            {
                case "stmt":
                    entityType = EntityType.Statement;
                    break;
                case "assign":
                    entityType = EntityType.Assign;
                    break;
                case "while":
                    entityType = EntityType.While;
                    break;
                case "procedure":
                    entityType = EntityType.Procedure;
                    break;
                case "variable":
                    entityType = EntityType.Variable;
                    break;
                case "constant":
                    entityType = EntityType.Constant;
                    break;
                case "prog_line":
                    entityType = EntityType.Prog_line;
                    break;
                case "if":
                    entityType = EntityType.If;
                    break;
                case "call":
                    entityType = EntityType.Call;
                    break;
                default:
                    throw new System.ArgumentException(string.Format("# Wrong argument: \"{0}\"", varTypeAsString));
            }

            for (int i = 1; i < variableParts.Length; i++)
            {
                if (variableParts[i] != "") //tak nawet takie coś jak "" dodawało...
                    variables.Add(variableParts[i], entityType);
            }
        }

        private static void ProcessSelectPart(string selectPart)
        {
            string[] splitSelectParts = Regex.Split(selectPart.ToLower(), "(such that)");
            List<string[]> splitSelectPartsArrays = new List<string[]>();
            List<string> mergedSelectParts = new List<string>();
            List<string> finalSelectParts = new List<string>();
            queryComponents.Add("SELECT", new List<string>());
            queryComponents.Add("SUCH THAT", new List<string>());
            queryComponents.Add("WITH", new List<string>());


            foreach (string part in splitSelectParts)
                splitSelectPartsArrays.Add(Regex.Split(part, "(with)"));

            foreach (string[] parts in splitSelectPartsArrays)
                foreach (string part in parts)
                    mergedSelectParts.Add(part);

            finalSelectParts.Add(mergedSelectParts[0]);
            for (int i = 1; i < mergedSelectParts.Count; i += 2)
                finalSelectParts.Add(mergedSelectParts[i] + mergedSelectParts[i + 1]);


            foreach (string part in finalSelectParts)
            {
               
                int index = selectPart.ToLower().IndexOf(part);

                string substring;
                string[] substrings;
                string[] separator = { " and ", " And ", " ANd ", " AND ", " anD ", " aND ", " aNd ", " AnD " };
                if (part.StartsWith("such that"))
                {
                    substring = selectPart.Substring(index, part.Length).Substring(9).Trim();
                    substrings = substring.Split(separator, StringSplitOptions.RemoveEmptyEntries);
                    foreach (string sbs in substrings)
                        queryComponents["SUCH THAT"].Add(sbs.Trim());
                }
                else if (part.StartsWith("with"))
                {
                    substring = selectPart.Substring(index, part.Length).Substring(4).Trim();
                    substrings = substring.Split(separator, StringSplitOptions.RemoveEmptyEntries);
                    foreach (string sbs in substrings)
                        queryComponents["WITH"].Add(sbs.Trim());
                }
                else if (part.StartsWith("select"))
                {
                    substring = selectPart.Substring(index, part.Length).Substring(6).Trim();
                    substrings = substring.Split(',');
                    foreach (string sbs in substrings)
                        queryComponents["SELECT"].Add(sbs.Trim().Trim(new Char[] { '<', '>' }));
                }
            }


        }
      

        private static void PrintParsingResults()
        {
            Console.WriteLine("QUERY VARIABLES:");
            foreach (KeyValuePair<string, EntityType> oneVar in variables)
            {
                Console.WriteLine("\t{0} - {1}", oneVar.Key, oneVar.Value);
            }

            foreach (KeyValuePair<string, List<string>> oneDetail in queryComponents)
            {
                Console.WriteLine("{0}:", oneDetail.Key);
                foreach (string word in oneDetail.Value)
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
            Dictionary<string, List<string>> variableAttributes = new Dictionary<string, List<string>>();
            if (queryComponents.ContainsKey("WITH"))
            {
                foreach (string attribute in queryComponents["WITH"])
                {
                    string[] attribtueWithValue = attribute.Split('=');
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
            return queryComponents["SELECT"];
        }

        public static EntityType GetVariableEnumType(string var)
        {
            try
            {
                return variables[var];
            }
            catch (Exception e)
            {
                throw new ArgumentException(string.Format("# Wrong argument: \"{0}\"", var));
            }
        }
    }
}
