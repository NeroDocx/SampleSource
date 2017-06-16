using RobotsRTS.AStar;
using RobotsRTS.NetWork;
using RobotsRTS.UI;
namespace RobotsRTS
{
    public interface IGameContoller: IScreenController
    {
        /// <summary>
        /// ссылка на IMainPlayerController mainPlayer
        /// </summary>
        IMainPlayerController MainPlayerController { get; }
        /// <summary>
        /// ссылка на UnitController
        /// </summary>
        IUnitController UnitController { get; }
        /// <summary>
        /// ссылка на BuildController
        /// </summary>
        IBuildController BuildController { get; }
        /// <summary>
        /// Ссылка на контроллер поиска пути по A*
        /// </summary>
        AStarController AStarController { get; }
        /// <summary>
        /// ссылка на контроллер выбора обьекта на сцене
        /// </summary>
        ISelectObject SelectObject { get; }
        /// <summary>
        /// ссылка на игровой UI
        /// </summary>
        IGameUI UI { get; }
        /// <summary>
        /// Задает стартовый NWGameStateInfo  
        /// </summary>
        /// <param name="_state"></param>
        void SetStartGameState(NWGameStateInfo _state);
        /// <summary>
        /// Создать запрос на готовность получить стартовый NWGameStateInfo
        /// </summary>
        void WaitStartValues();
        /// <summary>
        /// Создать запрос на готовность начать игру
        /// </summary>
        void BeginGame();
        /// <summary>
        /// Задает в GameContoller текущее состояние игры через актуальный NWGameStateInfo
        /// </summary>
        /// <param name="_gameState"></param>
        void SetGameState(NWGameStateInfo _gameState);
    }
}