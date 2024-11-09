using System;
using System.Collections;
using UnityEngine;
using System.Linq;
using UnityEngine.Assertions;
using UnityEngine.UI;

namespace Galaga.Game
{
    public class GameProcessor : MonoBehaviour
    {
        private GameObject levelStage;  // Ссылка на объект LevelStage
        public GameObject startShow;

        public event Action ScoreChanged = delegate { };
        public event Action LivesChanged = delegate { };

        public int Lives
        {
            get
            {
                return _lives;
            }
            set
            {
                _lives = value;
                LivesChanged();
            }
        }

        public int Scores
        {
            get
            {
                return _scores;
            }
            set
            {
                _scores = value;
                ScoreChanged();
            }
        }


        public HeroController HeroController;
        public Grid Grid;
        public EnemySpawner StartSpawner;
        public HiveMind HiveMind;
        public Transform Spawners;
        public Transform Monsters;
        public Transform Projectiles;
        public Transform Effects;

        public TextAsset ConfigShip;
        public TextAsset[] ConfigLevels;

        private ConfigShip _configShip;
        public ConfigLevel _configLevelCurrent;
        public int _levelConfigIndex;

        public Text _textStage;


        private int _scores;
        private int _lives;
        internal bool IsGameOver
        {
            get { return Lives < 0; }
        }
        internal bool IsLevelClear;
        private int _monstersToKill;


        void Awake()
        {
            Assert.IsNotNull(ConfigShip);
            Assert.IsTrue(ConfigLevels.Length > 0);
            _configShip = JsonUtility.FromJson<ConfigShip>(ConfigShip.text);

            // Находим объект LevelStage на сцене
            levelStage = GameObject.Find("LevelStage");
            Assert.IsNotNull(levelStage, "LevelStage object not found on the scene!");
            levelStage.SetActive(false);  // Отключаем его на старте

            var textStageObject = GameObject.Find("TextStage");  // Замените "TextStageName" на имя Text на сцене
            Assert.IsNotNull(textStageObject, "TextStage object not found on the scene!");
            _textStage = textStageObject.GetComponent<Text>();


            _configLevelCurrent = JsonUtility.FromJson<ConfigLevel>(ConfigLevels[_levelConfigIndex].text);
            _levelConfigIndex = (_levelConfigIndex) % ConfigLevels.Length;
            _textStage.text = _levelConfigIndex.ToString();
        }


        void Start()
        {
            Scores = 0;
            Lives = _configShip.Lifes;
            AudioManager.Instance.PlaySound("2");
            StartNextLevel();
            StartCoroutine(BlockHeroControlsCoroutine());
        }

        void Update()
        {
            if (IsLevelClear)
            {
                _configLevelCurrent = JsonUtility.FromJson<ConfigLevel>(ConfigLevels[_levelConfigIndex].text);
                _levelConfigIndex = (_levelConfigIndex) % ConfigLevels.Length;
                if (_levelConfigIndex == 10)
                {
                    AudioManager.Instance.PlaySound("8");
                    Time.timeScale = 0f; // Ставим игру на паузу
                }
                else
                {
                    IsLevelClear = false;
                    Scores += 100;
                    AudioManager.Instance.PlaySound("11");
                    StartCoroutine(StartNextLevelWithDelay());
                }
                
            }
            if (IsGameOver)
            {
                // Находим объект LevelStage на сцене
                levelStage = GameObject.Find("LevelStage");
                levelStage.SetActive(true);
                AudioManager.Instance.PlaySound("6");
                Assert.IsNotNull(levelStage, "LevelStage object not found on the scene!");
            }
        }

        private IEnumerator BlockHeroControlsCoroutine()
        {
            // Блокируем управление
            HeroController.Freeze(true);
            startShow.SetActive(true);

            // Ждем 3 секунды
            yield return new WaitForSeconds(3f);

            // Разблокируем управление
            HeroController.Freeze(false);
            startShow.SetActive(false);
        }

        private IEnumerator StartNextLevelWithDelay()
        {
            // Активируем LevelStage на 2 секунды
            levelStage.SetActive(true);
            yield return new WaitForSeconds(4f);
            levelStage.SetActive(false);

            StartNextLevel();  // Запускаем следующий уровень
        }

        private void StartNextLevel()
        {
            // load next level config
            _configLevelCurrent = JsonUtility.FromJson<ConfigLevel>(ConfigLevels[_levelConfigIndex].text);
            _levelConfigIndex = (_levelConfigIndex + 1) % ConfigLevels.Length;
            _textStage.text = _levelConfigIndex.ToString();
            

            // reconfigure grid
            Grid.Configure(_configLevelCurrent);

            // reconfigure spawners
            var spawners = Spawners.GetComponentsInChildren<EnemySpawner>();
            _monstersToKill = spawners.Sum(x => x.GetMobCount());
            foreach (var enemySpawner in spawners)
                enemySpawner.Reload();
            StartSpawner.Spawn();
            Debug.Log("Monsters to kill: " + _monstersToKill);

            // hive mind
            HiveMind.StartThink();
        }

        public ConfigShip GetShipConfiguration()
        {
            return _configShip;
        }

        public ConfigLevel GetLevelConfiguration()
        {
            return _configLevelCurrent;
        }

        public void OnMonsterDied(int scoreAward)
        {
            //Scores += scoreAward;
            --_monstersToKill;
            IsLevelClear = _monstersToKill <= 0;
            if (IsLevelClear)
                Debug.Log("Level clear");
        }

        [ContextMenu("DbgPrintConfig")]
        public void DbgPrintConfig()
        {
            Debug.Log(JsonUtility.ToJson(_configShip, true));
            Debug.Log(JsonUtility.ToJson(_configLevelCurrent, true));
        }
    }

}
