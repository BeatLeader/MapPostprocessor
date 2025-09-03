using Parser.Map.Difficulty.V3.Grid;
using ReplayDecoder;

namespace MapPostprocessor
{
    public class ChainWrapper : GenericWrapper<Note>
    {
        override public Note Note { get; set; }

        override public ScoringType ScoringType { get; set; } = ScoringType.ChainLink;
        override public int Color => Note.Color;
        public override int CutDirection => 8;
        public int SliceIndex { get; set; }
    }
}
