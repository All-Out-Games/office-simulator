13
1494648619009
326218211595496 1725578779055285000
{
  "name": "OverseerPromoNPC",
  "local_enabled": true,
  "local_position": {
    "X": 13.1565589904785156,
    "Y": -56.1682167053222656
  },
  "local_rotation": 0,
  "local_scale": {
    "X": 4,
    "Y": 4
  },
  "previous_sibling": "325826132243319:1725578656038314000",
  "next_sibling": "326709895032789:1725578933323564900",
  "spawn_as_networked_entity": true
},
{
  "cid": 1,
  "aoid": "326218211795012:1725578779055347200",
  "component_type": "Internal_Component",
  "internal_component_type": "Sprite_Renderer",
  "data": {
    "texture": "overseer/statue.png",
    "depth_offset": 0,
    "tint": {
      "X": 1,
      "Y": 0.3960784375667572,
      "Z": 0.4117647111415863,
      "W": 1
    },
    "layer": 0,
    "wait_for_load": false,
    "wrap": false,
    "mask_in_shadow": false
  }
},
{
  "cid": 3,
  "aoid": "327342769389728:1725579131891242600",
  "component_type": "Mono_Component",
  "mono_component_type": "OverseerPromoNPC",
  "data": {
    "fighter1Seat": "326709895222264:1725578933323623900",
    "fighter2Seat": "326756611462891:1725578947981091500"
  }
},
{
  "cid": 2,
  "aoid": "541375263656339:1725646289249933700",
  "component_type": "Internal_Component",
  "internal_component_type": "Edge_Collider",
  "data": {
    "is_trigger": true,
    "is_loop": true,
    "points": [
      {
        "X": -0.1367440223693848,
        "Y": -0.2473335266113281
      },
      {
        "X": -0.0674200057983398,
        "Y": 0.2027683258056641
      },
      {
        "X": 0.0954802036285400,
        "Y": 0.1862621307373047
      },
      {
        "X": 0.1301419734954834,
        "Y": -0.2456827163696289
      }
    ],
    "density": 1,
    "friction": 0.2000000029802322,
    "restitution": 0,
    "restitution_threshold": 1
  }
}
