using System.Numerics;
using AllianceDM.StdComponent;
using Rosidl.Messages.Geometry;

namespace AllianceDM.Nav
{
    public class CoiledAstar : Component
    {
        Transform2D gameobjec;
        Transform2D target;
        MapMsg map;

        PriorityQueue<Node3, float> Openlist;

        Vector2 forward = new();

        public CoiledAstar(uint uuid, uint[] revid, string[] arg) : base(uuid, revid, arg)
        {
            Console.WriteLine(string.Format("AllianceDM.Nav CoiledAstar : uuid:{0:D4}", uuid));
            gameobjec = new(0, [], []);
            target = new(0, [], []);
            map = new(0, [], []);
            Openlist = new PriorityQueue<Node3, float>();
        }
        public override void Awake()
        {
            gameobjec = DecisionMaker.FindComponent(RecieveID[0]) as Transform2D ?? throw new Exception();
            target = DecisionMaker.FindComponent(RecieveID[1]) as Transform2D ?? throw new Exception();
            map = DecisionMaker.FindComponent(RecieveID[2]) as MapMsg ?? throw new Exception();
        }
        public override void Update()
        {
            forward = gameobjec.Output.pos - target.Output.pos;
            if (forward.Length() == 0)
                return;
            forward /= forward.Length();
        }
        public override void Echo(string topic, int frameRate)
        {
            Task.Run(async () =>
            {
                using var pub = Ros2Def.node.CreatePublisher<Pose2D>(topic);
                using var nativeMsg = pub.CreateBuffer();
                using var timer = Ros2Def.context.CreateTimer(Ros2Def.node.Clock, TimeSpan.FromMilliseconds(value: 1000 / frameRate));

                while (true)
                {
                    nativeMsg.AsRef<Pose2D.Priv>().X = forward.X;
                    nativeMsg.AsRef<Pose2D.Priv>().Y = forward.Y;
                    pub.Publish(nativeMsg);
                    await timer.WaitOneAsync(false);
                }
            });
        }

        public Vector2 Output => forward;
    }

    class Node3(float x, float y, float t, Node3? f)
    {
        const int headings = 72;
        const float rotateAngle = MathF.PI / 3;
        const float halfRotateAngle = rotateAngle / 2;
        const float stepLength = 0.1f;
        readonly float _x = x;
        readonly float _y = y;
        readonly float _t = t;
        readonly Node3? _father = f;

        List<Node3> Successor
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

        List<Node3> InitCollection
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
    }
}