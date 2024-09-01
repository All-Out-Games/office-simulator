using AO;

public class Seat : Component
{
  [Serialized] public String Type;
  [Serialized] public bool FaceLeft;
  public SyncVar<bool> Occupied = new(false);
}