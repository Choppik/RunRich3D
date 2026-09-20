using DI;

namespace MyBuild.Scripts.Utils.MVP.UI
{
    /// <summary>
    /// Базовый класс менеджера окон сцены.
    /// </summary>
    public abstract class UIManager
    {
        protected readonly DIContainer Container;

        protected UIManager(DIContainer container)
        {
            Container = container;
        }
    }
}
