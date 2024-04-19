using AllianceDM.IO;
using AllianceDM.StdComponent;
using Rosidl.Messages.Geometry;

namespace AllianceDM.Nav
{
#pragma warning disable CS8618 // 在退出构造函数时，不可为 null 的字段必须包含非 null 值。请考虑声明为可以为 null。
    public class UnityNav(uint uuid, uint[] revid, string[] args) : Component(uuid, revid, args)
#pragma warning restore CS8618 // 在退出构造函数时，不可为 null 的字段必须包含非 null 值。请考虑声明为可以为 null。
    {
        Transform2D SentryPosition;
        Transform2D DestinationPosition;

        public override void Awake()
        {
            SentryPosition = DecisionMaker.FindComponent(RecieveID[0]) as Transform2D ?? throw new Exception("UnityNav, id" + ID.ToString());
            DestinationPosition = DecisionMaker.FindComponent(RecieveID[1]) as Transform2D ?? throw new Exception("UnityNav, id" + ID.ToString());

            Task.Run(async () =>
            {
                using var pub = Ros2Def.node.CreatePublisher<Pose2D>(Args[1]);
                using var pub2 = Ros2Def.node.CreatePublisher<Pose2D>(Args[2]);
                using var nativeMsg = pub.CreateBuffer();
                using var nativeMsg2 = pub2.CreateBuffer();
                using var timer = Ros2Def.context.CreateTimer(Ros2Def.node.Clock, TimeSpan.FromMilliseconds(value: 1000 / int.Parse(Args[0])));

                while (true)
                {
                    nativeMsg.AsRef<Pose2D.Priv>().X = SentryPosition.Output.pos.X;
                    nativeMsg.AsRef<Pose2D.Priv>().Y = SentryPosition.Output.pos.Y;
                    nativeMsg2.AsRef<Pose2D.Priv>().X = DestinationPosition.Output.pos.X;
                    nativeMsg2.AsRef<Pose2D.Priv>().Y = DestinationPosition.Output.pos.Y;
                    pub.Publish(nativeMsg);
                    pub2.Publish(nativeMsg2);
                    await timer.WaitOneAsync(false);
                }
            });

        }
        public override void Update()
        {

        }

    }
}