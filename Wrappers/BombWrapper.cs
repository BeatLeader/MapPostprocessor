using Parser.Map.Difficulty.V3.Grid;
using ReplayDecoder;

namespace MapPostprocessor
{
    public class BombWrapper : GenericWrapper<Bomb>
    {

        override public Bomb Note { get; set; }
        override public ScoringType ScoringType { get; set; } = ScoringType.NoScore;
        override public int Color => 3;
        public override int CutDirection => 9;
    }
}
