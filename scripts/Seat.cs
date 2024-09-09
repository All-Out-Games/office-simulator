using AO;

public class Seat : Component
{
  [Serialized] public string Type;
  [Serialized] public bool FaceLeft;
  public SyncVar<bool> Occupied = new(false);
}