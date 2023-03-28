using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SlugName = SlugcatStats.Name;

namespace CatSupplement
{
    public static class SubRegistry
    {
        private static readonly Dictionary<SlugName, CatSupplement> CatSubs
            = new Dictionary<SlugName, CatSupplement>();

        public static void RegisterSupplement(SlugName slug, CatSupplement value)
        {
            if (CatSubs.ContainsKey(slug)) return;
            CatSubs.Add(slug, value);
        }

        internal static bool TryGetSupplement(Player player, out CatSupplement sub)
        {
            sub = null;
            if (!CatSubs.TryGetValue(player.SlugCatClass, out sub)) return false;

            return true;
        }

    }
}
