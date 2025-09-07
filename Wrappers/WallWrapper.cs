using Parser.Map.Difficulty.V3.Grid;
using ReplayDecoder;

namespace MapPostprocessor
{
    public class WallWrapper : GenericWrapper<Wall>
    {

        override public Wall Note { get; set; }

        override public ScoringType ScoringType { get; set; } = ScoringType.Ignore;

        override public int Color => 4;
        public override int CutDirection => 0;

        public static float GetHorizontalWallPosition(float lineIndex) {
	        return (lineIndex - 2) * NOTE_LINES_DISTANCE;
        }

        public static float GetVerticalPosition(float lineLayer) {
	        return 0.25f + NOTE_LINES_DISTANCE * lineLayer;
        }

        public override float X {
            get {
                float horizontalPosition = Note.x;
                if (Note.customData?.coordinates != null) {
					horizontalPosition = Note.customData?.coordinates[0] + 2 ?? 0;
				}
                if (horizontalPosition <= -1000 || horizontalPosition >= 1000) {
					horizontalPosition = horizontalPosition < 0 ? horizontalPosition / 1000 + 1 : horizontalPosition / 1000 - 1;
				}

                return GetHorizontalWallPosition(horizontalPosition);
            }
        }

        public override float Y {
            get 
            { 
                float verticalPosition = Note.y;
                if (Note.customData?.coordinates != null) {
					verticalPosition = Note.customData?.coordinates[1] ?? 0;
				}

                return GetVerticalPosition(verticalPosition);
            } 
        }

        public float Width {
            get {
                float result = Note.Width;

                if (result >= 1000 || result <= -1000) {
					result = ((result <= -1000 ? result + 2000 : result) - 1000) / 1000;
				}

                return result * NOTE_LINES_DISTANCE;
            }
        }

        public float Height {
            get {
                return Note.Height * NOTE_LINES_DISTANCE;
            }
        }
    }
}
