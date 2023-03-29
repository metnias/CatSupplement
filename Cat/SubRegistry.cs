using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
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

        private static readonly ConditionalWeakTable<PlayerState, CatSupplement> CatSubs
            = new ConditionalWeakTable<PlayerState, CatSupplement>();

        internal static readonly HashSet<string> OutdatedSlugs = new HashSet<string>();

        /// <summary>
        /// Register constructor so that this mod will append this to player instances automatically.
        /// </summary>
        /// <param name="name"><see cref="SlugcatStats.Name"/> to append</param>
        /// <param name="factory"><c>state => new ExampleCatSupplement(state)</c></param>
        public static void Register<T>(SlugName name, Func<Player, T> factory) where T : CatSupplement, new()
        {
            if (OutdatedSlugs.Contains(name.value))
            {
                Debug.LogError("This mod is targeted for outdated CatSupplement!");
                return;
            }
            var proto = new T();
            if (string.IsNullOrEmpty(proto.TargetSubVersion) ||
                !SubPlugin.PLUGIN_VERSION.StartsWith(proto.TargetSubVersion))
            {
                Debug.LogError($"This mod is targeted for outdated CatSupplement!\nTarget: {proto.TargetSubVersion}, Current: {SubPlugin.PLUGIN_VERSION}");
                OutdatedSlugs.Add(name.value);
                return;
            }

            CatSubPrototype.Add(name, proto);
            CatSubFactory.Add(name, factory);
        }

        public static void Unregister(SlugName name)
        {
            CatSubPrototype.Remove(name);
            CatSubFactory.Remove(name);
        }

        public static void AddSub(PlayerState state)
        {
            if (!CatSubs.TryGetValue(state, out _)
                && state.creature.realizedObject is Player player
                && TryMakeSub(player, out CatSupplement sub))
                CatSubs.Add(state, sub);
        }

        public static bool TryMakeSub<T>(Player player, out T sub) where T : CatSupplement
        {
            sub = default;
            if (CatSubFactory.TryGetValue(player.SlugCatClass, out var factory))
            {
                var genericSub = factory(player);
                if (genericSub is T)
                { sub = genericSub as T; return true; }
            }
            return false;
        }

        public static bool TryGetSub<T>(PlayerState state, out T sub) where T : CatSupplement
        {
            if (CatSubs.TryGetValue(state, out var genericSub)
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