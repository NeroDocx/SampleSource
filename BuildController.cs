using RobotsRTS.Game;
using RobotsRTS.Helper;
using RobotsRTS.NetWork;
using RobotsRTS.SharedCode;
using System;
using System.Collections.Generic;
using UnityEngine;
namespace RobotsRTS
{
    public class BuildController : IBuildController
    {
        private Dictionary<int, Build> builds;
        private Dictionary<int, Turret> turrets;
        private IDictionary<BuildType, GameObject> buildPrefabs;
        private IDictionary<TurretType, GameObject> turretPrefabs;
        private IDictionary<TurretType, GameObject> turretGhostPrefabs;
        private GameObject turretPlatformPrefab;
        private int idSelectedBuild;

        public BuildController()
        {
            buildPrefabs = new Dictionary<BuildType, GameObject>();
            turretPrefabs = new Dictionary<TurretType, GameObject>();
            builds = new Dictionary<int, Build>(100);
            turrets = new Dictionary<int, Turret>(100);
            idSelectedBuild = int.MinValue;
        }
        #region Interface
        public bool IsDisposed { get; private set; }

        public void SetPrefbs(IDictionary<BuildType, GameObject> _buildPrefabs, IDictionary<TurretType, GameObject> _turretPrefabs, IDictionary<TurretType, GameObject> _turretGhostPrefabs, GameObject _turretPlatformPrefab)
        {
            if (!Util.CollectionIsNullOrEmpty(_buildPrefabs))
            {
                buildPrefabs = _buildPrefabs;
            }
            else
            {
                Console.LogError(ConsoleFilter.BuildController, "build Prefabs Is Null Or Empty");
            }

            if (!Util.CollectionIsNullOrEmpty(_turretPrefabs))
            {
                turretPrefabs = _turretPrefabs;
            }
            else
            {
                Console.LogError(ConsoleFilter.BuildController, "turret Prefabs Is Null Or Empty");
            }
            if (!Util.CollectionIsNullOrEmpty(_turretGhostPrefabs))
            {
                turretGhostPrefabs = _turretGhostPrefabs;
            }
            else
            {
                Console.LogError(ConsoleFilter.BuildController, "turretGhost Prefabs Is Null Or Empty");
            }
            if (_turretPlatformPrefab != null)
            {
                turretPlatformPrefab = _turretPlatformPrefab;
            }
            else
            {
                Console.LogError(ConsoleFilter.BuildController, "turretPlatform Prefab is null");
            }
        }
        public bool AddBuild(NWBuild _build)
        {
            if (_build != null)
            {
                if (builds.ContainsKey(_build.Id))
                {
                    Console.LogWarningFormat(ConsoleFilter.BuildController, "Added build Is Containce, id = {0}", _build.Id);
                    RemoveBuild(_build);
                }
                Build build = null;
                Vector3 position = new Vector3(_build.trasform.position.X, _build.trasform.position.Y, _build.trasform.position.Z);
                string name = (_build.Type == BuildType.Base ? "base_" : "factory_") + _build.Id;
                if (Util.InstancePrefabAndGetMBScript<Build>(buildPrefabs[_build.Type], name, null, position, out build))
                {
                    build.SetInfo(_build);
                    builds.Add(_build.Id, build);
                    Console.LogFormat(ConsoleFilter.BuildController, "New build added and Instance, id = {0}", _build.Id);
                }
                else
                {
                    Console.LogErrorFormat(ConsoleFilter.BuildController, "Can't instance build {0}", _build.Id);
                    return false;
                }

                if (!Util.ArrayIsNullOrEmpty(_build.TurretPlatforms))
                {
                    List<TurretPlatform> turretPlatforms = new List<TurretPlatform>(_build.TurretPlatforms.Length);
                    int id = 0;
                    foreach (NWTurretPlatform sTurretPlatform in _build.TurretPlatforms)
                    {
                        TurretPlatform turretPlatform = null;
                        Vector3 turretPos = new Vector3(sTurretPlatform.Position.X, sTurretPlatform.Position.Y, sTurretPlatform.Position.Z);
                        string turretName = string.Format("TurretPlatform_{0}_{1}", id, name);
                        if (Util.InstancePrefabAndGetMBScript<TurretPlatform>(turretPlatformPrefab, turretName, null, turretPos, out turretPlatform))
                        {
                            turretPlatform.SetIdTurret(id, sTurretPlatform.TurretId, sTurretPlatform.PlatformState);
                            turretPlatforms.Add(turretPlatform);
                            //build.SetInfo(_build);
                            //builds.Add(_build.Id, build);
                            Console.LogFormat(ConsoleFilter.BuildController, "New turret platform added and Instance, id = {0}", _build.Id);
                        }
                        else
                        {
                            Console.LogErrorFormat(ConsoleFilter.BuildController, "Can't instance turret platform {0}", _build.Id);
                            //return false;
                        }
                        ++id;
                    }
                    build.SetTurretPlatforms(turretPlatforms.ToArray());
                }
                return true;
            }
            else
            {
                Console.LogError(ConsoleFilter.BuildController, "Added build Is Null");
            }
            return false;
        }
        public bool UpdateBuild(NWBuild _build)
        {
            IBuild build = null;
            if (TryGetBuild(_build.Id, out build) && build != null)
            {
                build.UpdateInfo(_build);
                //build.transform.position = Util.ConvertRTSStructToVector3(serverBuild.trasform);

                return true;
            }
            return false;
        }
        public bool RemoveBuild(NWBuild _build)
        {
            if (_build != null)
            {
                RemoveBuild(_build.Id);
            }
            else
            {
                Console.LogError(ConsoleFilter.BuildController, "Removed build Is Null");
            }
            return false;
        }
        public bool RemoveBuild(int _buildId)
        {
            if (_buildId >= 0)
            {
                if (builds.ContainsKey(_buildId))
                {
                    UnityEngine.Object.DestroyImmediate(builds[_buildId].gameObject);
                    builds.Remove(_buildId);
                    Console.LogFormat(ConsoleFilter.BuildController, "build delete and DeInstance, id = {0}", _buildId);
                    return true;
                }
                else
                {
                    Console.LogFormat(ConsoleFilter.BuildController, "Removed build is Not Contains, id = {0}", _buildId);
                }
            }
            else
            {
                Console.LogError(ConsoleFilter.BuildController, "Removed build  id < 0");
            }
            return false;
        }

        public IBuild[] GetAllBuilds()
        {
            return null;
        }

        public bool TryGetBuild(int _id, out IBuild _build)
        {
            _build = null;
            if (builds.ContainsKey(_id))
            {
                _build = builds[_id];
            }
            return _build != null;
        }

        public bool Select(int _id)
        {
            idSelectedBuild = _id;
            if (!builds.ContainsKey(idSelectedBuild))
            {
                Console.LogFormat(ConsoleFilter.BuildController, "Selected build is Not Contains, id = {0}", idSelectedBuild);
                return false;
            }
            foreach (Build build in builds.Values)
            {
                if (build.Id != _id)
                {
                    build.transform.localScale = Vector3.one;
                    build.OnDeselect();
                }
                else
                {
                    build.transform.localScale = 2 * Vector3.one;
                    build.OnSelect(idSelectedBuild);
                }
            }
            return true;
        }

        public bool OnDeselect()
        {
            if (!builds.ContainsKey(idSelectedBuild))
            {
                Console.LogFormat(ConsoleFilter.BuildController, "OnDeselected build is Not Contains, id = {0}", idSelectedBuild);
                return false;
            }
            foreach (Build build in builds.Values)
            {
                if (build != null)
                {
                    build.transform.localScale = Vector3.one;
                }
            }
            idSelectedBuild = int.MinValue;
            return true;
        }

        public bool GetSelectedBuild(out IBuild _build)
        {
            _build = null;
            if (idSelectedBuild >= 0)
            {
                if (builds.ContainsKey(idSelectedBuild))
                {
                    _build = builds[idSelectedBuild];
                    return _build != null;
                }
                else
                {
                    Console.LogFormat(ConsoleFilter.BuildController, "GetSelectedBuild build is Not Contains, id = {0}", idSelectedBuild);
                }
            }
            return false;
        }

        public bool AddTurret(NWTurret _turret)
        {
            if (_turret != null)
            {
                if (_turret.Type == TurretType.None)
                {
                    Console.LogErrorFormat(ConsoleFilter.BuildController, "Added turret has incorrect Type {0}", _turret.Type);
                    return false;
                }
                if (turrets.ContainsKey(_turret.Id))
                {
                    Console.LogWarningFormat(ConsoleFilter.BuildController, "Added turret Is Containce, id = {0}", _turret.Id);
                    RemoveTurret(_turret.Id);
                }
                Turret turret = null;
                Vector3 position = new Vector3(_turret.transform.position.X, _turret.transform.position.Y, _turret.transform.position.Z);
                string name = string.Format("Turret_{0}_{1}", _turret.Type, _turret.Id);
                if (Util.InstancePrefabAndGetMBScript<Turret>(turretPrefabs[_turret.Type], name, null, position, out turret))
                {
                    turret.SetInfo(_turret);
                    turrets.Add(_turret.Id, turret);
                    Console.LogFormat(ConsoleFilter.BuildController, "New turret added and Instance, id = {0}", _turret.Id);
                }
                else
                {
                    Console.LogErrorFormat(ConsoleFilter.BuildController, "Can't instance turret {0}", _turret.Id);
                    return false;
                }
                return true;
            }
            else
            {
                Console.LogError(ConsoleFilter.BuildController, "Added turret Is Null");
            }
            return false;
        }

        public bool RemoveTurret(int _idTurret)
        {
            if (turrets.ContainsKey(_idTurret))
            {
                UnityEngine.Object.DestroyImmediate(turrets[_idTurret].gameObject);
                turrets.Remove(_idTurret);
                Console.LogFormat(ConsoleFilter.BuildController, "turret delete and DeInstance, id = {0}", _idTurret);
                return true;
            }
            else
            {
                Console.LogFormat(ConsoleFilter.BuildController, "Removed turret is Not Contains, id = {0}", _idTurret);
            }
            return false;
        }
        public int[] GetAllTurretsId()
        {
            if (!Util.CollectionIsNullOrEmpty(turrets))
            {
                int[] res = new int[turrets.Count];
                turrets.Keys.CopyTo(res, 0);
                return res;
            }
            else
            {
                return new int[0];
            }
        }
        public ITurret[] GetAllTurrets()
        {
            return null;
        }

        public bool TryGetTurret(int _id, out ITurret _turret)
        {
            _turret = null;
            if (turrets.ContainsKey(_id))
            {
                _turret = turrets[_id];
            }
            return _turret != null;
        }
        public bool TryGetTurretGhost(TurretType _type, out GameObject turretPrefab)
        {
            turretPrefab = null;
            if (Util.CollectionIsNullOrEmpty(turretGhostPrefabs))
            {
                Console.LogError(ConsoleFilter.BuildController, "Collection turret Ghost is Empty");
                return false;
            }
            if (_type == TurretType.None)
            {
                Console.LogErrorFormat(ConsoleFilter.BuildController, "Icorrect Turret Type {0}", _type);
                return false;
            }
            if (turretGhostPrefabs.ContainsKey(_type))
            {
                turretPrefab = turretGhostPrefabs[_type];
                return turretPrefab != null;
            }
            else
            {
                Console.LogErrorFormat(ConsoleFilter.BuildController, "Turret Type {0} is not containce in collection turret Ghost", _type);
                return false;
            }
        }
        public void PlayCommand()
        {

        }
        #endregion
        public void Dispose()
        {
            IsDisposed = true;

            if (!Util.CollectionIsNullOrEmpty(builds))
            {
                ICollection<int> Keys = builds.Keys;
                foreach (int val in Keys)
                {
                    RemoveBuild(val);
                }
            }
            if (!Util.CollectionIsNullOrEmpty(turrets))
            {
                ICollection<int> Keys = turrets.Keys;
                foreach (int val in Keys)
                {
                    RemoveTurret(val);
                }
            }

            if (!Util.CollectionIsNullOrEmpty(buildPrefabs))
            {
                buildPrefabs.Clear();
            }

            if (!Util.CollectionIsNullOrEmpty(turretPrefabs))
            {
                turretPrefabs.Clear();
            }

            if (!Util.CollectionIsNullOrEmpty(turretGhostPrefabs))
            {
                turretGhostPrefabs.Clear();
            }

            builds = null;
            turrets = null;
            buildPrefabs = null;
            turretPlatformPrefab = null;
            turretGhostPrefabs = null;
            turretPlatformPrefab = null;
        }
    }
}