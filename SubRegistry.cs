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
        private static readonly Dictionary<SlugName, CatSupplement> CatSubPrototype
            = new Dictionary<SlugName, CatSupplement>();

        private static readonly Dictionary<SlugName, Func<Player, CatSupplement>> CatSubFactory
            = new Dictionary<SlugName, Func<Player, CatSupplement>>();

        public static void RegisterSupplement(SlugName slug, CatSupplement instance)
        {
            if (CatSubPrototype.ContainsKey(slug)) return;
            CatSubPrototype.Add(slug, instance);
            CatSubFactory.Add(slug, (player) => (CatSupplement)Activator.CreateInstance(instance.GetType(), player));
        }

        public static bool TryCreateSupplement(Player player, out CatSupplement sub)
        {
            sub = null;
            if (!CatSubFactory.TryGetValue(player.SlugCatClass, out var func)) return false;
            sub = func(player);
            return true;
        }

        public static bool TryGetProtoType(SlugName name, out CatSupplement sub)
        {
            sub = null;
            if (!CatSubPrototype.TryGetValue(name, out sub)) return false;
            return true;
        }

    }
}
