using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using SlugName = SlugcatStats.Name;

namespace CatSub.Cat
{
    public static class SubRegistry
    {
        /// <summary>
        /// Instances not attached to real slugcat
        /// </summary>
        internal static readonly Dictionary<SlugName, CatSupplement> CatSubPrototype
            = new Dictionary<SlugName, CatSupplement>();

        private static readonly Dictionary<SlugName, Func<Player, CatSupplement>> CatSubFactory
            = new Dictionary<SlugName, Func<Player, CatSupplement>>();

        private readonly static ConditionalWeakTable<PlayerState, CatSupplement> CatSubs
            = new ConditionalWeakTable<PlayerState, CatSupplement>();

        internal static void Register<T>(SlugName name, Func<Player, T> factory) where T : CatSupplement, new()
        {
            CatSubPrototype.Add(name, new T());
            CatSubFactory.Add(name, factory);
        }

        public static void AddSub(PlayerState state)
        {
            if (!CatSubs.TryGetValue(state, out _)
                && state.creature.realizedObject is Player player
                && CatSubFactory.TryGetValue(player.SlugCatClass, out var factory))
            {
                CatSubs.Add(state, factory(player));
            }
        }

        public static bool TryGetSub<T>(PlayerState state, out T sub) where T : CatSupplement
        {
            if (CatSubs.TryGetValue(state, out var genericSup)
                && genericSup is T specificSup)
            {
                sub = specificSup;
                return true;
            }
            else
            {
                sub = default;
                return false;
            }
        }

        public static bool TryGetPrototype<T>(SlugName name, out T sub) where T : CatSupplement
        {
            if (CatSubPrototype.TryGetValue(name, out var genericSub)
                && genericSub is T specificSub)
            {
                sub = specificSub;
                return true;
            }
            else
            {
                sub = default;
                return false;
            }
        }

    }
}
