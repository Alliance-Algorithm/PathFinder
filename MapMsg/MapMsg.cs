namespace AllianceDM.Nav
{
    public class MapMsg(uint uuid, uint[] revid, string[] args) : Component(uuid, revid, args)
    {
        protected byte[,] _map = new byte[0, 0];
        public byte[,] Map => _map;
    }
}