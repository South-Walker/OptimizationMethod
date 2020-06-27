#define DEBUG
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.IO;

namespace OptimizationMethod
{
    class Secret
    {
        int GuessCount = 0;
        string value;
        public Secret(string val)
        {
            value = val;
        }
        public int Guess(string word)
        {
            GuessCount++;
            return CalcUtil.CalcHit(word, value);
        }
        public bool isSecret(string a)
        {
#if DEBUG
            Console.WriteLine("secret is \"{0}\",answer is \"{0}\"", value, a);
            Console.WriteLine("solve with {0} guess", GuessCount);
#endif
            return string.Equals(value, a);
        }
        public int HowManyTimeBeAsked()
        {
            return GuessCount;
        }
    }
    static class ValideUtil
    {
        static Random r;
        public static Question[] CreateValidation(int questionCount,int strlen, int dictionaryScale, int seed = 1996)
        {
            r = new Random(1996);
            Question[] questions = new Question[questionCount]; 
            string[] dict = CreateRandomDictionary(strlen, dictionaryScale);
            for (int i = 0; i < questionCount; i++)
            {
                int secret = r.Next(0, dict.Length);
                questions[i] = new Question(dict[secret], dict);
            }
            return questions;
        }
        private static string[] CreateRandomDictionary(int strlen, int scale)
        {
            //string[] dictionary = new string[scale];
            List<string> dictionary = new List<string>();
            for (int i = 0; i < scale; i++)
            {
                StringBuilder sb = new StringBuilder(strlen);
                for (int j = 0; j < strlen; j++)
                {
                    sb.Append((char)('a' + r.Next(0, 26)));
                }
                dictionary.Add(sb.ToString());
            }
            return dictionary.Distinct().ToArray();
        }
    }
    static class CalcUtil
    {
        public static int ZeroNorm(string a, string b)
        {
            int r = 0;
            int l = Math.Max(a.Length, b.Length);
            int s = Math.Min(a.Length, b.Length);
            for (int i = 0; i < s; i++)
            {
                if (a[i] != b[i])
                {
                    r++;
                }
            }
            return r;
        }
        public static int CalcHit(string a, string b)
        {
            int r = 0;
            int l = Math.Max(a.Length, b.Length);
            int s = Math.Min(a.Length, b.Length);
            for (int i = 0; i < s; i++)
            {
                if (a[i] == b[i])
                {
                    r++;
                }
            }
            return r;
        }
    }
    class Question
    {
        Secret Host;
        string[] Dictionary { get; }
        int strLen;
        bool[] isPossible;
        int PossibleWordsCount { get; set; }
#if DEBUG
        public Question(string ans, string[] wordlist) : this(new Secret(ans), wordlist)
        {

        }
#endif
        public Question(Secret ans, string[] wordlist)
        {
            Host = ans;
            Dictionary = wordlist;
            isPossible = new bool[wordlist.Length];
            PossibleWordsCount = wordlist.Length;
            for (int i = 0; i < isPossible.Length; i++)
            {
                isPossible[i] = true;
            }
            strLen = 0;
            for (int i = 0; i < Dictionary.Length; i++)
            {
                strLen = Math.Max(Dictionary[i].Length, strLen);
            }
        }
        private string FirstPossibilityWord()
        {
            for (int i = 0; i < Dictionary.Length; i++)
            {
                if (isPossible[i])
                    return Dictionary[i];
            }
            return null;
        }
        private void Choose(string ans, int hitcount)
        {
            for (int i = 0; i < Dictionary.Length; i++)
            {
                if (isPossible[i])
                {
                    if (!GateFunction(Dictionary[i], hitcount, ans))
                    {
                        isPossible[i] = false;
                        PossibleWordsCount--;
                    }
                }
            }
            strLen = 0;
            for (int i = 0; i < Dictionary.Length; i++)
            {
                if (isPossible[i])
                {
                    strLen = Math.Max(Dictionary[i].Length, strLen);
                }
            }
        }
        private string SelectByMinimumExpectedValue()
        { 
            string minExpectionWord = "";
            double minExpectionValue = double.MaxValue;

            string ak;
            double expection;


            List<string>[] dicts = new List<string>[strLen + 1];
            for (int i = 0; i < dicts.Length; i++)
            {
                dicts[i] = new List<string>();
            }
            for (int i = 0; i < Dictionary.Length; i++)
            {
                for (int j = 0; j < dicts.Length; j++)
                {
                    dicts[j].Clear();
                }
                if (!isPossible[i])
                {
                    continue;
                }
                ak = Dictionary[i];
                for (int j = 0; j < Dictionary.Length; j++)
                {
                    if (!isPossible[j])
                    {
                        continue;
                    }
                    dicts[CalcUtil.CalcHit(Dictionary[j], ak)].Add(Dictionary[j]);
                }
                expection = 0;
                for (int k = 0; k < dicts.Length; k++)
                {
                    double Probability = dicts[k].Count / (double)PossibleWordsCount;
                    //显然dicts[k]内任一元素都可以通过门函数
                    expection += Probability * e(dicts[k]);
                }
                if (expection < minExpectionValue)
                {
                    minExpectionValue = expection;
                    minExpectionWord = ak;
                }
            }
            return minExpectionWord;
        }
        private static bool GateFunction(string Dki, int hit, string ak)
        {
            return (hit == CalcUtil.CalcHit(Dki, ak));
        }
        private static int e(IEnumerable<string> dict)
        {
            int r = 0;
            foreach (string di in dict)
            {
                foreach (string dj in dict)
                {
                    r += CalcUtil.ZeroNorm(di, dj);
                }
            }
            return r;
        }
        private string SelectByPossibility()
        {
            StringBuilder sb = new StringBuilder(strLen);
            for (int i = 0; i < strLen; i++)
            {
                int[] count = new int[26];
                for (int j = 0; j < Dictionary.Length; j++)
                {
                    if (isPossible[j] && (Dictionary[j].Length > i)) 
                    {
                        count[Dictionary[j][i] - 'a']++;
                    }
                }
                int maxvalue=0, maxindex=0;
                for (int j = 0; j < count.Length; j++)
                {
                    if (count[j] > maxvalue)
                    {
                        maxvalue = count[j];
                        maxindex = j;
                    }
                }
                sb.Append((char)('a' + maxindex));
            }
            string best = sb.ToString();
            int mostsimilar = 0;
            int nowsimilar = 0;
            int index = 0;
            for (int i = 0; i < Dictionary.Length; i++)
            {
                if (isPossible[i])
                {
                    nowsimilar = CalcUtil.CalcHit(Dictionary[i], best);
                    if (nowsimilar > mostsimilar)
                    {
                        mostsimilar = nowsimilar;
                        index = i;
                    }
                }
            }
            return Dictionary[index];
        }
        private int Guess(string word)
        {
            return Host.Guess(word);
        }
        private void PrintAllPossibility()
        {
            for (int i = 0; i < Dictionary.Length; i++)
            {
                if (isPossible[i])
                {
                    Console.WriteLine(Dictionary[i]);
                }
            }
        }
        public int Solve()
        {
            string select;
            int hitcount;
            while (PossibleWordsCount > 1)
            {
                //select = q.SelectByPossibility();
                select = SelectByMinimumExpectedValue();
                hitcount = Guess(select);
                Choose(select, hitcount);
            }
            if (PossibleWordsCount != 1)
            {
                throw new Exception("can't solve");
            }
            return Host.HowManyTimeBeAsked();
        }
        public bool isSecret(string a)
        {
            return Host.isSecret(a);
        }
    }
    class Solution
    {
        public void FindSecretWord(string[] wordlist, Secret master)
        {
            Question q = new Question(master, wordlist);
            q.Solve();
        }
    }
    class Program
    {
        static void Main(string[] args)
        {
            Test3D();
            Console.Read();
        }
        static void Test3D()
        {
            string path = @"C:\Users\lenovo\Desktop\data.txt";
            int pertime = 20;
            int meantime = 0;
            using (FileStream fs = new FileStream(path,FileMode.Create,FileAccess.Write))
            {
                StreamWriter sw = new StreamWriter(fs);
                for (int length = 3; length < 10; length++)
                {
                    for (int scale = 100; scale <= 1000; scale += 100)
                    {
                        Console.WriteLine("strlen={0},scale={1}", length, scale);
                        meantime = 0;
                        sw.WriteLine("{0},{1}", length, scale);
                        Question[] qs = ValideUtil.CreateValidation(pertime, length, scale);
                        for (int i = 0; i < qs.Length; i++)
                        {
                            Console.WriteLine("Question:{0}", i);
                            meantime = qs[i].Solve();
                            sw.WriteLine("{0}:{1}", i, meantime);
                        }
                        sw.Flush();
                    }
                }
            }
        }
    }
}
