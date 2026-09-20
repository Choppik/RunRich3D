namespace MyBuild.Scripts.Utils.MVP.UI
{
    /// <summary>
    /// Интерфейс базового класса привязки окна.
    /// </summary>
    public interface IWindowBinder
    {
        /// <summary>
        /// Закрытие окна.
        /// </summary>
        /// <remarks>Полное уничтожение объекта.</remarks>
        void Close();
    }
}
