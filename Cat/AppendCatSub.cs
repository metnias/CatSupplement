using Menu;
using RWCustom;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static CatSub.Story.StoryRegistry;
using SlugName = SlugcatStats.Name;

namespace CatSub.Cat
{
    internal static class AppendCatSub
    {
        internal static void Patch()
        {
            On.Menu.ControlMap.ctor += ControlMapPatch;
            On.SlugcatStats.getSlugcatTimelineOrder += AppendTimelineOrder;
            On.RainWorldGame.ctor += StartGamePatch;
            On.Player.ctor += CtorPatch;
            On.Player.Update += UpdatePatch;
            On.Player.Destroy += DestroyPatch;
        }

        private static SlugName[] AppendTimelineOrder(On.SlugcatStats.orig_getSlugcatTimelineOrder orig)
        {
            LinkedList<SlugName> list = new LinkedList<SlugName>(orig());
            var queue = GetTimelinePointers();
            int search = 0;
            while (queue.Count > 0)
            {
                var p = queue.Dequeue();
                for (int i = 0; i < p.pivots.Length; ++i)
                {
                    var node = list.Find(p.pivots[i]);
                    if (node != null)
                    {
                        if (p.order == TimelinePointer.Relative.Before)
                            list.AddBefore(node, p.name);
                        else
                            list.AddAfter(node, p.name);
                        ++search;
                        goto LoopEnd;
                    }
                }
                if (p.search > search) continue;
                p.search = search + 1;
                queue.Enqueue(p); // re-search
            LoopEnd: continue;
            }
            return list.ToArray();
        }

        private static void ControlMapPatch(On.Menu.ControlMap.orig_ctor orig, ControlMap map, Menu.Menu menu, MenuObject owner, Vector2 pos, Options.ControlSetup.Preset preset, bool showPickupInstructions)
        {
            orig.Invoke(map, menu, owner, pos, preset, showPickupInstructions);
            if (!showPickupInstructions) return;
            if (!(Custom.rainWorld.processManager.currentMainLoop is RainWorldGame rwg)) return;
            if (!SubRegistry.TryGetPrototype(rwg.StoryCharacter, out CatSupplement sub)) return;
            string tutorial = sub.ControlTutorial();
            if (!string.IsNullOrEmpty(tutorial))
                map.pickupButtonInstructions.text = sub.ControlTutorial();
        }


        #region Player

        public static bool TryGetSub(Player self, out CatSupplement sub)
            => SubRegistry.TryGetSub(self.playerState, out sub);

        private static void StartGamePatch(On.RainWorldGame.orig_ctor orig, RainWorldGame self, ProcessManager manager)
        {
            orig(self, manager);
            if (!self.IsStorySession) return;

            for (int i = 0; i < self.Players.Count; i++)
                if (self.world.GetAbstractRoom(self.Players[i].pos) != null)
                    if (self.world.GetAbstractRoom(self.Players[i].pos).shelter) continue;
                    else if (TryGetStartTile(self.world.GetAbstractRoom(self.Players[i].pos).name, out var tile))
                        self.Players[i].pos.Tile = tile;
        }

        private static void CtorPatch(On.Player.orig_ctor orig, Player self, AbstractCreature abstractCreature, World world)
        {
            orig(self, abstractCreature, world);

            SubRegistry.AddSub(self.playerState);
            DecoRegistry.AddDeco(self.playerState);
        }

        private static void UpdatePatch(On.Player.orig_Update orig, Player self, bool eu)
        {
            if (TryGetSub(self, out var sub))
                sub.Update(orig, eu);
            else
                orig(self, eu);
        }

        private static void DestroyPatch(On.Player.orig_Destroy orig, Player self)
        {
            if (TryGetSub(self, out var sub))
                sub.Destroy(orig);
            else
                orig(self);
        }

        #endregion Player

    }
}
