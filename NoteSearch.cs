using Parser.Map;
using Parser.Utils;
using ReplayDecoder;

namespace MapPostprocessor
{
    public static class NoteSearch
    {
	 	public static Dictionary<int, bool> TryFindingNotes(MapWrapper map, Replay replay, List<NoteWrapper> mapnotes) {
			var foundNotes = new Dictionary<int, bool>();
			var nonBombs = replay.notes.Where(n => n.eventType != NoteEventType.bomb).ToList();
			for (var j = 0; j < mapnotes.Count; j++) {
				var mapnote = mapnotes[j];
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

		public static Dictionary<int, bool> TryFindingBombs(MapWrapper map, Replay replay, List<BombWrapper> mapnotes) {
			var foundNotes = new Dictionary<int, bool>();
			var nonBombs = replay.notes.Where(n => n.eventType == NoteEventType.bomb).ToList();
			for (var j = 0; j < mapnotes.Count; j++) {
				var mapnote = mapnotes[j];
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
			foreach (var item in map.Notes)
			{
				item.Event = null;
			}
			foreach (var item in map.Bombs)
			{
				item.Event = null;
			}
			foreach (var item in map.Chains)
			{
				item.Event = null;
			}

			var foundNotes = TryFindingNotes(map, replay, map.Notes);

			if (foundNotes.Keys.Count < map.Notes.Count) {
				var mirroredData = ChiralitySupport.Mirror_Horizontal(map.Difficulty.Data, 4, true, false);
				var mirrored = MapWrapper.Process(new DifficultySet(map.Difficulty.Difficulty, map.Difficulty.Characteristic, mirroredData, map.Difficulty.BeatMap));
				var foundMirrored = TryFindingNotes(map, replay, mirrored.Notes);

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
