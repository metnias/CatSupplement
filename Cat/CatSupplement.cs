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

        /// <summary>
        /// Register constructor so that this mod will append this to player instances automatically.
        /// </summary>
        /// <param name="factory"><c>state => new ExampleCatSupplement(state)</c></param>
        public static void Register<T>(SlugcatStats.Name name, Func<Player, T> factory) where T : CatSupplement, new()
            => SubRegistry.Register(name, factory);

        public readonly PlayerState state;
        public AbstractCreature Owner => state.creature;
        public Player self => Owner.realizedCreature as Player;
        public ChunkSoundEmitter soundLoop;

        public static bool TryGetSub(PlayerState self, out CatSupplement sub) =>
            SubRegistry.TryGetSub(self, out sub);

        public bool TryGetDeco(out CatDecoration deco) =>
            DecoRegistry.TryGetDeco(state, out deco);

        protected internal virtual void Update(On.Player.orig_Update orig, bool eu)
        {
            orig(self, eu);
        }

        /// <summary>
        /// This destroys <see cref="soundLoop"/> if it exists
        /// </summary>
        protected internal virtual void Destroy(On.Player.orig_Destroy orig)
        {
            orig(self);
            soundLoop?.Destroy();
        }

        #region Prototype

        /// <summary>
        /// Returns text to be displayed instead of Pick up interaction
        /// </summary>
        protected internal virtual string ControlTutorial()
        {
            return "";
        }

        /// <summary>
        /// Creates new save data dependent to <see cref="SaveState.miscWorldSaveData"/>,
        /// and is wiped by death or quit.
        /// </summary>
        protected internal virtual SaveDataTable AppendNewProgSaveData()
        {
            return new SaveDataTable();
        }

        /// <summary>
        /// Creates new save data dependent to <see cref="SaveState.deathPersistentSaveData"/>,
        /// which stays with death or quit, but gets wiped with new run.
        /// </summary>
        protected internal virtual SaveDataTable AppendNewPersSaveData()
        {
            return new SaveDataTable();
        }

        /// <summary>
        /// Creates new save data dependent to <see cref="PlayerProgression.MiscProgressionData"/>,
        /// which stays with new runs or different campaign, and only gets wiped by resetting save slot
        /// </summary>
        protected internal virtual SaveDataTable AppendNewMiscSaveData()
        {
            return new SaveDataTable();
        }

        #endregion Prototype
    }
}