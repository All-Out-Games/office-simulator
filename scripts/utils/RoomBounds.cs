using AO;
    // DO NOT CHANGE ORDER OR ADD TO FRONT
    public enum Room
    {
        LOBBY,
        GYM,
        HALLS,
        CATETERIA,
        FINANCE,
        SERVERS,
        LIBRARY,
        JANITORCLOSET,
        HR,
        CONFERENCE,
        CONFERENCE_SPEAKER,
        
        OFFICE1,
        OFFICE2,
        OFFICE3,
        OFFICE4,
        OFFICE5,
        OFFICE6,
        OFFICE7,
        OFFICE8,
        OFFICE_MANAGER,
        OFFICE_CEO,
        OFFICE_OVERSEER,
    }

public class RoomBounds : Component
{
    [Serialized]
    public Room RoomName;

    public override void Start()
    {
        if (!Network.IsServer) return;

        Entity.GetComponent<Box_Collider>().OnCollisionEnter = (Entity other) =>
        {
            var op = other.GetComponent<OfficePlayer>();
            if (!op.Alive()) return;
            op.CurrentRoom = RoomName;
        };

        Entity.GetComponent<Box_Collider>().OnCollisionExit = (Entity other) =>
        {
            var player = other.GetComponent<OfficePlayer>();
            if (!player.Alive()) return;
            player.CurrentRoom = Room.HALLS;
        };
    }

    public static List<Player> GetPlayersInRoom(Room room)
    {
        var players = new List<Player>();

        foreach (Player player in Player.AllPlayers)
        {
            var op = (OfficePlayer)player;
            if (op.CurrentRoom == room) players.Add(player);
        }

        return players;
    }

    public List<Player> GetPlayersInRoom()
    {
        var players = new List<Player>();

        foreach (Player player in Player.AllPlayers)
        {
            var op = (OfficePlayer)player;
            if (op.CurrentRoom == RoomName) players.Add(player);
        }

        return players;
    }
}
