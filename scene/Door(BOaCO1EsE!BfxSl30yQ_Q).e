13
1005022347265
344836730538756 1725241899658842000
{
  "name": "Door",
  "local_enabled": true,
  "local_position": {
    "X": -5.3883562088012695,
    "Y": 3.0365681648254395
  },
  "local_rotation": 0,
  "local_scale": {
    "X": 6.8400001525878906,
    "Y": 3.4500000476837158
  },
  "previous_sibling": "344836730570069:1725241899658852000",
  "next_sibling": "344836730506054:1725241899658831800",
  "parent": "344836730178951:1725241899658729600",
  "spawn_as_networked_entity": true
},
{
  "cid": 1,
  "aoid": "344836731204750:1725241899659050900",
  "component_type": "Internal_Component",
  "internal_component_type": "Sprite_Renderer",
  "data": {
    "texture": "Condominium_Singles_48x48_82.png",
    "depth_offset": 0,
    "tint": {
      "X": 1,
      "Y": 1,
      "Z": 1,
      "W": 1
    },
    "layer": 0,
    "wait_for_load": false,
    "wrap": false,
    "mask_in_shadow": false
  }
},
{
  "cid": 2,
  "aoid": "344836731332844:1725241899659091700",
  "component_type": "Internal_Component",
  "internal_component_type": "Edge_Collider",
  "data": {
    "is_trigger": false,
    "is_loop": false,
    "points": [
      {
        "X": 0.1882953643798828,
        "Y": -0.1375435590744019
      },
      {
        "X": -0.2494535446166992,
        "Y": -0.1396120786666870
      },
      {
        "X": -0.2546467781066895,
        "Y": 0.2395392656326294
      },
      {
        "X": -0.1999495029449463,
        "Y": 0.2484735250473022
      },
      {
        "X": -0.1839163303375244,
        "Y": 0.1852042675018311
      },
      {
        "X": 0.1327321529388428,
        "Y": 0.1880619525909424
      },
      {
        "X": 0.1486492156982422,
        "Y": 0.2379815578460693
      },
      {
        "X": 0.1894481182098389,
        "Y": 0.2314314842224121
      },
      {
        "X": 0.1883466243743896,
        "Y": -0.1419104337692261
      }
    ],
    "density": 1,
    "friction": 0.2000000029802322,
    "restitution": 0,
    "restitution_threshold": 1
  }
},
{
  "cid": 3,
  "aoid": "344836731433675:1725241899659122700",
  "component_type": "Mono_Component",
  "mono_component_type": "OfficeDoor",
  "data": {
    "Inside": "344836730475611:1725241899658822200",
    "Outside": "344836730506054:1725241899658831800",
    "RoomName": 13
  }
}