using Parser.Map;
using Parser.Map.Difficulty.V3.Base;
using Parser.Map.Difficulty.V3.Grid;
using System.Numerics;

namespace MapPostprocessor
{
    public class MapWrapper
    {
		public DifficultySet Difficulty { get; set; }
        public List<NoteWrapper> Notes { get; set; }
        public List<BombWrapper> Bombs { get; set; }
        public List<WallWrapper> Walls { get; set; }
        public List<ChainWrapper> Chains { get; set; }

		private bool CompareSlider(Note note, BeatmapColorGridObjectWithTail slider) {
			if (Math.Round(note.BpmTime, 2) != Math.Round(slider.BpmTime, 2)) return false;

			if (note.x == slider.x && note.y == slider.y)
				return true;

			if (note.customData != null && note.customData.coordinates != null)
			{
				if (slider.customData != null && slider.customData.coordinates != null)
				{
					if (
						Math.Round(note.customData.coordinates[0], 2) == Math.Round(slider.customData.coordinates[0], 2) &&
						Math.Round(note.customData.coordinates[1], 2) == Math.Round(slider.customData.coordinates[1], 2))
						return true;
				} else
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

		private bool CompareSliderTail(Note note, BeatmapColorGridObjectWithTail slider) {

			if (Math.Round(note.BpmTime, 2) != Math.Round(slider.TailBpmTime, 2)) return false;

			if (note.x == slider.tx && note.y == slider.ty)
				return true;

			if (note.customData != null && note.customData.coordinates != null)
			{
				if (slider.customData != null && slider.customData.tailCoordinates != null)
				{
					if (
						Math.Round(note.customData.coordinates[0], 2) == Math.Round(slider.customData.tailCoordinates[0], 2) &&
						Math.Round(note.customData.coordinates[1], 2) == Math.Round(slider.customData.tailCoordinates[1], 2))
						return true;
				} else
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

		private float LerpUnclamped(float a, float b, float t) {
			return a + (b - a) * t;
		}

		public static void BezierCurve(
            Vector2 p0,
            Vector2 p1,
            Vector2 p2,
            float t,
            out Vector2 pos,
            out Vector2 tangent) {
            float num = 1f - t;
            pos = num * num * p0 + 2f * num * t * p1 + t * t * p2;
            tangent = (float)(2.0 * (1.0 - (double)t)) * (p1 - p0) + 2f * t * (p2 - p1);
        }

        private void AddScoringTypeAndChains(DifficultyV3 difficulty) {

			foreach (var note in Notes) {
				if (note.Note.Color == 1 || note.Note.Color == 0) {
					note.ScoringType = ScoringType.Normal;
				} else {
					note.ScoringType = ScoringType.NoScore;
				}
			}

			foreach (var slider in difficulty.Arcs) {
				var head = Notes.FirstOrDefault(n => CompareSlider(n.Note, slider));
				if (head != null) {
					if (head.ScoringType == ScoringType.Normal) {
						head.ScoringType = ScoringType.ArcHead;
					} else if (head.ScoringType == ScoringType.ArcTail) {
						head.ScoringType = ScoringType.ArcHeadArcTail;
					}
				}
				var tail = slider.TailBpmTime == slider.BpmTime ? null : Notes.FirstOrDefault(n => CompareSliderTail(n.Note, slider));
				//if (head) {
				//	head.tail = tail;
				//}

				//slider.tail = tail;
				if (tail != null) {
					if (tail.ScoringType == ScoringType.Normal) {
						tail.ScoringType = ScoringType.ArcTail;
					} else if (tail.ScoringType == ScoringType.ArcHead) {
						tail.ScoringType = ScoringType.ArcHeadArcTail;
					}
				}
			}

			var chains = new List<ChainWrapper>();

			foreach (var slider in difficulty.Chains) {
				var head = Notes.FirstOrDefault(n => CompareSlider(n.Note, slider));
				if (head != null) {
					if (head.ScoringType == ScoringType.Normal) {
						head.ScoringType = ScoringType.ChainHead;
					} else if (head.ScoringType == ScoringType.ArcTail) {
						head.ScoringType = ScoringType.ChainHeadArcTail;
					} else if (head.ScoringType == ScoringType.ArcHead) {
						head.ScoringType = ScoringType.ChainHeadArcHead;
					} else if (head.ScoringType == ScoringType.ArcHeadArcTail) {
						head.ScoringType = ScoringType.ChainHeadArcHeadArcTail;
					}
					//head.sliderhead = slider;
				}
				for (var i = 1; i < slider.SliceCount; ++i) {
					var chain = new Note {
						x = slider.x,
						y = slider.y,
						CutDirection = 8,
						Color = slider.Color,
					};

					chain.Seconds = LerpUnclamped(slider.Seconds, slider.TailInSeconds, i / (slider.SliceCount - 1));

					Vector3 vector3_1 = new Vector3(head.X, head.Y, 0f);

					float horizontalPosition = slider.tx;
					if (slider.customData?.tailCoordinates != null) {
						horizontalPosition = slider.customData?.tailCoordinates[0] + 4 / 2 ?? 0;
					}
					if (horizontalPosition <= -1000 || horizontalPosition >= 1000) {
						horizontalPosition = horizontalPosition < 0 ? horizontalPosition / 1000 + 1 : horizontalPosition / 1000 - 1;
					}

					float tailX = GenericWrapper<BeatmapGridObject>.GetHorizontalPosition(horizontalPosition);
					float verticalPosition = slider.ty;
					if (slider.customData?.tailCoordinates != null) {
						verticalPosition = slider.customData?.tailCoordinates[1] ?? 0;
					}
					if (verticalPosition <= -1000 || verticalPosition >= 1000) {
						verticalPosition = verticalPosition < 0 ? verticalPosition / 1000 + 1 : verticalPosition / 1000 - 1;
					}

					float tailY = GenericWrapper<BeatmapGridObject>.HighestJumpPosYForLineLayer(verticalPosition);
					Vector3 vector3_2 = new Vector3(tailX, tailY, 0f);
					Vector2 p2 = new Vector2(vector3_2.X - vector3_1.X, vector3_2.Y - vector3_1.Y);
					float magnitude = p2.Length();
					float f = (float)(((double)NoteCutDirectionExtensions.RotationAngle((NoteCutDirection)slider.CutDirection) - 90.0 + (double)head.cutDirectionAngleOffset) * (Math.PI / 180.0));

					Vector2 p1 = (0.5f * magnitude * new Vector2((float)Math.Cos(f), (float)Math.Sin(f)));
					int sliceCount = slider.SliceCount;
					float squishAmount = slider.Squish;
					var index = i;

					float t = (float)index / (float)(sliceCount - 1);

					Vector2 pos;
					Vector2 tangent;
					BezierCurve(new Vector2(0.0f, 0.0f), p1, p2, t * squishAmount, out pos, out tangent);

					var arcSlider = difficulty.Arcs.FirstOrDefault(n => CompareSlider(chain, n));
					chains.Add(new ChainWrapper {
						Note = chain,
						SliceIndex = i,
						ScoringType = arcSlider != null ? ScoringType.ChainLinkArcHead : ScoringType.ChainLink,
						chainRotation = NoteCutDirectionExtensions.SignedAngle(new Vector2(0.0f, -1f), tangent),
						chainX = pos.X,
						chainY = pos.Y,
					});
				}
			}

			Chains = chains;
		}

		public void SetIds() {
			var allElements = new List<IWrapper<BeatmapGridObject>>();
			allElements.AddRange(Notes);
			allElements.AddRange(Bombs);
			allElements.AddRange(Chains);

			foreach (var mapnote in allElements) {
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
				if (mapnote.ScoringType == ScoringType.ChainHead) {
					altscoringType = ScoringType.ArcHead + 2;
				} else if (mapnote.ScoringType == ScoringType.ArcHead) {
					altscoringType = ScoringType.ChainHead + 2;
				} else if (mapnote.ScoringType == ScoringType.ChainHeadArcTail) {
					altscoringType = ScoringType.ArcTail + 2;
				} else if (mapnote.ScoringType == ScoringType.ChainHeadArcHead) {
					altscoringType = ScoringType.ArcHead + 2;
				} else if (mapnote.ScoringType == ScoringType.ChainHeadArcHeadArcTail) {
					altscoringType = ScoringType.ArcTail + 2;
				} else if (mapnote.ScoringType == ScoringType.ChainLinkArcHead) {
					altscoringType = ScoringType.ChainLink + 2;
				} else if (mapnote.ScoringType == ScoringType.ArcHeadArcTail) {
					altscoringType = ScoringType.ArcHead + 2;
				}

				if (mapnote.ScoringType == ScoringType.ArcHeadArcTail) {
					legacyScoringType = ScoringType.ArcTail + 2;
				} else if (mapnote.ScoringType == ScoringType.ChainHeadArcTail) {
					legacyScoringType = ScoringType.ChainHead + 2;
				} else if (mapnote.ScoringType == ScoringType.ChainLinkArcHead) {
					legacyScoringType = ScoringType.ChainLink + 2;
				} else if (mapnote.ScoringType == ScoringType.ChainHeadArcHead) {
					legacyScoringType = ScoringType.ChainHead + 2;
				} else if (mapnote.ScoringType == ScoringType.ChainHeadArcHeadArcTail) {
					legacyScoringType = ScoringType.ChainHeadArcTail + 2;
				}

				mapnote.IdWithAlternativeScoring = id + (int)altscoringType * 10000;
				mapnote.IdWithLegacyScoring = id + (int)legacyScoringType * 10000;
			}
		}

		private static float getVerticalPosition(float lineLayer) {
			return 0.25f + 0.6f * lineLayer;
		}

		private static float getHorizontalPosition(float lineIndex) {
			return (-(4 - 1) * 0.5f + lineIndex) * 0.6f;
		}

		private static Vector2 get2DNoteOffset(float noteLineIndex, float noteLineLayer) {
			return new Vector2(getHorizontalPosition(noteLineIndex), getVerticalPosition(noteLineLayer));
		}

		public static float SignedAngleToLine(Vector2 vec, Vector2 line)
        {
			float f1 = NoteCutDirectionExtensions.SignedAngle(vec, line);
			float f2 = NoteCutDirectionExtensions.SignedAngle(vec, -line);
			return (double) Math.Abs(f1) >= (double) Math.Abs(f2) ? f2 : f1;
        }

		private static void processNotesByColorType(List<NoteWrapper> notesWithTheSameColorTypeList) {
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
			if (theSameColorType1.CutDirection != 8) {
				noteData1 = theSameColorType1;
				noteData2 = theSameColorType2;
			} else {
				noteData1 = theSameColorType2;
				noteData2 = theSameColorType1;
			}


			var line1 = get2DNoteOffset(noteData2.LineIndex, noteData2.LineLayer) - get2DNoteOffset(noteData1.LineIndex, noteData1.LineLayer);
			var line2 = SignedAngleToLine(
				noteData1.CutDirection == (int)NoteCutDirection.Any ? new Vector2(0.0f, 1f) : NoteCutDirectionExtensions.Direction((NoteCutDirection)noteData1.CutDirection),
				line1
			);
			if (noteData2.CutDirection == (int)NoteCutDirection.Any && noteData1.CutDirection == (int)NoteCutDirection.Any) {
				noteData1.cutDirectionAngleOffset = line2;
				noteData2.cutDirectionAngleOffset = line2;
			} else {
				if (Math.Abs(line2) > 40) return;
				noteData1.cutDirectionAngleOffset = line2;
				if (noteData2.CutDirection == (int)NoteCutDirection.Any && noteData1.CutDirection > (int)NoteCutDirection.Right) {
					noteData2.cutDirectionAngleOffset = line2 + 45;
				} else {
					noteData2.cutDirectionAngleOffset = line2;
				}
			}
		}

		private static void processTimingGroups(MapWrapper map) {
			List<NoteWrapper>? group = null;
			List<NoteWrapper>? previousGroup = null;
			float groupTime = 0;

			var processGroup = () => {
				var leftNotes = new List<NoteWrapper>();
				var rightNotes = new List<NoteWrapper>();
				if (group != null) {
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
			for (var i = 0; i < notes.Count; i++) {
				var note = notes[i];
				if (note.Color == 0 || note.Color == 1) {
					if (group == null) {
						group = [note];
						groupTime = note.Note.BpmTime;
					} else {
						if (Math.Abs(groupTime - note.Note.BpmTime) < 0.0001) {
							group.Add(note);
						} else {
							processGroup();
							group = null;
							i--;
						}
					}
				}
			}
			processGroup();
		}

        public static MapWrapper Process(DifficultySet difficulty) {
            var map = new MapWrapper { Difficulty = difficulty };

            map.Notes = difficulty.Data.Notes.Select(n => new NoteWrapper { Note = n }).ToList();
            map.Bombs = difficulty.Data.Bombs.Select(b => new BombWrapper { Note = b }).ToList();
            map.Walls = difficulty.Data.Walls.Select(w => new WallWrapper { Note = w }).ToList();

			map.AddScoringTypeAndChains(difficulty.Data);
			processTimingGroups(map);
			map.SetIds();

            return map;
        }
    }
}
