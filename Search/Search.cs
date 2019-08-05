using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


using System.Text.RegularExpressions;
using System.IO;

namespace Search
{
    class Search
    {
        Dictionary<string, KeyValuePair<int, int>> dictionary;

        string fileInvert, fileDictonary;

        FileStream invertFile;

        public Func<string, string> InvertList;

        public bool Error { get; private set; }

        public Search(string FileDictonary, string FileInvert, bool inDictonary = true)
        {
            fileInvert = FileInvert;
            fileDictonary = FileDictonary;

            invertFile = new FileStream(fileInvert, FileMode.Open);

            if (inDictonary)
            {
                InvertList = SearchInDictonary;
                DictonaryInit();
            }
            else InvertList = SearchInvertFile;

            SortedArray<KeyValuePair<string, long>>.Key = (elem) => long.Parse(elem.Key);
            SortedArray<KeyValuePair<string, long>>.ForEquals =
                (elem0, elem1) =>
                {
                    long value = elem1.Value;
                    if (elem0.Value > elem1.Value) value = elem0.Value;
                    return new KeyValuePair<string, long>(elem0.Key, value);
                };
        }

        public void DictonaryInit()
        {
            dictionary = new Dictionary<string, KeyValuePair<int, int>>();

            using (var file = new StreamReader(fileDictonary))
            {
                while (!file.EndOfStream)
                {
                    string[] parseStr = file.ReadLine().Split(' ', ',');
                    dictionary.Add(parseStr[0], new KeyValuePair<int, int>(int.Parse(parseStr[1]), int.Parse(parseStr[2])));
                }
            }

            GC.Collect();
        }

        public string SearchInDictonary(string word)
        {
            if (dictionary.ContainsKey(word))
            {
                var posWord = dictionary[word];
                invertFile.Seek(posWord.Key, SeekOrigin.Begin);

                Byte[] readBytes = new Byte[posWord.Value];
                invertFile.Read(readBytes, 0, readBytes.Length);

                return Encoding.UTF8.GetString(readBytes);
            }

            return null;
        }

        public string SearchInvertFile(string word)
        {
            string res = "";
            long last = invertFile.Length - 1, first = 0;
            long seek = 0;

            string lineInFile()
            {
                invertFile.Seek(seek, SeekOrigin.Begin);

                LinkedList<string> chars = new LinkedList<string>();
                string ch = "";

                for (Byte[] Char = new Byte[2]; ch != Environment.NewLine || invertFile.Position - 2 < 0;
                    ch = Encoding.Unicode.GetString(Char))
                {
                    chars.AddFirst(ch);
                    invertFile.Seek(-2, SeekOrigin.Current);
                    Char[0] = (Byte)invertFile.ReadByte();
                    Char[1] = (Byte)invertFile.ReadByte();
                    invertFile.Seek(-2, SeekOrigin.Current);
                }

                ch = "";
                invertFile.Seek(seek + 1, SeekOrigin.Begin);

                for (Byte[] Char = new Byte[2]; ch != Environment.NewLine || invertFile.Position + 2 <invertFile.Length;
                    ch = Encoding.Unicode.GetString(Char))
                {
                    chars.AddLast(ch);
                    Char[0] = (Byte)invertFile.ReadByte();
                    Char[1] = (Byte)invertFile.ReadByte();
                }

                return string.Join("", chars);
            }

            while (true)
            {
                seek = (last - first) / 2 + first; 

                res = lineInFile();
                string wordLine = res.Substring(0, res.IndexOf(" ") + 1);

                int compare = wordLine.CompareTo(word);

                if (compare == 0)
                    return res;
                else if (compare == 1)
                    last = seek - 1;
                else
                    first = seek + 1;
            }

        }

        public string[] ReducQuery(string str)
        {
            string[] parse = TryParse(str);

            if (parse == null) return null;

            var stack = new Stack<KeyValuePair<int, Dictionary<string, int>>>();

            var dictonary = new Dictionary<string, int>();

            for (int i = 0; i < parse.Length; i++)
            {
                if (parse[i] == "(")
                {
                    stack.Push(new KeyValuePair<int, Dictionary<string, int>>(i, dictonary));
                    dictonary = new Dictionary<string, int>();
                }
                else if (parse[i] == ")")
                {
                    var prevDictonary = stack.Peek();
                    int j = prevDictonary.Key - 1;

                    if ((j == -1 || parse[j] == "OR" || parse[j] == "(")
                        && (i + 1 == parse.Length || parse[i + 1] == "OR" || parse[i + 1] == ")"))
                    {
                        parse[j + 1] = null;
                        parse[i] = null;

                        foreach (var keyValue in prevDictonary.Value)
                        {
                            if (!dictonary.ContainsKey(keyValue.Key))
                                dictonary.Add(keyValue.Key, keyValue.Value);
                            else
                            {
                                var ii = dictonary[keyValue.Key];
                                parse[ii] = null;
                                parse[ii - 1] = null;
                            }
                        }
                    }
                    else dictonary = prevDictonary.Value;
                }
                else if (parse[i] != "AND" && parse[i] != "OR")
                {
                    int plusI = i + 1, minusI = i - 1;

                    while (parse.Length > plusI && parse[plusI] == null) plusI++;
                    while (minusI > -1 && parse[minusI] == null) minusI--;

                    if ((parse.Length == plusI || parse[plusI] == "OR" || parse[plusI] == ")")
                        && (minusI == -1 || parse[minusI] == "OR" || parse[minusI] == "("))
                    {
                        if (dictonary.ContainsKey(parse[i]))
                        {
                            parse[i] = null;
                            parse[i - 1] = null;
                        }
                        else
                            dictonary.Add(parse[i], i);
                    }
                }
            }

            return parse.Where((elem) => elem != null).ToArray();
        }

        public string[] TryParse(string str)
        {
            LinkedList<string> parseStr = new LinkedList<string>();
            Regex reg = new Regex(@"\w+|AND|OR|NOT|\(|\)|[\s\t]+");
            int breakes = 0;
            bool Operator = false, not = false, isOperator = false;

            Dictionary<string, bool> isOperators = new Dictionary<string, bool>();
            isOperators.Add("AND", true);
            isOperators.Add("OR", true);
            isOperators.Add(")", true);

            Match match = null;

            for (int i = 0; i < str.Length; i = match.Index + match.Value.Length)
            {
                match = reg.Match(str, i);

                if (!match.Success) return null;

                if (match.Value == @" " || match.Value == @"    ") continue;
                isOperator = isOperators.ContainsKey(match.Value);
                if (Operator && !isOperator || !Operator && isOperator) return null;

                if (match.Value == "(")
                    breakes++;
                else if (match.Value == ")")
                    breakes--;
                else if (match.Value != "NOT")
                    Operator = !Operator;
                else if (not) continue;

                parseStr.AddLast(match.Value);
            }

            if (breakes != 0 || match.Value == "OR" || match.Value == "AND" || match.Value == "NOT") return null;

            return parseStr.ToArray();
        }   

        public SortedArray<KeyValuePair<string, long>> SearchBool(string str)
        {
            string[] parse = TryParse(str);

            if (parse == null) return null;

            var stack = new Stack<LinkedList<KeyValuePair<string, SortedArray<KeyValuePair<string, long>>>>>();
            var stackIndexs = new Stack<int>();

            var listOpertors = new LinkedList<KeyValuePair<string, SortedArray<KeyValuePair<string, long>>>>();

            for (int i = 0; i < parse.Length; i++)
            {
                if (parse[i] == "(")
                {
                    stackIndexs.Push(i);
                    stack.Push(listOpertors);
                    listOpertors = new LinkedList<KeyValuePair<string, SortedArray<KeyValuePair<string, long>>>>();
                }
                else
                {
                    SortedArray<KeyValuePair<string, long>> sortedArray;

                    if (parse[i] == ")")
                    {
                        sortedArray = ResOperations(listOpertors);

                        int indexPrev = stackIndexs.Peek();
                        if (indexPrev != 0 && parse[indexPrev - 1] == "NOT") sortedArray.No();

                        listOpertors = stack.Peek();
                    }
                    else if (parse[i] != "AND" && parse[i] != "OR" && parse[i] != "NOT")
                    {
                        sortedArray = new SortedArray<KeyValuePair<string, long>>(Algo.InvertList(InvertList(parse[i])), 
                            i != 0 && parse[i - 1] == "NOT");
                    }
                    else continue;

                    string Operator = null;
                    if (i + 1 < parse.Length && (parse[i + 1] == "AND" || parse[i + 1] == "OR"))
                        Operator = parse[i + 1];

                    listOpertors.AddLast(new KeyValuePair<string, SortedArray<KeyValuePair<string, long>>>(Operator, sortedArray));
                }
            }

            return ResOperations(listOpertors);
        }

        public SortedArray<KeyValuePair<string, long>> ResOperations(
            LinkedList<KeyValuePair<string, SortedArray<KeyValuePair<string, long>>>> valueOpreations)
        {
            var res = new SortedArray<KeyValuePair<string, long>>(new KeyValuePair<string, long>[0]);
            SortedArray<KeyValuePair<string, long>> andRes = null;

            bool and = false;

            var ands = new LinkedList<SortedArray<KeyValuePair<string, long>>>();

            try
            {
                foreach (var elem in valueOpreations)
                {
                    if (and)
                    {
                        if (elem.Value.Length < andRes.Length && !elem.Value.Not)
                        {
                            ands.AddLast(andRes);
                            andRes = elem.Value;                          
                        }
                        else ands.AddLast(elem.Value);
                        
                        if (!(and = elem.Key == "AND"))
                        {
                            foreach (var andElem in ands) andRes = andRes.Intersect(andElem);
                            res = res.Union(andRes);
                            ands = new LinkedList<SortedArray<KeyValuePair<string, long>>>();
                        }
                    }
                    else if (and = elem.Key == "AND")
                    {
                        andRes = elem.Value;
                    }
                    else res = res.Union(elem.Value);
                }

                if (res.Not)
                {
                    res = new SortedArray<KeyValuePair<string, long>>(All().Except(res.Array));
                }
            }
            catch
            {
                Error = true;
            }

            return res;
        }

        public SortedArray<KeyValuePair<string, long>> All()
        {
            SortedArray<KeyValuePair<string, long>> res
                 = new SortedArray<KeyValuePair<string, long>>(new KeyValuePair<string, long>[0]);

            try
            {
                foreach (var word in dictionary)
                {
                    res = res.Union(new SortedArray<KeyValuePair<string, long>>(Algo.InvertList(InvertList(word.Key))));
                }
            }
            catch
            {
                Error = true;
            }

            return res;
        }

        public void Close()
        {
            invertFile.Close();
            invertFile.Dispose();
            GC.Collect();
        }
    }
}
