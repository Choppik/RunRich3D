using R3;
using MyBuild.Scripts.Game.State.Root.Proxy;

namespace MyBuild.Scripts.Game.State.Providers
{
    /// <summary>
    /// Интерфейс провайдера данных игры.
    /// </summary>
    public interface IGameStateProvider
    {
        public GameStateProxy GameState {  get; }

        public Observable<GameStateProxy> LoadGameState();
        public Observable<bool> SaveGameState();
        public Observable<bool> ResetGameState();
    }
}
