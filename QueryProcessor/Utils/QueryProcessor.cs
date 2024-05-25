using System.Text.RegularExpressions;
using Parser.AST.Enums;

namespace QueryProcessor.Utils
{
    public class QueryProcessor
    {
        private static Dictionary<string, EntityTypeEnum> vars = null;
        private static Dictionary<string, List<string>> queryDetails = null;

        private static void Init()
        {
            vars = new Dictionary<string, EntityTypeEnum>();
            queryDetails = new Dictionary<string, List<string>>();

        }
        public static List<string> ProcessQuery(String query, bool testing = false)
        {
            Init();
            query = Regex.Replace(query, @"\t|\n|\r", ""); //usunięcie znaków przejścia do nowej linii i tabulatorów


            string[] queryParts = query.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);

            for (int i = 0; i < queryParts.Length - 1; i++)
            {
                DecodeVarDefinitionAndInsertToDict(queryParts[i].Trim()); //dekoduje np. assign a, a1;
            }

            String selectPart = queryParts[queryParts.Length - 1];
            List<string> errors;
            errors = CheckQuery(selectPart.ToLower());
            if (errors.Count > 0)
                return errors;
            ProcessSelectPart(selectPart.Trim()); //dekoduje część "Select ... "
            try
            {
                return QueryDataGetter.GetData(testing);
            }
            catch (ArgumentException e)
            {
                errors = new List<string>();
                errors.Add(e.Message);
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
            String[] strlist = query.Split(spearator, StringSplitOptions.RemoveEmptyEntries);
            if (strlist[0].Contains(","))
                errors.Add("Tuple not supported");

            return errors;
        }

        private static void DecodeVarDefinitionAndInsertToDict(String varsDefinition)
        {
            string[] varsParts = varsDefinition.Replace(" ", ",").Split(',');
            string varTypeAsString = varsParts[0]; //Typ jako string (statement, assign, wgile albo procedure)
            EntityTypeEnum typeEnum;

            switch (varTypeAsString.ToLower())
            {
                case "stmt":
                    typeEnum = EntityTypeEnum.Statement;
                    break;
                case "assign":
                    typeEnum = EntityTypeEnum.Assign;
                    break;
                case "while":
                    typeEnum = EntityTypeEnum.While;
                    break;
                case "procedure":
                    typeEnum = EntityTypeEnum.Procedure;
                    break;
                case "variable":
                    typeEnum = EntityTypeEnum.Variable;
                    break;
                case "constant":
                    typeEnum = EntityTypeEnum.Constant;
                    break;
                case "prog_line":
                    typeEnum = EntityTypeEnum.Prog_line;
                    break;
                case "if":
                    typeEnum = EntityTypeEnum.If;
                    break;
                case "call":
                    typeEnum = EntityTypeEnum.Call;
                    break;
                default:
                    throw new System.ArgumentException(string.Format("# Wrong argument: \"{0}\"", varTypeAsString));
            }

            for (int i = 1; i < varsParts.Length; i++)
            {
                if (varsParts[i] != "") //tak nawet takie coś jak "" dodawało...
                    vars.Add(varsParts[i], typeEnum);
            }
        }

        private static void ProcessSelectPart(string selectPart)
        {
            string[] splittedselectPart = Regex.Split(selectPart.ToLower(), "(such that)");
            List<string[]> splittedselectPart2 = new List<string[]>();
            List<string> splittedselectPart3 = new List<string>();
            List<string> splittedselectPart4 = new List<string>();
            queryDetails.Add("SELECT", new List<string>());
            queryDetails.Add("SUCH THAT", new List<string>());
            queryDetails.Add("WITH", new List<string>());


            foreach (string s in splittedselectPart)
                splittedselectPart2.Add(Regex.Split(s, "(with)"));

            foreach (string[] ss in splittedselectPart2)
                foreach (string s in ss)
                    splittedselectPart3.Add(s);

            splittedselectPart4.Add(splittedselectPart3[0]);
            for (int i = 1; i < splittedselectPart3.Count; i += 2)
                splittedselectPart4.Add(splittedselectPart3[i] + splittedselectPart3[i + 1]);


            foreach (string s in splittedselectPart4)
            {
               
                int index = selectPart.ToLower().IndexOf(s);

                string substring;
                string[] substrings;
                string[] separator = { " and ", " And ", " ANd ", " AND ", " anD ", " aND ", " aNd ", " AnD " };
                if (s.StartsWith("such that"))
                {
                    substring = selectPart.Substring(index, s.Length).Substring(9).Trim();
                    substrings = substring.Split(separator, StringSplitOptions.RemoveEmptyEntries);
                    foreach (string sbs in substrings)
                        queryDetails["SUCH THAT"].Add(sbs.Trim());
                }
                else if (s.StartsWith("with"))
                {
                    substring = selectPart.Substring(index, s.Length).Substring(4).Trim();
                    substrings = substring.Split(separator, StringSplitOptions.RemoveEmptyEntries);
                    foreach (string sbs in substrings)
                        queryDetails["WITH"].Add(sbs.Trim());
                }
                else if (s.StartsWith("select"))
                {
                    substring = selectPart.Substring(index, s.Length).Substring(6).Trim();
                    substrings = substring.Split(',');
                    foreach (string sbs in substrings)
                        queryDetails["SELECT"].Add(sbs.Trim().Trim(new Char[] { '<', '>' }));
                }
            }


        }
      

        private static void PrintParsingResults()
        {
            Console.WriteLine("QUERY VARIABLES:");
            foreach (KeyValuePair<string, EntityTypeEnum> oneVar in vars)
            {
                Console.WriteLine("\t{0} - {1}", oneVar.Key, oneVar.Value);
            }

            foreach (KeyValuePair<string, List<string>> oneDetail in queryDetails)
            {
                Console.WriteLine("{0}:", oneDetail.Key);
                foreach (string word in oneDetail.Value)
                {
                    Console.WriteLine("\t\"{0}\"", word);
                }

            }
        }

        public static Dictionary<string, EntityTypeEnum> GetQueryVars()
        {
            return vars;
        }

        public static Dictionary<string, List<string>> GetQueryDetails()
        {
            return queryDetails;
        }

        public static Dictionary<string, List<string>> GetVarAttributes()
        {
            Dictionary<string, List<string>> varAttributes = new Dictionary<string, List<string>>();
            if (queryDetails.ContainsKey("WITH"))
            {
                foreach (string attribute in queryDetails["WITH"])
                {
                    string[] attribtueWithValue = attribute.Split('=');
                    if (!varAttributes.ContainsKey(attribtueWithValue[0].Trim()))
                    {
                        varAttributes[attribtueWithValue[0].Trim()] = new List<string>();
                    }
                    varAttributes[attribtueWithValue[0].Trim()].Add(attribtueWithValue[1].Trim());
                }
            }
            return varAttributes;
        }

        public static List<string> GetVarToSelect()
        {
            return queryDetails["SELECT"];
        }

        public static EntityTypeEnum GetVarEnumType(string var)
        {
            try
            {
                return vars[var];
            }
            catch (Exception e)
            {
                throw new ArgumentException(string.Format("# Wrong argument: \"{0}\"", var));
            }
        }
    }
}
