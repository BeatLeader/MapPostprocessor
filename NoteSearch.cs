using Parser.Map;
using Parser.Map.Difficulty.V3.Base;
using Parser.Utils;
using ReplayDecoder;

namespace MapPostprocessor
{
    public static class NoteSearch
    {
	 	public static Dictionary<int, bool> TryFindingNotes(MapWrapper map, Replay replay, IWrapper<BeatmapGridObject>[] mapnotes) {
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

			for (var j = 0; j < mapnotes.Length; j++) {
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
			var bombs = replay.notes.Where(n => n.eventType == NoteEventType.bomb).ToList();
			for (var j = 0; j < mapnotes.Length; j++) {
				var mapnote = mapnotes[j];
				for (var m = 0; m < bombs.Count; m++) {
					var replaynote = bombs[m];

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

		public static Dictionary<int, bool> TryFindingWalls(MapWrapper map, Replay replay, WallWrapper[] mapnotes) {
			var foundNotes = new Dictionary<int, bool>();
			var walls = replay.walls.ToList();
			for (var j = 0; j < mapnotes.Length; j++) {
				var mapnote = mapnotes[j];
				for (var m = 0; m < walls.Count; m++) {
					var replaynote = walls[m];

					if (!foundNotes.ContainsKey(m)) {
						if (
							replaynote.time >= mapnote.Note.Seconds && replaynote.time <= (mapnote.Note.Seconds + mapnote.Note.DurationInSeconds) &&
							(replaynote.wallID == mapnote.Id)
						) {
							mapnote.WallEvent = replaynote;
							foundNotes[m] = true;
							break;
						}
					}
				}
			}

			return foundNotes;
		}

        public static MapWrapper ForReplay(this MapWrapper map, Replay replay) {
			var result = map.Clone();

			var foundNotes = TryFindingNotes(map, replay, result.AllCuttableObjects);

			if (foundNotes.Keys.Count < map.Notes.Length) {
				var mirrored = map.MirroredMap;
				var foundMirrored = TryFindingNotes(map, replay, mirrored.AllCuttableObjects);

				if (foundMirrored.Keys.Count > foundNotes.Keys.Count) {
					result = mirrored;
				} else {
					//Console.WriteLine($"Broken replay {replay.info.hash} {replay.info.playerName} {replay.info.difficulty} {replay.info.mode}");
				}
			}

			TryFindingBombs(result, replay, result.Bombs);
			TryFindingWalls(result, replay, result.Walls);

			return result;
        }
    }
}
