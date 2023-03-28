using CatSupplement.Cat;
using CatSupplement.Story;

namespace CatSupplement
{
    public abstract class CatSupplement
    {
        public CatSupplement(Player player)
        {
            this.owner = player.abstractCreature;
        }

        public CatSupplement() { }

        public readonly AbstractCreature owner;
        public Player self => owner.realizedCreature as Player;
        public CatDecoration Deco { get { AppendCatDeco.TryGetDeco(owner, out var sub); return sub; } }
        public ChunkSoundEmitter soundLoop;
        
        public bool TryGetSub(AbstractCreature self, out CatSupplement sub) =>
            AppendCatSub.TryGetSub(self, out sub);

        protected internal virtual void Update(On.Player.orig_Update orig, bool eu)
        {
            orig(self, eu);
        }

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
        /// Creates new save data dependent to <see cref="SaveState"/>,
        /// and is wiped by death or quit.
        /// </summary>
        /// <param name="table"></param>
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
        /// Creates new save data dependent to <see cref="SaveState.miscWorldSaveData"/>,
        /// 
        /// </summary>
        /// <param name="table"></param>
        protected internal virtual SaveDataTable AppendNewMiscSaveData()
        {
            return new SaveDataTable();
        }

        #endregion Prototype
    }
}