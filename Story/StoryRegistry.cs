using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SlugName = SlugcatStats.Name;

namespace CatSupplement.Story
{
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

    public static class StoryRegistry
    {
        private static readonly List<TimelinePointer> timelinePointers = new List<TimelinePointer>();

        public static void RegisterTimeline(TimelinePointer pointer)
        {
            if(!timelinePointers.Contains(pointer))
                timelinePointers.Add(pointer);
        }

        internal static Queue<TimelinePointer> GetTimelinePointers()
            => new Queue<TimelinePointer>(timelinePointers);
        

    }
}
