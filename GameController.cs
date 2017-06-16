using RobotsRTS.AStar;
using RobotsRTS.Game;
using RobotsRTS.Helper;
using RobotsRTS.NetWork;
using RobotsRTS.SharedCode;
using RobotsRTS.SharedCode.XML;
using RobotsRTS.UI;
using RobotsRTS.UtilLevelData;
using System;
using System.Collections.Generic;
using UnityEngine;
namespace RobotsRTS
{
    public class GameController : MonoBehaviour, IGameContoller
    {
        [SerializeField]
        private CCamera cameraController;
        private IGameContollers gameControllers;
        private ISceneResourceManager resourceManager;
        private LoadScreenInfo loadScreenInfo;
        private MapController mapController;

        private Dictionary<BuildType, int> BuildPrefabIds;
        private Dictionary<TurretType, int> TurretPrefabIds;
        private Dictionary<TurretType, int> TurretGhostPrefabIds;
        private int TurretPlatformId;
        private Queue<Action> commandQueue;
        private Dictionary<TeamId, NWTeamInfo> teams;
        
        //public GameController(string _LevelName, SelectObject _selectObject)
        //{
        //    LevelData levelData = null;
        //    string path = Application.dataPath + Constants.LevelsPath + _LevelName;
        //    SaveAndReadXmlData.LoadXmllData(path, out levelData);
        //    AStarController = new AStarController(levelData.Grid);
        //}
        #region Interface

        public IMainPlayerController MainPlayerController { get; private set; }
        public IUnitController UnitController { get; private set; }
        public IBuildController BuildController { get; private set; }
        public AStarController AStarController { get; private set; }
        public ISelectObject SelectObject { get; private set; }
        public IGameUI UI { get; private set; }
        public bool IsDisposed { get; private set; }

        public ScreenType ScreenType { get { return ScreenType.Game; } }

        public bool IsInitComplite
        {
            get
            {
                return loadScreenInfo != null ? loadScreenInfo.IsDone : false;
            }
        }
        public int LoadScreenPersents
        {
            get
            {
                return loadScreenInfo != null ? loadScreenInfo.TotalProgress : 0;
            }
        }
        public void InitContoller()
        {
            Console.Log(ConsoleFilter.MainController, "Game contoller Init is Start");
            IsDisposed = true;
            if (Main.Instance == null)
            {
                Console.LogError(ConsoleFilter.MainController, "Main is null in InitContoller Game");
                return;
            }
            commandQueue = new Queue<Action>();
            teams = new Dictionary<TeamId, NWTeamInfo>();
            gameControllers = Main.Instance;
            gameControllers.ServerSimulator.GameCommandResultResponse -= GameCommandResult;
            gameControllers.ServerSimulator.GameCommandResultResponse += GameCommandResult;
            gameControllers.ServerSimulator.StartGameStateResponse -= SetStartGameState;
            gameControllers.ServerSimulator.StartGameStateResponse += SetStartGameState;
            gameControllers.ServerSimulator.UpdateGameState -= SetGameState;
            gameControllers.ServerSimulator.UpdateGameState += SetGameState;
            SelectObject = GetComponent<SelectObject>() as ISelectObject;
            if (SelectObject == null)
            {
                Console.LogError(ConsoleFilter.MainController, "Get Component SelectObject is fail");
                return;
            }

            SelectObject.SetGameControoler(this);
            //SelectObject.SelectObjRequest -= SelectObjRequest;
            //SelectObject.SelectObjRequest += SelectObjRequest;
            //SelectObject.SendCommandRequest -= SendCommandRequest;
            //SelectObject.SendCommandRequest += SendCommandRequest;
            SelectObject.SelectBuild -= SelectBuild;
            SelectObject.SelectBuild += SelectBuild;

            UnitController = new UnitController();
            BuildController = new BuildController();
            MainPlayerController = new MainPlayerController(this, TeamId.Red);
            MainPlayerController.SendGameCommandEvent -= MainPlayerCommand;
            MainPlayerController.SendGameCommandEvent += MainPlayerCommand;

            UpdateLoadUnfo(LoadScreeResutType.Done, LevelModules.Scene, 1.0f);
            if (!gameControllers.MainUI.SwhichCurrentScreen(ScreenType, CallBackInitUI))
            {
                Console.LogError(ConsoleFilter.MainController, "Game create fail, UI is fail");
                return;
            }

            resourceManager = new SceneResourceManager(gameControllers);


            Dictionary<int, string> prefabs = new Dictionary<int, string>();
            LevelDescription levelDescription = null;
            int idLevel = gameControllers.ServerSimulator.GetGameInfo();
            if (idLevel < 0)
            {
                Console.LogError(ConsoleFilter.MainController, "Get Game Info from simulator is fail");
                return;
            }

            if (!gameControllers.GameXml.Levels.TryGetValue(idLevel, out levelDescription) || levelDescription == null)
            {
                Console.LogErrorFormat(ConsoleFilter.MainController, "get levelDescription id fail, level id {0}", idLevel);
                return;
            }
            int id = 0;
            prefabs.Add(id, levelDescription.Path);

            BuildPrefabIds = new Dictionary<BuildType, int>();
            foreach(BuildType type in Enum.GetValues(typeof(BuildType))as BuildType[])
            {
                if (gameControllers.GameXml.Builds.ContainsKey(type))
                {
                    id++;
                    BuildPrefabIds.Add(type, id);
                    prefabs.Add(id, gameControllers.GameXml.Builds[type].Path);
                }
                else
                {
                    Console.LogErrorFormat(ConsoleFilter.MainController, "Can't get buildinfo for {0}", type);
                }
            }

            TurretPrefabIds = new Dictionary<TurretType, int>();
            foreach (TurretType type in Enum.GetValues(typeof(TurretType)) as TurretType[])
            {
                if (type != TurretType.None)
                {
                    if (gameControllers.GameXml.Turrets.ContainsKey(type))
                    {
                        id++;
                        TurretPrefabIds.Add(type, id);
                        prefabs.Add(id, gameControllers.GameXml.Turrets[type].Path);
                    }
                    else
                    {
                        Console.LogErrorFormat(ConsoleFilter.MainController, "Can't get turretInfo for {0}", type);
                    }
                }
            }

            TurretGhostPrefabIds = new Dictionary<TurretType, int>();
            foreach (TurretType type in Enum.GetValues(typeof(TurretType)) as TurretType[])
            {
                if (type != TurretType.None)
                {
                    if (gameControllers.GameXml.Turrets.ContainsKey(type))
                    {
                        id++;
                        TurretGhostPrefabIds.Add(type, id);
                        prefabs.Add(id, gameControllers.GameXml.Turrets[type].PathGhost);
                    }
                    else
                    {
                        Console.LogErrorFormat(ConsoleFilter.MainController, "Can't get turretInfo for {0}", type);
                    }
                }
            }

            id++;
            TurretPlatformId = id;
            prefabs.Add(TurretPlatformId, gameControllers.GameXml.TurretPlatformPath);

            if (!resourceManager.StartLoadResources(prefabs, CallBackLoadResources))
            {
                Console.LogError(ConsoleFilter.MainController, "Game create fail, LoadResources is fail");
                return;
            }

            IsDisposed = false;
            Console.Log(ConsoleFilter.MainController, "Game contoller Init is Done");
        }
        public void SetStartGameState(NWGameStateInfo _state)
        {
            if (_state == null)
            {
                Console.LogError(ConsoleFilter.GameController, "Start GameStateInfo is null");
                return;
            }

            if (Util.ArrayIsNullOrEmpty(_state.Teams))
            {
                Console.LogError(ConsoleFilter.GameController, "Start GameStateInfo  Teams is null");
                return;
            }
            foreach (NWTeamInfo team in _state.Teams)
            {
                if (teams.ContainsKey(team.Id))
                {
                    Console.LogErrorFormat(ConsoleFilter.GameController, "Dublicate Team {0}", team.Id);
                    continue;
                }
                teams.Add(team.Id, team);
            }


            if (!Util.ArrayIsNullOrEmpty(_state.Units))
            {
                foreach (NWUnit unit in _state.Units)
                {
                    UnitController.AddUnit(unit);
                }
            }

            if (!Util.ArrayIsNullOrEmpty(_state.Builds))
            {
                foreach (NWBuild build in _state.Builds)
                {
                    if (build == null)
                    {
                        Console.LogError(ConsoleFilter.MainController, "ServerBuild  is null");
                        continue;
                    }
                    BuildController.AddBuild(build);
                }
            }

            if (!Util.ArrayIsNullOrEmpty(_state.Turrets))
            {
                foreach (NWTurret turret in _state.Turrets)
                {
                    if (turret == null)
                    {
                        Console.LogError(ConsoleFilter.MainController, "ServerTurret is null");
                    }
                    BuildController.AddTurret(turret);
                }
            }

            if (gameControllers.SwichScreenController != null)
            {
                gameControllers.SwichScreenController.LoadScreenDone();
            }

            commandQueue.Enqueue(() =>
            {
                if (teams.ContainsKey(MainPlayerController.TeamId))
                {
                    UI.SetMainPlayerTeamInfo(teams[MainPlayerController.TeamId]);
                }
            });
        }
        public void WaitStartValues()
        {
            NWGameCommand com = new NWGameCommand();
            com.GameCommandType = GameCommandType.WaitStartValues;
            gameControllers.ServerSimulator.AddCommand(com); 
        }
        public void BeginGame()
        {
            NWGameCommand com = new NWGameCommand();
            com.GameCommandType = GameCommandType.GameReady;
            gameControllers.ServerSimulator.AddCommand(com);
        }
        public void CreateRobotRequest(int _buldId)
        {
            NWBuildTask task = new NWBuildTask();
            task.progress = 0.0f;
            task.Id = 0;
            task.BuildId = _buldId;

            NWGameCommand com = new NWGameCommand();
            com.GameCommandType = GameCommandType.AddBuildTask;
            com.BuildTasks = new NWBuildTask[1] { task };

            gameControllers.ServerSimulator.AddCommand(com);
        }
        public void CreateTurretRequest(int _buldId, int idPlatform, TurretType _turretType)
        {
            NWBuildTask task = new NWBuildTask();
            task.progress = 0.0f;
            task.Id = 0;
            task.BuildId = _buldId;
            task.Type = BuildTaskType.Turret;
            task.CreateTurretType = _turretType;
            task.PlatformId = idPlatform;

            NWGameCommand com = new NWGameCommand();
            com.GameCommandType = GameCommandType.AddBuildTask;
            com.BuildTasks = new NWBuildTask[1] { task };

            gameControllers.ServerSimulator.AddCommand(com);
        }
        public void RemoveBuildObjRequest(int _buldId, int _idTask)
        {
            NWBuildTask task = new NWBuildTask();
            task.progress = 0.0f;
            task.Id = _idTask;
            task.BuildId = _buldId;

            NWGameCommand com = new NWGameCommand();
            com.GameCommandType = GameCommandType.RemoveBuildTask;
            com.BuildTasks = new NWBuildTask[1] { task };

            gameControllers.ServerSimulator.AddCommand(com);
        }
        public void SetGameState(NWGameStateInfo _gameState)
        {
            if (_gameState == null)
            {
                Console.LogError(ConsoleFilter.GameController, "Update GameStateInfo is null");
                return;
            }

            if (Util.ArrayIsNullOrEmpty(_gameState.Teams))
            {
                Console.LogError(ConsoleFilter.GameController, "Update GameStateInfo  Teams is null");
                return;
            }
            foreach (NWTeamInfo team in _gameState.Teams)
            {
                if (!teams.ContainsKey(team.Id))
                {
                    Console.LogErrorFormat(ConsoleFilter.GameController, "Team {0} is not Containce", team.Id);
                    continue;
                }
                teams[team.Id].Resources = team.Resources;
                teams[team.Id].Units = team.Units;
                teams[team.Id].Builds = team.Builds;
                teams[team.Id].Turrets = team.Turrets;
            }

            if (!Util.ArrayIsNullOrEmpty(_gameState.Units))
            {
                foreach (NWUnit serverUnit in _gameState.Units)
                {
                    Unit unit = null;
                    if (UnitController.TryGetUnit(serverUnit.Id, out unit) && unit != null)
                    {
                        unit.transform.position = Util.ConvertRTSStructToVector3(serverUnit.trasform);
                    }
                }
            }
            if (!Util.ArrayIsNullOrEmpty(_gameState.Builds))
            {
                foreach (NWBuild serverBuild in _gameState.Builds)
                {
                    BuildController.UpdateBuild(serverBuild);
                }
            }

            //add and Update Turrets
            int[] turretsId = BuildController.GetAllTurretsId();
            HashSet<int> Turrets = new HashSet<int>(turretsId);

            int[] serverTurretsId = Util.ArrayIsNullOrEmpty(_gameState.Turrets) ? 
                new int[0] : 
                Array.ConvertAll(_gameState.Turrets, x => x.Id);
            
			HashSet<int> ServerTurrets = new HashSet<int>(serverTurretsId);
            HashSet<int> removeTurrets = new HashSet<int>(Turrets);
			
            removeTurrets.ExceptWith(ServerTurrets);

            foreach (NWTurret nwTurret in _gameState.Turrets)
            {
                int id = nwTurret.Id;
                ITurret turret = null;
                if (BuildController.TryGetTurret(id, out turret) && turret != null)
                {
                    turret.UpdateInfo(nwTurret);
                }
                else
                {
                    BuildController.AddTurret(nwTurret);
                }
            }

            foreach (int turretId in removeTurrets)
            {
                BuildController.RemoveTurret(turretId);
            }
            commandQueue.Enqueue(() =>
            {
                UI.SetMainPlayerTeamInfo(teams[MainPlayerController.TeamId]);
            });
        }
        #endregion


        #region SelectObject
        private void SelectBuild(int _id)
        {
            BuildController.Select(_id);
            IBuild build;
            if (BuildController.TryGetBuild(_id, out build))
            {
                UI.SelectBuild(build);
            }
        }
        private void SelectObjRequest(int[] _ids)
        {
            Debug.LogError("call SelectObjRequest");
        }
        private void SendCommandRequest(UnitCommandType _commandType, Vector3 _target)
        {
            Debug.LogError("call SendCommandRequest");
        }
        #endregion
        private void UpdateLoadUnfo(LoadScreeResutType _result, LevelModules _module, float _progress)
        {
            if (loadScreenInfo == null)
            {
                loadScreenInfo = new LoadScreenInfo(gameControllers.SceneController.CurrentScreen, ScreenType);
            }
            if (IsInitComplite)
            {
                Console.LogWarningFormat(ConsoleFilter.GameController, "Incorrect call UpdateLoadUnfo, IsInitComplite = true: {0}, {1}, {2}", _result, _module, _progress);
                return;
            }
            loadScreenInfo.UpdateInfo(_result, _module, _progress);
            gameControllers.SwichScreenController.CallBackForMainMenuScreen(_result, _module, _progress);

            switch (_module)
            {
                case LevelModules.UI:
                    if (_result == LoadScreeResutType.Done)
                    {
                        UI = gameControllers.MainUI.ScreenUI as IGameUI;
                        if (UI == null)
                        {
                            Console.LogError(ConsoleFilter.MainController, "Fail Get IMainMenuUI");
                            break;
                        }
                        UI.SetGameControoler(this);
                        UI.EventCreatingTurret -= SelectObject.CreatingTurret;
                        UI.EventCancelCreateTurret -= SelectObject.CancelCreatingTurret;
                        UI.CreateRobotRequest -= CreateRobotRequest;
                        UI.CreateTurretRequest -= CreateTurretRequest;
                        UI.RemoveBuildObjRequest -= RemoveBuildObjRequest;

                        UI.EventCreatingTurret += SelectObject.CreatingTurret;
                        UI.EventCancelCreateTurret += SelectObject.CancelCreatingTurret;
                        UI.CreateRobotRequest += CreateRobotRequest;
                        UI.CreateTurretRequest += CreateTurretRequest;
                        UI.RemoveBuildObjRequest += RemoveBuildObjRequest;

                        SelectObject.SelectPlatformForTurret -= UI.SelectPlatformForTurret;
                        SelectObject.CancelSelectPlatformForTurret -= UI.CancelCreateTurret;

                        SelectObject.SelectPlatformForTurret += UI.SelectPlatformForTurret;
                        SelectObject.CancelSelectPlatformForTurret -= UI.CancelCreateTurret;
                    }
                    break;
                case LevelModules.Resouces:
                    commandQueue.Enqueue(() =>
                    {
                        InstansingResource();
                    });
                    break;
                default:
                    break;
            }
            if (IsInitComplite)
            {
                ulong timeActionId = gameControllers.TimeActionContoller.AddTimeAction((obj) =>
                {
                    WaitStartValues();
                    return true;
                }, null, 1.0f);
            }
        }
        private void CallBackInitUI(LoadScreeResutType _result, float _progress)
        {
            UpdateLoadUnfo(_result, LevelModules.UI, _progress);
        }
        private void CallBackLoadResources(LoadScreeResutType _result, float _progress)
        {
            UpdateLoadUnfo(_result, LevelModules.Resouces, _progress);
        }
        private void CallBackLoadInstansingResource(LoadScreeResutType _result, float _progress)
        {
            UpdateLoadUnfo(_result, LevelModules.InstansingResource, _progress);
        }
        private void InstansingResource()
        {
            Console.Log(ConsoleFilter.GameController, "Start Instansing");
            CallBackLoadInstansingResource(LoadScreeResutType.Progress, 0.0f);
            GameObject mapPrefab = null;
            if (!resourceManager.GetPrefab(0, out mapPrefab) || mapPrefab == null)
            {
                Console.LogError(ConsoleFilter.MainController, "Can't  get prefab for Map");
                CallBackLoadInstansingResource(LoadScreeResutType.Fail, 0.0f);
                return;
            }

            CallBackLoadInstansingResource(LoadScreeResutType.Progress, 0.1f);
            GameObject MapObj = UnityEngine.Object.Instantiate(mapPrefab, Vector3.zero, new Quaternion()) as GameObject;
            mapController = MapObj.GetComponent<MapController>() as MapController;
            if (mapController == null)
            {
                Console.LogError(ConsoleFilter.MainController, "Can't  get GetComponent Map Controller");
                CallBackLoadInstansingResource(LoadScreeResutType.Fail, 0.1f);
                return;
            }
            CallBackLoadInstansingResource(LoadScreeResutType.Progress, 0.1f);
            if (mapController.MapXml == null)
            {
                Console.LogError(ConsoleFilter.MainController, "Can't  get xml for level");
                CallBackLoadInstansingResource(LoadScreeResutType.Fail, 0.1f);
                return;
            }
            LevelData leveldata = null;
            if (!SaveAndReadXmlData.LoadXmllDataFromText(mapController.MapXml, out leveldata) || leveldata == null)
            {
                Console.LogError(ConsoleFilter.MainController, "Can't  parse xml for level");
                CallBackLoadInstansingResource(LoadScreeResutType.Fail, 0.1f);
                return;
            }

            gameControllers.ServerSimulator.SetStartGameInfo(leveldata);

            if (cameraController == null)
            {
                Console.LogError(ConsoleFilter.MainController, "camera Controller is null");
                CallBackLoadInstansingResource(LoadScreeResutType.Fail, 0.1f);
                return;
            }
            if (Util.ArrayIsNullOrEmpty(leveldata.Teams))
            {
                Console.LogError(ConsoleFilter.MainController, "not found teams, is null");
                CallBackLoadInstansingResource(LoadScreeResutType.Fail, 0.1f);
                return;
            }

            cameraController.transform.position = new Vector3( 
                leveldata.Teams[0].CameraPosition.Position.X, 
                leveldata.Teams[0].CameraPosition.Position.Y,
                leveldata.Teams[0].CameraPosition.Position.Z);
            cameraController.transform.rotation = new Quaternion(
                leveldata.Teams[0].CameraPosition.Rotation.X,
                leveldata.Teams[0].CameraPosition.Rotation.Y,
                leveldata.Teams[0].CameraPosition.Rotation.Z,
                leveldata.Teams[0].CameraPosition.Rotation.W);


            IDictionary<BuildType, GameObject> prefabsBuild = new Dictionary<BuildType, GameObject>();
            foreach (BuildType type in BuildPrefabIds.Keys)
            {
                GameObject prefab = null;
                if (!resourceManager.GetPrefab(BuildPrefabIds[type], out prefab) || prefab == null)
                {
                    Console.LogErrorFormat(ConsoleFilter.MainController, "Can't  get prefab {0}", type);
                    continue;
                }
                prefabsBuild.Add(type, prefab);
            }

            IDictionary<TurretType, GameObject> prefabsTurret = new Dictionary<TurretType, GameObject>();
            foreach (TurretType type in TurretPrefabIds.Keys)
            {
                GameObject prefab = null;
                if (!resourceManager.GetPrefab(TurretPrefabIds[type], out prefab) || prefab == null)
                {
                    Console.LogErrorFormat(ConsoleFilter.MainController, "Can't  get prefab {0}", type);
                    continue;
                }
                prefabsTurret.Add(type, prefab);
            }
            IDictionary<TurretType, GameObject> prefabsTurretGhost = new Dictionary<TurretType, GameObject>();
            foreach (TurretType type in TurretGhostPrefabIds.Keys)
            {
                GameObject prefab = null;
                if (!resourceManager.GetPrefab(TurretGhostPrefabIds[type], out prefab) || prefab == null)
                {
                    Console.LogErrorFormat(ConsoleFilter.MainController, "Can't  get prefab turret Ghost {0}", type);
                    continue;
                }
                prefabsTurretGhost.Add(type, prefab);
            }
            GameObject turretPlatform = null;
            if (!resourceManager.GetPrefab(TurretPlatformId, out turretPlatform) || turretPlatform == null)
            {
                Console.LogError(ConsoleFilter.MainController, "Can't  get prefab turret Platform");
            }

            BuildController.SetPrefbs(prefabsBuild, prefabsTurret, prefabsTurretGhost, turretPlatform);

            CallBackLoadInstansingResource(LoadScreeResutType.Done, 1.0f);

        }
        private void MainPlayerCommand(NWGameCommand _gameCommand)
        {
            if (_gameCommand != null)
            {
               gameControllers.ServerSimulator.AddCommand(_gameCommand);
            }
            else
            {
                Console.LogError(ConsoleFilter.MainController, "MainPlayer GameCommand is Null");
            }
        }
        private void GameCommandResult(RezCode _code, string _msg)
        {
            switch (_code)
            {
                //TODO сделать вывод для всех ошибок
                case RezCode.Ok:
                    break;
                default:
                    Console.LogErrorFormat(ConsoleFilter.GameController, "Bad responce from Server {0}, {1}", _code, _msg);
                    break;
            }
        }
        void Update()
        {
            if (commandQueue != null)
            {
                while (commandQueue.Count > 0)
                {
                    Action act = commandQueue.Dequeue();
                    if (act != null)
                    {
                        act();
                    }
                }
            }
        }

        public void Dispose()
        {
			//
			//
        }
    }
}