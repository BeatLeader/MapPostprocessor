using Parser.Map.Difficulty.V3.Base;
using ReplayDecoder;

namespace MapPostprocessor
{
    public enum ScoringType
    {
        Ignore = -1, // 0xFFFFFFFF
        NoScore = 0,
        Normal = 1,
        ArcHead = 2,
        ArcTail = 3,
        ChainHead = 4,
        ChainLink = 5,
        ArcHeadArcTail = 6,
        ChainHeadArcTail = 7,
        ChainLinkArcHead = 8,
        ChainHeadArcHead = 9,
        ChainHeadArcHeadArcTail = 10, // 0x0000000A
    }

    public interface IWrapper<out T> where T : BeatmapGridObject
    {
        public float Time { get; }
        public int LineIndex { get; }
        public int LineLayer { get; }
        public int Color { get; }
        public int CutDirection { get; }

        public T Note { get; }

        public ScoringType ScoringType { get; set; }

        public NoteEvent? Event { get; set; }

        public int Id { get; set; }
        public int IdWithLegacyScoring { get; set; }
        public int IdWithAlternativeScoring { get; set; }
        public int IdWithScoring { get; set; }
    }
    public class GenericWrapper<T> : IWrapper<T> where T : BeatmapGridObject {
        public int Id { get; set; }
        public int IdWithLegacyScoring { get; set; }
        public int IdWithAlternativeScoring { get; set; }
        public int IdWithScoring { get; set; }

        public float Time => Note.Seconds;
        public int LineIndex => Note.x;
        public int LineLayer => Note.y;
        virtual public int Color => 0;
        virtual public int CutDirection => 8;

        virtual public T Note { get; set; }
        public NoteEvent? Event { get; set; }

        virtual public ScoringType ScoringType { get; set; }
    }
}
