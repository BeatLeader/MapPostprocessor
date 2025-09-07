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

    public enum NoteCutDirection {
        Up = 0,
        Down = 1,
        Left = 2,
        Right = 3,
        UpLeft = 4,
        UpRight = 5,
        DownLeft = 6,
        DownRight = 7,
        Any = 8,
        NONE = 9
    }

    public interface IWrapper<out T> where T : BeatmapGridObject
    {
        public float Time { get; }
        public int LineIndex { get; }
        public int LineLayer { get; }
        public int Color { get; }
        public int CutDirection { get; }

        public float X { get; }
        public float Y { get; }
        public float Rotation { get; }

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

        public virtual float Time => Note.Seconds;
        public int LineIndex => Note.x;
        public int LineLayer => Note.y;
        virtual public int Color => 0;
        virtual public int CutDirection => 8;

        public static int NOTE_LINES_COUNT = 4;
        public static float NOTE_LINES_DISTANCE = 0.6f;

        public static float GetHorizontalPosition(float lineIndex) {
	        return (-(NOTE_LINES_COUNT - 1) * 0.5f + lineIndex) * NOTE_LINES_DISTANCE;
        }

        public static float HighestJumpPosYForLineLayer(float lineLayer) {
	        return NOTE_LINES_DISTANCE * (lineLayer + 1) + 0.05f * (5 - lineLayer - (lineLayer > 1 ? 1 : 0));
        }

        public virtual float X {
            get 
            { 
                float horizontalPosition = Note.x;
                if (Note.customData?.coordinates != null) {
					horizontalPosition = Note.customData?.coordinates[0] + 4 / 2 ?? 0;
				}
                if (horizontalPosition <= -1000 || horizontalPosition >= 1000) {
					horizontalPosition = horizontalPosition < 0 ? horizontalPosition / 1000 + 1 : horizontalPosition / 1000 - 1;
				}

                return GetHorizontalPosition(horizontalPosition);
            } 
        }
        public virtual float Y {
            get 
            { 
                float verticalPosition = Note.y;
                if (Note.customData?.coordinates != null) {
					verticalPosition = Note.customData?.coordinates[1] ?? 0;
				}
                if (verticalPosition <= -1000 || verticalPosition >= 1000) {
					verticalPosition = verticalPosition < 0 ? verticalPosition / 1000 + 1 : verticalPosition / 1000 - 1;
				}

                return HighestJumpPosYForLineLayer(verticalPosition);
            } 
        }

        public virtual float Rotation => 0;

        virtual public T Note { get; set; }
        public NoteEvent? Event { get; set; }

        virtual public ScoringType ScoringType { get; set; }
    }
}
