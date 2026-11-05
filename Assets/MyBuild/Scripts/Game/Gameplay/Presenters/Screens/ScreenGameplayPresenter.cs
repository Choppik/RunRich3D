using MyBuild.Scripts.Game.Components.LevelGenerate;
using MyBuild.Scripts.Game.Gameplay.Binderes;
using MyBuild.Scripts.Game.Gameplay.Managers;
using MyBuild.Scripts.Game.Settings;
using MyBuild.Scripts.Game.State;
using MyBuild.Scripts.Game.State.Providers;
using MyBuild.Scripts.Utils.MVP.UI;
using R3;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace MyBuild.Scripts.Game.Gameplay.Presenters
{
    /// <summary>
    /// Презентер основного окна геймплея.
    /// </summary>
    public class ScreenGameplayPresenter : WindowPresenterBase
    {
        public ScreenGameplayPresenter(
            GameplayUIManager manager,
            ISettingsProvider settingsProvider,
            LevelManager levelManager,
            ScoreManager scoreManager,
            IGameStateProvider gameState)
        {
            _manager = manager;
            _settingsProvider = settingsProvider;
            _levelManager = levelManager;
            _scoreManager = scoreManager;
            _gameState = gameState;
        }

        public override void BindWindow(IWindowBinder window)
        {
            base.BindWindow(window);
            BindSubscriptions();

            var data = _gameState.GameState.GetGurrentMap().Progress;
            Binder.UpdateCurrency(data.Currency.CurrentValue.ToString());
            Binder.SetCurrentLevel(data.CurrentLevel.CurrentValue.ToString());

            _manager.OpenPreviewPresenter(this);
        }

        /// <summary>
        /// Подписка на события.
        /// </summary>
        private void BindSubscriptions()
        {
            _scoreManager.OnScoreChanged.Subscribe(OnScoreChange).AddTo(_compositeDisposable);
            EndTrigger.OnEndLevel.Subscribe(_ => LevelEnd()).AddTo(_compositeDisposable);
        }

        public void RunCurrenLevel(bool value)
        {
            if (value)
            {
                if (_levelManager.IsLevelGenerate)
                {
                    _levelManager.Run();
                }
            }
        }

        public void RunNextLevel(int nextLevel)
        {
            var data = _gameState.GameState.GetGurrentMap().Progress;
            data.CurrentLevel.OnNext(nextLevel); // Обновляем текущий уровень
            Binder.UpdateCurrency(data.Currency.CurrentValue.ToString());

            _gameState.SaveGameState();

            _levelManager.StartLevel(nextLevel);
            Binder.SetCurrentLevel(nextLevel.ToString());
            _manager.OpenPreviewPresenter(this);
        }

        private void OnScoreChange(int value)
        {
            Binder.UpdateCurrentScore(value.ToString());
        }

        private void LevelEnd()
        {
            // TODO: Запуспк анимации, звуков
            _levelManager.Stop();

            var data = _gameState.GameState.GetGurrentMap().Progress;
            data.Currency.Value += _scoreManager.Score; // Когда уровень закончен, прибавляем счет к валюте игрока.

            if (data.Record.CurrentValue < _scoreManager.Score)
            {
                data.Record.OnNext(_scoreManager.Score); // Обновление рекорда
            }

            _gameState.SaveGameState();

            _manager.OpenEndLevelPresenter(this);
        }

        public override void Dispose()
        {
            Window?.Close();
            _compositeDisposable.Dispose();
        }

        public override string Name => "ScreenGameplay";

        public ScreenGameplayBinder Binder => base.GetWindow<ScreenGameplayBinder>();


        private readonly GameplayUIManager _manager;
        private readonly ISettingsProvider _settingsProvider;
        private readonly LevelManager _levelManager;
        private readonly ScoreManager _scoreManager;
        private PopupPreviewPresenter _popupPreviewPresenter;
        private IGameStateProvider _gameState;

        // Все подписки.
        private readonly CompositeDisposable _compositeDisposable = new();
    }
}
