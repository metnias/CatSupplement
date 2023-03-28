using Menu;
using Menu.Remix;
using RWCustom;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;
using MiscProgressionData = PlayerProgression.MiscProgressionData;
using SaveGameData = Menu.SlugcatSelectMenu.SaveGameData;
using SlugName = SlugcatStats.Name;

namespace CatSupplement.Story
{
    public static class SaveManager
    {
        internal static void Patch()
        {
            progDataTable = new ConditionalWeakTable<SaveState, SaveDataTable>();
            persDataTable = new ConditionalWeakTable<DeathPersistentSaveData, SaveDataTable>();
            miscDataTable = new ConditionalWeakTable<MiscProgressionData, SaveDataTable>();

            On.SaveState.ctor += ProgDataCtorPatch;
            On.SaveState.SaveToString += ProgDataToStringPatch;
            On.SaveState.LoadGame += ProgDataFromStringPatch;

            On.DeathPersistentSaveData.ctor += PersDataCtorPatch;
            On.DeathPersistentSaveData.SaveToString += PersDataToStringPatch;
            On.DeathPersistentSaveData.FromString += PersDataFromStringPatch;

            On.PlayerProgression.MiscProgressionData.ctor += MiscDataCtorPatch;
            On.PlayerProgression.MiscProgressionData.ToString += MiscDataToStringPatch;
            On.PlayerProgression.MiscProgressionData.FromString += MiscDataFromStringPatch;

        }

        public static readonly string PREFIX = "<CatSubData>";

        #region InitialSave

        private static SaveDataTable CreateNewProgSaveData(SlugName saveStateNumber)
        {
            if (SubRegistry.TryGetPrototype(saveStateNumber, out CatSupplement sub))
                return sub.AppendNewProgSaveData();
            return new SaveDataTable();
        }


        private static SaveDataTable CreateNewPersSaveData(SlugName slugcat)
        {
            if (SubRegistry.TryGetPrototype(slugcat, out CatSupplement sub))
                return sub.AppendNewProgSaveData();
            return new SaveDataTable();
        }

        private static void AppendNewMiscSaveData(ref SaveDataTable misc)
        {
            foreach (var pair in SubRegistry.CatSubPrototype)
            {
                var table = pair.Value.AppendNewMiscSaveData();
                foreach (var p in table.table)
                    if (!misc.table.ContainsKey(p.Key)) misc.table.Add(p.Key, p.Value);
            }
        }

        #endregion InitialSave

        #region ProgData

        private static ConditionalWeakTable<SaveState, SaveDataTable> progDataTable;

        private static void ProgDataCtorPatch(On.SaveState.orig_ctor orig, SaveState self, SlugcatStats.Name saveStateNumber, PlayerProgression progression)
        {
            orig(self, saveStateNumber, progression);

            //if (saveStateNumber != PlanterEnums.SlugPlanter) return;

            SaveDataTable prog = CreateNewProgSaveData(saveStateNumber);
            progDataTable.Add(self, prog);
        }

        private static string ProgDataToStringPatch(On.SaveState.orig_SaveToString orig, SaveState self)
        {
            if (progDataTable.TryGetValue(self, out var saveData))
            {
                var saveDataPos = -1;
                for (var i = 0; i < self.unrecognizedSaveStrings.Count; i++)
                    if (self.unrecognizedSaveStrings[i].StartsWith(PREFIX))
                        saveDataPos = i;

                if (saveDataPos > -1)
                    self.unrecognizedSaveStrings[saveDataPos] = saveData.ToString();
                else
                    self.unrecognizedSaveStrings.Add(saveData.ToString());
            }

            return orig(self);
        }

        private static void ProgDataFromStringPatch(On.SaveState.orig_LoadGame orig, SaveState self, string str, RainWorldGame game)
        {
            orig(self, str, game);

            if (!progDataTable.TryGetValue(self, out var saveData)) return;

            var saveDataPos = -1;
            for (var i = 0; i < self.unrecognizedSaveStrings.Count; i++)
            {
                if (self.unrecognizedSaveStrings[i].StartsWith(PREFIX))
                    saveDataPos = i;
            }

            if (saveDataPos > -1)
                saveData.FromString(self.unrecognizedSaveStrings[saveDataPos]);
        }

        internal static T GetProgValue<T>(SaveState data, string key)
            => progDataTable.TryGetValue(data, out var table) ? table.GetValue<T>(key) : default;

        internal static void SetProgValue<T>(SaveState data, string key, T value)
        { if (progDataTable.TryGetValue(data, out var table)) table.SetValue(key, value); }

        #endregion ProgData

        #region PersData

        private static ConditionalWeakTable<DeathPersistentSaveData, SaveDataTable> persDataTable;

        private static void PersDataCtorPatch(On.DeathPersistentSaveData.orig_ctor orig, DeathPersistentSaveData self, SlugcatStats.Name slugcat)
        {
            orig(self, slugcat);

            //if (slugcat != PlanterEnums.SlugPlanter) return;

            SaveDataTable pers = CreateNewPersSaveData(slugcat);
            persDataTable.Add(self, pers);
        }

        private static string PersDataToStringPatch(On.DeathPersistentSaveData.orig_SaveToString orig, DeathPersistentSaveData self, bool saveAsIfPlayerDied, bool saveAsIfPlayerQuit)
        {
            if (persDataTable.TryGetValue(self, out var saveData))
            {
                UpdatePersSaveData(self, ref saveData, saveAsIfPlayerDied, saveAsIfPlayerQuit);

                var saveDataPos = -1;
                for (var i = 0; i < self.unrecognizedSaveStrings.Count; i++)
                    if (self.unrecognizedSaveStrings[i].StartsWith(PREFIX))
                        saveDataPos = i;

                if (saveDataPos > -1)
                    self.unrecognizedSaveStrings[saveDataPos] = saveData.ToString();
                else
                    self.unrecognizedSaveStrings.Add(saveData.ToString());
            }

            return orig(self, saveAsIfPlayerDied, saveAsIfPlayerQuit);
        }

        private static void UpdatePersSaveData(DeathPersistentSaveData self, ref SaveDataTable persData, bool saveAsIfPlayerDied, bool saveAsIfPlayerQuit)
        {
            //if (saveAsIfPlayerDied && !self.reinforcedKarma)
            
        }

        private static void PersDataFromStringPatch(On.DeathPersistentSaveData.orig_FromString orig, DeathPersistentSaveData self, string s)
        {
            orig(self, s);

            if (!persDataTable.TryGetValue(self, out var saveData)) return;

            var saveDataPos = -1;
            for (var i = 0; i < self.unrecognizedSaveStrings.Count; i++)
            {
                if (self.unrecognizedSaveStrings[i].StartsWith(PREFIX))
                    saveDataPos = i;
            }

            if (saveDataPos > -1)
                saveData.FromString(self.unrecognizedSaveStrings[saveDataPos]);
        }

        internal static T GetPersValue<T>(DeathPersistentSaveData data, string key)
            => persDataTable.TryGetValue(data, out var table) ? table.GetValue<T>(key) : default;

        internal static void SetPersValue<T>(DeathPersistentSaveData data, string key, T value)
        { if (persDataTable.TryGetValue(data, out var table)) table.SetValue(key, value); }

        #endregion PersData

        #region MiscData

        private static ConditionalWeakTable<MiscProgressionData, SaveDataTable> miscDataTable;

        private static void MiscDataCtorPatch(On.PlayerProgression.MiscProgressionData.orig_ctor orig, MiscProgressionData self, PlayerProgression owner)
        {
            orig(self, owner);

            SaveDataTable misc = new SaveDataTable();
            AppendNewMiscSaveData(ref misc);
            miscDataTable.Add(self, misc);
        }

        private static string MiscDataToStringPatch(On.PlayerProgression.MiscProgressionData.orig_ToString orig, MiscProgressionData self)
        {
            if (miscDataTable.TryGetValue(self, out var saveData))
            {
                var saveDataPos = -1;
                for (var i = 0; i < self.unrecognizedSaveStrings.Count; i++)
                    if (self.unrecognizedSaveStrings[i].StartsWith(PREFIX))
                        saveDataPos = i;

                if (saveDataPos > -1)
                    self.unrecognizedSaveStrings[saveDataPos] = saveData.ToString();
                else
                    self.unrecognizedSaveStrings.Add(saveData.ToString());
            }

            return orig(self);
        }

        private static void MiscDataFromStringPatch(On.PlayerProgression.MiscProgressionData.orig_FromString orig, MiscProgressionData self, string s)
        {
            orig(self, s);

            if (!miscDataTable.TryGetValue(self, out var saveData)) return;

            var saveDataPos = -1;
            for (var i = 0; i < self.unrecognizedSaveStrings.Count; i++)
            {
                if (self.unrecognizedSaveStrings[i].StartsWith(PREFIX))
                    saveDataPos = i;
            }

            if (saveDataPos > -1)
                saveData.FromString(self.unrecognizedSaveStrings[saveDataPos]);

            AppendNewMiscSaveData(ref saveData);
        }

        internal static T GetMiscValue<T>(MiscProgressionData data, string key)
            => miscDataTable.TryGetValue(data, out var table) ? table.GetValue<T>(key) : default;

        internal static void SetMiscValue<T>(MiscProgressionData data, string key, T value)
        { if (miscDataTable.TryGetValue(data, out var table)) table.SetValue(key, value); }

        #endregion MiscData

    }

    public class SaveDataTable
    {
        internal readonly Dictionary<string, string> table
            = new Dictionary<string, string>();

        internal const char SPR = '|';
        internal const char DIV = '~';

        public SaveDataTable()
        {
        }

        public T GetValue<T>(string key) => ValueConverter.ConvertToValue<T>(table[key]);

        public void SetValue<T>(string key, T value)
        {
            var t = ValueConverter.ConvertToString(value);
            if (table.ContainsKey(key)) table[key] = t;
            else table.Add(key, t);
        }

        public void FromString(string text)
        {
            text = text.Substring(SaveManager.PREFIX.Length);
            var data = text.Split(SPR);
            foreach (var d in data)
            {
                var e = d.Split(DIV);
                if (e.Length < 2) continue;
                SetValue(e[0], e[1]);
            }
        }

        public override string ToString()
        {
            var text = new StringBuilder(SaveManager.PREFIX);

            foreach (var d in table)
            {
                text.Append(d.Key);
                text.Append(DIV);
                text.Append(d.Value);
                text.Append(SPR);
            }

            return text.ToString();
        }
    }
}