using Menu;
using RWCustom;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static CatSub.Story.StoryRegistry;
using SlugName = SlugcatStats.Name;

namespace CatSub.Cat
{
    public static class AppendCatSub
    {
        internal static void Patch()
        {
            On.Menu.ControlMap.ctor += ControlMapPatch;
            On.SlugcatStats.SlugcatTimelineOrder += AppendTimelineOrder;
            On.RainWorldGame.ctor += StartGamePatch;
            On.Player.ctor += CtorPatch;
            On.Player.Update += UpdatePatch;
            On.Player.Destroy += DestroyPatch;
        }

        private static void ControlMapPatch(On.Menu.ControlMap.orig_ctor orig, ControlMap map, Menu.Menu menu, MenuObject owner, Vector2 pos, Options.ControlSetup.Preset preset, bool showPickupInstructions)
        {
            orig.Invoke(map, menu, owner, pos, preset, showPickupInstructions);
            if (!showPickupInstructions) return;
            if (!(Custom.rainWorld.processManager.currentMainLoop is RainWorldGame rwg)) return;
            if (!SubRegistry.TryGetPrototype(rwg.StoryCharacter, out CatSupplement sub)) return;
            string tutorial = sub.ControlTutorial();
            if (!string.IsNullOrEmpty(tutorial))
                map.pickupButtonInstructions.text = tutorial;
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