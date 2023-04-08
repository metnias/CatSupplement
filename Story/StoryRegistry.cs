using RWCustom;
using System.Collections.Generic;
using System.Linq;
using SlugName = SlugcatStats.Name;

namespace CatSub.Story
{
    public static class StoryRegistry
    {
        #region TimelineOrder

        public struct TimelinePointer
        {
            /// <summary>
            /// Struct containing data to determine relative time position
            /// </summary>
            /// <param name="name">The name of slugcat to be registered</param>
            /// <param name="order">The relative order against pivot slugcat</param>
            /// <param name="pivots">Pivot slugcats from top priority to bottom. This must contain at least one of three vanilla slugcats.</param>
            public TimelinePointer(SlugName name, Relative order, params SlugName[] pivots) : this()
            {
                this.name = name;
                this.order = order;
                this.pivots = pivots;
                search = 0;
            }

            /// <summary>
            /// Struct containing data to determine relative time position
            /// </summary>
            /// <param name="name">The name of slugcat to be registered</param>
            /// <param name="order">The relative order against pivot slugcat</param>
            /// <param name="pivotNames">Pivot slugcats from top priority to bottom. This must contain at least one of three vanilla slugcats.</param>
            public TimelinePointer(SlugName name, Relative order, params string[] pivotNames)
                : this(name, order, NamesToSlugs(pivotNames))
            { }

            private static SlugName[] NamesToSlugs(string[] names)
            {
                var res = new SlugName[names.Length];
                for (int i = 0; i < names.Length; ++i) res[i] = new SlugName(names[i], false);
                return res;
            }

            public SlugName name;
            public Relative order;
            public SlugName[] pivots;
            internal int search;

            public enum Relative
            { Before, After };
        }

        private static bool isTimelineDirty = true;
        private static int lastOrigTimelineCount = -1;
        private static SlugName[] savedTimeline = null;

        internal static void SetTimelineDirty() => isTimelineDirty = true;

        private static readonly Dictionary<SlugName, TimelinePointer> timelinePointers
            = new Dictionary<SlugName, TimelinePointer>();

        /// <summary>
        /// Register timeline positions for slugcat
        /// </summary>
        public static void RegisterTimeline(TimelinePointer pointer)
        {
            if (!timelinePointers.ContainsKey(pointer.name))
                timelinePointers.Add(pointer.name, pointer);
            else timelinePointers[pointer.name] = pointer; // update
            SetTimelineDirty();
        }

        /// <summary>
        /// Unregister timeline position for slugcat
        /// </summary>
        public static void UnregisterTimeline(SlugName name)
        { timelinePointers.Remove(name); SetTimelineDirty(); }

        private static Queue<TimelinePointer> GetTimelinePointers()
        => new Queue<TimelinePointer>(timelinePointers.Values);

        internal static SlugName[] AppendTimelineOrder(On.SlugcatStats.orig_getSlugcatTimelineOrder orig)
        {
            var origTimeline = orig();
            if (lastOrigTimelineCount != origTimeline.Length) { lastOrigTimelineCount = origTimeline.Length; SetTimelineDirty(); }
            if (!isTimelineDirty) return savedTimeline;
            LinkedList<SlugName> list = new LinkedList<SlugName>(origTimeline);
            var queue = GetTimelinePointers();
            int search = 0;
            while (queue.Count > 0)
            {
                var p = queue.Dequeue();
                if (p.name.Index < 0) continue; // unregistered
                for (int i = 0; i < p.pivots.Length; ++i)
                {
                    if (p.pivots[i].Index < 0) continue; // unregistered
                    var node = list.Find(p.pivots[i]);
                    if (node == null) continue;

                    if (p.order == TimelinePointer.Relative.Before)
                        list.AddBefore(node, p.name);
                    else
                        list.AddAfter(node, p.name);
                    ++search;
                    goto LoopEnd;
                }
                if (p.search > search) continue; // give up search
                p.search = search + 1;
                queue.Enqueue(p); // re-search
            LoopEnd: continue;
            }
            isTimelineDirty = false;
            return savedTimeline = list.ToArray();
        }

        /// <summary>
        /// Checks whether the timeline is in between left and right. (leave one as null for one-sided range)
        /// </summary>
        public static bool IsTimelineInbetween(SlugName check, SlugName leftExclusive, SlugName rightExclusive)
        {
            var timeline = SlugcatStats.getSlugcatTimelineOrder();
            int c = -1, l = -1, r = timeline.Length;
            for (int i = 0; i < timeline.Length; ++i)
            {
                if (timeline[i] == check) c = i;
                if (timeline[i] == leftExclusive) l = i;
                if (timeline[i] == rightExclusive) r = i;
            }
            //Debug.Log($"Timeline Check: {l}<{c}<{r}");
            return l < c && c < r;
        }

        #endregion TimelineOrder

        #region StartPos

        private static readonly HashSet<string> startRooms = new HashSet<string>();

        private static readonly Dictionary<string, IntVector2> startPos
            = new Dictionary<string, IntVector2>();

        /// <summary>
        /// Register new Player starting tile when the story starts in this room
        /// </summary>
        /// <param name="roomName">The name of the start room</param>
        /// <param name="startTile">Tile position where the player starts</param>
        public static void RegisterStartPos(string roomName, IntVector2 startTile)
        {
            if (startRooms.Contains(roomName))
            { startPos[roomName] = startTile; return; } // Update
            startRooms.Add(roomName);
            startPos.Add(roomName, startTile);
        }

        public static void UnregisterStartPos(string roomName)
            => startRooms.Remove(roomName);

        public static bool TryGetStartTile(string roomName, out IntVector2 tile)
        {
            tile = default;
            if (!startRooms.Contains(roomName)) return false;
            tile = startPos[roomName];
            return true;
        }

        #endregion StartPos
    }
}