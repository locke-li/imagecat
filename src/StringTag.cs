using System;
using System.Collections.Generic;
using System.Linq;

namespace liveitbe.ImageCat
{
    class StringTag
    {
        static Dictionary<string, StringTag> refs = new Dictionary<string, StringTag>(64);
        static List<StringTag> tagFilter = new List<StringTag>(16);

        public static StringTag NewTag()
        {
            return Ref("new tag");
        }

        public static StringTag Ref(string name_)
        {
            return Ref(name_, 1);
        }

        private static StringTag Ref(string name_, int rc)
        {
            StringTag ret;
            if (!refs.TryGetValue(name_, out ret))
                refs.Add(name_, ret = new StringTag() { name = name_ });
            ret.count += rc;
            return ret;
        }

        public static void FilterTags(IEnumerable<string> tagStrs)
        {
            tagFilter.Clear();
            if (tagStrs != null)
                tagFilter.AddRange(tagStrs.Select(s => Ref(s, 0)));
        }

        public static bool ShouldFilter(ImageLink link)
        {
            //Console.WriteLine(link.file.Name + " = " + link.tags.Any(t => tagFilter.Any(f => t.name.Contains(f.name))));
            return tagFilter.Count > 0 && tagFilter.Any(f => link.tags.All(l => !l.name.StartsWith(f.name)));
        }

        public string name;
        public int count;

        public void Unref()
        {
            --count;
        }

        public override string ToString()
        {
            return name;
        }
    }
}
