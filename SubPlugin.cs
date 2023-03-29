using BepInEx;
using System;
using System.Reflection;
using System.Security.Permissions;
using System.Security;
using CatSub;
using BepInEx.Logging;
using UnityEngine;
using CatSub.Cat;
using CatSub.Story;

#region Assembly attributes

#pragma warning disable CS0618
[assembly: AssemblyVersion(SubPlugin.PLUGIN_VERSION)]
[assembly: AssemblyFileVersion(SubPlugin.PLUGIN_VERSION)]
[assembly: AssemblyTitle(SubPlugin.PLUGIN_NAME + " (" + SubPlugin.PLUGIN_ID + ")")]
[assembly: AssemblyProduct(SubPlugin.PLUGIN_NAME)]
[module: UnverifiableCode]
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]
#pragma warning restore CS0618

#endregion Assembly attributes

namespace CatSub
{
    [BepInPlugin(PLUGIN_ID, PLUGIN_NAME, PLUGIN_VERSION)]
    [BepInProcess("RainWorld.exe")]
    public class SubPlugin : BaseUnityPlugin
    {
        public const string PLUGIN_ID = "com.rainworldgame.topicular.catsupplement.plugin";
        public const string PLUGIN_NAME = "CatSupplement";
        public const string PLUGIN_VERSION = "1.0.0.1";

        public void OnEnable()
        {
            instance = this;
            LogSource = Logger;

            On.RainWorld.OnModsInit += WrapInit(Init);
        }

        private static bool init = false;
        internal static ManualLogSource LogSource;

        public static SubPlugin instance;

        private static void Init(RainWorld rw)
        {
            SaveManager.Patch();
            AppendCatSub.Patch();
            AppendCatDeco.Patch();

            instance.Logger.LogMessage("CatSupplement is Intialized.");
        }

        private static On.RainWorld.hook_OnModsInit WrapInit(Action<RainWorld> loadResources)
        {
            return (orig, self) =>
            {
                orig(self);
                if (init) return;

                try
                {
                    loadResources(self);
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                }
                init = true;
            };
        }
    }
}