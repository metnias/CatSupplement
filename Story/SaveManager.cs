using CatSub.Cat;
using Menu.Remix;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using MiscProgressionData = PlayerProgression.MiscProgressionData;
using SlugName = SlugcatStats.Name;

namespace CatSub.Story
{
    public static class SaveManager
    {
        internal static void Patch()
        {
            progDataTable = new ConditionalWeakTable<MiscWorldSaveData, SaveDataTable>();
            persDataTable = new ConditionalWeakTable<DeathPersistentSaveData, SaveDataTable>();
            miscDataTable = new ConditionalWeakTable<MiscProgressionData, SaveDataTable>();

            On.MiscWorldSaveData.ctor += ProgDataCtorPatch;
            On.MiscWorldSaveData.ToString += ProgDataToStringPatch;
            On.MiscWorldSaveData.FromString += ProgDataFromStringPatch;

            On.DeathPersistentSaveData.ctor += PersDataCtorPatch;
            On.DeathPersistentSaveData.SaveToString += PersDataToStringPatch;
            On.DeathPersistentSaveData.FromString += PersDataFromStringPatch;

            On.PlayerProgression.MiscProgressionData.ctor += MiscDataCtorPatch;
            On.PlayerProgression.MiscProgressionData.ToString += MiscDataToStringPatch;
            On.PlayerProgression.MiscProgressionData.FromString += MiscDataFromStringPatch;

        }

        public static readonly string PREFIX = "<CatSubData>";

        public static readonly string PERSSLUGCAT = "CatSub_DeathPersDataTableSlugcat";

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
                return sub.AppendNewPersSaveData();
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

        private static ConditionalWeakTable<MiscWorldSaveData, SaveDataTable> progDataTable;

        private static void ProgDataCtorPatch(On.MiscWorldSaveData.orig_ctor orig, MiscWorldSaveData self, SlugName saveStateNumber)
        {
            orig(self, saveStateNumber);

            SaveDataTable prog = CreateNewProgSaveData(saveStateNumber);
            progDataTable.Add(self, prog);
        }

        private static string ProgDataToStringPatch(On.MiscWorldSaveData.orig_ToString orig, MiscWorldSaveData self)
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

        private static void ProgDataFromStringPatch(On.MiscWorldSaveData.orig_FromString orig, MiscWorldSaveData self, string s)
        {
            orig(self, s);

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

        public static T GetProgValue<T>(MiscWorldSaveData data, string key)
            => progDataTable.TryGetValue(data, out var table) ? table.GetValue<T>(key) : default;

        public static void SetProgValue<T>(MiscWorldSaveData data, string key, T value)
        { if (progDataTable.TryGetValue(data, out var table)) table.SetValue(key, value); }

        #endregion ProgData

        #region PersData

        private static ConditionalWeakTable<DeathPersistentSaveData, SaveDataTable> persDataTable;

        private static void PersDataCtorPatch(On.DeathPersistentSaveData.orig_ctor orig, DeathPersistentSaveData self, SlugName slugcat)
        {
            orig(self, slugcat);

            SaveDataTable pers = CreateNewPersSaveData(slugcat);
            pers.SetValue(PERSSLUGCAT, slugcat);
            persDataTable.Add(self, pers);
        }

        private static string PersDataToStringPatch(On.DeathPersistentSaveData.orig_SaveToString orig, DeathPersistentSaveData self, bool saveAsIfPlayerDied, bool saveAsIfPlayerQuit)
        {
            if (persDataTable.TryGetValue(self, out var saveData))
            {
                var cloneSaveData = saveData.Clone();

                UpdatePersSaveData(ref cloneSaveData, self, saveAsIfPlayerDied, saveAsIfPlayerQuit);

                var saveDataPos = -1;
                for (var i = 0; i < self.unrecognizedSaveStrings.Count; i++)
                    if (self.unrecognizedSaveStrings[i].StartsWith(PREFIX))
                        saveDataPos = i;

                if (saveDataPos > -1)
                    self.unrecognizedSaveStrings[saveDataPos] = cloneSaveData.ToString();
                else
                    self.unrecognizedSaveStrings.Add(cloneSaveData.ToString());
            }

            return orig(self, saveAsIfPlayerDied, saveAsIfPlayerQuit);
        }

        private static void UpdatePersSaveData(ref SaveDataTable persData, DeathPersistentSaveData data, bool saveAsIfPlayerDied, bool saveAsIfPlayerQuit)
        {
            var slugcat = persData.GetValue<SlugName>(PERSSLUGCAT);
            if (SubRegistry.TryGetPrototype(slugcat, out CatSupplement sub))
                sub.UpdatePersSaveData(ref persData, data, saveAsIfPlayerDied, saveAsIfPlayerQuit);
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

        public static T GetPersValue<T>(DeathPersistentSaveData data, string key)
            => persDataTable.TryGetValue(data, out var table) ? table.GetValue<T>(key) : default;

        public static void SetPersValue<T>(DeathPersistentSaveData data, string key, T value)
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

        public static T GetMiscValue<T>(MiscProgressionData data, string key)
            => miscDataTable.TryGetValue(data, out var table) ? table.GetValue<T>(key) : default;

        public static void SetMiscValue<T>(MiscProgressionData data, string key, T value)
        { if (miscDataTable.TryGetValue(data, out var table)) table.SetValue(key, value); }

        #endregion MiscData

        public static SaveDataTable Clone(this SaveDataTable table)
        {
            var clone = new SaveDataTable();
            foreach (var pair in table.table)
                clone.table.Add(pair.Key, pair.Value);
            return clone;
        }
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