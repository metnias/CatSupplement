using CatSupplement.Cat;

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

        protected internal virtual string ControlTutorial()
        {
            return "";
        }
    }
}