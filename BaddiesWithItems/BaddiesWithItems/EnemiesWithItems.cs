﻿using System;
using System.Linq;
using System.Collections.Generic;
using BepInEx;
using BepInEx.Configuration;
using RoR2;
using UnityEngine;
using RoR2.CharacterAI;
using UnityEngine.Networking;


namespace BaddiesWithItems
{
    [BepInDependency("com.bepis.r2api")]
    [BepInPlugin("com.Basil.EnemiesWithItems", "EnemiesWithItems", "1.2.0")]

    public class EnemiesWithItems : BaseUnityPlugin
    {
        public static ConfigWrapper<bool> GenerateItems;
        public static ConfigWrapper<string> ItemMultiplier;

        public static ConfigWrapper<int> StageReq;
        public static ConfigWrapper<bool> InheritItems;

        //public static ConfigWrapper<string> MaxItems;
        //public static ConfigWrapper<string> ScaleAmount;
        public static ConfigWrapper<string> Tier1GenCap;
        public static ConfigWrapper<string> Tier2GenCap;
        public static ConfigWrapper<string> Tier3GenCap;
        public static ConfigWrapper<string> LunarGenCap;
        public static ConfigWrapper<string> Tier1GenChance;
        public static ConfigWrapper<string> Tier2GenChance;
        public static ConfigWrapper<string> Tier3GenChance;
        public static ConfigWrapper<string> LunarGenChance;
        public static ConfigWrapper<string> EquipGenChance;

        public static ConfigWrapper<string> CustomBlacklist;

        public static ConfigWrapper<bool> ItemsBlacklist;
        public static ConfigWrapper<bool> Limiter;
        public static ConfigWrapper<bool> Tier1Items;
        public static ConfigWrapper<bool> Tier2Items;
        public static ConfigWrapper<bool> Tier3Items;
        public static ConfigWrapper<bool> LunarItems;
        public static ConfigWrapper<bool> EquipItems;
        public static ConfigWrapper<bool> EquipBlacklist;

        public static ConfigWrapper<bool> DropItems;
        public static ConfigWrapper<string> DropChance;

        public void InitConfig()
        {
            GenerateItems = Config.Wrap(
                "Generator Settings",
                "GenerateItems",
                "Toggles item generation for enemies.",
                true);

            /*
            MaxItems = Config.Wrap(
                "Generator Settings",
                "MaxItems",
                "Sets the cap for items to be generated. Max possible items generated + ScaleAmount * Stages Cleared + Avg # of Player Items / 2",
                "5");

            ScaleAmount = Config.Wrap(
                "Generator Settings",
                "ScaleAmount",
                "Sets the scaling value to be added to the MaxItems cap. Every stage cleared will increase the max item cap by this value.",
                "2");
            */

            Tier1GenCap = Config.Wrap(
                "Generator Settings",
                "Tier1GenCap",
                "The multiplicative max item cap for generating Tier 1 (white) items.",
                "4");

            Tier2GenCap = Config.Wrap(
                "Generator Settings",
                "Tier2GenCap",
                "The multiplicative max item cap for generating Tier 2 (green) items.",
                "2");

            Tier3GenCap = Config.Wrap(
                "Generator Settings",
                "Tier3GenCap",
                "The multiplicative max item cap for generating Tier 3 (red) items.",
                "1");

            LunarGenCap = Config.Wrap(
                "Generator Settings",
                "LunarGenCap",
                "The multiplicative max item cap for generating Lunar (blue) items.",
                "1");

            Tier1GenChance = Config.Wrap(
                "Generator Settings",
                "Tier1GenChance",
                "The percent chance for generating a Tier 1 (white) item.",
                "40");

            Tier2GenChance = Config.Wrap(
                "Generator Settings",
                "Tier2GenChance",
                "The percent chance for generating a Tier 2 (green) item.",
                "20");

            Tier3GenChance = Config.Wrap(
                "Generator Settings",
                "Tier3GenChance",
                "The percent chance for generating a Tier 3 (red) item.",
                "1");

            LunarGenChance = Config.Wrap(
                "Generator Settings",
                "LunarGenChance",
                "The percent chance for generating a Lunar (blue) item.",
                "0.5");

            EquipGenChance = Config.Wrap(
                "Generator Settings",
                "EquipGenChance",
                "The percent chance for generating a Use item.",
                "10");

            InheritItems = Config.Wrap(
                "Inherit Settings",
                "InheritItems",
                "Toggles enemies to randomly inherit items from a random player. Overrides Generator Settings.",
                false);

            ItemMultiplier = Config.Wrap(
                "General Settings",
                "ItemMultiplier",
                "Sets the multiplier for items to be inherited/generated.",
                "1");

            StageReq = Config.Wrap(
                "General Settings",
                "StageReq",
                "Sets the minimum stage to be cleared before having enemies inherit/generate items.",
                4);

            ItemsBlacklist = Config.Wrap(
                "General Settings",
                "BlacklistItems",
                "Toggles hard blacklisted items to be inherited/generated.",
                false);

            Limiter = Config.Wrap(
                "General Settings",
                "Limiter",
                "Toggles certain items to be capped. For more information, check this mod's FAQ at the thunderstore!",
                true);

            Tier1Items = Config.Wrap(
                "General Settings",
                "Tier1Items",
                "Toggles Tier 1 (white) items to be inherited/generated.",
                true);

            Tier2Items = Config.Wrap(
                "General Settings",
                "Tier2Items",
                "Toggles Tier 2 (green) items to be inherited/generated.",
                true);

            Tier3Items = Config.Wrap(
                "General Settings",
                "Tier3Items",
                "Toggles Tier 3 (red) items to be inherited/generated.",
                true);

            LunarItems = Config.Wrap(
                "General Settings",
                "LunarItems",
                "Toggles Lunar (blue) items to be inherited/generated.",
                true);

            EquipItems = Config.Wrap(
                "General Settings",
                "EquipItems",
                "Toggles Use items to be inherited/generated.",
                false);

            EquipBlacklist = Config.Wrap(
                "General Settings",
                "EquipBlacklist",
                "Toggles hard blacklisted Use items to be inherited/generated. MOST BLACKLISTED USE ITEMS ARE UNDODGEABLE.",
                false);

            CustomBlacklist = Config.Wrap(
                "General Settings",
                "CustomBlacklist",
                "Enter items ids separated by a comma and a space to blacklist certain items. ex) 41, 23, 17 \nItem ids: https://github.com/risk-of-thunder/R2Wiki/wiki/Item-&-Equipment-IDs-and-Names",
                "");
        }

        public static EquipmentIndex[] EquipmentBlacklist = new EquipmentIndex[]
        {
            EquipmentIndex.Blackhole,
            EquipmentIndex.BFG,
            EquipmentIndex.Lightning,
            EquipmentIndex.Scanner,
            EquipmentIndex.CommandMissile,
            EquipmentIndex.LunarPotion, // no idea what this is but it has lunar on it :D
            EquipmentIndex.BurnNearby
        };

        public static ItemIndex[] ItemBlacklist = new ItemIndex[]
        {
            ItemIndex.StickyBomb,
            ItemIndex.StunChanceOnHit,
            ItemIndex.NovaOnHeal,
            ItemIndex.ShockNearby,
            ItemIndex.Mushroom
        };

        public static ItemIndex[] ItemsNeverUsed = new ItemIndex[]
        {
            ItemIndex.TreasureCache,
            ItemIndex.BossDamageBonus,
            ItemIndex.Feather,
            ItemIndex.Firework,
            ItemIndex.SprintArmor,
            ItemIndex.JumpBoost,
            ItemIndex.GoldOnHit,
            ItemIndex.WardOnLevel,
            ItemIndex.BeetleGland,
            ItemIndex.CrippleWardOnLevel
        };

        public static float ConfigToFloat(string configline)
        {
            if (float.TryParse(configline, out float x))
            {
                return x;
            }
            return 1f;
        }

        private System.Random rand = new System.Random();

        public void Awake()
        {
            InitConfig();

            baddiesItems();

        }

        public void baddiesItems()
        {
            On.RoR2.CombatDirector.AttemptSpawnOnTarget += (orig, self, spawnTarget) =>
            {
                if (spawnTarget)
                {
                    if (self.GetFieldValue<DirectorCard>("currentMonsterCard") == null)
                    {
                        self.SetFieldValue("currentMonsterCard", self.GetPropertyValue<WeightedSelection<DirectorCard>>("monsterCards").Evaluate(self.GetFieldValue<Xoroshiro128Plus>("rng").nextNormalizedFloat));
                        self.lastAttemptedMonsterCard = self.GetFieldValue<DirectorCard>("currentMonsterCard");
                        self.SetFieldValue("currentActiveEliteIndex", self.GetFieldValue<Xoroshiro128Plus>("rng").NextElementUniform<EliteIndex>(EliteCatalog.eliteList));
                    }
                    int num = 0;
                    for (int i = 0; i < self.GetPropertyValue<WeightedSelection<DirectorCard>>("monsterCards").Count; i++)
                    {
                        DirectorCard value = self.GetPropertyValue<WeightedSelection<DirectorCard>>("monsterCards").GetChoice(i).value;
                        int num2 = value.cost;
                        if (!(value.spawnCard as CharacterSpawnCard).noElites)
                        {
                            num2 = (int)((float)num2 * CombatDirector.eliteMultiplierCost);
                        }
                        num = Mathf.Max(num, num2);
                    }
                    int num3 = self.GetFieldValue<DirectorCard>("currentMonsterCard").cost;
                    bool flag = !(self.GetFieldValue<DirectorCard>("currentMonsterCard").spawnCard as CharacterSpawnCard).noElites && self.monsterCredit >= (float)num3 * CombatDirector.eliteMultiplierCost;
                    if (flag)
                    {
                        num3 = (int)((float)num3 * CombatDirector.eliteMultiplierCost);
                    }
                    bool flag2 = self.GetFieldValue<DirectorCard>("currentMonsterCard").CardIsValid();
                    bool flag3 = self.monsterCredit >= (float)num3;
                    bool flag4 = !self.skipSpawnIfTooCheap || self.monsterCredit <= (float)num3 * CombatDirector.maximumNumberToSpawnBeforeSkipping;
                    bool flag5 = num3 >= num;
                    if (flag2 && flag3 && (flag4 || flag5))
                    {
                        SpawnCard spawnCard = self.GetFieldValue<DirectorCard>("currentMonsterCard").spawnCard;
                        DirectorPlacementRule directorPlacementRule = new DirectorPlacementRule
                        {
                            placementMode = DirectorPlacementRule.PlacementMode.Approximate,
                            spawnOnTarget = spawnTarget.transform,
                            preventOverhead = self.GetFieldValue<DirectorCard>("currentMonsterCard").preventOverhead
                        };
                        DirectorCore.GetMonsterSpawnDistance(self.GetFieldValue<DirectorCard>("currentMonsterCard").spawnDistance, out directorPlacementRule.minDistance, out directorPlacementRule.maxDistance);
                        directorPlacementRule.minDistance *= self.spawnDistanceMultiplier;
                        directorPlacementRule.maxDistance *= self.spawnDistanceMultiplier;
                        GameObject gameObject = DirectorCore.instance.TrySpawnObject(spawnCard, directorPlacementRule, self.GetFieldValue<Xoroshiro128Plus>("rng"));

                        if (gameObject)
                        {
                            float num4 = 1f;
                            float num5 = 1f;
                            CharacterMaster component = gameObject.GetComponent<CharacterMaster>();
                            GameObject bodyObject = component.GetBodyObject();
                            if (self.isBoss)
                            {
                                if (!self.bossGroup)
                                {
                                    GameObject gameObject2 = UnityEngine.Object.Instantiate<GameObject>(Resources.Load<GameObject>("Prefabs/NetworkedObjects/BossGroup"));
                                    NetworkServer.Spawn(gameObject2);
                                    self.SetPropertyValue("bossGroup", gameObject2.GetComponent<BossGroup>());
                                    self.bossGroup.dropPosition = self.dropPosition;
                                }
                                self.bossGroup.AddMember(component);
                            }
                            if (flag)
                            {
                                num4 = 4.7f;
                                num5 = 2f;
                                component.inventory.SetEquipmentIndex(EliteCatalog.GetEliteDef(self.GetFieldValue<EliteIndex>("currentActiveEliteIndex")).eliteEquipmentIndex);
                            }
                            self.monsterCredit -= (float)num3;
                            if (self.isBoss)
                            {
                                int livingPlayerCount = Run.instance.livingPlayerCount;
                                num4 *= Mathf.Pow((float)livingPlayerCount, 1f);
                            }

                            // this is it
                            if (Run.instance.stageClearCount >= StageReq.Value)
                            {
                                CharacterMaster player = PlayerCharacterMasterController.instances[rand.Next(0, Run.instance.livingPlayerCount)].master;
                                component.inventory.CopyItemsFrom(player.inventory);
                                checkConfig(component.inventory, player);
                            }

                            component.inventory.GiveItem(ItemIndex.BoostHp, Mathf.RoundToInt((num4 - 1f) * 10f));
                            component.inventory.GiveItem(ItemIndex.BoostDamage, Mathf.RoundToInt((num5 - 1f) * 10f));
                            DeathRewards component2 = bodyObject.GetComponent<DeathRewards>();
                            if (component2)
                            {
                                component2.expReward = (uint)((float)num3 * self.expRewardCoefficient * Run.instance.compensatedDifficultyCoefficient);
                                component2.goldReward = (uint)((float)num3 * self.expRewardCoefficient * 2f * Run.instance.compensatedDifficultyCoefficient);
                            }
                            if (self.spawnEffectPrefab && NetworkServer.active)
                            {
                                Vector3 origin = gameObject.transform.position;
                                CharacterBody component3 = bodyObject.GetComponent<CharacterBody>();
                                if (component3)
                                {
                                    origin = component3.corePosition;
                                }
                                EffectManager.instance.SpawnEffect(self.spawnEffectPrefab, new EffectData
                                {
                                    origin = origin
                                }, true);
                            }
                            return true;
                        }
                    }
                }
                return false;
            };

        }


        public static void checkConfig(Inventory inventory, CharacterMaster master)
        {
            if (InheritItems.Value) // inheritance
            {
                updateInventory(inventory, master);
            }
            else if (GenerateItems.Value) // Using generator instead
            {
                resetInventory(inventory);
                int scc = Run.instance.stageClearCount;

                // Get average # of items among all players.
                int totalItems = 0;
                foreach (PlayerCharacterMasterController player in PlayerCharacterMasterController.instances)
                {
                    foreach (ItemIndex index in ItemCatalog.allItems)
                    {
                        totalItems += player.master.inventory.GetItemCount(index);
                    }
                }
                int avgItems = (int)Math.Pow(scc,2) + totalItems / PlayerCharacterMasterController.instances.Count;

                int numItems = 0;
                int amount;
                if (Tier1Items.Value)
                {
                    foreach (ItemIndex index in ItemCatalog.tier1ItemList)
                    {
                        if (Util.CheckRoll(ConfigToFloat(Tier1GenChance.Value) + (scc - StageReq.Value)))
                        {
                            amount = UnityEngine.Random.Range(0, (int)(Math.Pow(scc, 2) * ConfigToFloat(Tier1GenCap.Value) + 1));
                            if (numItems + amount > avgItems)
                            {
                                amount = avgItems - numItems;
                            }
                            numItems += amount;
                            inventory.GiveItem(index, amount);
                        }
                        if (numItems >= avgItems)
                        {
                            break;
                        }
                    }
                }
                if (Tier2Items.Value && numItems < avgItems)
                {
                    foreach (ItemIndex index in ItemCatalog.tier2ItemList)
                    {
                        if (Util.CheckRoll(ConfigToFloat(Tier2GenChance.Value) + (scc - StageReq.Value)))
                        {
                            amount = UnityEngine.Random.Range(0, (int)(Math.Pow(scc, 2) * ConfigToFloat(Tier2GenCap.Value) + 1));
                            if (numItems + amount > avgItems)
                            {
                                amount = avgItems - numItems;
                            }
                            numItems += amount;
                            inventory.GiveItem(index, amount);
                        }
                        if (numItems >= avgItems)
                        {
                            break;
                        }
                    }
                }
                if (Tier3Items.Value && numItems < avgItems)
                {
                    foreach (ItemIndex index in ItemCatalog.tier3ItemList)
                    {
                        if (Util.CheckRoll(ConfigToFloat(Tier3GenChance.Value) + (scc - StageReq.Value)))
                        {
                            amount = UnityEngine.Random.Range(0, (int)(Math.Pow(scc, 2) * ConfigToFloat(Tier3GenCap.Value) + 1));
                            if (numItems + amount > avgItems)
                            {
                                amount = avgItems - numItems;
                            }
                            numItems += amount;
                            inventory.GiveItem(index, amount);
                        }
                        if (numItems >= avgItems)
                        {
                            break;
                        }
                    }
                }
                if (LunarItems.Value && numItems < avgItems)
                {
                    foreach (ItemIndex index in ItemCatalog.lunarItemList)
                    {
                        if (Util.CheckRoll(ConfigToFloat(LunarGenChance.Value) + (scc - StageReq.Value)))
                        {
                            amount = UnityEngine.Random.Range(0, (int)(Math.Pow(scc, 2) * ConfigToFloat(LunarGenCap.Value) + 1));
                            if (numItems + amount > avgItems)
                            {
                                amount = avgItems - numItems;
                            }
                            numItems += amount;
                            inventory.GiveItem(index, amount);
                        }
                        if (numItems >= avgItems)
                        {
                            break;
                        }
                    }
                }
                if (EquipItems.Value)
                {
                    if (Util.CheckRoll(ConfigToFloat(EquipGenChance.Value)))
                    {
                        inventory.ResetItem(ItemIndex.AutoCastEquipment);
                        inventory.GiveItem(ItemIndex.AutoCastEquipment, 1);
                        EquipmentIndex equipmentIndex = Run.instance.availableEquipmentDropList[Run.instance.spawnRng.RangeInt(0, Run.instance.availableEquipmentDropList.Count)].equipmentIndex;

                        if (!EquipBlacklist.Value)
                        {
                            foreach (EquipmentIndex item in EquipmentBlacklist)
                            {
                                if (equipmentIndex == item)
                                {
                                    equipmentIndex = EquipmentIndex.Fruit; // default to fruit.
                                    break;
                                }
                            }
                        }


                        inventory.SetEquipmentIndex(equipmentIndex);
                    }
                }

                multiplier(inventory);
                blacklist(inventory);

            }
            else
            {
                resetInventory(inventory);
            }
        }

        public static void resetInventory(Inventory inventory)
        {
            foreach (ItemIndex index in ItemCatalog.tier1ItemList)
            {
                inventory.ResetItem(index);
            }
            foreach (ItemIndex index in ItemCatalog.tier2ItemList)
            {
                inventory.ResetItem(index);
            }
            foreach (ItemIndex index in ItemCatalog.tier3ItemList)
            {
                inventory.ResetItem(index);
            }
            foreach (ItemIndex index in ItemCatalog.lunarItemList)
            {
                inventory.ResetItem(index);
            }
        }

        public static void updateInventory(Inventory inventory, CharacterMaster master)
        {
            if (!Tier1Items.Value)
            {
                foreach (ItemIndex index in ItemCatalog.tier1ItemList)
                {
                    inventory.ResetItem(index);
                }
            }
            if (!Tier2Items.Value)
            {
                foreach (ItemIndex index in ItemCatalog.tier2ItemList)
                {
                    inventory.ResetItem(index);
                }
            }
            if (!Tier3Items.Value)
            {
                foreach (ItemIndex index in ItemCatalog.tier3ItemList)
                {
                    inventory.ResetItem(index);
                }
            }
            if (!LunarItems.Value)
            {
                foreach (ItemIndex index in ItemCatalog.lunarItemList)
                {
                    inventory.ResetItem(index);
                }
            }

            multiplier(inventory);

            if (EquipItems.Value)
            {
                inventory.ResetItem(ItemIndex.AutoCastEquipment);
                inventory.GiveItem(ItemIndex.AutoCastEquipment, 1);
                inventory.CopyEquipmentFrom(master.inventory);

                foreach (EquipmentIndex item in EquipmentBlacklist)
                {
                    if (inventory.GetEquipmentIndex() == item)
                    {
                        inventory.SetEquipmentIndex(EquipmentIndex.Fruit); // default to fruit
                        break;
                    }
                }

            }

            blacklist(inventory);
        }

        public static void multiplier(Inventory inventory)
        {
            float itemMultiplier = ConfigToFloat(ItemMultiplier.Value);
            if (itemMultiplier != 1f)
            {
                int count = 0;
                if (Tier1Items.Value)
                {
                    foreach (ItemIndex index in ItemCatalog.tier1ItemList)
                    {
                        count = inventory.GetItemCount(index);
                        inventory.ResetItem(index);
                        inventory.GiveItem(index, (int)Math.Ceiling(count * itemMultiplier));
                    }
                }
                if (Tier2Items.Value)
                {
                    foreach (ItemIndex index in ItemCatalog.tier2ItemList)
                    {
                        count = inventory.GetItemCount(index);
                        inventory.ResetItem(index);
                        inventory.GiveItem(index, (int)Math.Ceiling(count * itemMultiplier));
                    }
                }
                if (Tier3Items.Value)
                {
                    foreach (ItemIndex index in ItemCatalog.tier3ItemList)
                    {
                        count = inventory.GetItemCount(index);
                        inventory.ResetItem(index);
                        inventory.GiveItem(index, (int)Math.Ceiling(count * itemMultiplier));
                    }
                }
                if (LunarItems.Value)
                {
                    foreach (ItemIndex index in ItemCatalog.lunarItemList)
                    {
                        count = inventory.GetItemCount(index);
                        inventory.ResetItem(index);
                        inventory.GiveItem(index, (int)Math.Ceiling(count * itemMultiplier));
                    }
                }
            }
        }

        public static void blacklist(Inventory inventory)
        {
            foreach (ItemIndex item in ItemsNeverUsed)
            {
                inventory.ResetItem(item);
            }

            if (!ItemsBlacklist.Value)
            {
                foreach (ItemIndex item in ItemBlacklist)
                {
                    inventory.ResetItem(item);
                }
            }

            if (Limiter.Value)
            {
                if (inventory.GetItemCount(ItemIndex.Bear) > 7)
                {
                    inventory.ResetItem(ItemIndex.Bear);
                    inventory.GiveItem(ItemIndex.Bear, 7);
                }
                if (inventory.GetItemCount(ItemIndex.HealWhileSafe) > 30)
                {
                    inventory.ResetItem(ItemIndex.HealWhileSafe);
                    inventory.GiveItem(ItemIndex.HealWhileSafe, 30);
                }
                if (inventory.GetItemCount(ItemIndex.EquipmentMagazine) > 3)
                {
                    inventory.ResetItem(ItemIndex.EquipmentMagazine);
                    inventory.GiveItem(ItemIndex.EquipmentMagazine, 3);
                }
                if (inventory.GetItemCount(ItemIndex.SlowOnHit) > 1)
                {
                    inventory.ResetItem(ItemIndex.SlowOnHit);
                    inventory.GiveItem(ItemIndex.SlowOnHit, 1);
                }
                if (inventory.GetItemCount(ItemIndex.Behemoth) > 2)
                {
                    inventory.ResetItem(ItemIndex.Behemoth);
                    inventory.GiveItem(ItemIndex.Behemoth, 2);
                }
                if (inventory.GetItemCount(ItemIndex.BleedOnHit) > 3)
                {
                    inventory.ResetItem(ItemIndex.BleedOnHit);
                    inventory.GiveItem(ItemIndex.BleedOnHit, 3);
                }
                if (inventory.GetItemCount(ItemIndex.IgniteOnKill) > 2)
                {
                    inventory.ResetItem(ItemIndex.IgniteOnKill);
                    inventory.GiveItem(ItemIndex.IgniteOnKill, 2);
                }
                if (inventory.GetItemCount(ItemIndex.AutoCastEquipment) > 1)
                {
                    inventory.ResetItem(ItemIndex.AutoCastEquipment);
                    inventory.GiveItem(ItemIndex.AutoCastEquipment, 1);
                }

            }

            string[] customlist = CustomBlacklist.Value.Split(new[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries);

            foreach (string item in customlist)
            {
                int x = 0;
                if (Int32.TryParse(item, out x))
                {
                    inventory.ResetItem(((ItemIndex)x));
                }
            }
        }
    }

}