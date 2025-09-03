using Parser.Map.Difficulty.V3.Base;

namespace MapPostprocessor
{
    public static class NoteExtensions 
    {
        static void encodeInt(float[,] array, int count, ref int index, int min, int value, int limit)
        {
            for (int i = min; i <= limit; i++)
            {
                array[count, +index++] = i == value ? 1 : 0;
            }
        }

        public static void EncodeToArray<T>(this IWrapper<T> note, float[,] array, int count, float _time) where T : BeatmapGridObject
        {
            int index = 0;
            array[count, +index++] = note.Time - _time;
            //array[count, index++] = type == 0 ? param1 * 0.5f : param1;
            //array[count, index++] = type == 0 ? param2 * 0.5f : param2;
            //array[count, index++] = type == 0 ? param3 * 0.5f : param3;
            

            if (note.Event != null && note.Event.noteCutInfo != null) {
                var noteCutInfo = note.Event.noteCutInfo;

                array[count, +index++] = noteCutInfo.beforeCutRating / 2;
                array[count, +index++] = noteCutInfo.afterCutRating / 2;
                array[count, +index++] = Math.Max(0.0f, 1.0f - 3.0f * noteCutInfo.cutDistanceToCenter);
                array[count, +index++] = noteCutInfo.timeDeviation;
            } else if (note.Event != null) {
                array[count, +index++] = (float)note.Event.eventType / 5.0f;
                array[count, +index++] = (float)note.Event.eventType / 5.0f;
                array[count, +index++] = (float)note.Event.eventType / 5.0f;
                array[count, +index++] = (float)note.Event.eventType / 5.0f;
            } else {
                array[count, +index++] = 0;
                array[count, +index++] = 0;
                array[count, +index++] = 0;
                array[count, +index++] = 0;
            }

            if (note.Color == 0)
            {
                encodeInt(array, count, ref index, 0, note.LineIndex, 3);
                encodeInt(array, count, ref index, 0, note.LineLayer, 2);
                encodeInt(array, count, ref index, 0, note.CutDirection, 9);
                encodeInt(array, count, ref index, 0, -2, 16);
            } else if (note.Color == 1)
            {
                encodeInt(array, count, ref index, 0, -2, 16);
                encodeInt(array, count, ref index, 0, note.LineIndex, 3);
                encodeInt(array, count, ref index, 0, note.LineLayer, 2);
                encodeInt(array, count, ref index, 0, note.CutDirection, 9);

            } else
            {
                encodeInt(array, count, ref index, 0, note.LineIndex, 3);
                encodeInt(array, count, ref index, 0, note.LineLayer, 2);
                encodeInt(array, count, ref index, 0, 9, 9);
                encodeInt(array, count, ref index, 0, note.LineIndex, 3);
                encodeInt(array, count, ref index, 0, note.LineLayer, 2);
                encodeInt(array, count, ref index, 0, 9, 9);
            }
            //encodeInt(array, -1, _scoringType, 5);
        }

    }
}

