using UnityEngine;

namespace CatSupplement.Cat
{
    public static class AppendCatDeco
    {
        public static void Patch()
        {
            On.PlayerGraphics.InitiateSprites += InitSprPatch;
            On.PlayerGraphics.Reset += ResetPatch;
            On.PlayerGraphics.SuckedIntoShortCut += SuckedIntoShortCutPatch;
            On.PlayerGraphics.DrawSprites += DrawSprPatch;
            On.PlayerGraphics.AddToContainer += AddToCtnrPatch;
            On.PlayerGraphics.ApplyPalette += PalettePatch;
            On.PlayerGraphics.Update += UpdatePatch;
        }

        #region Graphics

        public static bool TryGetDeco(PlayerGraphics self, out CatDecoration deco)
            => DecoRegistry.TryGetDeco(self.player.playerState, out deco);

        private static void UpdatePatch(On.PlayerGraphics.orig_Update orig, PlayerGraphics self)
        {
            if (TryGetDeco(self, out var deco))
                deco.Update(orig);
            else
                orig(self);
        }

        private static void InitSprPatch(On.PlayerGraphics.orig_InitiateSprites orig, PlayerGraphics self,
            RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
        {
            if (TryGetDeco(self, out var deco))
                deco.InitiateSprites(orig, sLeaser, rCam);
            else
                orig(self, sLeaser, rCam);
        }

        private static void SuckedIntoShortCutPatch(On.PlayerGraphics.orig_SuckedIntoShortCut orig,
            PlayerGraphics self, Vector2 shortCutPosition)
        {
            if (TryGetDeco(self, out var deco))
                deco.SuckedIntoShortCut(orig, shortCutPosition);
            else
                orig(self, shortCutPosition);
        }

        private static void ResetPatch(On.PlayerGraphics.orig_Reset orig, PlayerGraphics self)
        {
            if (TryGetDeco(self, out var deco))
                deco.Reset(orig);
            else
                orig(self);
        }

        private static void DrawSprPatch(On.PlayerGraphics.orig_DrawSprites orig, PlayerGraphics self,
            RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
        {
            if (TryGetDeco(self, out var deco))
                deco.DrawSprites(orig, sLeaser, rCam, timeStacker, camPos);
            else
                orig(self, sLeaser, rCam, timeStacker, camPos);
        }

        private static void AddToCtnrPatch(On.PlayerGraphics.orig_AddToContainer orig, PlayerGraphics self,
            RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContatiner)
        {
            if (TryGetDeco(self, out var deco))
                deco.AddToContainer(orig, sLeaser, rCam, newContatiner);
            else
                orig(self, sLeaser, rCam, newContatiner);
        }

        private static void PalettePatch(On.PlayerGraphics.orig_ApplyPalette orig, PlayerGraphics self,
            RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
        {
            if (TryGetDeco(self, out var deco))
                deco.ApplyPalette(orig, sLeaser, rCam, palette);
            else
                orig(self, sLeaser, rCam, palette);
        }

        #endregion Graphics
    }
}