using AO;

public class Seat : Component
{
  [Serialized] public String Type;
  public SyncVar<bool> Occupied = new(false);
}