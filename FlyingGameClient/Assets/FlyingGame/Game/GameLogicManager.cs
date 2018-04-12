﻿using System;
using System.Collections.Generic;
using UnityEngine;

using Kurisu.Service.Core;
using Kurisu.Game.Map;
using Kurisu.Game.Player;
using Kurisu.Game.Data;
using Kurisu.Game.Entity.Factory;
using SGF;

namespace Kurisu.Game
{
    public class GameLogicManager : ServiceModule<GameLogicManager>
    {

        private GameLogicManager()
        {

        }

        private string LOG_TAG = "GameLogicManager";

        private bool m_isRunning;

        /// <summary>
        /// 游戏的上下文
        /// </summary>
        private GameContext m_context;

        /// <summary>
        /// 游戏地图
        /// </summary>
        private GameMap m_map;

        private List<FlyingPlayer> m_playerList = new List<FlyingPlayer>();

        private DictionaryEx<uint, PlayerData> m_playerDataMap = new DictionaryEx<uint, PlayerData>();

        public event PlayerDieEvent onPlayerDie;

        public event PlayerWinEvent onPlayerWin;

        public event PlayerFailedEvent onPlayerFailed;

        //==============================================================================================================

        public bool IsRunning
        {
            get
            {
                return m_isRunning;
            }
        }

        public GameContext Context
        {
            get
            {
                return m_context;
            }
        }

        public GameMode GameMode
        {
            get
            {
                return m_context.param.mode;
            }
        }

        //==============================================================================================================
        
        public void Init()
        {

        }

        //==============================================================================================================

        public void CreateGame(GameParam param)
        {
            if (m_isRunning)
            {
                this.LogError("CreateGame() Failed, Because Game Ir Running Already!");
                return;
            }

            this.Log("CreateGame() param:{0}", param);

            // 创建上下文，保存战斗全局参数
            m_context = new GameContext();
            m_context.param = param;
            m_context.random.Seed = param.randSeed;
            m_context.curFrameIndex = 0;

            // 创建地图
            m_map = new GameMap();
            m_map.Load(param.mapData);

            // 初始化工厂
            EntityFactory.Init();
            ViewFactory.Init(m_map.View.transform);

            // 初始化摄像机
            GameCamera.Create();

            m_isRunning = true;
        }

        public void ReleaseGame()
        {
            if (!m_isRunning)
            {
                return;
            }

            m_isRunning = false;

            GameCamera.Release();

            foreach (FlyingPlayer player in m_playerList)
            {
                player.Release();
            }
            m_playerList.Clear();

            ViewFactory.Release();
            EntityFactory.Release();

            if (m_map != null)
            {
                m_map.Unload();
                m_map = null;
            }

            onPlayerDie = null;
            onPlayerWin = null;
            onPlayerFailed = null;
        }

        //==============================================================================================================

        public void InputVkey(GameVkey vkey, float arg, uint playerId)
        {
            if (playerId == 0)
            {
                HandleOtherVkey(vkey, arg, playerId);
                return;
            }

            FlyingPlayer player = GetPlayer(playerId);
            if (player != null)
            {
                player.InputVkey(vkey, arg);
            }
            else
            {
                HandleOtherVkey(vkey, arg, playerId);
            }
        }

        public bool HandleOtherVkey(GameVkey vkey, float arg, uint playerId)
        {
            return HandleCreatePlayerVkey(vkey, arg, playerId) 
                   || HandleReleasePlayerVkey(vkey, arg, playerId);
        }

        //==============================================================================================================
        private bool HandleCreatePlayerVkey(GameVkey vkey, float arg, uint playerId)
        {
            if (vkey == GameVkey.CreatePlayer)
            {
                CreatePlayer(playerId);
                return true;
            }

            return false;
        }

        private bool HandleReleasePlayerVkey(GameVkey vkey, float arg, uint playerId)
        {
            if (vkey == GameVkey.ReleasePlayer)
            {
                ReleasePlayer(playerId);
                return true;
            }

            return false;
        }
        //==============================================================================================================

        public void EnterFrame(int frameIndex)
        {
            if (!m_isRunning)
            {
                return;
            }

            if (frameIndex < 0)
            {
                m_context.curFrameIndex++;
            }
            else
            {
                m_context.curFrameIndex = frameIndex;
            }

            // 清理被释放的对象
            EntityFactory.ClearReleasedObjects();

            // 更新玩家动作
            foreach (FlyingPlayer player in m_playerList)
            {
                player.EnterFrame(frameIndex);
            }

            // 更新地图逻辑
            if (m_map != null)
            {
                m_map.EnterFrame(frameIndex);
            }

            // 如果有玩家死亡，调用玩家死亡事件
            if (onPlayerDie != null)
            {
                foreach (FlyingPlayer player in m_playerList)
                {
                    if (player.GameState == PlayerGameState.Death)
                    {
                        onPlayerDie(player.Id);
                    }
                }
            }
        }
        //==============================================================================================================

        public void RegPlayerData(PlayerData data)
        {
            m_playerDataMap[data.id] = data;
        }

        //==============================================================================================================

        internal void CreatePlayer(uint playerId)
        {
            PlayerData data = m_playerDataMap[playerId];
            if (data == null)
                return;

            FlyingPlayer player = new FlyingPlayer();

            player.Create(data, Vector3.zero);

            m_playerList.Add(player);
        }

        private void ReleasePlayer(uint playerId)
        {
            int index = GetPlayerIndex(playerId);
            ReleasePlayerAt(index);
        }

        private void ReleasePlayerAt(int index)
        {
            if (index < 0)
                return;

            FlyingPlayer player = m_playerList[index];
            m_playerList.RemoveAt(index);

            player.Release();
        }

        private FlyingPlayer GetPlayer(uint playerId)
        {
            int index = GetPlayerIndex(playerId);
            if (index >= 0)
            {
                return m_playerList[index];
            }

            return null;
        }

        private int GetPlayerIndex(uint playerId)
        {
            for (int i = 0; i < m_playerList.Count; i++)
            {
                if (m_playerList[i].Id == playerId)
                {
                    return i;
                }
            }

            return -1;
        }

        internal List<FlyingPlayer> GetPlayerList()
        {
            return m_playerList;
        }

        //==============================================================================================================

    }
}
