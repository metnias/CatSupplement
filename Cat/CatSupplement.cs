using CatSub.Story;
using System;

namespace CatSub.Cat
{
    public abstract class CatSupplement
    {
        /// <summary>
        /// Supplement class attached to <see cref="PlayerState"/> instance to keep
        /// stuff related to <see cref="Player"/>
        /// </summary>
        public CatSupplement(Player player)
        {
            state = player.playerState;
        }

        public CatSupplement() { }

        public readonly PlayerState state;
        public AbstractCreature Owner => state.creature;
        public Player self => Owner.realizedCreature as Player;
        public ChunkSoundEmitter soundLoop;

        public static bool TryGetSub<T>(PlayerState self, out T sub) where T : CatSupplement
            => SubRegistry.TryGetSub(self, out sub);

        public bool TryGetDeco<T>(out T deco) where T : CatDecoration
            => DecoRegistry.TryGetDeco(state, out deco);

        public virtual void Update(On.Player.orig_Update orig, bool eu)
        {
            orig(self, eu);
        }

        /// <summary>
        /// This destroys <see cref="soundLoop"/> if it exists
        /// </summary>
        public virtual void Destroy(On.Player.orig_Destroy orig)
        {
            orig(self);
            soundLoop?.Destroy();
        }

        #region Prototype

        /// <summary>
        /// Returns text to be displayed instead of Pick up interaction
        /// </summary>
        public virtual string ControlTutorial()
        {
            return "";
        }

        /// <summary>
        /// Creates new save data dependent to <see cref="SaveState.miscWorldSaveData"/>,
        /// and is wiped by death or quit.
        /// </summary>
        public virtual SaveDataTable AppendNewProgSaveData()
        {
            return new SaveDataTable();
        }

        /// <summary>
        /// Creates new save data dependent to <see cref="SaveState.deathPersistentSaveData"/>,
        /// which stays with death or quit, but gets wiped with new run.
        /// </summary>
        public virtual SaveDataTable AppendNewPersSaveData()
        {
            return new SaveDataTable();
        }

        /// <summary>
        /// Updates death persistant save data for situations like win, or force quit etc.
        /// </summary>
        public virtual void UpdatePersSaveData(ref SaveDataTable table, DeathPersistentSaveData data, bool saveAsIfPlayerDied, bool saveAsIfPlayerQuit)
        {
        }

        /// <summary>
        /// Creates new save data dependent to <see cref="PlayerProgression.MiscProgressionData"/>,
        /// which stays with new runs or different campaign, and only gets wiped by resetting save slot
        /// </summary>
        public virtual SaveDataTable AppendNewMiscSaveData()
        {
            return new SaveDataTable();
        }

        #endregion Prototype
    }
}