using Parser.Map.Difficulty.V3.Grid;
using ReplayDecoder;

namespace MapPostprocessor
{
    public class NoteWrapper : GenericWrapper<Note>
    {
        override public Note Note { get; set; }

        override public ScoringType ScoringType { get; set; } = ScoringType.Normal;

        override public int Color => Note.Color;
        override public int CutDirection => Note.CutDirection;
        public float cutDirectionAngleOffset { get; set; }

        public override float Rotation {
            get 
            {
                float result = NoteCutDirectionExtensions.RotationAngle((NoteCutDirection)CutDirection);
                float angleOffset = Note.AngleOffset;

                float rotationOffset = cutDirectionAngleOffset;

                if (Note.CutDirection >= 1000 && Note.CutDirection <= 1360) {
					rotationOffset = 1000 - Note.CutDirection;
				} else if (Note.CutDirection >= 2000 && Note.CutDirection <= 2360) {
					rotationOffset = 2000 - Note.CutDirection;
				}
                if (rotationOffset != 0) {
                    angleOffset = rotationOffset;
                }

                return result + angleOffset;
            }
        }
    }
}
