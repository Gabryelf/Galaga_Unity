using Galaga.Game;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;

namespace Galaga.GUI
{
    public class StageControl : MonoBehaviour
    {
        private GameProcessor _gameProcessor;
        public Text _textStage;

        void Awake()
        {
            _textStage = GetComponent<Text>();
        }

        void RefreshScore()
        {
            _textStage.text = _gameProcessor._levelConfigIndex.ToString();
        }

        public void Subscribe(GameProcessor gameProcessor)
        {
            Assert.IsNotNull(gameProcessor);
            Debug.Log("ScoreControl:Subscribe: to new gameProcessor " + gameProcessor.GetHashCode());
            _gameProcessor = gameProcessor;
            gameProcessor.ScoreChanged += RefreshScore;
        }
    }
}
