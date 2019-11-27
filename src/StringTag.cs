using System;
using System.Collections.Generic;
using System.Linq;

namespace liveitbe.ImageCat {
    class StringTag {
        #region static

        static readonly Dictionary<string, StringTag> refs = new Dictionary<string, StringTag>(64);
        static readonly List<StringTag> tagFilter = new List<StringTag>(16);

        public static StringTag Ref(string name_) => Ref(name_, 1);

        private static StringTag Ref(string name_, int rc) {
            if (!refs.TryGetValue(name_, out StringTag ret))
                refs.Add(name_, ret = new StringTag() { name = name_ });
            ret.count += rc;
            return ret;
        }

        public static void FilterTags(IEnumerable<string> tagStrs) {
            tagFilter.Clear();
            if (tagStrs != null)
                tagFilter.AddRange(tagStrs.Select(s => Ref(s, 0)));
        }

        public static bool ShouldFilter(ImageLink link) =>
            tagFilter.Count > 0 && tagFilter.Any(f => link.tag.All(l => !l.name.StartsWith(f.name, StringComparison.Ordinal)));

        #endregion

        public string name;
        public int count;

        public void Unref() => --count;

        public override string ToString() => name;
    }
}
