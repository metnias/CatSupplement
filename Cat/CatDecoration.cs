using RWCustom;
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

        public CatDecoration()
        { }

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

        #region ZRot

        protected virtual bool UpdateZRot => false;

        private readonly float[,] zRot = new float[5, 2];

        public virtual void Update(On.PlayerGraphics.orig_Update orig)
        {
            orig?.Invoke(self);
            if (!UpdateZRot) return;
            BackupZRotation();
            Vector2 upDir = Custom.DirVec(self.drawPositions[1, 0], self.drawPositions[0, 0]);
            float upRot = Custom.VecToDeg(upDir);
            CalculateHeadRotation();
            CalculateTailRotation();

            void BackupZRotation()
            {
                for (int q = 0; q < zRot.GetLength(0); q++) { zRot[q, 1] = zRot[q, 0]; }
            }

            void CalculateHeadRotation()
            {
                Vector2 lookDir = self.lookDirection * 3f * (1f - player.sleepCurlUp);
                if (player.sleepCurlUp > 0f)
                {
                    lookDir.y -= 2f * player.sleepCurlUp;
                    lookDir.x -= 4f * Mathf.Sign(self.drawPositions[0, 0].x - self.drawPositions[1, 0].x) * player.sleepCurlUp;
                }
                else if (player.room.gravity == 0f) { }
                else if (player.Consious)
                {
                    if (player.bodyMode == Player.BodyModeIndex.Stand && player.input[0].x != 0)
                    { lookDir.x += 4f * Mathf.Sign(player.input[0].x); lookDir.y++; }
                    else if (player.bodyMode == Player.BodyModeIndex.Crawl)
                    { lookDir.x += 4f * Mathf.Sign(self.drawPositions[0, 0].x - self.drawPositions[1, 0].x); lookDir.y++; }
                }
                else { lookDir *= 0f; }
                float lookRot = lookDir.magnitude > float.Epsilon ? (Custom.VecToDeg(lookDir) -
                    (player.Consious && player.bodyMode == Player.BodyModeIndex.Crawl ? 0f : upRot)) : 0f;
                if (Mathf.Abs(lookRot) < 90f)
                { zRot[0, 0] = Custom.LerpMap(lookRot, 0f, Mathf.Sign(lookRot) * 90f, 0f, Mathf.Sign(lookRot) * 60f, 0.5f); }
                else
                { zRot[0, 0] = Custom.LerpMap(lookRot, Mathf.Sign(lookRot) * 180f, Mathf.Sign(lookRot) * 90f, Mathf.Sign(lookRot) * 60f, 0f, 0.5f); }
            }

            void CalculateTailRotation()
            {
                float totTailRot = zRot[0, 0], lastTailRot = -upRot;
                for (int t = 0; t < 4; t++)
                {
                    float tailRot = -Custom.AimFromOneVectorToAnother(t == 0 ? self.drawPositions[1, 0]
                        : self.tail[t - 1].pos, self.tail[t].pos);
                    tailRot -= lastTailRot; lastTailRot += tailRot;
                    totTailRot += tailRot;
                    //dbg[t] = tailRot;
                    zRot[1 + t, 0] = totTailRot < 0f ? Mathf.Clamp(totTailRot, -90f, 0f) : Mathf.Clamp(totTailRot, 0f, 90f);
                }
            }
        }

        /// <summary>
        /// Returns Zrotation. Only use this when <see cref="UpdateZRot"/> is enabled.
        /// </summary>
        public float GetZRot(int idx, float timeStacker) => -Mathf.Lerp(zRot[idx, 1], zRot[idx, 0], timeStacker);

        /// <summary>
        /// Returns Zrotation. Only use this when <see cref="UpdateZRot"/> is enabled.
        /// </summary>
        public float GetZRot(float idx, float timeStacker) =>
            Mathf.Lerp(GetZRot(Mathf.FloorToInt(idx), timeStacker), GetZRot(Mathf.FloorToInt(idx) + 1, timeStacker), idx - Mathf.FloorToInt(idx));

        #endregion ZRot

        public virtual void InitiateSprites(On.PlayerGraphics.orig_InitiateSprites orig, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
        {
            orig?.Invoke(self, sLeaser, rCam);

            if (container != null)
            { container.RemoveAllChildren(); container.RemoveFromContainer(); container = null; }
            container = new FContainer();
            //AddToContainer(sLeaser, rCam, null);
        }

        public virtual void AddToContainer(On.PlayerGraphics.orig_AddToContainer orig, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContatiner)
        {
            orig?.Invoke(self, sLeaser, rCam, newContatiner);

            if (container == null) return;
            if (newContatiner == null) { newContatiner = rCam.ReturnFContainer("Midground"); }
            container.RemoveFromContainer();
            newContatiner.AddChild(container);
        }

        public virtual void ApplyPalette(On.PlayerGraphics.orig_ApplyPalette orig, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
        {
            orig?.Invoke(self, sLeaser, rCam, palette);

            bodyColor = sLeaser.sprites[0].color;
            faceColor = sLeaser.sprites[9].color;
        }

        public virtual void DrawSprites(On.PlayerGraphics.orig_DrawSprites orig, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
        {
            orig?.Invoke(self, sLeaser, rCam, timeStacker, camPos);

            if (player == null || player.room == null || self == null)
            { container.isVisible = false; return; }
            container.isVisible = true;
        }

        public virtual void SuckedIntoShortCut(On.PlayerGraphics.orig_SuckedIntoShortCut orig, Vector2 shortCutPosition)
        {
            orig?.Invoke(self, shortCutPosition);

            container.RemoveFromContainer();
        }

        public virtual void Reset(On.PlayerGraphics.orig_Reset orig)
        {
            orig?.Invoke(self);
            if (UpdateZRot)
            {
                for (int i = 0; i < zRot.GetLength(0); i++)
                { zRot[i, 0] = 0f; zRot[i, 1] = 0f; }
            }
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