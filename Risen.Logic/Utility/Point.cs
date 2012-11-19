namespace Risen.Logic.Utility
{
    public class Point
    {
        public Point()
        {
        }

        public Point(Point point)
        {
            X = point.X;
            Y = point.Y;
        }

        public Point(int x, int y)
        {
            X = x;
            Y = y;
        }

        public int X { get; set; }
        public int Y { get; set; }

        public static bool operator ==(Point current, Point comparingValue)
        {
            if ((object)current == null || (object)comparingValue == null) return false;
            return current.X == comparingValue.X && current.Y == comparingValue.Y;
        }

        public static bool operator !=(Point current, Point comparingValue)
        {
            if (current == null || comparingValue == null) return false;
            return current.X != comparingValue.X && current.Y != comparingValue.Y;
        }

        protected bool Equals(Point other)
        {
            return X == other.X && Y == other.Y;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((Point)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (X * 397) ^ Y;
            }
        }
    }
}
