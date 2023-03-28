﻿using RWCustom;
using System;
using UnityEngine;
using static PlayerGraphics;

namespace CatSub.Cat
{
    public abstract class CatDecoration
    {
        /// <summary>
        /// Supplement class attached to <see cref="PlayerState"/> instance to keep
        /// stuff related to <see cref="PlayerGraphics"/>
        /// </summary>
        public CatDecoration(Player player)
        {
            state = player.playerState;
        }

        public CatDecoration() { }

        public readonly PlayerState state;
        public AbstractCreature Owner => state.creature;
        public Player player => Owner.realizedCreature as Player;
        public PlayerGraphics self => player.graphicsModule as PlayerGraphics;

        public bool TryGetSub<T>(out T sub) where T : CatSupplement
            => SubRegistry.TryGetSub(state, out sub);

        public static bool TryGetDeco<T>(PlayerState self, out T deco) where T : CatDecoration
            => DecoRegistry.TryGetDeco(self, out deco);

        public FSprite[] sprites;
        public FContainer container;

        public virtual void Update(On.PlayerGraphics.orig_Update orig)
        {
            orig(self);
        }

        public virtual void InitiateSprites(On.PlayerGraphics.orig_InitiateSprites orig, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
        {
            orig(self, sLeaser, rCam);

            if (container != null)
            { container.RemoveAllChildren(); container.RemoveFromContainer(); container = null; }
            container = new FContainer();
            //AddToContainer(sLeaser, rCam, null);
        }

        public virtual void AddToContainer(On.PlayerGraphics.orig_AddToContainer orig, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContatiner)
        {
            orig(self, sLeaser, rCam, newContatiner);

            if (container == null) return;
            if (newContatiner == null) { newContatiner = rCam.ReturnFContainer("Midground"); }
            container.RemoveFromContainer();
            newContatiner.AddChild(container);
        }

        public virtual void ApplyPalette(On.PlayerGraphics.orig_ApplyPalette orig, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
        {
            orig(self, sLeaser, rCam, palette);

            bodyColor = sLeaser.sprites[0].color;
            faceColor = sLeaser.sprites[9].color;
        }

        public virtual void DrawSprites(On.PlayerGraphics.orig_DrawSprites orig, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
        {
            orig(self, sLeaser, rCam, timeStacker, camPos);

            if (player == null || player.room == null || self == null)
            { container.isVisible = false; return; }
            container.isVisible = true;
        }

        public virtual void SuckedIntoShortCut(On.PlayerGraphics.orig_SuckedIntoShortCut orig, Vector2 shortCutPosition)
        {
            orig(self, shortCutPosition);

            container.RemoveFromContainer();
        }

        public virtual void Reset(On.PlayerGraphics.orig_Reset orig)
        {
            orig(self);
        }

        #region Getters

        public Vector2 GetPos(int idx, float timeStacker) => idx < 1 ? Vector2.Lerp(self.drawPositions[idx, 1], this.self.drawPositions[idx, 0], timeStacker) :
                Vector2.Lerp(self.tail[idx - 1].lastPos, self.tail[idx - 1].pos, timeStacker);

        public Vector2 GetPos(float idx, float timeStacker) => Vector2.Lerp(GetPos(Mathf.FloorToInt(idx), timeStacker), GetPos(Mathf.FloorToInt(idx) + 1, timeStacker), idx - Mathf.FloorToInt(idx));

        public float GetRad(int idx) => idx < 1 ? player.bodyChunks[0].rad : self.tail[idx - 1].StretchedRad;

        public float GetRad(float idx) => Mathf.Lerp(GetRad(Mathf.FloorToInt(idx)), GetRad(Mathf.FloorToInt(idx) + 1), idx - Mathf.FloorToInt(idx));

        public Vector2 GetDir(float idx, float timeStacker) =>
            Custom.DirVec(GetPos(Mathf.FloorToInt(idx), timeStacker), GetPos(Mathf.FloorToInt(idx) + 1, timeStacker));


        public Color GetBodyColor() => bodyColor;

        public Color GetFaceColor() => faceColor;

        public Color GetThirdColor() =>
            ModManager.CoopAvailable && self.useJollyColor
            ? JollyColor(player.playerState.playerNumber, 2) :
            CustomColorsEnabled() ? CustomColorSafety(2) : DefaultThirdColor;

        protected Color bodyColor = Color.white;
        protected Color faceColor = new Color(0.01f, 0.01f, 0.01f);
        protected virtual Color DefaultThirdColor => new Color(0.01f, 0.01f, 0.01f);

        #endregion Getters
    }
}