using System.Collections;
using UnityEngine;
using System.Linq;
using UnityEngine.Assertions;

namespace Galaga.Game
{
    public class EnemySpawner : MonoBehaviour
    {
        public float Delay;
        public Grid Grid;
        public int Raw;
        public bool IsReverseOrder;
        public bool IsEmpty { get; private set; }
        public EnemySpawner NextSpawner;

        private GameProcessor _gameProcessor;
        private int spawnIndex = 0; // To track the current spawn point index

        private const char CharEmptySlot = '.';
        private const char CharRedMonster = 'r';
        private const char CharGreenMonster = 'g';
        private const char CharBlueMonster = 'b';
        private const char CharPurpleMonster = 'p';

        // Array of base spawn points along the X-axis
        private float[] spawnPositionsX = new float[] { -50f, 0f, 50f };

        // Stage-specific spawn and maneuver configurations
        private Vector3[] maneuverOffsets;
        private float[] maneuverRadii;

        void Awake()
        {
            Reload();
            _gameProcessor = GetComponentInParent<GameProcessor>();

            InitializeStageConfigurations();
        }

        public void Reload()
        {
            IsEmpty = false;
        }

        public void Spawn()
        {
            if (IsEmpty)
                return;

            int stage = _gameProcessor._levelConfigIndex;
            if (stage <= 10)
            {
                StartCoroutine(Spawning(stage));
            }
        }

        public int GetMobCount()
        {
            var config = Grid.GetRawConfig(Raw);
            return config.Length - config.Count(x => x == CharEmptySlot);
        }

        private void InitializeStageConfigurations()
        {
            // Setup different configurations for each stage (1 through 10)
            maneuverOffsets = new Vector3[]
            {
                new Vector3(0, 0, 0),                // Stage 1
                new Vector3(-200, -200, 0),          // Stage 2
                new Vector3(200, -200, 0),           // Stage 3
                new Vector3(100, -150, 0),           // Stage 4
                new Vector3(-100, -150, 0),          // Stage 5
                new Vector3(150, -100, 0),           // Stage 6
                new Vector3(-150, -100, 0),          // Stage 7
                new Vector3(0, -200, 0),             // Stage 8
                new Vector3(-50, -150, 0),           // Stage 9
                new Vector3(50, -150, 0)             // Stage 10
            };

            maneuverRadii = new float[] { 3f, 3.5f, 4f, 4.5f, 5f, 5.5f, 6f, 6.5f, 7f, 7.5f };
        }

        IEnumerator Spawning(int stage)
        {
            var rawConfig = Grid.GetRawConfig(Raw);
            Vector3 maneuverCenter = maneuverOffsets[stage - 1];
            float maneuverRadius = maneuverRadii[stage - 1];

            for (int i = 0; i < rawConfig.Length; ++i)
            {
                var c = rawConfig[i];
                if (c == CharEmptySlot)  // Skip empty slots
                    continue;

                float dynamicDelay = Delay + Random.Range(0.1f, 0.2f);
                yield return new WaitForSeconds(dynamicDelay);

                var monsterName = MapSymbolToMonsterName(c);

                // Cycle through spawn points for each enemy
                spawnIndex = (spawnIndex + 1) % spawnPositionsX.Length;
                float spawnPosX = spawnPositionsX[spawnIndex];
                var spawnPosition = new Vector3(spawnPosX, transform.position.y, transform.position.z);

                var gObj = Factory.Create(monsterName, _gameProcessor.Monsters, spawnPosition);
                var enemy = gObj.GetComponent<BaseEnemy>();

                var monsterConfig = _gameProcessor.GetLevelConfiguration()
                    .MonsterConfig.FirstOrDefault(x => x.Name == monsterName);
                enemy.SetContext(_gameProcessor, monsterConfig);

                enemy.RespawnToStartPosition(i, Raw);
                StartCoroutine(StartEnemyFlight(enemy, i, maneuverCenter, maneuverRadius));
            }

            yield return new WaitForSeconds(Delay);
            IsEmpty = true;

            if (NextSpawner != null)
                NextSpawner.Spawn();
        }

        IEnumerator StartEnemyFlight(BaseEnemy enemy, int gridX, Vector3 maneuverCenter, float radius)
        {
            if (enemy == null || enemy.transform == null)
                yield break;

            yield return StartCoroutine(enemy.FlyToCenterAndWaitForManeuver());

            if (enemy == null || enemy.transform == null)
                yield break;

            enemy.StartCircling();

            yield return new WaitForSeconds(0.2f);

            if (enemy == null || enemy.transform == null)
                yield break;

            var gridFollower = enemy.GetComponent<FollowerGrid>();
            if (gridFollower != null)
            {
                gridFollower.GridX = IsReverseOrder ? Grid.GetRawConfig(Raw).Length - gridX - 1 : gridX;
                gridFollower.GridY = Raw;
                gridFollower.Target = _gameProcessor.Grid;
                gridFollower.enabled = true;
            }
        }

        private string MapSymbolToMonsterName(char c)
        {
            Assert.IsTrue(c == 'r' || c == 'g' || c == 'b' || c == 'p');
            return c switch
            {
                CharRedMonster => "Red",
                CharGreenMonster => "Green",
                CharBlueMonster => "Blue",
                CharPurpleMonster => "Purple",
                _ => ""
            };
        }
    }
}











