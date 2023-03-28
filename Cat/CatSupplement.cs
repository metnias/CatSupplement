﻿using CatSupplement.Cat;
using CatSupplement.Story;
using System;

namespace CatSupplement
{
    public abstract class CatSupplement
    {
        public CatSupplement(Player player)
        {
            state = player.playerState;
        }

        public CatSupplement() { }

        public static void Register<T>(SlugcatStats.Name name, Func<PlayerState, T> factory) where T : CatSupplement, new()
            => SubRegistry.Register<T>(name, factory);

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