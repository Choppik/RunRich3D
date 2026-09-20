using MyBuild.Scripts.Game.Common;
using MyBuild.Scripts.Utils.MVP.UI;

namespace MyBuild.Scripts.Game.Gameplay
{
    /// <summary>
    /// Презентер корневого UI сцены геймплея.
    /// </summary>
    public class UIGameplayRootPresenter : UIRootPresenter
    {
        public override string PathPrefabs => AppConsts.PathPrefabsGameplay;
    }
}
