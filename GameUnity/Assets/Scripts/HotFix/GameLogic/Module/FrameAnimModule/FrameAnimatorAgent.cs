using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using DGame;
using GameProto;
using UnityEngine;

namespace GameLogic
{
    /// <summary>
    /// 场景帧动画播放状态。
    /// </summary>
    public enum FrameAnimState
    {
        /// <summary>
        /// 待机状态。
        /// </summary>
        Idle,
        /// <summary>
        /// 移动状态。
        /// </summary>
        Move,
        /// <summary>
        /// 死亡状态。
        /// </summary>
        Death,
        /// <summary>
        /// 技能1状态。
        /// </summary>
        Skill1,
        /// <summary>
        /// 技能2状态。
        /// </summary>
        Skill2,
        /// <summary>
        /// 受击1状态。
        /// </summary>
        Hurt1,
        /// <summary>
        /// 受击2状态。
        /// </summary>
        Hurt2,
        /// <summary>
        /// 状态数量上限。
        /// </summary>
        Max
    }

    /// <summary>
    /// 场景帧动画参数定义。
    /// </summary>
    public static class FrameAnimParamDefine
    {
        /// <summary>
        /// 移动状态参数。
        /// </summary>
        public static readonly int Moving = Animator.StringToHash("Moving");
        /// <summary>
        /// 技能索引参数。
        /// </summary>
        public static readonly int SkillIndex = Animator.StringToHash("SkillIndex");
        /// <summary>
        /// 受击表现参数。
        /// </summary>
        public static readonly int ImpactId = Animator.StringToHash("ImpactId");
        /// <summary>
        /// 死亡状态参数。
        /// </summary>
        public static readonly int Death = Animator.StringToHash("Death");
        /// <summary>
        /// 移动速度缩放参数。
        /// </summary>
        public static readonly int MoveSpeedScale = Animator.StringToHash("MoveSpeed");
        /// <summary>
        /// 技能速度缩放参数。
        /// </summary>
        public static readonly int SkillSpeedScale = Animator.StringToHash("SkillSpeed");
        /// <summary>
        /// 显示状态参数。
        /// </summary>
        public static readonly int Show = Animator.StringToHash("Show");
        /// <summary>
        /// 受击动画索引参数。
        /// </summary>
        public static readonly int HurtIndex = Animator.StringToHash("HurtIndex");
    }

    /// <summary>
    /// 场景帧动画播放代理。
    /// </summary>
    public sealed class FrameAnimatorAgent : MemoryObject
    {
        #region 字段

        private const float FRAME_INTERVAL = 0.125f; // 1秒8帧
        private const float FRAME_TIMER_INTERVAL = FRAME_INTERVAL * 0.25f * 0.5f; // 提高八倍采样率
        private const float NORMAL_BASE_SPEED = 1.5f; // 1秒12帧
        private const float ELITE_BASE_SPEED = 1.5f; // 1秒12帧

        private static readonly System.Random s_random = new System.Random();

        private GameTimer m_gameTimer;
        private FrameSpritePool m_frameSpritePool;
        private SpriteRenderer m_spriteRenderer;
        private bool m_isInit;
        private FrameAnimState m_curFrameAnimName = FrameAnimState.Idle;
        private FrameAnimState m_changeFrameAnimName = FrameAnimState.Idle;
        private FrameClip[] m_animClips = new FrameClip[(int)FrameAnimState.Max];
        private readonly Dictionary<int, float> m_floatMap = new Dictionary<int, float>();
        private string m_curCfgLocation;
        private bool m_isBindSpriteRenderer;
        private float m_deathSpeed = 1.0f;
        private Vector3 m_modelScale;
        private bool m_isSetFirstFrame;
        private bool m_isUnscaledTime;
        private float m_preFrameTime;

        private float m_speedScale = 1.0f;
        private float m_curBaseSpeed;
        private bool m_isDestroy;

        /// <summary>
        /// 是否有效（未销毁、已初始化、已绑定SpriteRenderer）
        /// </summary>
        public bool IsValid => !m_isDestroy && m_isInit && m_spriteRenderer != null;

        #endregion

        /// <summary>
        /// 创建帧动画代理实例
        /// </summary>
        public static FrameAnimatorAgent Create() => MemoryObject.Spawn<FrameAnimatorAgent>();

        /// <summary>
        /// 初始化帧动画代理，异步加载帧动画资源
        /// </summary>
        /// <param name="modelConfig">模型配置</param>
        public async UniTask Init(ModelConfig modelConfig)
        {
            if (modelConfig == null || string.IsNullOrEmpty(modelConfig.FrameCfgLocation))
            {
                DLogger.Error($"请检查模型配置表，模型配置表为空");
                return;
            }
            m_curCfgLocation = modelConfig.FrameCfgLocation;
            m_frameSpritePool = await FrameSpriteMgr.Instance.GetFrameSpritePool(m_curCfgLocation);
            if (m_frameSpritePool == null)
            {
                DLogger.Error($"没有找到帧动画配置文件: {m_curCfgLocation}");
                return;
            }

            m_modelScale = modelConfig.ModelScale > 0 ? new Vector3(modelConfig.ModelScale, modelConfig.ModelScale, modelConfig.ModelScale) : Vector3.one;
            m_deathSpeed = modelConfig.DeathFrameSpeed > 0 ? modelConfig.DeathFrameSpeed : 1;
            m_curBaseSpeed = NORMAL_BASE_SPEED;
            m_isBindSpriteRenderer = false;

            if (m_isDestroy)
            {
                return;
            }

            m_animClips[(int)FrameAnimState.Idle] = new FrameClip(FrameAnimName.idle,
                m_frameSpritePool.GetSprites(FrameAnimName.idle), IsLoopAnim(FrameAnimState.Idle));
            var moveClip = new FrameClip(FrameAnimName.run,
                m_frameSpritePool.GetSprites(FrameAnimName.run), IsLoopAnim(FrameAnimState.Move));
            moveClip.RandomSetInitIndex(s_random);
            m_animClips[(int)FrameAnimState.Move] = moveClip;
            m_animClips[(int)FrameAnimState.Death] = new FrameClip(FrameAnimName.death,
                m_frameSpritePool.GetSprites(FrameAnimName.death), IsLoopAnim(FrameAnimState.Death));
            m_animClips[(int)FrameAnimState.Skill1] = new FrameClip(FrameAnimName.skill1,
                m_frameSpritePool.GetSprites(FrameAnimName.skill1), IsLoopAnim(FrameAnimState.Skill1));
            m_animClips[(int)FrameAnimState.Skill2] = new FrameClip(FrameAnimName.skill2,
                m_frameSpritePool.GetSprites(FrameAnimName.skill2), IsLoopAnim(FrameAnimState.Skill2));
            m_animClips[(int)FrameAnimState.Hurt1] = new FrameClip(FrameAnimName.hurt1,
                m_frameSpritePool.GetSprites(FrameAnimName.hurt1), IsLoopAnim(FrameAnimState.Hurt1));
            m_animClips[(int)FrameAnimState.Hurt2] = new FrameClip(FrameAnimName.hurt2,
                m_frameSpritePool.GetSprites(FrameAnimName.hurt2), IsLoopAnim(FrameAnimState.Hurt2));
            m_isInit = true;
            SetFirstFrame();
        }

        /// <summary>
        /// 设置是否使用不受时间缩放影响的时间
        /// </summary>
        /// <param name="isUnscaledTime">true=使用UnscaledTime，false=使用普通Time</param>
        public void SetUnscaledTime(bool isUnscaledTime)
        {
            m_isUnscaledTime = isUnscaledTime;
        }

        /// <summary>
        /// 绑定显示用的SpriteRenderer组件
        /// </summary>
        /// <param name="spriteRenderer">SpriteRenderer组件</param>
        public void BindDisplayRender(SpriteRenderer spriteRenderer)
        {
            if (m_isBindSpriteRenderer)
            {
                return;
            }
            m_isBindSpriteRenderer = true;
            m_spriteRenderer = spriteRenderer;
            SetFirstFrame();
        }

        /// <summary>
        /// 绑定显示用的SpriteRenderer组件。
        /// </summary>
        /// <param name="spriteRenderer">SpriteRenderer组件。</param>
        public void BindSpriteRenderer(SpriteRenderer spriteRenderer)
        {
            BindDisplayRender(spriteRenderer);
        }

        private void SetFirstFrame()
        {
            if (!m_isInit)
            {
                if (m_spriteRenderer != null)
                {
                    m_spriteRenderer.sprite = null;
                }
                return;
            }

            if (m_isSetFirstFrame)
            {
                return;
            }

            m_isSetFirstFrame = true;
            m_curFrameAnimName = m_changeFrameAnimName;
            var curClip = m_animClips[(int)m_curFrameAnimName];

            if (curClip == null)
            {
                DLogger.Warning($"没找到动画Clip: {m_curFrameAnimName}");
                return;
            }

            SetSprite(curClip.GetNext());
            SetSpriteRendererSize();
        }

        private void SetSprite(Sprite sprite)
        {
            if (m_isDestroy || !m_isInit || m_spriteRenderer == null || sprite == null)
            {
                return;
            }

            m_spriteRenderer.sprite = sprite;
        }

        /// <summary>
        /// 开始播放帧动画
        /// </summary>
        public void StartAnim()
        {
            if (!IsValid)
            {
                return;
            }

            if (m_isUnscaledTime)
            {
                if (GameTimer.IsNull(m_gameTimer))
                {
                    m_gameTimer = GameModule.GameTimerModule.CreateUnscaledLoopGameTimer(FRAME_TIMER_INTERVAL, Update);
                }
            }
            else
            {
                if (GameTimer.IsNull(m_gameTimer))
                {
                    m_gameTimer = GameModule.GameTimerModule.CreateLoopGameTimer(FRAME_TIMER_INTERVAL, Update);
                }
            }
        }

        private void Update(object[] args)
        {
            if (!IsValid)
            {
                return;
            }

            var curClip = m_animClips[(int)m_curFrameAnimName];

            if (curClip == null)
            {
                return;
            }

            if (curClip.IsStop())
            {
                return;
            }

            float gameTime = m_isUnscaledTime ? GameTime.UnscaledTime : GameTime.Time;
            var deltaTime = gameTime - m_preFrameTime;

            if (deltaTime * GetSpeed() > FRAME_INTERVAL)
            {
                SetSprite(curClip.GetNext());
                SetSpriteRendererSize();
                m_preFrameTime = gameTime;
            }
        }

        private void SetSpriteRendererSize(bool revert = false)
        {
            if (m_spriteRenderer == null)
            {
                return;
            }

            m_spriteRenderer.transform.localScale = revert ? Vector3.one : m_modelScale;
        }

        /// <summary>
        /// 获取当前动画播放速度
        /// </summary>
        /// <returns>当前动画播放速度。</returns>
        public float GetSpeed()
        {
            if (m_curFrameAnimName == FrameAnimState.Move)
            {
                return m_curBaseSpeed;
            }

            if (m_curFrameAnimName == FrameAnimState.Death)
            {
                return m_deathSpeed;
            }

            return m_speedScale * m_curBaseSpeed;
        }

        /// <summary>
        /// 切换动画状态
        /// </summary>
        /// <param name="animName">目标动画状态</param>
        public void SwitchAnim(FrameAnimState animName)
        {
            if (!IsValid)
            {
                m_changeFrameAnimName = animName;
                return;
            }

            var oldAnimName = m_curFrameAnimName;
            if (animName != oldAnimName)
            {
                m_curFrameAnimName = animName;
                var oldClip = m_animClips[(int)oldAnimName];
                oldClip?.Leave();
            }
        }

        /// <summary>
        /// 根据布尔参数切换动画状态。
        /// </summary>
        /// <param name="id">动画参数ID。</param>
        /// <param name="value">参数值。</param>
        public void SwitchAnim(int id, bool value)
        {
            var state = FrameAnimState.Idle;
            if (id == FrameAnimParamDefine.Death)
            {
                if (value)
                {
                    state = FrameAnimState.Death;
                }
            }
            else if (id == FrameAnimParamDefine.Show)
            {
                if (value)
                {
                    state = FrameAnimState.Idle;
                }
            }
            else if (id == FrameAnimParamDefine.Moving)
            {
                if (value)
                {
                    state = FrameAnimState.Move;
                }
            }

            SwitchAnim(state);
        }

        /// <summary>
        /// 根据整型参数切换动画状态。
        /// </summary>
        /// <param name="id">动画参数ID。</param>
        /// <param name="value">参数值。</param>
        public void SwitchAnim(int id, int value)
        {
            var state = FrameAnimState.Idle;
            if (id == FrameAnimParamDefine.SkillIndex)
            {
                switch (value)
                {
                    case 1:
                        state = FrameAnimState.Skill1;
                        break;
                    case 2:
                        state = FrameAnimState.Skill2;
                        break;
                    default:
                        state = FrameAnimState.Idle;
                        break;
                }
            }
            else if (id == FrameAnimParamDefine.HurtIndex)
            {
                switch (value)
                {
                    case 1:
                        state = FrameAnimState.Hurt1;
                        break;
                    case 2:
                        state = FrameAnimState.Hurt2;
                        break;
                    default:
                        state = FrameAnimState.Idle;
                        break;
                }
            }
            else if (id == FrameAnimParamDefine.ImpactId)
            {
                state = m_isInit ? m_curFrameAnimName : m_changeFrameAnimName;
            }

            SwitchAnim(state);
        }

        /// <summary>
        /// 判断指定动画是否循环播放
        /// </summary>
        /// <param name="animName">动画状态</param>
        /// <returns>true=循环播放</returns>
        public bool IsLoopAnim(FrameAnimState animName)
            => animName == FrameAnimState.Idle || animName == FrameAnimState.Move;

        /// <summary>
        /// 设置动画播放速度缩放
        /// </summary>
        /// <param name="speed">速度缩放倍数</param>
        public void SetAnimSpeed(float speed)
        {
            m_speedScale = speed;
        }

        /// <summary>
        /// 设置全部动画播放速度缩放。
        /// </summary>
        /// <param name="speed">速度缩放倍数。</param>
        public void SetAllAnimSpeed(float speed)
        {
            SetAnimSpeed(speed);
        }

        /// <summary>
        /// 设置浮点类型动画参数。
        /// </summary>
        /// <param name="id">动画参数ID。</param>
        /// <param name="value">参数值。</param>
        public void SetFloat(int id, float value)
        {
            if (m_floatMap.ContainsKey(id))
            {
                m_floatMap[id] = value;
            }
            else
            {
                m_floatMap.Add(id, value);
            }
        }

        /// <summary>
        /// 设置是否使用不受时间缩放影响的时间。
        /// </summary>
        /// <param name="isUnScale">true=使用UnscaledTime，false=使用普通Time。</param>
        public void SetUnScale(bool isUnScale)
        {
            SetUnscaledTime(isUnScale);
        }

        #region 释放资源

        /// <summary>
        /// 主动释放
        /// </summary>
        public void Release()
        {
            MemoryObject.Release(this);
        }

        /// <summary>
        /// 释放资源回调
        /// </summary>
        public override void OnRelease()
        {
            GameModule.GameTimerModule.DestroyGameTimer(m_gameTimer);
            m_isInit = false;
            m_isDestroy = true;
            m_frameSpritePool = null;
            SetSpriteRendererSize(true);
            if (m_spriteRenderer != null)
            {
                m_spriteRenderer.sprite = null;
            }
            m_spriteRenderer = null;
            m_curFrameAnimName = FrameAnimState.Idle;
            m_changeFrameAnimName = FrameAnimState.Idle;
            m_curCfgLocation = string.Empty;
            m_isBindSpriteRenderer = false;
            m_deathSpeed = 1.0f;
            m_modelScale = Vector3.one;
            m_isSetFirstFrame = false;
            m_isUnscaledTime = false;
            m_preFrameTime = 0;
            m_speedScale = 1.0f;
            m_floatMap.Clear();

            for (int i = 0; i < m_animClips.Length; i++)
            {
                m_animClips[i]?.OnDestroy();
                m_animClips[i] = null;
            }
        }

        #endregion
    }
}