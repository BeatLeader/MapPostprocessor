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
    }
}
