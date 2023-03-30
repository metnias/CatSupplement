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

        public CatSupplement()
        { }

        public readonly PlayerState state;
        public AbstractCreature Owner => state.creature;
        public Player self => Owner.realizedCreature as Player;
        public ChunkSoundEmitter soundLoop;

        /// <summary>
        /// This is detected with StartsWith.
        /// <para>1.(major update).(minor patch).(hotfixes)</para>
        /// Minor patches may turn stuff obsolete, but won't destroy compatibility.
        /// Major patches may break compatibility, and major updates surely will.
        /// It's best to write down to major update number. (e.g. 1.0)
        /// </summary>
        public abstract string TargetSubVersion { get; }

        public static bool TryGetSub<T>(PlayerState self, out T sub) where T : CatSupplement
            => SubRegistry.TryGetSub(self, out sub);

        public bool TryGetDeco<T>(out T deco) where T : CatDecoration
            => DecoRegistry.TryGetDeco(state, out deco);

        public virtual void Update(On.Player.orig_Update orig, bool eu)
        {
            orig?.Invoke(self, eu);
        }

        /// <summary>
        /// This destroys <see cref="soundLoop"/> if it exists
        /// </summary>
        public virtual void Destroy(On.Player.orig_Destroy orig)
        {
            orig?.Invoke(self);
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
        /// Updates clone of <see cref="SaveDataTable"/> dependent to <see cref="SaveState.deathPersistentSaveData"/>
        /// for situations like force quit
        /// </summary>
        /// <param name="table">Cloned data table from last cycle</param>
        /// <param name="data"><see cref="DeathPersistentSaveData"/> instance calling <see cref="DeathPersistentSaveData.SaveToString(bool, bool)"/></param>
        /// <param name="saveAsIfPlayerDied">Whether this save clone is for death or quit</param>
        /// <param name="saveAsIfPlayerQuit">Whether this save clone is for quit</param>
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