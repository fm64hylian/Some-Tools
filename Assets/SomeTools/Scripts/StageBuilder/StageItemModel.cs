using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum GroundBlockType
{
    Grass,
    Dirt,
    Rock,
    Snow
}

public enum ObjectType
{
    Wall,
    Boulder,
    Rock_Pillar,
    Trunk,
    Small_Trunk,
    Player_Spawn,
    Enemy_Spawn,
    Tori,
    Lantern,
    Lamp_Ground,
    Item
}

public enum DecorationType
{
    Grass_Small,
    Grass_Medium,
    Grass_Large,
    Pebble,
    Bush
}

public class StageItemModel
{
    [SerializeField]
    public GroundBlockType GroundBlockType { get; set; }
    [SerializeField]
    public ObjectType ObjectType { get; set; }
    [SerializeField]
    public DecorationType DecoType { get; set; }
    public string JsonKey { get; set; }
    public bool IsGround = false;
    public bool IsStackable = false;
    /// <summary>
    /// items and objects  will be destructible, lblocks won't
    /// </summary>
    public bool IsDestructible = false;
    public float ItemHeight { get; set; }

    public string PrefabPath { get; set; }
    public float PreviewScale = 1f;

    /// <summary>
    /// for objects that don't have a center pivot (eg. negative number if pivot is at the lefts)
    /// </summary>
    public float PreviewOffSet = 0f;


    static List<StageItemModel> allModels = new List<StageItemModel>();
    static List<StageItemModel> blocks = new List<StageItemModel>();
    static List<StageItemModel> objects = new List<StageItemModel>();
    static List<StageItemModel> randomObjects = new List<StageItemModel>();
    static List<StageItemModel> decorations = new List<StageItemModel>();

    public static List<StageItemModel> AllModels
    {
        get
        {
            if (allModels.Count > 0)
            {
                return allModels;
            }

            ///use the same order as enum
            allModels = new List<StageItemModel>()
            {
                //ground
                Grass,
                Dirt,
                Rock,
                Snow,
                //objects
                Wall,
                Boulder,
                RockPillar,
                Trunk,
                SmallTrunk,
                PlayerSpawn,
                EnemySpawn,
                Tori,
                Lantern,
                LampGround,
                Item,
                //decoration
                GrassSmall,
                GrassMedium,
                GrassLarge,
                Pebble,
                Bush
            };

            return allModels;
        }
    }
    public static List<StageItemModel> Blocks
    {
        get
        {
            if (blocks.Count > 0)
            {
                return blocks;
            }

            ///use the same order as enum
            blocks = new List<StageItemModel>()
            {
                Grass,
                Dirt,
                Rock,
                Snow
            };

            return blocks;
        }
    }
    public static List<StageItemModel> Objects
    {
        get
        {
            if (objects.Count > 0)
            {
                return objects;
            }

            objects = new List<StageItemModel>()
            {
                    Wall,
                    Boulder,
                    RockPillar,
                    Trunk,
                    SmallTrunk,
                    PlayerSpawn,
                    EnemySpawn,
                    Tori,
                    Lantern,
                    LampGround,
                    Item
            };

            return objects;
        }
    }
    public static List<StageItemModel> RandomObjects
    {
        get
        {
            if (randomObjects.Count > 0)
            {
                return randomObjects;
            }

            randomObjects = new List<StageItemModel>()
            {
                    //Wall,
                    Boulder,
                    RockPillar,
                    Trunk,
                    SmallTrunk,
                    //Tori,
                    Lantern,
                    //LampGround,
            };

            return randomObjects;
        }
    }
    public static List<StageItemModel> Decorations
    {
        get
        {
            if (decorations.Count > 0)
            {
                return decorations;
            }

            decorations = new List<StageItemModel>()
            {
                GrassSmall,
                GrassMedium,
                GrassLarge,
                Pebble,
                Bush
            };

            return decorations;
        }
    }

    public static StageItemModel GetModelFromKey(string key)
    {
        return AllModels.Find(x => x.JsonKey.Equals(key));
    }

    /*---------------GROUND BLOCKS-----------*/

    public static StageItemModel Grass
    {
        get
        {
            StageItemModel model = new StageItemModel();
            model.JsonKey = "grass";
            model.GroundBlockType = GroundBlockType.Grass;
            model.IsGround = true;
            model.IsStackable = true;
            model.IsDestructible = true;
            model.ItemHeight = 1f;
            model.PrefabPath = "Prefabs/StagePrefabs/stage_block_grass";

            return model;
        }
    }
    public static StageItemModel Dirt
    {
        get
        {
            StageItemModel model = new StageItemModel();
            model.JsonKey = "dirt";
            model.GroundBlockType = GroundBlockType.Dirt;
            model.IsGround = true;
            model.IsStackable = true;
            model.IsDestructible = true;
            model.ItemHeight = 1f;
            model.PrefabPath = "Prefabs/StagePrefabs/stage_block_dirt";

            return model;
        }
    }

    public static StageItemModel Rock
    {
        get
        {
            StageItemModel model = new StageItemModel();
            model.JsonKey = "rock";
            model.GroundBlockType = GroundBlockType.Rock;
            model.IsGround = true;
            model.IsStackable = true;
            model.IsDestructible = true;
            model.ItemHeight = 1f;
            model.PrefabPath = "Prefabs/StagePrefabs/stage_block_rock";

            return model;
        }
    }

    public static StageItemModel Snow
    {
        get
        {
            StageItemModel model = new StageItemModel();
            model.JsonKey = "snow";
            model.GroundBlockType = GroundBlockType.Snow;
            model.IsGround = true;
            model.IsStackable = true;
            model.IsDestructible = true;
            model.ItemHeight = 1f;
            model.PrefabPath = "Prefabs/StagePrefabs/stage_block_snow";

            return model;
        }
    }


    /*---------------ITEMS / OBJECTS-----------*/
    public static StageItemModel Wall
    {
        get
        {
            StageItemModel model = new StageItemModel();
            model.JsonKey = "wall";
            model.ObjectType = ObjectType.Wall;
            model.IsStackable = true;
            model.IsDestructible = true;
            model.ItemHeight = 1f;
            model.PrefabPath = "Prefabs/stage_obt_wall";

            return model;
        }
    }

    //public static StageBlockModel Stone
    //{
    //    get
    //    {
    //        StageBlockModel model = new StageBlockModel();
    //        model.JsonKey = "stone";
    //        model.PrefabType = BlockType.Stone;
    //        model.IsGround = false;
    //        model.IsStackable = true;
    //        model.IsDestructible = true;
    //        model.BlockHeight = 1f;
    //        model.PrefabPath = "Prefabs/stage_stone";
    //        model.PreviewScale = 1f;

    //        return model;
    //    }
    //}

    public static StageItemModel Boulder
    {
        get
        {
            StageItemModel model = new StageItemModel();
            model.JsonKey = "boulder";
            model.ObjectType = ObjectType.Boulder;
            model.IsDestructible = true;
            model.ItemHeight = 1f;
            model.PrefabPath = "Prefabs/stage_obt_boulder";
            model.PreviewScale = 0.5f;
            //model.PreviewOffSet = -0.5f;

            return model;
        }
    }

    public static StageItemModel RockPillar
    {
        get
        {
            StageItemModel model = new StageItemModel();
            model.JsonKey = "rock_pillar";
            model.ObjectType = ObjectType.Rock_Pillar;
            model.IsDestructible = true;
            model.ItemHeight = 2.8f;
            model.PrefabPath = "Prefabs/stage_obt_rock_pillar";
            model.PreviewScale = 0.5f;
            return model;
        }
    }

    public static StageItemModel Trunk
    {
        get
        {
            StageItemModel model = new StageItemModel();
            model.JsonKey = "trunk";
            model.ObjectType = ObjectType.Trunk;
            model.IsDestructible = true;
            model.ItemHeight = 1f;
            model.PrefabPath = "Prefabs/stage_obt_trunk";
            model.PreviewScale = 0.3f;
            return model;
        }
    }

    public static StageItemModel SmallTrunk
    {
        get
        {
            StageItemModel model = new StageItemModel();
            model.JsonKey = "small_trunk";
            model.ObjectType = ObjectType.Small_Trunk;
            model.IsDestructible = true;
            model.ItemHeight = 1f;
            model.PrefabPath = "Prefabs/stage_obt_small_trunk";
            model.PreviewScale = 0.5f;
            return model;
        }
    }

    public static StageItemModel PlayerSpawn
    {
        get
        {
            StageItemModel model = new StageItemModel();
            model.JsonKey = "player_spawn";
            model.ObjectType = ObjectType.Player_Spawn;
            model.IsDestructible = false;
            model.ItemHeight = 0.25f;
            model.PrefabPath = "Prefabs/stage_obt_playerspawn";

            return model;
        }
    }

    public static StageItemModel EnemySpawn
    {
        get
        {
            StageItemModel model = new StageItemModel();
            model.JsonKey = "enemy_spawn";
            model.ObjectType = ObjectType.Enemy_Spawn;
            model.IsDestructible = false;
            model.ItemHeight = 0.25f;
            model.PrefabPath = "Prefabs/stage_obt_enemyspawn";

            return model;
        }
    }

    public static StageItemModel Tori
    {
        get
        {
            StageItemModel model = new StageItemModel();
            model.JsonKey = "tori";
            model.ObjectType = ObjectType.Tori;
            model.IsDestructible = false;
            model.ItemHeight = 6f;
            model.PrefabPath = "Prefabs/stage_obt_tori";
            model.PreviewScale = 0.2f;
            model.PreviewOffSet = -2.5f;

            return model;
        }
    }

    public static StageItemModel Lantern
    {
        get
        {
            StageItemModel model = new StageItemModel();
            model.JsonKey = "lantern";
            model.ObjectType = ObjectType.Lantern;
            model.IsDestructible = false;
            model.ItemHeight = 4f;
            model.PrefabPath = "Prefabs/stage_obt_lantern";
            model.PreviewScale = 0.3f;
            model.PreviewOffSet = -0.25f;

            return model;
        }
    }

    public static StageItemModel LampGround
    {
        get
        {
            StageItemModel model = new StageItemModel();
            model.JsonKey = "lamp_ground";
            model.ObjectType = ObjectType.Lamp_Ground;
            model.IsDestructible = false;
            model.ItemHeight = 1f;
            model.PrefabPath = "Prefabs/stage_obt_lamp_ground";

            return model;
        }
    }

    public static StageItemModel Item
    {
        get
        {
            StageItemModel model = new StageItemModel();
            model.JsonKey = "item";
            model.ObjectType = ObjectType.Item;
            model.IsDestructible = true;
            model.ItemHeight = 1f;
            model.PrefabPath = "Prefabs/stage_item_default";

            return model;
        }
    }


    /*--------------- DECORATION-----------*/

    public static StageItemModel GrassSmall
    {
        get
        {
            StageItemModel model = new StageItemModel();
            model.JsonKey = "grass_small";
            model.DecoType = DecorationType.Grass_Small;
            model.ItemHeight = 0.1f;
            model.PrefabPath = "Prefabs/stage_deco_grass_small";
            model.PreviewScale = 3f;

            return model;
        }
    }
    public static StageItemModel GrassMedium
    {
        get
        {
            StageItemModel model = new StageItemModel();
            model.JsonKey = "grass_medium";
            model.DecoType = DecorationType.Grass_Medium;
            model.ItemHeight = 0.1f;
            model.PrefabPath = "Prefabs/stage_deco_grass_medium";
            model.PreviewScale = 2f;

            return model;
        }
    }
    public static StageItemModel GrassLarge
    {
        get
        {
            StageItemModel model = new StageItemModel();
            model.JsonKey = "grass_large";
            model.DecoType = DecorationType.Grass_Large;
            model.ItemHeight = 0.1f;
            model.PrefabPath = "Prefabs/stage_deco_grass_large";
            model.PreviewScale = 1.5f;

            return model;
        }
    }

    public static StageItemModel Pebble
    {
        get
        {
            StageItemModel model = new StageItemModel();
            model.JsonKey = "pebble";
            model.DecoType = DecorationType.Pebble;
            model.ItemHeight = 0.1f;
            model.PrefabPath = "Prefabs/stage_deco_pebble";
            model.PreviewScale = 2f;

            return model;
        }
    }

    public static StageItemModel Bush
    {
        get
        {
            StageItemModel model = new StageItemModel();
            model.JsonKey = "bush";
            model.DecoType = DecorationType.Bush;
            model.ItemHeight = 1f;
            model.PrefabPath = "Prefabs/stage_deco_bush";

            return model;
        }
    }

}
