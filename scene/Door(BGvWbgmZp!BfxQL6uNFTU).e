13
584115552258
311117155034729 1725231319970239700
{
  "name": "Door",
  "local_enabled": true,
  "local_position": {
    "X": -19.6865272521972656,
    "Y": 4.0282120704650879
  },
  "local_rotation": 0,
  "local_scale": {
    "X": 2.8116047382354736,
    "Y": 11.3747434616088867
  },
  "previous_sibling": "311117155064249:1725231319970249000",
  "next_sibling": "311117155003165:1725231319970230000",
  "parent": "311117154685767:1725231319970130900",
  "spawn_as_networked_entity": true
},
{
  "cid": 1,
  "aoid": "311117155513431:1725231319970389900",
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
  "aoid": "311117155569731:1725231319970407600",
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
  "aoid": "311117155601461:1725231319970417500",
  "component_type": "Mono_Component",
  "mono_component_type": "OfficeDoor",
  "data": {
    "Inside": "311117154972544:1725231319970220300",
    "Outside": "311117155003165:1725231319970230000",
    "RoomName": 20
  }
}
