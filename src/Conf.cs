using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace liveitbe.ImageCat
{
    static class Conf
    {
        const string FilePath = "ImageCatConf";
        const string sep = ": ";
        static readonly Dictionary<string, string> confs = new Dictionary<string, string>(8);

        public static void Init()
        {
            using var reader = new StreamReader(File.Open(FilePath, FileMode.OpenOrCreate));
            string k, v;
            while (!reader.EndOfStream)
            {
                var l = reader.ReadLine();
                if (l.StartsWith("#", StringComparison.Ordinal))
                    continue;
                int sepi = l.IndexOf(sep, StringComparison.Ordinal);
                if (sepi < 0)
                    continue;
                k = l.Substring(0, sepi);
                v = l.Substring(sepi + sep.Length);
                SetValue(k, v);
            }
        }

        public static void Save()
        {
            using StreamWriter sw = new StreamWriter(File.Open(FilePath, FileMode.OpenOrCreate));
            foreach (var kv in confs) {
                sw.WriteLine(kv.Key + ": " + kv.Value);
            }
        }

        public static void SetValue(string k, string v)
        {
            if (confs.ContainsKey(k))
                confs[k] = v;
            else
                confs.Add(k, v);
        }

        public static string Value(string k, string def = null)
        {
            if (!confs.TryGetValue(k, out var ret))
                ret = def;
            return ret;
        }
    }
}
