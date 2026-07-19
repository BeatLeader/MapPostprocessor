using Parser.Map;
using Parser.Map.Difficulty.V3.Base;
using Parser.Map.Difficulty.V3.Grid;
using Parser.Utils;
using System.Numerics;

namespace MapPostprocessor
{
    public class MapWrapper
    {
        public NoteWrapper[] Notes { get; set; }
        public BombWrapper[] Bombs { get; set; }
        public WallWrapper[] Walls { get; set; }
        public ChainWrapper[]? Chains { get; set; }
        public IWrapper<BeatmapGridObject>[] AllCuttableObjects { get; set; }
        public MapWrapper MirroredMap { get; set; }

        private bool CompareSlider(Note note, BeatmapColorGridObjectWithTail slider)
        {
            if (Math.Round(note.BpmTime, 2) != Math.Round(slider.BpmTime, 2)) return false;

            if (note.x == slider.x && note.y == slider.y)
                return true;

            if (note.customData != null && note.customData.coordinates != null)
            {
                if (slider.customData != null && slider.customData.coordinates != null)
                {
                    if (
                        Math.Round(note.customData.coordinates[0], 2) ==
                        Math.Round(slider.customData.coordinates[0], 2) &&
                        Math.Round(note.customData.coordinates[1], 2) ==
                        Math.Round(slider.customData.coordinates[1], 2))
                        return true;
                }
                else
                {
                    if (
                        Math.Round(note.customData.coordinates[0] + 4 / 2) == slider.x &&
                        Math.Round(note.customData.coordinates[1]) == slider.y
                    )
                        return true;
                }
            }

            return false;
        }

        private bool CompareSliderTail(Note note, BeatmapColorGridObjectWithTail slider)
        {
            if (Math.Round(note.BpmTime, 2) != Math.Round(slider.TailBpmTime, 2)) return false;

            if (note.x == slider.tx && note.y == slider.ty)
                return true;

            if (note.customData != null && note.customData.coordinates != null)
            {
                if (slider.customData != null && slider.customData.tailCoordinates != null)
                {
                    if (
                        Math.Round(note.customData.coordinates[0], 2) ==
                        Math.Round(slider.customData.tailCoordinates[0], 2) &&
                        Math.Round(note.customData.coordinates[1], 2) ==
                        Math.Round(slider.customData.tailCoordinates[1], 2))
                        return true;
                }
                else
                {
                    if (
                        Math.Round(note.customData.coordinates[0] + 4 / 2) == slider.tx &&
                        Math.Round(note.customData.coordinates[1]) == slider.ty
                    )
                        return true;
                }
            }

            return false;
        }

        private float LerpUnclamped(float a, float b, float t)
        {
            return a + (b - a) * t;
        }

        public static void BezierCurve(
            Vector2 p0,
            Vector2 p1,
            Vector2 p2,
            float t,
            out Vector2 pos,
            out Vector2 tangent)
        {
            float num = 1f - t;
            pos = num * num * p0 + 2f * num * t * p1 + t * t * p2;
            tangent = (float)(2.0 * (1.0 - (double)t)) * (p1 - p0) + 2f * t * (p2 - p1);
        }

        private void AddScoringTypeAndChains(DifficultyV3 difficulty)
        {
            foreach (var note in Notes)
            {
                if (note.Note.Color == 1 || note.Note.Color == 0)
                {
                    note.ScoringType = ScoringType.Normal;
                }
                else
                {
                    note.ScoringType = ScoringType.NoScore;
                }
            }

            foreach (var slider in difficulty.Arcs)
            {
                var head = Notes.FirstOrDefault(n => CompareSlider(n.Note, slider));
                if (head != null)
                {
                    if (head.ScoringType == ScoringType.Normal)
                    {
                        head.ScoringType = ScoringType.ArcHead;
                    }
                    else if (head.ScoringType == ScoringType.ArcTail)
                    {
                        head.ScoringType = ScoringType.ArcHeadArcTail;
                    }
                }

                var tail = slider.TailBpmTime == slider.BpmTime
                    ? null
                    : Notes.FirstOrDefault(n => CompareSliderTail(n.Note, slider));
                //if (head) {
                //	head.tail = tail;
                //}

                //slider.tail = tail;
                if (tail != null)
                {
                    if (tail.ScoringType == ScoringType.Normal)
                    {
                        tail.ScoringType = ScoringType.ArcTail;
                    }
                    else if (tail.ScoringType == ScoringType.ArcHead)
                    {
                        tail.ScoringType = ScoringType.ArcHeadArcTail;
                    }
                }
            }

            var chains = new List<ChainWrapper>();

            foreach (var slider in difficulty.Chains)
            {
                var head = Notes.FirstOrDefault(n => CompareSlider(n.Note, slider));
                if (head != null)
                {
                    if (head.ScoringType == ScoringType.Normal)
                    {
                        head.ScoringType = ScoringType.ChainHead;
                    }
                    else if (head.ScoringType == ScoringType.ArcTail)
                    {
                        head.ScoringType = ScoringType.ChainHeadArcTail;
                    }
                    else if (head.ScoringType == ScoringType.ArcHead)
                    {
                        head.ScoringType = ScoringType.ChainHeadArcHead;
                    }
                    else if (head.ScoringType == ScoringType.ArcHeadArcTail)
                    {
                        head.ScoringType = ScoringType.ChainHeadArcHeadArcTail;
                    }
                    //head.sliderhead = slider;
                }

                var tail = slider.TailBpmTime == slider.BpmTime
                    ? null
                    : Notes.FirstOrDefault(n => CompareSliderTail(n.Note, slider));
                if (tail != null)
                {
                    if (tail.ScoringType == ScoringType.Normal)
                    {
                        tail.ScoringType = ScoringType.ArcTail;
                    }
                    else if (tail.ScoringType == ScoringType.ArcHead)
                    {
                        tail.ScoringType = ScoringType.ArcHeadArcTail;
                    }
                    else if (tail.ScoringType == ScoringType.ChainHead)
                    {
                        tail.ScoringType = ScoringType.ChainHeadArcTail;
                    }
                    else if (tail.ScoringType == ScoringType.ChainHeadArcHead)
                    {
                        tail.ScoringType = ScoringType.ChainHeadArcHeadArcTail;
                    }
                }

                for (var i = 1; i < slider.SliceCount; ++i)
                {
                    var chain = new Note
                    {
                        x = slider.x,
                        y = slider.y,
                        CutDirection = 8,
                        Color = slider.Color,
                    };

                    chain.Seconds = LerpUnclamped(slider.Seconds, slider.TailInSeconds, (float)i / (slider.SliceCount - 1));

                    Vector3 vector3_1;
                    double headCutOffset = 0;

                    if (head == null) {
                        float hHorizontalPosition = slider.x;
                        if (slider.customData?.coordinates != null)
                        {
                            hHorizontalPosition = slider.customData?.coordinates[0] + 4 / 2 ?? 0;
                        }

                        if (hHorizontalPosition <= -1000 || hHorizontalPosition >= 1000)
                        {
                            hHorizontalPosition = hHorizontalPosition < 0
                                ? hHorizontalPosition / 1000 + 1
                                : hHorizontalPosition / 1000 - 1;
                        }

                        float headX = GenericWrapper<BeatmapGridObject>.GetHorizontalPosition(hHorizontalPosition);
                        float hVerticalPosition = slider.y;
                        if (slider.customData?.coordinates != null)
                        {
                            hVerticalPosition = slider.customData?.coordinates[1] ?? 0;
                        }

                        if (hVerticalPosition <= -1000 || hVerticalPosition >= 1000)
                        {
                            hVerticalPosition = hVerticalPosition < 0
                                ? hVerticalPosition / 1000 + 1
                                : hVerticalPosition / 1000 - 1;
                        }

                        float headY = GenericWrapper<BeatmapGridObject>.HighestJumpPosYForLineLayer(hVerticalPosition);
                        vector3_1 = new Vector3(headX, headY, 0f);

                    } else {
                        vector3_1 = new Vector3(head.X, head.Y, 0f);
                        headCutOffset = head.cutDirectionAngleOffset;
                    }

                    float horizontalPosition = slider.tx;
                    if (slider.customData?.tailCoordinates != null)
                    {
                        horizontalPosition = slider.customData?.tailCoordinates[0] + 4 / 2 ?? 0;
                    }

                    if (horizontalPosition <= -1000 || horizontalPosition >= 1000)
                    {
                        horizontalPosition = horizontalPosition < 0
                            ? horizontalPosition / 1000 + 1
                            : horizontalPosition / 1000 - 1;
                    }

                    float tailX = GenericWrapper<BeatmapGridObject>.GetHorizontalPosition(horizontalPosition);
                    float verticalPosition = slider.ty;
                    if (slider.customData?.tailCoordinates != null)
                    {
                        verticalPosition = slider.customData?.tailCoordinates[1] ?? 0;
                    }

                    if (verticalPosition <= -1000 || verticalPosition >= 1000)
                    {
                        verticalPosition = verticalPosition < 0
                            ? verticalPosition / 1000 + 1
                            : verticalPosition / 1000 - 1;
                    }

                    float tailY = GenericWrapper<BeatmapGridObject>.HighestJumpPosYForLineLayer(verticalPosition);
                    Vector3 vector3_2 = new Vector3(tailX, tailY, 0f);

                    
                    Vector2 p2 = new Vector2(vector3_2.X - vector3_1.X, vector3_2.Y - vector3_1.Y);
                    float magnitude = p2.Length();
                    float f =
                        (float)(
                            ((double)NoteCutDirectionExtensions.RotationAngle((NoteCutDirection)slider.CutDirection) -
                                90.0 + headCutOffset) * (Math.PI / 180.0));

                    Vector2 p1 = (0.5f * magnitude * new Vector2((float)Math.Cos(f), (float)Math.Sin(f)));
                    int sliceCount = slider.SliceCount;
                    float squishAmount = slider.Squish;
                    var index = i;

                    float t = (float)index / (float)(sliceCount - 1);

                    Vector2 pos;
                    Vector2 tangent;
                    BezierCurve(new Vector2(0.0f, 0.0f), p1, p2, t * squishAmount, out pos, out tangent);

                    var arcSlider = difficulty.Arcs.FirstOrDefault(n => CompareSlider(chain, n));
                    chains.Add(new ChainWrapper
                    {
                        Note = chain,
                        SliceIndex = i,
                        ScoringType = arcSlider != null ? ScoringType.ChainLinkArcHead : ScoringType.ChainLink,
                        chainRotation = NoteCutDirectionExtensions.SignedAngle(new Vector2(0.0f, -1f), tangent),
                        chainX = pos.X,
                        chainY = pos.Y,
                        SliderData = slider
                    });
                }
            }

            Chains = chains.ToArray();
        }

        public void SetIds()
        {
            foreach (var mapnote in AllCuttableObjects)
            {
                var lineIndex = mapnote.LineIndex;
                var lineLayer = mapnote.LineLayer;
                var colorType = mapnote.Color;
                var cutDirection = mapnote.CutDirection;
                var scoringType = mapnote.ScoringType + 2;

                var id = lineIndex * 1000 + lineLayer * 100 + colorType * 10 + cutDirection;
                mapnote.Id = id;
                mapnote.IdWithScoring = id + (int)scoringType * 10000;

                var altscoringType = scoringType;
                var legacyScoringType = scoringType;
                if (mapnote.ScoringType == ScoringType.ChainHead)
                {
                    altscoringType = ScoringType.ArcHead + 2;
                }
                else if (mapnote.ScoringType == ScoringType.ArcHead)
                {
                    altscoringType = ScoringType.ChainHead + 2;
                }
                else if (mapnote.ScoringType == ScoringType.ChainHeadArcTail)
                {
                    altscoringType = ScoringType.ArcTail + 2;
                }
                else if (mapnote.ScoringType == ScoringType.ChainHeadArcHead)
                {
                    altscoringType = ScoringType.ArcHead + 2;
                }
                else if (mapnote.ScoringType == ScoringType.ChainHeadArcHeadArcTail)
                {
                    altscoringType = ScoringType.ArcTail + 2;
                }
                else if (mapnote.ScoringType == ScoringType.ChainLinkArcHead)
                {
                    altscoringType = ScoringType.ChainLink + 2;
                }
                else if (mapnote.ScoringType == ScoringType.ArcHeadArcTail)
                {
                    altscoringType = ScoringType.ArcHead + 2;
                }

                if (mapnote.ScoringType == ScoringType.ArcHeadArcTail)
                {
                    legacyScoringType = ScoringType.ArcTail + 2;
                }
                else if (mapnote.ScoringType == ScoringType.ChainHeadArcTail)
                {
                    legacyScoringType = ScoringType.ChainHead + 2;
                }
                else if (mapnote.ScoringType == ScoringType.ChainLinkArcHead)
                {
                    legacyScoringType = ScoringType.ChainLink + 2;
                }
                else if (mapnote.ScoringType == ScoringType.ChainHeadArcHead)
                {
                    legacyScoringType = ScoringType.ChainHead + 2;
                }
                else if (mapnote.ScoringType == ScoringType.ChainHeadArcHeadArcTail)
                {
                    legacyScoringType = ScoringType.ChainHeadArcTail + 2;
                }

                mapnote.IdWithAlternativeScoring = id + (int)altscoringType * 10000;
                mapnote.IdWithLegacyScoring = id + (int)legacyScoringType * 10000;

                if (mapnote.ScoringType == ScoringType.ChainLink) {
                    mapnote.IdWithAlternativeScoring = ((ChainWrapper)mapnote).SliderData.tx * 1000 + ((ChainWrapper)mapnote).SliderData.ty * 100 + colorType * 10 + cutDirection + (int)scoringType * 10000;
                }
            }
        }

        private static float getVerticalPosition(float lineLayer)
        {
            return 0.25f + 0.6f * lineLayer;
        }

        private static float getHorizontalPosition(float lineIndex)
        {
            return (-(4 - 1) * 0.5f + lineIndex) * 0.6f;
        }

        private static Vector2 get2DNoteOffset(float noteLineIndex, float noteLineLayer)
        {
            return new Vector2(getHorizontalPosition(noteLineIndex), getVerticalPosition(noteLineLayer));
        }

        public static float SignedAngleToLine(Vector2 vec, Vector2 line)
        {
            float f1 = NoteCutDirectionExtensions.SignedAngle(vec, line);
            float f2 = NoteCutDirectionExtensions.SignedAngle(vec, -line);
            return (double)Math.Abs(f1) >= (double)Math.Abs(f2) ? f2 : f1;
        }

        private static void processNotesByColorType(List<NoteWrapper> notesWithTheSameColorTypeList)
        {
            if (notesWithTheSameColorTypeList.Count != 2) return;
            var theSameColorType1 = notesWithTheSameColorTypeList[0];
            var theSameColorType2 = notesWithTheSameColorTypeList[1];

            if (
                theSameColorType1.CutDirection != theSameColorType2.CutDirection &&
                theSameColorType1.CutDirection != (int)NoteCutDirection.Any &&
                theSameColorType2.CutDirection != (int)NoteCutDirection.Any
            )
                return;
            NoteWrapper noteData1;
            NoteWrapper noteData2;
            if (theSameColorType1.CutDirection != 8)
            {
                noteData1 = theSameColorType1;
                noteData2 = theSameColorType2;
            }
            else
            {
                noteData1 = theSameColorType2;
                noteData2 = theSameColorType1;
            }


            var line1 = get2DNoteOffset(noteData2.LineIndex, noteData2.LineLayer) -
                        get2DNoteOffset(noteData1.LineIndex, noteData1.LineLayer);
            var line2 = SignedAngleToLine(
                noteData1.CutDirection == (int)NoteCutDirection.Any
                    ? new Vector2(0.0f, 1f)
                    : NoteCutDirectionExtensions.Direction((NoteCutDirection)noteData1.CutDirection),
                line1
            );
            if (noteData2.CutDirection == (int)NoteCutDirection.Any &&
                noteData1.CutDirection == (int)NoteCutDirection.Any)
            {
                noteData1.cutDirectionAngleOffset = line2;
                noteData2.cutDirectionAngleOffset = line2;
            }
            else
            {
                if (Math.Abs(line2) > 40) return;
                noteData1.cutDirectionAngleOffset = line2;
                if (noteData2.CutDirection == (int)NoteCutDirection.Any &&
                    noteData1.CutDirection > (int)NoteCutDirection.Right)
                {
                    noteData2.cutDirectionAngleOffset = line2 + 45;
                }
                else
                {
                    noteData2.cutDirectionAngleOffset = line2;
                }
            }
        }

        private static void processTimingGroups(MapWrapper map)
        {
            List<NoteWrapper>? group = null;
            List<NoteWrapper>? previousGroup = null;
            float groupTime = 0;

            var processGroup = () =>
            {
                var leftNotes = new List<NoteWrapper>();
                var rightNotes = new List<NoteWrapper>();
                if (group != null)
                {
                    foreach (var note in group)
                    {
                        (note.Color == 1 ? leftNotes : rightNotes).Add(note);
                    }

                    processNotesByColorType(leftNotes);
                    processNotesByColorType(rightNotes);

                    previousGroup = group;
                }
            };

            var notes = map.Notes;
            for (var i = 0; i < notes.Length; i++)
            {
                var note = notes[i];
                if (note.Color == 0 || note.Color == 1)
                {
                    if (group == null)
                    {
                        group = [note];
                        groupTime = note.Note.BpmTime;
                    }
                    else
                    {
                        if (Math.Abs(groupTime - note.Note.BpmTime) < 0.0001)
                        {
                            group.Add(note);
                        }
                        else
                        {
                            processGroup();
                            group = null;
                            i--;
                        }
                    }
                }
            }

            processGroup();
        }

        private static MapWrapper ProcessPrivate(DifficultySet difficulty)
        {
            var map = new MapWrapper();

            map.Notes = difficulty.Data.Notes.Select(n => new NoteWrapper { Note = n }).ToArray();
            map.Bombs = difficulty.Data.Bombs.Select(b => new BombWrapper { Note = b }).ToArray();
            map.Walls = difficulty.Data.Walls.Select(w => new WallWrapper { Note = w }).ToArray();
            var numberOfObjects = map.Notes.Length + map.Bombs.Length + (map.Chains?.Length ?? 0);
            var allCuttableObjects = new List<IWrapper<BeatmapGridObject>>(numberOfObjects);
            allCuttableObjects.AddRange(map.Notes);
            allCuttableObjects.AddRange(map.Bombs);

            map.AddScoringTypeAndChains(difficulty.Data);
            if (map.Chains != null) {
                allCuttableObjects.AddRange(map.Chains);
            }
            map.AllCuttableObjects = allCuttableObjects.OrderBy(e => e.Time).ToArray();

            processTimingGroups(map);
            map.SetIds();

            return map;
        }

        public static MapWrapper Process(DifficultySet difficulty)
        {
            var map = ProcessPrivate(difficulty);
            var mirroredData = ChiralitySupport.Mirror_Horizontal(difficulty.Data, 4, true, false);
            map.MirroredMap = ProcessPrivate(new DifficultySet(difficulty.Difficulty, difficulty.Characteristic, mirroredData, difficulty.BeatMap));

            return map;
        }

        public MapWrapper Clone()
        {
            var clonedMap = new MapWrapper();

            // Clone Notes
            clonedMap.Notes = Notes.Select(n => new NoteWrapper
            {
                Note = n.Note,
                ScoringType = n.ScoringType,
                cutDirectionAngleOffset = n.cutDirectionAngleOffset,
                Id = n.Id,
                IdWithLegacyScoring = n.IdWithLegacyScoring,
                IdWithAlternativeScoring = n.IdWithAlternativeScoring,
                IdWithScoring = n.IdWithScoring
            }).ToArray();

            // Clone Bombs
            clonedMap.Bombs = Bombs.Select(b => new BombWrapper
            {
                Note = b.Note,
                ScoringType = b.ScoringType,
                Id = b.Id,
                IdWithLegacyScoring = b.IdWithLegacyScoring,
                IdWithAlternativeScoring = b.IdWithAlternativeScoring,
                IdWithScoring = b.IdWithScoring
            }).ToArray();

            // Clone Walls
            clonedMap.Walls = Walls.Select(w => new WallWrapper
            {
                Note = w.Note,
                ScoringType = w.ScoringType,
                Id = w.Id,
                IdWithLegacyScoring = w.IdWithLegacyScoring,
                IdWithAlternativeScoring = w.IdWithAlternativeScoring,
                IdWithScoring = w.IdWithScoring
            }).ToArray();

            // Clone Chains
            if (Chains != null)
            {
                clonedMap.Chains = Chains.Select(c => new ChainWrapper
                {
                    Note = c.Note,
                    SliderData = c.SliderData,
                    ScoringType = c.ScoringType,
                    SliceIndex = c.SliceIndex,
                    chainX = c.chainX,
                    chainY = c.chainY,
                    chainRotation = c.chainRotation,
                    Id = c.Id,
                    IdWithLegacyScoring = c.IdWithLegacyScoring,
                    IdWithAlternativeScoring = c.IdWithAlternativeScoring,
                    IdWithScoring = c.IdWithScoring
                }).ToArray();
            }

            // Clone AllCuttableObjects
            var allCuttableObjects = new List<IWrapper<BeatmapGridObject>>();
            allCuttableObjects.AddRange(clonedMap.Notes);
            allCuttableObjects.AddRange(clonedMap.Bombs);
            if (clonedMap.Chains != null)
            {
                allCuttableObjects.AddRange(clonedMap.Chains);
            }
            clonedMap.AllCuttableObjects = allCuttableObjects.OrderBy(e => e.Time).ToArray();

            // Clone MirroredMap recursively (but MirroredMap's MirroredMap will point to the original)
            if (MirroredMap != null)
            {
                clonedMap.MirroredMap = MirroredMap.Clone();
            }

            return clonedMap;
        }
    }

    public enum MultiplierEventType
    {
        Positive,
        Neutral,
        Negative
    }

    public class NoteScoreDefinition
    {
        public readonly int maxCenterDistanceCutScore;
        public readonly int minBeforeCutScore;
        public readonly int maxBeforeCutScore;
        public readonly int minAfterCutScore;
        public readonly int maxAfterCutScore;
        public readonly int fixedCutScore;

        public int maxCutScore => this.maxCenterDistanceCutScore + this.maxBeforeCutScore + this.maxAfterCutScore + this.fixedCutScore;

        public int executionOrder => this.maxCutScore;

        public NoteScoreDefinition(
            int maxCenterDistanceCutScore,
            int minBeforeCutScore,
            int maxBeforeCutScore,
            int minAfterCutScore,
            int maxAfterCutScore,
            int fixedCutScore)
        {
            this.maxCenterDistanceCutScore = maxCenterDistanceCutScore;
            this.minBeforeCutScore = minBeforeCutScore;
            this.maxBeforeCutScore = maxBeforeCutScore;
            this.minAfterCutScore = minAfterCutScore;
            this.maxAfterCutScore = maxAfterCutScore;
            this.fixedCutScore = fixedCutScore;
        }
    }

    public class MaxScoreCounterElement
    {
        public NoteScoreDefinition ScoreDef { get; }
        public float Time { get; }

        public MaxScoreCounterElement(NoteScoreDefinition scoreDef, float time)
        {
            ScoreDef = scoreDef;
            Time = time;
        }
    }

    public class ScoreMultiplierCounter
    {
        public int Multiplier { get; }
        public int MultiplierIncreaseProgress { get; }
        public int MultiplierIncreaseMaxProgress { get; }

        public ScoreMultiplierCounter(int multiplier = 1, int multiplierIncreaseProgress = 0, int multiplierIncreaseMaxProgress = 2)
        {
            Multiplier = multiplier;
            MultiplierIncreaseProgress = multiplierIncreaseProgress;
            MultiplierIncreaseMaxProgress = multiplierIncreaseMaxProgress;
        }

        public float NormalizedProgress() => MultiplierIncreaseProgress / (float)MultiplierIncreaseMaxProgress;

        public ScoreMultiplierCounter ProcessMultiplierEvent(MultiplierEventType type)
        {
            switch (type)
            {
                case MultiplierEventType.Positive:
                    if (Multiplier < 8)
                    {
                        if (MultiplierIncreaseProgress >= MultiplierIncreaseMaxProgress - 1)
                        {
                            return new ScoreMultiplierCounter(Multiplier * 2, 0, Multiplier * 4);
                        } else
                        {
                            return new ScoreMultiplierCounter(Multiplier, MultiplierIncreaseProgress + 1, MultiplierIncreaseMaxProgress);
                        }
                    } else
                    {
                        return this;
                    }
                case MultiplierEventType.Negative:
                    if (MultiplierIncreaseProgress > 0)
                    {
                        return new ScoreMultiplierCounter(Multiplier, 0, MultiplierIncreaseMaxProgress);
                    } else if (Multiplier > 1)
                    {
                        return new ScoreMultiplierCounter(Multiplier / 2, MultiplierIncreaseProgress, Multiplier);
                    } else
                    {
                        return this;
                    }
                case MultiplierEventType.Neutral:
                    return this;
                default:
                    throw new ArgumentOutOfRangeException(nameof(type), "Unknown MultiplierEventType");
            }
        }
    }

    public static class ScoringExtensions
    {
        public static readonly Dictionary<ScoringType, NoteScoreDefinition> ScoreDefinitions = new Dictionary<ScoringType, NoteScoreDefinition>()
        {
            {
                ScoringType.Ignore,
                (NoteScoreDefinition) null
            },
            {
                ScoringType.NoScore,
                new NoteScoreDefinition(0, 0, 0, 0, 0, 0)
            },
            {
                ScoringType.Normal,
                new NoteScoreDefinition(15, 0, 70, 0, 30, 0)
            },
            {
                ScoringType.ArcHead,
                new NoteScoreDefinition(15, 0, 70, 30, 30, 0)
            },
            {
                ScoringType.ArcTail,
                new NoteScoreDefinition(15, 70, 70, 0, 30, 0)
            },
            {
                ScoringType.ChainHead,
                new NoteScoreDefinition(15, 0, 70, 0, 0, 0)
            },
            {
                ScoringType.ChainLink,
                new NoteScoreDefinition(0, 0, 0, 0, 0, 20)
            },
            {
                ScoringType.ArcHeadArcTail,
                new NoteScoreDefinition(15, 70, 70, 30, 30, 0)
            },
            {
                ScoringType.ChainHeadArcTail,
                new NoteScoreDefinition(15, 70, 70, 30, 30, 0)
            },
            {
                ScoringType.ChainLinkArcHead,
                new NoteScoreDefinition(0, 0, 0, 0, 0, 20)
            },
            {
                ScoringType.ChainHeadArcHead,
                new NoteScoreDefinition(15, 0, 70, 30, 30, 0)
            },
            {
                ScoringType.ChainHeadArcHeadArcTail,
                new NoteScoreDefinition(15, 70, 70, 30, 30, 0)
            }
        };

        public static List<(float, int)> MaxScoreGraph(this MapWrapper self)
        {
            var maxScores = new List<(float, int)>();
            var smc = new ScoreMultiplierCounter();
            var score = 0;
            foreach (var item in self.AllCuttableObjects.Where(n => n.ScoringType >= ScoringType.Normal)) {
                smc = smc.ProcessMultiplierEvent(MultiplierEventType.Positive);
                score += ScoreDefinitions[item.ScoringType].maxCutScore * smc.Multiplier;

                maxScores.Add((item.Time, score));
            }

            return maxScores;
        }

        public static bool IsV3Pepega(this DifficultySet self)
        {
            var notes = self.Data.Notes.Where(note => note.Color == 0 || note.Color == 1);
            var sliders = self.Data.Arcs;
            var burstSliders = self.Data.Chains;

            var slidersByBeat = sliders.GroupBy(s => s.BpmTime).ToDictionary(g => g.Key, g => g.ToList());
            var slidersByTailBeat = sliders.GroupBy(s => s.TailBpmTime).ToDictionary(g => g.Key, g => g.ToList());
            var burstSlidersByBeat = burstSliders.GroupBy(s => s.BpmTime).ToDictionary(g => g.Key, g => g.ToList());

            var noteItems = notes.Any(note =>
            {
                var matchesHead = slidersByBeat.ContainsKey(note.BpmTime) && slidersByBeat[note.BpmTime].Any(s => note.Color == s.Color && note.x == s.x && note.y == s.y);
                var matchesTail = slidersByTailBeat.ContainsKey(note.BpmTime) && slidersByTailBeat[note.BpmTime].Any(s => note.Color == s.Color && note.x == s.tx && note.y == s.ty);
                var matchesBurst = burstSlidersByBeat.ContainsKey(note.BpmTime) && burstSlidersByBeat[note.BpmTime].Any(s => note.Color == s.Color && note.x == s.x && note.y == note.y);

                return matchesBurst && matchesHead && matchesTail || matchesBurst && matchesHead || matchesBurst && matchesTail || matchesHead && matchesTail;
            });

            return noteItems;
        }

        public static int MaxScore(this MapWrapper self) {
            var graph = MaxScoreGraph(self);
            return graph.Count > 0 ? graph.Last().Item2 : 0;
        }
    }
}
