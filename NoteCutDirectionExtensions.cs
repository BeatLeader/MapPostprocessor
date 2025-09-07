using System.Numerics;
namespace MapPostprocessor
{
    public static class NoteCutDirectionExtensions {
        public static Vector3 Multiply(Quaternion rotation, Vector3 point)
        {
            float x = rotation.X * 2F;
            float y = rotation.Y * 2F;
            float z = rotation.Z * 2F;
            float xx = rotation.X * x;
            float yy = rotation.Y * y;
            float zz = rotation.Z * z;
            float xy = rotation.X * y;
            float xz = rotation.X * z;
            float yz = rotation.Y * z;
            float wx = rotation.W * x;
            float wy = rotation.W * y;
            float wz = rotation.W * z;

            Vector3 res;
            res.X = (1F - (yy + zz)) * point.X + (xy - wz) * point.Y + (xz + wy) * point.Z;
            res.Y = (xy + wz) * point.X + (1F - (xx + zz)) * point.Y + (yz - wx) * point.Z;
            res.Z = (xz - wy) * point.X + (yz + wx) * point.Y + (1F - (xx + yy)) * point.Z;
            return res;
        }

        public static Vector2 Direction(this NoteCutDirection cutDirection) {
            switch ((int)cutDirection)
            {
                case >= 1000 and <= 1360:
                {
                    var quaternion = Quaternion.CreateFromYawPitchRoll(0f, 0f, 1000 - (int)cutDirection);
                    Vector3 dir = Multiply(quaternion, new Vector3(0, -1, 0));
                    return new Vector2(dir.X, dir.Y);
                }
                case >= 2000 and <= 2360:
                {
                    var quaternion = Quaternion.CreateFromYawPitchRoll(0f, 0f, 2000 - (int)cutDirection);
                    Vector3 dir = Multiply(quaternion, new Vector3(0, -1, 0));
                    return new Vector2(dir.X, dir.Y);
                }
            }

            switch (cutDirection) {
                case NoteCutDirection.Up:
                    return new Vector2(0.0f, 1f);
                case NoteCutDirection.Down:
                    return new Vector2(0.0f, -1f);
                case NoteCutDirection.Left:
                    return new Vector2(-1f, 0.0f);
                case NoteCutDirection.Right:
                    return new Vector2(1f, 0.0f);
                case NoteCutDirection.UpLeft:
                    return new Vector2(-0.7071f, 0.7071f);
                case NoteCutDirection.UpRight:
                    return new Vector2(0.7071f, 0.7071f);
                case NoteCutDirection.DownLeft:
                    return new Vector2(-0.7071f, -0.7071f);
                case NoteCutDirection.DownRight:
                    return new Vector2(0.7071f, -0.7071f);
                default:
                    return new Vector2(0.0f, 0.0f);
            }
        }

        public static float RotationAngle(this NoteCutDirection cutDirection) {
            switch ((int)cutDirection)
            {
                case >= 1000 and <= 1360:
                    return 1000 - (int)cutDirection;
                case >= 2000 and <= 2360:
                    return 2000 - (int)cutDirection;
            }

            switch (cutDirection) {
                case NoteCutDirection.Up:
                    return -180f;
                case NoteCutDirection.Down:
                    return 0.0f;
                case NoteCutDirection.Left:
                    return -90f;
                case NoteCutDirection.Right:
                    return 90f;
                case NoteCutDirection.UpLeft:
                    return -135f;
                case NoteCutDirection.UpRight:
                    return 135f;
                case NoteCutDirection.DownLeft:
                    return -45f;
                case NoteCutDirection.DownRight:
                    return 45f;
                default:
                    return 0.0f;
            }
        }
        
        public static bool IsMainDirection(this NoteCutDirection cutDirection) {
            switch (cutDirection) {
                case NoteCutDirection.Up:
                    return true;
                case NoteCutDirection.Down:
                    return true;
                case NoteCutDirection.Left:
                    return true;
                case NoteCutDirection.Right:
                    return true;
                default:
                    return false;
            }
        }

        public static NoteCutDirection MainNoteCutDirectionFromCutDirAngle(float angle) {
            angle %= 360f;
            if ((double)angle < 0.0)
                angle += 360f;
            if ((double)angle < 45.0 || (double)angle > 315.0)
                return NoteCutDirection.Right;
            if ((double)angle < 135.0)
                return NoteCutDirection.Up;
            return (double)angle < 225.0 ? NoteCutDirection.Left : NoteCutDirection.Down;
        }

        public static NoteCutDirection Mirrored(this NoteCutDirection cutDirection) {
            switch (cutDirection) {
                case NoteCutDirection.Left:
                    return NoteCutDirection.Right;
                case NoteCutDirection.Right:
                    return NoteCutDirection.Left;
                case NoteCutDirection.UpLeft:
                    return NoteCutDirection.UpRight;
                case NoteCutDirection.UpRight:
                    return NoteCutDirection.UpLeft;
                case NoteCutDirection.DownLeft:
                    return NoteCutDirection.DownRight;
                case NoteCutDirection.DownRight:
                    return NoteCutDirection.DownLeft;
                default:
                    return cutDirection;
            }
        }

        public static NoteCutDirection Opposite(this NoteCutDirection cutDirection) {
            switch (cutDirection) {
                case NoteCutDirection.Up:
                    return NoteCutDirection.Down;
                case NoteCutDirection.Down:
                    return NoteCutDirection.Up;
                case NoteCutDirection.Left:
                    return NoteCutDirection.Right;
                case NoteCutDirection.Right:
                    return NoteCutDirection.Left;
                case NoteCutDirection.UpLeft:
                    return NoteCutDirection.DownRight;
                case NoteCutDirection.UpRight:
                    return NoteCutDirection.DownLeft;
                case NoteCutDirection.DownLeft:
                    return NoteCutDirection.UpRight;
                case NoteCutDirection.DownRight:
                    return NoteCutDirection.UpLeft;
                default:
                    return cutDirection;
            }
        }

        public static bool Approximately(float a, float b) => (double)Math.Abs(b - a) < (double)Math.Max(1E-06f * Math.Max(Math.Abs(a), Math.Abs(b)), 1.17549435E-38f * 8f);

        public static bool IsOnSamePlane(
          this NoteCutDirection noteCutDirection1,
          NoteCutDirection noteCutDirection2) {
            float a = Math.Abs(noteCutDirection1.RotationAngle() - noteCutDirection2.RotationAngle());
            return Approximately(a, 0.0f) || Approximately(a, 180f);
        }

        public static float Angle(Vector2 from, Vector2 to) {
            float num = (float)Math.Sqrt((double)from.LengthSquared() * (double)to.LengthSquared());
            return (double)num < 1.0000000036274937E-15 ? 0.0f : (float)Math.Acos((double)Math.Clamp(Vector2.Dot(from, to) / num, -1f, 1f)) * 57.29578f;
        }

        public static float SignedAngle(Vector2 from, Vector2 to) {
            return Angle(from, to) * Math.Sign((float) ((double) from.X * (double) to.Y - (double) from.Y * (double) to.X));
        }

        public static NoteCutDirection NoteCutDirectionFromDirection(Vector3 direction) {
            float num = Angle(new Vector2(direction.X, direction.Y), new Vector2(0.0f, 1f));
            if ((double)direction.X < 0.0) {
                if ((double)num <= 22.5)
                    return NoteCutDirection.Up;
                if ((double)num > 22.5 && (double)num <= 67.5)
                    return NoteCutDirection.UpLeft;
                if ((double)num > 67.5 && (double)num <= 112.5)
                    return NoteCutDirection.Left;
                return (double)num > 112.5 && (double)num <= 157.5 ? NoteCutDirection.DownLeft : NoteCutDirection.Down;
            }
            if ((double)num <= 22.5)
                return NoteCutDirection.Up;
            if ((double)num > 22.5 && (double)num <= 67.5)
                return NoteCutDirection.UpRight;
            if ((double)num > 67.5 && (double)num <= 112.5)
                return NoteCutDirection.Right;
            return (double)num > 112.5 && (double)num <= 157.5 ? NoteCutDirection.DownRight : NoteCutDirection.Down;
        }
    }
}
