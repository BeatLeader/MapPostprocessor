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
    }
}
