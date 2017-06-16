using RobotsRTS.Game;
using RobotsRTS.NetWork;
using System.Collections.Generic;
using UnityEngine;
namespace RobotsRTS
{
    /// <summary>
    /// контроллер зданий и турелей
    /// </summary>
    public interface IBuildController: IBaseInterface
    {
        /// <summary>
        /// Установить список префабов
        /// </summary>
        /// <param name="_buildPrefabs">Префабы зданий</param>
        /// <param name="_turretPrefabs">Префабы турелей</param>
        /// <param name="_turretGhostPrefabsGameObject">Префабы призраков турелей</param>
        /// <param name="_turretPlatformPrefab">Префаб Платформы под турель</param>
        void SetPrefbs(IDictionary<BuildType, GameObject> _buildPrefabs, IDictionary<TurretType, GameObject> _turretPrefabs, IDictionary<TurretType, GameObject> _turretGhostPrefabs, GameObject _turretPlatformPrefab);
        /// <summary>
        /// Добавить Здание
        /// </summary>
        /// <param name="_unit">Серверная структура Build</param>
        /// <returns>результат оперции</returns>
        bool AddBuild(NWBuild _build);
        /// <summary>
        /// Обновить информацию о здании
        /// </summary>
        /// <param name="_build">Серверная структура Build</param>
        /// <returns>результат оперции</returns>
        bool UpdateBuild(NWBuild _build);
        /// <summary>
        /// Удалить здание
        /// </summary>
        /// <param name="_build">Серверная структура Build</param>
        /// <returns>результат оперции</returns>
        bool RemoveBuild(NWBuild _build);
        /// <summary>
        /// Удалить здание
        /// </summary>
        /// <param name="_buildId">Id удаляемого здания</param>
        /// <returns>результат оперции</returns>
        bool RemoveBuild(int _buildId);
        /// <summary>
        /// Получить все здания массивом
        /// </summary>
        /// <returns></returns>
        IBuild[] GetAllBuilds();
        /// <summary>
        /// попытаться получить здание
        /// </summary>
        /// <param name="_id">Id здания</param>
        /// <param name="_build">результат</param>
        /// <returns></returns>
        bool TryGetBuild(int _id, out IBuild _build);
        /// <summary>
        /// Пометить здание как "выбранный"
        /// </summary>
        /// <param name="_id"></param>
        /// <returns>результат</returns>
        bool Select(int _id);
        /// <summary>
        /// Снять со здания выделение
        /// </summary>
        /// <returns>результат</returns>
        bool OnDeselect();
        /// <summary>
        /// Получить выделенное здание 
        /// </summary>
        /// <param name="">ссылка на здание </param>
        /// <returns>результат</returns>
        bool GetSelectedBuild(out IBuild _build);
        /// <summary>
        /// Добавить турель
        /// </summary>
        /// <param name="_turret">Серверная структура Turret</param>
        /// <returns>результат оперции</returns>
        bool AddTurret(NWTurret _turret);
        /// <summary>
        /// Удалить турель
        /// </summary>
        /// <param name="_idTurret">Серверная структура Turret</param>
        /// <returns>результат оперции</returns>
        bool RemoveTurret(int _idTurret);
        /// <summary>
        /// Получить id всех турелей
        /// </summary>
        /// <returns>массив шв всех турелей</returns>
        int[] GetAllTurretsId();
        /// <summary>
        /// Получить все турели массивом
        /// </summary>
        /// <returns></returns>
        ITurret[] GetAllTurrets();
        /// <summary>
        /// попытаться получить турель
        /// </summary>
        /// <param name="_id">Id турели</param>
        /// <param name="_turret">результат</param>
        /// <returns></returns>
        bool TryGetTurret(int _id, out ITurret _turret);
        /// <summary>
        /// Попытаться получить призрак турели
        /// </summary>
        /// <param name="_type">Тип турели</param>
        /// <param name="turretPrefab">префба призрака</param>
        /// <returns>результат операции</returns>
        bool TryGetTurretGhost(TurretType _type, out GameObject turretPrefab);
        /// <summary>
        /// Выполнить игровую команду
        /// </summary>
        void PlayCommand();
    }
}