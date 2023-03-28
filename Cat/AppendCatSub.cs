using Menu;
using RWCustom;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEngine;
using SlugName = SlugcatStats.Name;

namespace CatSupplement.Cat
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

        internal static void OnMSCEnablePatch()
        {
        }

        internal static void OnMSCDisablePatch()
        {
        }

        private static SlugName[] AppendTimelineOrder(On.SlugcatStats.orig_getSlugcatTimelineOrder orig)
        {
            LinkedList<SlugName> list = new LinkedList<SlugName>(orig());
            //var yellow = list.Find(SlugName.Yellow);
            //list.AddAfter(yellow, SlugPlanter);
            return list.ToArray();
        }

        private static void ControlMapPatch(On.Menu.ControlMap.orig_ctor orig, ControlMap map, Menu.Menu menu, MenuObject owner, Vector2 pos, Options.ControlSetup.Preset preset, bool showPickupInstructions)
        {
            orig.Invoke(map, menu, owner, pos, preset, showPickupInstructions);
            if (!showPickupInstructions) return;
            if (!(Custom.rainWorld.processManager.currentMainLoop is RainWorldGame rwg)) return;
            //if (rwg.StoryCharacter != catsub) return;

            /*
            string text = string.Empty;
            text = menu.Translate("Planter interactions:") + Environment.NewLine + Environment.NewLine;
            text += "- " + menu.Translate("Sporecat's diet is exclusively insectivore, regardless of the prey's size") + Environment.NewLine;
            text += "- " + menu.Translate("Hold UP and press PICK UP to grab a Puffball from the tail") + Environment.NewLine;
            text += "- " + menu.Translate("Hold DOWN and PICK UP for charged explosion") + Environment.NewLine;
            text += "- " + menu.Translate("However, using too many Puffballs costs hunger");
            Vector2 position = map.pickupButtonInstructions.pos;
            map.RemoveSubObject(map.pickupButtonInstructions);
            map.pickupButtonInstructions = new MenuLabel(menu, map, text, position, new Vector2(100f, 20f), false);
            map.pickupButtonInstructions.label.alignment = FLabelAlignment.Left;
            map.subObjects.Add(map.pickupButtonInstructions);
            */

        }


        #region Player

        private readonly static ConditionalWeakTable<AbstractCreature, CatSupplement> catSubs
            = new ConditionalWeakTable<AbstractCreature, CatSupplement>();

        public static bool TryGetSub(AbstractCreature self, out CatSupplement sub)
        {
            sub = null;
            if (!(self.state is PlayerState)) return false;
            if (catSubs.TryGetValue(self, out sub)) return true;
            return false;
        }

        private static void StartGamePatch(On.RainWorldGame.orig_ctor orig, RainWorldGame self, ProcessManager manager)
        {
            orig(self, manager);
            /*
            if (!self.IsStorySession) return;
            for (int i = 0; i < self.Players.Count; i++)
                if (self.world.GetAbstractRoom(self.Players[i].pos) != null)
                    if (self.world.GetAbstractRoom(self.Players[i].pos).shelter) continue;
                    else if (self.world.GetAbstractRoom(self.Players[i].pos).name == "LF_A11") // Sporecat
                        self.Players[i].pos.Tile = new IntVector2(11, 30);
            */
        }

        private static void CtorPatch(On.Player.orig_ctor orig, Player self, AbstractCreature abstractCreature, World world)
        {
            orig(self, abstractCreature, world);
            if (TryGetSub(abstractCreature, out var _)) return;

            if (!SubRegistry.TryGetSupplement(self, out var sub)) return;
            catSubs.Add(self.abstractCreature, (CatSupplement)Activator.CreateInstance(sub.GetType(), self));

            //catSubs.Add(self.abstractCreature, new PlanterCatSupplement(self.abstractCreature));
            //catDecos.Add(self.abstractCreature, new PlanterCatDecoration(self.abstractCreature));
        }

        private static void UpdatePatch(On.Player.orig_Update orig, Player self, bool eu)
        {
            if (TryGetSub(self.abstractCreature, out var sub))
                sub.Update(orig, eu);
            else
                orig(self, eu);
        }

        private static void DestroyPatch(On.Player.orig_Destroy orig, Player self)
        {
            if (TryGetSub(self.abstractCreature, out var sub))
                sub.Destroy(orig);
            else
                orig(self);
        }

        #endregion Player

    }
}
