using System.ComponentModel;

namespace AllianceDM.Nav
{

    class GridMap(MapMsg msg)
    {
        int _height = msg.Map.GetLength(0);
        int _widthm = msg.Map.GetLength(1);
        float _resolution = msg.Resolution;
        sbyte[,] _map = msg.Map;
        sbyte[,] _value = new sbyte[msg.Map.GetLength(0), msg.Map.GetLength(1)];

        public int Height => _height;
        public int Width => _widthm;

        public float Value(Node3 node)
        {
            int x = (int)MathF.Round(node.X / _resolution + _widthm / 2.0f);
            int y = (int)MathF.Round(node.Y / _resolution + _height / 2.0f);
            if (x < _widthm && y < _height && x >= 0 && y >= 0)
            {
                return _value[x, y];
            }
            else throw new WarningException("node is out of the Grid");
        }
        public bool Reachable(Node3 node)
        {
            int x = (int)MathF.Round(node.X / _resolution + _widthm / 2.0f);
            int y = (int)MathF.Round(node.Y / _resolution + _height / 2.0f);
            if (x < _widthm && y < _height && x >= 0 && y >= 0)
            {
                return _map[x, y] == 0;
            }
            else throw new WarningException("node is out of the Grid");
        }

        internal bool IsSame(Node3 c, Node3 tar)
        {
            int x1 = (int)MathF.Round(c.X / _resolution + _widthm / 2.0f);
            int y1 = (int)MathF.Round(c.Y / _resolution + _height / 2.0f);
            int x2 = (int)MathF.Round(tar.X / _resolution + _widthm / 2.0f);
            int y2 = (int)MathF.Round(tar.Y / _resolution + _height / 2.0f);
            return x1 == x2 && y1 == y2;
        }
    }
}