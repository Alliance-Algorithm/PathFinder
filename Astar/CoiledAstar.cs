using System.Numerics;
using AllianceDM.StdComponent;
using Rosidl.Messages.Geometry;

namespace AllianceDM.Nav
{
    public class CoiledAstar : Component
    {
        Transform2D gameobjec;
        Transform2D target;

        Vector2 forward = new();

        public CoiledAstar(uint uuid, uint[] revid, string[] arg) : base(uuid, revid, arg)
        {
            Console.WriteLine(string.Format("AllianceDM.Nav CoiledAstar : uuid:{0:D4}", uuid));
            gameobjec = new(0, [], []);
            target = new(0, [], []);
        }
        public override void Awake()
        {
            gameobjec = DecisionMaker.FindComponent(RecieveID[0]) as Transform2D ?? throw new Exception();
            target = DecisionMaker.FindComponent(RecieveID[1]) as Transform2D ?? throw new Exception();
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
}