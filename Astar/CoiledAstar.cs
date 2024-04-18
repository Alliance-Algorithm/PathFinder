using System.Numerics;
using AllianceDM.StdComponent;
using Rosidl.Messages.Geometry;

namespace AllianceDM.Nav
{
    public class CoiledAstar : Component
    {
        Transform2D player;
        Transform2D target;
        MapMsg map;

        PriorityQueue<Node3, float> Openlist;
        List<Node3> CloseList;

        Vector2 forward = new();

        Stack<Node3> path;

        public CoiledAstar(uint uuid, uint[] revid, string[] arg) : base(uuid, revid, arg)
        {
            Console.WriteLine(string.Format("AllianceDM.Nav CoiledAstar : uuid:{0:D4}", uuid));
            player = new(0, [], []);
            target = new(0, [], []);
            map = new(0, [], []);
            Openlist = new PriorityQueue<Node3, float>();
            Openlist = new();
            CloseList = [];
            path = new();
        }
        public override void Awake()
        {
            player = DecisionMaker.FindComponent(RecieveID[0]) as Transform2D ?? throw new Exception();
            target = DecisionMaker.FindComponent(RecieveID[1]) as Transform2D ?? throw new Exception();
            map = DecisionMaker.FindComponent(RecieveID[2]) as MapMsg ?? throw new Exception();
        }
        public override void Update()
        {
            forward = player.Output.pos - target.Output.pos;
            if (forward.Length() == 0)
                return;

            CalculatePath();
        }
        bool CalculatePath()
        {
            GridMap gridMap = new(map);
            Openlist.Clear();
            CloseList.Clear();
            path.Clear();

            Node3 tar = new Node3(target.Output.pos.X - player.Output.pos.X, target.Output.pos.Y - player.Output.pos.Y, 0, null);
            Node3 pathHead = new Node3(0, 0, 0, null);

            var temp = pathHead.InitCollection;
            foreach (var i in temp)
            {
                Openlist.Enqueue(i, i.GetF(tar));
            }
            CloseList.Add(pathHead);

            while (Openlist.Count > 0)
            {
                var c = Openlist.Dequeue();
                if (CloseList.Contains(c))
                    continue;
                CloseList.Add(c);
                if (gridMap.IsSame(c, tar))
                {
                    tar.Father = c.Father;
                    var k = tar;
                    while (k != pathHead && k != null)
                    {
                        path.Push(k);
                        k = k.Father;
                    }
                    return true;
                }

                temp = c.Successor;
                foreach (var i in temp)
                {
                    Openlist.Enqueue(i, i.GetF(tar)); //It could be large but is ok,we cant check if it contains i;
                }
            }
            return false;
        }
        public override void Echo(string topic, int frameRate)
        {
            Task.Run(async () =>
            {
                using var pub = Ros2Def.node.CreatePublisher<Pose2D>(topic + "_pose");
                using var pub2 = Ros2Def.node.CreatePublisher<Rosidl.Messages.Nav.Path>(topic + "_path");
                using var nativeMsg = pub.CreateBuffer();
                using var nativeMsg2 = pub2.CreateBuffer();
                using var timer = Ros2Def.context.CreateTimer(Ros2Def.node.Clock, TimeSpan.FromMilliseconds(value: 1000 / frameRate));

                while (true)
                {
                    nativeMsg.AsRef<Pose2D.Priv>().X = forward.X;
                    nativeMsg.AsRef<Pose2D.Priv>().Y = forward.Y;

                    nativeMsg2.AsRef<Rosidl.Messages.Nav.Path.Priv>().Header = new Rosidl.Messages.Std.Header.Priv();
                    pub.Publish(nativeMsg);
                    await timer.WaitOneAsync(false);
                }
            });
        }

        public Vector2 Output => forward;
    }
}