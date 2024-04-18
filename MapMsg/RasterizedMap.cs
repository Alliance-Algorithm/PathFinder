using AllianceDM.IO;
using Rosidl.Messages.Nav;

namespace AllianceDM.Nav
{
    public class RasterizedMap(uint uuid, uint[] recvid, string[] args) : MapMsg(uuid, recvid, args)
    {
        public override void Awake()
        {
            IOManager.RegistryMassage(Args[0], (OccupancyGrid msg) =>
            {
                _map = new byte[msg.Info.Height, msg.Info.Width];
                Buffer.BlockCopy(msg.Data, 0, _map, 0, msg.Data.Length);
            });
        }
    }
}