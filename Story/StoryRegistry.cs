using RWCustom;
using System.Collections.Generic;
using SlugName = SlugcatStats.Name;

namespace CatSub.Story
{
    public static class StoryRegistry
    {
        #region TimelineOrder
        public struct TimelinePointer
        {
            public TimelinePointer(SlugName name, Relative order, params SlugName[] pivots) : this()
            {
                this.name = name;
                this.order = order;
                this.pivots = pivots;
                search = 0;
            }

            public SlugName name;
            public Relative order;
            public SlugName[] pivots;
            internal int search;

            public enum Relative
            { Before, After };
        }


        private static readonly List<TimelinePointer> timelinePointers = new List<TimelinePointer>();

        public static void RegisterTimeline(TimelinePointer pointer)
        {
            if (!timelinePointers.Contains(pointer))
                timelinePointers.Add(pointer);
        }

        internal static Queue<TimelinePointer> GetTimelinePointers()
            => new Queue<TimelinePointer>(timelinePointers);
        #endregion TimelineOrder

        #region StartPos

        private static readonly HashSet<string> startRooms = new HashSet<string>();

        private static readonly Dictionary<string, IntVector2> startPos
            = new Dictionary<string, IntVector2>();

        public static void RegisterStartPos(string roomName, IntVector2 startTile)
        {
            if (startRooms.Contains(roomName)) return;
            startRooms.Add(roomName);
            startPos.Add(roomName, startTile);
        }

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
