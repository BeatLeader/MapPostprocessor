using Parser.Map;
using Parser.Map.Difficulty.V3.Base;
using Parser.Utils;
using ReplayDecoder;

namespace MapPostprocessor
{
    public static class NoteSearch
    {
	 	public static Dictionary<int, bool> TryFindingNotes(MapWrapper map, Replay replay, List<IWrapper<BeatmapGridObject>> mapnotes) {
			var foundNotes = new Dictionary<int, bool>();
			var nonBombs = replay.notes.Where(n => n.eventType != NoteEventType.bomb).ToList();
			foreach (var mapnote in mapnotes)
			{
				for (var m = 0; m < nonBombs.Count; m++) {
					var replaynote = nonBombs[m];

					if (!foundNotes.ContainsKey(m)) {
						if (
							Math.Abs(replaynote.spawnTime - mapnote.Time) < 0.0005 &&
							(replaynote.noteID == mapnote.Id ||
							 replaynote.noteID == mapnote.IdWithScoring ||
							 replaynote.noteID == mapnote.IdWithAlternativeScoring ||
							 replaynote.noteID == mapnote.IdWithLegacyScoring)
						) {
							mapnote.Event = replaynote;
							foundNotes[m] = true;
							break;
						}
					}
				}
			}

			for (var j = 0; j < mapnotes.Count; j++) {
				var mapnote = mapnotes[j];
				if (mapnote.Event == null) {
					for (var m = 0; m < nonBombs.Count; m++) {
						var replaynote = nonBombs[m];

						if (!foundNotes.ContainsKey(m)) {
							if (
								replaynote.noteID == mapnote.Id ||
								replaynote.noteID == mapnote.IdWithScoring ||
								replaynote.noteID == mapnote.IdWithAlternativeScoring ||
								replaynote.noteID == mapnote.IdWithLegacyScoring
							) {
								mapnote.Event = replaynote;
								break;
							}
						}
					}
				}
			}

			return foundNotes;
		}

		public static Dictionary<int, bool> TryFindingBombs(MapWrapper map, Replay replay, BombWrapper[] mapnotes) {
			var foundNotes = new Dictionary<int, bool>();
			var nonBombs = replay.notes.Where(n => n.eventType == NoteEventType.bomb).ToList();
			foreach (var mapnote in mapnotes)
			{
				for (var m = 0; m < nonBombs.Count; m++) {
					var replaynote = nonBombs[m];

					if (!foundNotes.ContainsKey(m)) {
						if (
							Math.Abs(replaynote.spawnTime - mapnote.Time) < 0.0005 &&
							(replaynote.noteID == mapnote.Id ||
							 replaynote.noteID == mapnote.IdWithScoring ||
							 replaynote.noteID == mapnote.IdWithAlternativeScoring ||
							 replaynote.noteID == mapnote.IdWithLegacyScoring)
						) {
							mapnote.Event = replaynote;
							foundNotes[m] = true;
							break;
						}
					}
				}
			}

			return foundNotes;
		}

        public static MapWrapper ForReplay(this MapWrapper map, Replay replay) {
			var result = map;

            var allElements = new List<IWrapper<BeatmapGridObject>>();
            allElements.AddRange(map.Notes);
            allElements.AddRange(map.Bombs);
            if (map.Chains != null) {
                allElements.AddRange(map.Chains);
            }
            foreach (var item in allElements)
			{
				item.Event = null;
			}

			var foundNotes = TryFindingNotes(map, replay, allElements.OrderBy(n => n.Time).ToList());

			if (foundNotes.Keys.Count < map.Notes.Length) {
				var mirroredData = ChiralitySupport.Mirror_Horizontal(map.Difficulty.Data, 4, true, false);
				var mirrored = MapWrapper.Process(new DifficultySet(map.Difficulty.Difficulty, map.Difficulty.Characteristic, mirroredData, map.Difficulty.BeatMap));
                var mirroredElements = new List<IWrapper<BeatmapGridObject>>();
                mirroredElements.AddRange(mirrored.Notes);
                mirroredElements.AddRange(mirrored.Bombs);
                if (mirrored.Chains != null) {
                    mirroredElements.AddRange(mirrored.Chains);
                }
                foreach (var item in mirroredElements)
			    {
				    item.Event = null;
			    }

                var sortedMirrored = mirroredElements.OrderBy(n => n.Time).ToList();
				var foundMirrored = TryFindingNotes(map, replay, sortedMirrored);

				if (foundMirrored.Keys.Count > foundNotes.Keys.Count) {
					result = mirrored;
				} else {
					//Console.WriteLine($"Broken replay {replay.info.hash} {replay.info.playerName} {replay.info.difficulty} {replay.info.mode}");
				}
			}

			TryFindingBombs(result, replay, result.Bombs);

			return result;
        }
    }
}
