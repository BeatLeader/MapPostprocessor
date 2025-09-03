using Parser.Map.Difficulty.V3.Base;
using Parser.Map.Difficulty.V3.Grid;

namespace MapPostprocessor
{
    public class MapWrapper
    {
		public DifficultyV3 Difficulty { get; set; }
        public List<NoteWrapper> Notes { get; set; }
        public List<BombWrapper> Bombs { get; set; }
        public List<WallWrapper> Walls { get; set; }
        public List<ChainWrapper> Chains { get; set; }

		private bool CompareSlider(Note note, BeatmapColorGridObjectWithTail slider) {
			if (Math.Round(note.Beats, 2) != Math.Round(slider.Beats, 2)) return false;

			if (note.x == slider.x && note.y == slider.y)
				return true;

			if (note.CustomData != null && note.CustomData._position != null) {
				if (slider.CustomData != null && slider.CustomData._position != null) {
					if (
						Math.Round(note.CustomData._position[0], 2) == Math.Round(slider.CustomData._position[0], 2) &&
						Math.Round(note.CustomData._position[1], 2) == Math.Round(slider.CustomData._position[1], 2))
						return true;
				} else {
					if (
						Math.Round(note.CustomData._position[0] + 4 / 2) == slider.x &&
						Math.Round(note.CustomData._position[1]) == slider.y
					)
						return true;
				}
			}

			return false;
		}

		private bool CompareSliderTail(Note note, BeatmapColorGridObjectWithTail slider) {

			if (Math.Round(note.Beats, 2) != Math.Round(slider.TailBpmTime, 2)) return false;

			if (note.x == slider.tx && note.y == slider.ty)
				return true;

			if (note.CustomData != null && note.CustomData._position != null) {
				if (slider.CustomData != null && slider.CustomData._tailPosition != null) {
					if (
						Math.Round(note.CustomData._position[0], 2) == Math.Round(slider.CustomData._tailPosition[0], 2) &&
						Math.Round(note.CustomData._position[1], 2) == Math.Round(slider.CustomData._tailPosition[1], 2))
						return true;
				} else {
					if (
						Math.Round(note.CustomData._position[0] + 4 / 2) == slider.tx &&
						Math.Round(note.CustomData._position[1]) == slider.ty
					)
						return true;
				}
			}

			return false;
		}

		private float LerpUnclamped(float a, float b, float t) {
			return a + (b - a) * t;
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
				var tail = Notes.FirstOrDefault(n => CompareSliderTail(n.Note, slider));
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

					var arcSlider = difficulty.Arcs.FirstOrDefault(n => CompareSlider(chain, n));
					chains.Add(new ChainWrapper {
						Note = chain,
						SliceIndex = i,
						ScoringType = arcSlider != null ? ScoringType.ChainLinkArcHead : ScoringType.ChainLink,
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
					legacyScoringType = ScoringType.ChainHead + 2;
				}

				mapnote.IdWithAlternativeScoring = id + (int)altscoringType * 10000;
				mapnote.IdWithLegacyScoring = id + (int)legacyScoringType * 10000;
			}
		}

        public static MapWrapper Process(DifficultyV3 difficulty) {
            var map = new MapWrapper { Difficulty = difficulty };

            map.Notes = difficulty.Notes.Select(n => new NoteWrapper { Note = n }).ToList();
            map.Bombs = difficulty.Bombs.Select(b => new BombWrapper { Note = b }).ToList();
            map.Walls = difficulty.Walls.Select(w => new WallWrapper { Note = w }).ToList();

			map.AddScoringTypeAndChains(difficulty);
			map.SetIds();

            return map;
        }
    }
}
