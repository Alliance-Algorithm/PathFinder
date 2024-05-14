using AllianceDM.IO;
using Rosidl.Messages.Nav;

// args[0] = map topic
// args[1] = BoxSize
// args[2] = ESDF_MaxDistance
namespace AllianceDM.Nav
{
    public class ESDFBuilder(uint uuid, uint[] recvid, string[] args) : MapMsg(uuid, recvid, args)
    {
        float boxSize_;
        float ESDF_MaxDistance;
        const float SQRT2 = 1.414213562f;
        public override void Awake()
        {
            IOManager.RegistryMassage(Args[0], (OccupancyGrid msg) =>
            {
                if (Map.GetLength(0) != msg.Info.Height)
                    _map = new sbyte[msg.Info.Height, msg.Info.Width];
                Buffer.BlockCopy(msg.Data, 0, _map, 0, msg.Data.Length);
                _resolution = msg.Info.Resolution;
            });

            boxSize_ = float.Parse(Args[1]);
            ESDF_MaxDistance = float.Parse(Args[2]);
        }

        public override void Update()
        {
            MakeESDF(ref _map);
        }

        public void MakeESDF(ref sbyte[,] map)
        {
            if (_map.Length == 0)
                return;
            Queue<(int x, int y)> queue = new();

            for (float x = Resolution; x < boxSize_; x += Resolution)
            {
                for (float y = Resolution; y < boxSize_; y += Resolution)
                {
                    (int x, int y) pos = ((int)(x / Resolution), (int)(y / Resolution));
                    if (map[pos.x, pos.y] == 0)
                    {
                        if (map[pos.x + 1, pos.y] != 0)
                        {
                            map[pos.x + 1, pos.y] = Math.Min(map[pos.x + 1, pos.y], (sbyte)(Resolution / ESDF_MaxDistance * 100));
                            queue.Enqueue((pos.x + 1, pos.y));
                        }
                        if (map[pos.x - 1, pos.y] != 0)
                        {
                            map[pos.x - 1, pos.y] = Math.Min(map[pos.x - 1, pos.y], (sbyte)(Resolution / ESDF_MaxDistance * 100));
                            queue.Enqueue((pos.x - 1, pos.y));
                        }
                        if (map[pos.x, pos.y + 1] != 0)
                        {
                            map[pos.x, pos.y + 1] = Math.Min(map[pos.x, pos.y + 1], (sbyte)(Resolution / ESDF_MaxDistance * 100));
                            queue.Enqueue((pos.x, pos.y + 1));
                        }
                        if (map[pos.x, pos.y - 1] != 0)
                        {
                            map[pos.x, pos.y - 1] = Math.Min(map[pos.x, pos.y - 1], (sbyte)(Resolution / ESDF_MaxDistance * 100));
                            queue.Enqueue((pos.x, pos.y - 1));
                        }
                        if (map[pos.x + 1, pos.y + 1] != 0)
                        {
                            map[pos.x + 1, pos.y + 1] = Math.Min(map[pos.x + 1, pos.y + 1], (sbyte)(SQRT2 * Resolution / ESDF_MaxDistance * 100));
                            queue.Enqueue((pos.x + 1, pos.y + 1));
                        }
                        if (map[pos.x - 1, pos.y - 1] != 0)
                        {
                            map[pos.x - 1, pos.y - 1] = Math.Min(map[pos.x - 1, pos.y - 1], (sbyte)(SQRT2 * Resolution / ESDF_MaxDistance * 100));
                            queue.Enqueue((pos.x - 1, pos.y - 1));
                        }
                        if (map[pos.x - 1, pos.y + 1] != 0)
                        {
                            map[pos.x - 1, pos.y + 1] = Math.Min(map[pos.x - 1, pos.y + 1], (sbyte)(SQRT2 * Resolution / ESDF_MaxDistance * 100));
                            queue.Enqueue((pos.x - 1, pos.y + 1));
                        }
                        if (map[pos.x + 1, pos.y - 1] != 0)
                        {
                            map[pos.x + 1, pos.y - 1] = Math.Min(map[pos.x + 1, pos.y - 1], (sbyte)(SQRT2 * Resolution / ESDF_MaxDistance * 100));
                            queue.Enqueue((pos.x + 1, pos.y - 1));
                        }
                    }
                }
            }

            while (queue.Count > 0)
            {
                (int x, int y) pos = queue.Dequeue();
                if (map[pos.x, pos.y] != 100)
                {
                    if (pos.x + 1 < (int)Math.Ceiling(boxSize_ / Resolution) && map[pos.x + 1, pos.y] == 100)
                    {
                        uint temp = (uint)map[pos.x, pos.y] + (uint)(Resolution / ESDF_MaxDistance * 100);
                        map[pos.x + 1, pos.y] = (sbyte)Math.Clamp(temp, 0, 100);
                        queue.Enqueue((pos.x + 1, pos.y));
                    }
                    if (pos.x - 1 >= 0 && map[pos.x - 1, pos.y] == 100)
                    {
                        uint temp = (uint)map[pos.x, pos.y] + (uint)(Resolution / ESDF_MaxDistance * 100);
                        map[pos.x - 1, pos.y] = (sbyte)Math.Clamp(temp, 0, 100);
                        queue.Enqueue((pos.x - 1, pos.y));
                    }
                    if (pos.y + 1 < (int)Math.Ceiling(boxSize_ / Resolution) && map[pos.x, pos.y + 1] == 100)
                    {
                        uint temp = (uint)map[pos.x, pos.y] + (uint)(Resolution / ESDF_MaxDistance * 100);
                        map[pos.x, pos.y + 1] = (sbyte)Math.Clamp(temp, 0, 100);
                        queue.Enqueue((pos.x, pos.y + 1));
                    }
                    if (pos.y - 1 >= 0 && map[pos.x, pos.y - 1] == 100)
                    {
                        uint temp = (uint)map[pos.x, pos.y] + (uint)(Resolution / ESDF_MaxDistance * 100);
                        map[pos.x, pos.y - 1] = (sbyte)Math.Clamp(temp, 0, 100);
                        queue.Enqueue((pos.x, pos.y - 1));
                    }
                    if (pos.x + 1 < (int)Math.Ceiling(boxSize_ / Resolution) && pos.y + 1 < (int)Math.Ceiling(boxSize_ / Resolution) && map[pos.x + 1, pos.y + 1] == 100)
                    {
                        uint temp = (uint)map[pos.x, pos.y] + (uint)Math.Ceiling(SQRT2 * Resolution / ESDF_MaxDistance * 100);
                        map[pos.x + 1, pos.y + 1] = (sbyte)Math.Clamp(temp, 0, 100);
                        queue.Enqueue((pos.x + 1, pos.y + 1));
                    }
                    if (pos.x - 1 >= 0 && pos.y - 1 >= 0 && map[pos.x - 1, pos.y - 1] == 100)
                    {
                        uint temp = (uint)map[pos.x, pos.y] + (uint)Math.Ceiling(SQRT2 * Resolution / ESDF_MaxDistance * 100);
                        map[pos.x - 1, pos.y - 1] = (sbyte)Math.Clamp(temp, 0, 100);
                        queue.Enqueue((pos.x - 1, pos.y - 1));
                    }
                    if (pos.y + 1 < (int)Math.Ceiling(boxSize_ / Resolution) && pos.x - 1 >= 0 && map[pos.x - 1, pos.y + 1] == 100)
                    {
                        uint temp = (uint)map[pos.x, pos.y] + (uint)Math.Ceiling(SQRT2 * Resolution / ESDF_MaxDistance * 100);
                        map[pos.x - 1, pos.y + 1] = (sbyte)Math.Clamp(temp, 0, 100);
                        queue.Enqueue((pos.x - 1, pos.y + 1));
                    }
                    if (pos.x + 1 < (int)Math.Ceiling(boxSize_ / Resolution) && pos.y - 1 >= 0 && map[pos.x + 1, pos.y - 1] == 100)
                    {
                        uint temp = (uint)(map[pos.x, pos.y] + (uint)Math.Ceiling(SQRT2 * Resolution / ESDF_MaxDistance * 100));
                        map[pos.x + 1, pos.y - 1] = (sbyte)Math.Clamp(temp, 0, 100);
                        queue.Enqueue((pos.x + 1, pos.y - 1));
                    }
                }
            }

        }
    }
}