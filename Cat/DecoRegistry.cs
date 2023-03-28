using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using SlugName = SlugcatStats.Name;

namespace CatSub.Cat
{
    public static class DecoRegistry
    {
        /// <summary>
        /// Instances not attached to real slugcat
        /// </summary>
        internal static readonly Dictionary<SlugName, CatDecoration> CatDecoPrototype
            = new Dictionary<SlugName, CatDecoration>();

        private static readonly Dictionary<SlugName, Func<Player, CatDecoration>> CatDecoFactory
            = new Dictionary<SlugName, Func<Player, CatDecoration>>();

        private readonly static ConditionalWeakTable<PlayerState, CatDecoration> CatDecos
            = new ConditionalWeakTable<PlayerState, CatDecoration>();

        /// <summary>
        /// Register constructor so that this mod will append this to player instances automatically.
        /// </summary>
        /// <param name="name"><see cref="SlugcatStats.Name"/> to append</param>
        /// <param name="factory"><c>state => new ExampleCatDecoration(state)</c></param>
        public static void Register<T>(SlugName name, Func<Player, T> factory) where T : CatDecoration, new()
        {
            CatDecoPrototype.Add(name, new T());
            CatDecoFactory.Add(name, factory);
        }

        public static void Unregister(SlugName name)
        {
            CatDecoPrototype.Remove(name);
            CatDecoFactory.Remove(name);
        }

        public static void AddDeco(PlayerState state)
        {
            if (!CatDecos.TryGetValue(state, out _)
                && state.creature.realizedObject is Player player
                && TryMakeDeco(player, out CatDecoration deco))
                CatDecos.Add(state, deco);
        }

        public static bool TryMakeDeco<T>(Player player, out T sub) where T : CatDecoration
        {
            sub = default;
            if (CatDecoFactory.TryGetValue(player.SlugCatClass, out var factory))
            {
                var genericSub = factory(player);
                if (genericSub is T)
                { sub = genericSub as T; return true; }
            }
            return false;
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
