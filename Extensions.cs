using Parser.Map.Difficulty.V3.Base;
using Parser.Map.Difficulty.V3.Grid;

namespace MapPostprocessor
{
    public struct ParamOverride {
        public float Param1 { get; set; }
        public float Param2 { get; set; }
        public float Param3 { get; set; }
        public float Param4 { get; set; }

        public ParamOverride(float param1, float param2, float param3, float param4) : this()
        {
            this.Param1 = param1;
            this.Param2 = param2;
            this.Param3 = param3;
            this.Param4 = param4;
        }

        
    }
    public static class NoteExtensions 
    {
        static void encodeInt(float[,] array, int count, ref int index, int min, int value, int limit)
        {
            for (int i = min; i <= limit; i++)
            {
                array[count, +index++] = i == value ? 1 : 0;
            }
        }

        public static void EncodeToArray<T>(this IWrapper<T> note, float[,] array, int count, float _time, ParamOverride? paramOverride = null) where T : BeatmapGridObject
        {
            int index = 0;
            array[count, +index++] = note.Time - _time;
            //array[count, index++] = type == 0 ? param1 * 0.5f : param1;
            //array[count, index++] = type == 0 ? param2 * 0.5f : param2;
            //array[count, index++] = type == 0 ? param3 * 0.5f : param3;
            
            if (paramOverride != null) {

                array[count, +index++] = paramOverride?.Param1 ?? 0;
                array[count, +index++] = paramOverride?.Param2 ?? 0;
                array[count, +index++] = paramOverride?.Param3 ?? 0;
                array[count, +index++] = paramOverride?.Param4 ?? 0;
            } else if (note.Event != null && note.Event.noteCutInfo != null) {
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
                //encodeInt(array, count, ref index, 0, note.LineIndex, 3);
                //encodeInt(array, count, ref index, 0, note.LineLayer, 2);
                array[count, +index++] = note.X / 2.0f;
                array[count, +index++] = note.Y / 2.0f;
                array[count, +index++] = note.Rotation / 360.0f;
                encodeInt(array, count, ref index, 0, note.CutDirection, 9);
                encodeInt(array, count, ref index, 0, -2, 12);
            } else if (note.Color == 1)
            {
                encodeInt(array, count, ref index, 0, -2, 12);
                //encodeInt(array, count, ref index, 0, note.LineIndex, 3);
                //encodeInt(array, count, ref index, 0, note.LineLayer, 2);
                array[count, +index++] = note.X / 2.0f;
                array[count, +index++] = note.Y / 2.0f;
                array[count, +index++] = note.Rotation / 360.0f;

                encodeInt(array, count, ref index, 0, note.CutDirection, 9);

            } else
            {
                //encodeInt(array, count, ref index, 0, note.LineIndex, 3);
                //encodeInt(array, count, ref index, 0, note.LineLayer, 2);
                array[count, +index++] = note.X / 2.0f;
                array[count, +index++] = note.Y / 2.0f;
                array[count, +index++] = 0;
                encodeInt(array, count, ref index, 0, 9, 9);
                array[count, +index++] = note.X / 2.0f;
                array[count, +index++] = note.Y / 2.0f;
                //encodeInt(array, count, ref index, 0, note.LineIndex, 3);
                //encodeInt(array, count, ref index, 0, note.LineLayer, 2);
                array[count, +index++] = 0;
                encodeInt(array, count, ref index, 0, 9, 9);
            }
            //encodeInt(array, -1, _scoringType, 5);
        }


        public static void EncodeToArray(this WallWrapper note, float[,] array, int count, float _time, ParamOverride? paramOverride = null)
        {
            int index = 0;
            array[count, +index++] = Math.Max(note.Time - _time, 0);
            array[count, +index++] = note.Time - _time > 0 || note.Time + note.Note.DurationInSeconds - _time < 0 ? 0 : 1;
            //array[count, index++] = type == 0 ? param1 * 0.5f : param1;
            //array[count, index++] = type == 0 ? param2 * 0.5f : param2;
            //array[count, index++] = type == 0 ? param3 * 0.5f : param3;
            
            //if (paramOverride != null) {

            //    array[count, +index++] = paramOverride?.Param1 ?? 0;
            //    array[count, +index++] = paramOverride?.Param2 ?? 0;
            //    array[count, +index++] = paramOverride?.Param3 ?? 0;
            //    array[count, +index++] = paramOverride?.Param4 ?? 0;
            //} else if (note.Event != null && note.Event.noteCutInfo != null) {
            //    var noteCutInfo = note.Event.noteCutInfo;

            //    array[count, +index++] = noteCutInfo.beforeCutRating / 2;
            //    array[count, +index++] = noteCutInfo.afterCutRating / 2;
            //    array[count, +index++] = Math.Max(0.0f, 1.0f - 3.0f * noteCutInfo.cutDistanceToCenter);
            //    array[count, +index++] = noteCutInfo.timeDeviation;
            //} else if (note.Event != null) {
            //    array[count, +index++] = (float)note.Event.eventType / 5.0f;
            //    array[count, +index++] = (float)note.Event.eventType / 5.0f;
            //    array[count, +index++] = (float)note.Event.eventType / 5.0f;
            //    array[count, +index++] = (float)note.Event.eventType / 5.0f;
            //} else {
            //    array[count, +index++] = 0;
            //    array[count, +index++] = 0;
            //    array[count, +index++] = 0;
            //    array[count, +index++] = 0;
            //}

            array[count, +index++] = note.X / 2.0f;
            array[count, +index++] = note.Y / 2.0f;
            array[count, +index++] = note.Width / 2.0f;
            array[count, +index++] = note.Height / 2.0f;
            //encodeInt(array, -1, _scoringType, 5);
        }
    }
}

