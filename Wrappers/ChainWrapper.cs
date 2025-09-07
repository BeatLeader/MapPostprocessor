using Parser.Map.Difficulty.V3.Grid;
using ReplayDecoder;
using System.Numerics;

namespace MapPostprocessor
{
    public class ChainWrapper : GenericWrapper<Note>
    {
        override public Note Note { get; set; }
        public Chain SliderData { get; set; }

        override public ScoringType ScoringType { get; set; } = ScoringType.ChainLink;
        override public int Color => Note.Color;
        public override int CutDirection => 8;
        public int SliceIndex { get; set; }

        public float chainX { get; set; }
        public float chainY { get; set; }
        public float chainRotation { get; set; }

        public override float X => chainX;
        public override float Y => chainY;
        public override float Rotation => chainRotation;
    }
}
