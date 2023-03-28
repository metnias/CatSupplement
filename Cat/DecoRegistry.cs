using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using SlugName = SlugcatStats.Name;

namespace CatSupplement.Cat
{
    public static class DecoRegistry
    {
        /// <summary>
        /// Instances not attached to real slugcat
        /// </summary>
        internal static readonly Dictionary<SlugName, CatDecoration> CatDecoPrototype
            = new Dictionary<SlugName, CatDecoration>();

        private static readonly Dictionary<SlugName, Func<PlayerState, CatDecoration>> CatDecoFactory
            = new Dictionary<SlugName, Func<PlayerState, CatDecoration>>();

        private readonly static ConditionalWeakTable<PlayerState, CatDecoration> CatDecos
            = new ConditionalWeakTable<PlayerState, CatDecoration>();

        internal static void Register<T>(SlugName name, Func<PlayerState, T> factory) where T : CatDecoration, new()
        {
            CatDecoPrototype.Add(name, new T());
            CatDecoFactory.Add(name, factory);
        }

        public static void AddDeco(PlayerState state)
        {
            if (!CatDecos.TryGetValue(state, out _)
                && state.creature.realizedObject is Player player
                && CatDecoFactory.TryGetValue(player.SlugCatClass, out var factory))
            {
                CatDecos.Add(state, factory(state));
            }
        }

        public static bool TryGetDeco<T>(PlayerState state, out T sup) where T : CatDecoration
        {
            if (CatDecos.TryGetValue(state, out var genericSup)
                && genericSup is T specificSup)
            {
                sup = specificSup;
                return true;
            }
            else
            {
                sup = default;
                return false;
            }
        }

        public static bool TryGetPrototype<T>(SlugName name, out T sup) where T : CatDecoration
        {
            if (CatDecoPrototype.TryGetValue(name, out var genericSup)
                && genericSup is T specificSup)
            {
                sup = specificSup;
                return true;
            }
            else
            {
                sup = default;
                return false;
            }
        }

    }
}
