using AO;

public class RoomBounds : Component
{
    [Serialized]
    public string RoomName;

    public override void Start()
    {
        if (!Network.IsServer) return;

        Entity.GetComponent<Box_Collider>().OnCollisionEnter = (Entity other) =>
        {
            var op = other.GetComponent<OfficePlayer>();
            op.CurrentRoom.Set(RoomName);
        };

        Entity.GetComponent<Box_Collider>().OnCollisionExit = (Entity other) =>
        {
            var player = other.GetComponent<OfficePlayer>();
            player.CurrentRoom.Set(null);
        };
    }
}
