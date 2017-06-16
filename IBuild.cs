using RobotsRTS.NetWork;
using System;
namespace RobotsRTS.Game
{
    /// <summary>
    /// Интерфейс здания
    /// </summary>
    public interface IBuild : ISelect, IDisposable
    {
        /// <summary>
        /// id Здания
        /// </summary>
        int Id { get; }
        /// <summary>
        /// тип здания
        /// </summary>
        BuildType Type { get; }
        /// <summary>
        /// Уровень здоровья
        /// </summary>
        int Health { get; }
        /// <summary>
        /// Список строящихся заднием обьектов
        /// </summary>
        NWBuildTask[] BuildTasks { get; }
        /// <summary>
        /// Список платформ турелей
        /// </summary>
        TurretPlatform[] TurretPlatforms { get; }
        /// <summary>
        /// задать текущую информацию по зданию
        /// </summary>
        /// <param name="_buildDescription">информанция о здании с сервера</param>
        void SetInfo(NWBuild _buildDescription);
        /// <summary>
        /// Задает для здания список платформ турелей
        /// </summary>
        /// <param name="_platforms">платформы турелей</param>
        void SetTurretPlatforms(TurretPlatform[] _platforms);
        /// <summary>
        /// Обновить информацию о строении
        /// </summary>
        /// <param name="_buildDescription">информанция о здании с сервера</param>
        void UpdateInfo(NWBuild _buildDescription);
        /// <summary>
        /// Показать плашку со здоровьем
        /// </summary>
        void ShowHealthBar();
    }
}