﻿using RoR2;
using System;

namespace BaddiesWithItems
{
    public static class SpawnCardSubscription
    {
        // Enemies w/ items
        [SystemInitializer(new Type[]
        {   typeof(BodyCatalog)
        })]
        public static void BaddiesItems()
        {
            SpawnCard.onSpawnedServerGlobal += SpawnResultItemAdder;
            SpawnCard.onSpawnedServerGlobal += ItemDropperComponentAdder;
        }

        public static bool CanStartGivingItems(TeamIndex? teamIndexOfBodyToGive = null)
        {
            if (Run.instance.stageClearCount + 1 < EnemiesWithItems.StageReq.Value)
                return false;
            if (EnemiesWithItems.StageReq.Value == 6 && SceneCatalog.mostRecentSceneDef.isFinalStage && Run.instance.loopClearCount == 0)
                return false;
            if (teamIndexOfBodyToGive != null)
                if (!TeamManager.IsTeamEnemy(teamIndexOfBodyToGive.GetValueOrDefault(), TeamIndex.Player))
                    return false;
            return true;
        }

        private static void ItemDropperComponentAdder(SpawnCard.SpawnResult obj)
        {
            if (!obj.success) //First check, chances are that if it wasnt successful theres not going to be any way to get the master
                return;
            CharacterMaster spawnResultMaster = obj.spawnedInstance ? obj.spawnedInstance.GetComponent<CharacterMaster>() : null;
            if (spawnResultMaster == null || !Util.CheckRoll(EnemiesWithItems.ConfigToFloat(EnemiesWithItems.DropChance.Value)))
                return;

            TeamIndex? teamIndexOverride = obj.spawnRequest.teamIndexOverride;
            if (!CanStartGivingItems(teamIndexOverride))
                return;

            UnityEngine.GameObject gameObject = spawnResultMaster.GetBodyObject();
            if (gameObject)
                gameObject.AddComponent<EWIDeathRewards>();
        }

        public static void SpawnResultItemAdder(SpawnCard.SpawnResult spawnResult)
        {
            CharacterMaster spawnResultMaster = spawnResult.spawnedInstance ? spawnResult.spawnedInstance.GetComponent<CharacterMaster>() : null;
            if (spawnResultMaster == null || !spawnResult.success || spawnResultMaster.inventory == null)
                return;

            TeamIndex? teamIndexOverride = spawnResult.spawnRequest.teamIndexOverride;
            if (!CanStartGivingItems(teamIndexOverride))
                return;
            //Xoroshiro throws off a range issue here at the beginning of the run, might have something to do with Run.instance.livingPlayerCount being zero in the very first frame.
            CharacterMaster playerToCopyFrom = PlayerCharacterMasterController.instances[RoR2.Run.instance.nextStageRng.RangeInt(0, Run.instance.livingPlayerCount)].master;
            ItemGeneration.GenerateItemsToInventory(spawnResultMaster.inventory, playerToCopyFrom);
        }
    }
}