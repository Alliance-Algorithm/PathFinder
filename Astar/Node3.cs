using System.Numerics;

namespace AllianceDM.Nav
{

    class Node3(float x, float y, float t, Node3? f) : IEquatable<Node3>
    {
        const int headings = 72;
        const float rotateAngle = MathF.PI / 3;
        const float halfRotateAngle = rotateAngle / 2;
        const float stepLength = 0.1f;
        readonly float _x = x;
        readonly float _y = y;
        readonly float _t = t;

        float _h = f != null ? f.H : 0 + stepLength;

        Node3? _father = f;

        public float X => _x;
        public float Y => _y;
        public float T => _t;
        public float H => _h;
        public Node3? Father { get => _father; set { _father = value; _h = _father != null ? _father.H : 0 + stepLength; } }

        public List<Node3> Successor
        {
            get
            {
                List<Node3> node3s = [];
                for (float i = 0; i < headings; i++)
                {
                    float angle = i * rotateAngle / headings - halfRotateAngle;
                    (float Sin, float Cos) x = MathF.SinCos(angle);
                    var vec2 = stepLength * new Vector2(x.Sin, x.Cos);
                    node3s.Add(new Node3(_x + vec2.X, _y + vec2.Y, _t + angle, this));
                }
                return node3s;
            }
        }

        public List<Node3> InitCollection
        {

            get
            {
                int headings = Node3.headings * (int)MathF.Round(rotateAngle / MathF.Tau);
                List<Node3> node3s = [];
                for (float i = 0; i < headings; i++)
                {
                    float angle = i * MathF.Tau / headings - MathF.PI;
                    (float Sin, float Cos) x = MathF.SinCos(angle);
                    var vec2 = stepLength * new Vector2(x.Sin, x.Cos);
                    node3s.Add(new Node3(_x + vec2.X, _y + vec2.Y, _t + angle, this));
                }
                return node3s;
            }
        }

        public float GetF(Node3 target)
        {
            return _h + MathF.Sqrt(MathF.Pow(target.X - X, 2) + MathF.Pow(target.Y - Y, 2));
        }

        //########################################

        public static bool operator ==(Node3? obj1, Node3? obj2)
        {
            if (obj1 == null && obj2 == null)
                return true;
            else if (obj1 == null || obj2 == null)
                return false;

            return obj1.Equals(obj2);
        }

        public static bool operator !=(Node3? obj1, Node3? obj2)
        {

            if (obj1 == null && obj2 == null)
                return false;
            else if (obj1 == null || obj2 == null)
                return true;

            return !obj1.Equals(obj2);
        }
        public override bool Equals(object? obj)
        {
            if (obj is not Node3)
                return false;
            else
            {
                Node3? node = obj as Node3;
                if (node == null)
                    return false;
                return node.X == X && node.Y == Y && node.T == T;
            }
        }

        public override int GetHashCode()
        {
            throw new NotImplementedException();
        }

        public bool Equals(Node3? other)
        {
            if (other == null)
                return false;
            return other.X == X && other.Y == Y && other.T == T;
        }
    }


}