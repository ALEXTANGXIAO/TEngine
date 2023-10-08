using System.Collections.Generic;
using TEngine;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace GameLogic
{
    //定义的顺序跟游戏中实际的顺序是一致的（从左到右依次是商店、英雄、战斗、天赋、活动(玩法)...等）
    public enum GameMainButton
    {
        //商店
        BtnShop = 0,

        //英雄
        BtnHero,

        //主界面
        BtnFight,

        ////天赋
        //BtnTalent,
        //主角
        BtnZhuJue,

        //活动(玩法)
        BtnActivity,

        BtnMax,
    }

    [Window(UILayer.UI, fullScreen: true)]
    public class GameMainUI : UIWindow
    {
        #region 声明

        private int m_curIndex; //当前选择的按钮索引
        private int m_lastIndex; //上一次选择的按钮索引
        private const float m_aniTime = 0.2f; //按钮上下移动时间
        private const int m_checkThesHold = 100; //拖拽阈值
        private UICenterOnChild m_centerOnChild = new UICenterOnChild();
        private Dictionary<GameMainButton, GameObject> m_btnDic = new Dictionary<GameMainButton, GameObject>();
        private Dictionary<GameMainButton, Image> m_btnImgDic = new Dictionary<GameMainButton, Image>();
        private Dictionary<GameMainButton, TextMeshProUGUI> m_btnTextDic = new Dictionary<GameMainButton, TextMeshProUGUI>();
        private Dictionary<GameMainButton, TextMeshProUGUI> m_btnActiveTextDic = new Dictionary<GameMainButton, TextMeshProUGUI>();
        private Dictionary<GameMainButton, Image> m_btnLockDic = new Dictionary<GameMainButton, Image>();
        private Dictionary<GameMainButton, Transform> m_btnRedPointDict = new Dictionary<GameMainButton, Transform>();

        private Vector2 m_tempV2 = new Vector2();
        private float m_curScaleRatio = 0.9f;
        private float m_lastScaleRatio = 0.8f;
        //private HomeChatInfoBlock m_homeChat;

        private float m_cellSize;

        #endregion

        private Dictionary<GameMainButton, HomeChildPage> m_dictHomeChilds = new Dictionary<GameMainButton, HomeChildPage>();
        private List<HomeChildPage> m_listHomeChilds = new List<HomeChildPage>();
        private GridLayoutGroup m_gridContent;
        private Image m_imgActivityTips;
        private Text m_textActivityTips;

        #region 脚本工具生成的代码

        private RectTransform m_rectInfo;
        private ScrollRect m_scrollView;
        private RectTransform m_rectContent;
        private Image m_imgCheck;
        private GridLayoutGroup m_rectbtns;
        private Button m_btnShop;
        private Transform m_tfShopRed;
        private Button m_btnHero;
        private Transform m_tfHeroRed;
        private Button m_btnFight;
        private Button m_btnZhuJue;
        private Transform m_tfZhuJueRed;
        private Button m_btnActivity;
        private Transform m_tfActivityRed;
        private GameObject m_goMask;
        private Button m_btnGm;
        private Button m_btnhomeChatMsg;
        private GameObject m_goChat;
        private Image m_dimgType;
        private Text m_textChannlType;
        private Image m_dimgSex;
        private GameObject m_goEmpty;
        private Image m_imgbkg;

        public override void ScriptGenerator()
        {
            m_imgbkg = FindChildComponent<Image>("m_imgbkg");
            m_rectInfo = FindChildComponent<RectTransform>("TopLayout/m_rectInfo");
            m_scrollView = FindChildComponent<ScrollRect>("TopLayout/m_rectInfo/m_scrollView");
            m_rectContent = FindChildComponent<RectTransform>("TopLayout/m_rectInfo/m_scrollView/Viewport/m_rectContent");
            m_imgCheck = FindChildComponent<Image>("TopLayout/m_rectInfo/bottom/m_rectbtns/m_imgCheck");
            m_rectbtns = FindChildComponent<GridLayoutGroup>("TopLayout/m_rectInfo/bottom/m_rectbtns");
            m_btnShop = FindChildComponent<Button>("TopLayout/m_rectInfo/bottom/m_rectbtns/m_btnShop");

            m_tfShopRed = FindChild("TopLayout/m_rectInfo/bottom/m_rectbtns/m_btnShop/m_tfShopRed");
            m_btnHero = FindChildComponent<Button>("TopLayout/m_rectInfo/bottom/m_rectbtns/m_btnHero");
            m_tfHeroRed = FindChild("TopLayout/m_rectInfo/bottom/m_rectbtns/m_btnHero/m_tfHeroRed");
            m_btnFight = FindChildComponent<Button>("TopLayout/m_rectInfo/bottom/m_rectbtns/m_btnFight");
            m_btnActivity = FindChildComponent<Button>("TopLayout/m_rectInfo/bottom/m_rectbtns/m_btnActivity");
            m_tfActivityRed = FindChild("TopLayout/m_rectInfo/bottom/m_rectbtns/m_btnActivity/m_tfActivityRed");
            m_btnZhuJue = FindChildComponent<Button>("TopLayout/m_rectInfo/bottom/m_rectbtns/m_btnZhuJue");
            m_tfZhuJueRed = FindChild("TopLayout/m_rectInfo/bottom/m_rectbtns/m_btnZhuJue/m_tfZhuJueRed");
            m_goMask = FindChild("TopLayout/m_rectInfo/m_goMask").gameObject;
            m_btnGm = FindChildComponent<Button>("TopLayout/m_rectInfo/m_btnGm");
            m_btnShop.onClick.AddListener(OnClickShopBtn);
            m_btnHero.onClick.AddListener(OnClickHeroBtn);
            m_btnFight.onClick.AddListener(OnClickFightBtn);
            m_btnZhuJue.onClick.AddListener(OnClickZhuJueBtn);
            m_btnActivity.onClick.AddListener(OnClickActivityBtn);
        }

        #endregion

        public override void BindMemberProperty()
        {
            m_gridContent = m_rectContent.GetComponent<GridLayoutGroup>();

            //商店
            m_btnDic.Add(GameMainButton.BtnShop, m_btnShop.gameObject);
            m_btnImgDic.Add(GameMainButton.BtnShop, FindChildComponent<Image>(m_btnShop.transform, "m_imgIcon"));
            m_btnTextDic.Add(GameMainButton.BtnShop, FindChildComponent<TextMeshProUGUI>(m_btnShop.transform, "m_textName"));
            m_btnLockDic.Add(GameMainButton.BtnShop, FindChildComponent<Image>(m_btnShop.transform, "m_imgLock"));
            m_btnRedPointDict.Add(GameMainButton.BtnShop, m_tfShopRed);
            m_btnActiveTextDic.Add(GameMainButton.BtnShop, FindChildComponent<TextMeshProUGUI>(m_btnShop.transform, "m_textActiveName"));
            // m_shopRedEffGo = CreateUIEffect((uint)CommonEffectID.EffectRedPoint, m_tfShopRed);

            //英雄
            m_btnDic.Add(GameMainButton.BtnHero, m_btnHero.gameObject);
            m_btnImgDic.Add(GameMainButton.BtnHero, FindChildComponent<Image>(m_btnHero.transform, "m_imgIcon"));
            m_btnTextDic.Add(GameMainButton.BtnHero, FindChildComponent<TextMeshProUGUI>(m_btnHero.transform, "m_textName"));
            m_btnLockDic.Add(GameMainButton.BtnHero, FindChildComponent<Image>(m_btnHero.transform, "m_imgLock"));
            m_btnActiveTextDic.Add(GameMainButton.BtnHero, FindChildComponent<TextMeshProUGUI>(m_btnHero.transform, "m_textActiveName"));
            m_btnRedPointDict.Add(GameMainButton.BtnHero, m_tfHeroRed);
            // m_heroRedEffGo = CreateUIEffect((uint)CommonEffectID.EffectRedPoint, m_tfHeroRed);

            //战斗
            m_btnDic.Add(GameMainButton.BtnFight, m_btnFight.gameObject);
            m_btnImgDic.Add(GameMainButton.BtnFight, FindChildComponent<Image>(m_btnFight.transform, "m_imgIcon"));
            m_btnTextDic.Add(GameMainButton.BtnFight, FindChildComponent<TextMeshProUGUI>(m_btnFight.transform, "m_textName"));
            m_btnLockDic.Add(GameMainButton.BtnFight, FindChildComponent<Image>(m_btnFight.transform, "m_imgLock"));
            m_btnActiveTextDic.Add(GameMainButton.BtnFight, FindChildComponent<TextMeshProUGUI>(m_btnFight.transform, "m_textActiveName"));

            ////天赋
            //m_btnDic.Add(GameMainButton.BtnTalent, m_btnTalent.gameObject);
            //m_btnImgDic.Add(GameMainButton.BtnTalent, FindChildComponent<Image>(m_btnTalent.transform, "m_imgIcon"));
            //m_btnTextDic.Add(GameMainButton.BtnTalent, FindChildComponent<TextMeshProUGUI>(m_btnTalent.transform, "m_textName"));
            //m_btnLockDic.Add(GameMainButton.BtnTalent, FindChildComponent<Image>(m_btnTalent.transform, "m_imgLock"));
            //m_btnRedPointDict.Add(GameMainButton.BtnTalent, m_tfTalentRed);
            //m_talentRedEffGo = CreateUIEffect((uint)CommonEffectID.EffectRedPoint, m_tfTalentRed);

            //主角
            m_btnDic.Add(GameMainButton.BtnZhuJue, m_btnZhuJue.gameObject);
            m_btnImgDic.Add(GameMainButton.BtnZhuJue, FindChildComponent<Image>(m_btnZhuJue.transform, "m_imgIcon"));
            m_btnTextDic.Add(GameMainButton.BtnZhuJue, FindChildComponent<TextMeshProUGUI>(m_btnZhuJue.transform, "m_textName"));
            m_btnLockDic.Add(GameMainButton.BtnZhuJue, FindChildComponent<Image>(m_btnZhuJue.transform, "m_imgLock"));
            m_btnActiveTextDic.Add(GameMainButton.BtnZhuJue, FindChildComponent<TextMeshProUGUI>(m_btnZhuJue.transform, "m_textActiveName"));
            m_btnRedPointDict.Add(GameMainButton.BtnZhuJue, m_tfZhuJueRed);
            // m_zhujueRedEffGo = CreateUIEffect((uint)CommonEffectID.EffectRedPoint, m_tfZhuJueRed);

            //活动
            m_btnDic.Add(GameMainButton.BtnActivity, m_btnActivity.gameObject);
            m_btnImgDic.Add(GameMainButton.BtnActivity, FindChildComponent<Image>(m_btnActivity.transform, "m_imgIcon"));
            m_btnTextDic.Add(GameMainButton.BtnActivity, FindChildComponent<TextMeshProUGUI>(m_btnActivity.transform, "m_textName"));
            m_btnLockDic.Add(GameMainButton.BtnActivity, FindChildComponent<Image>(m_btnActivity.transform, "m_imgLock"));
            m_btnActiveTextDic.Add(GameMainButton.BtnActivity, FindChildComponent<TextMeshProUGUI>(m_btnActivity.transform, "m_textActiveName"));
            m_btnRedPointDict.Add(GameMainButton.BtnActivity, m_tfActivityRed);
            // m_activeRedEffGo = CreateUIEffect((uint)CommonEffectID.EffectRedPoint, m_tfActivityRed);

            // //头顶的信息栏位
            // CreateWidgetByType<CurrencyDisplayBlock>(transform, "TopLayout/m_rectInfo");

            m_imgActivityTips = FindChildComponent<Image>(m_btnActivity.transform, "m_imgActivityTips");
            if (m_imgActivityTips != null)
            {
                m_textActivityTips = FindChildComponent<Text>(m_imgActivityTips.transform, "m_textActivityTips");
                if (m_textActivityTips != null)
                {
                    // m_textActivityTips.text = TextConfigMgr.Instance.GetText(TextDefine.ID_LABEL_WORLD_BOOS_ACTIVITY_OPEN_TIPS);
                }

                m_imgActivityTips.gameObject.SetActive(false);
            }

            ChangeBtnStyle();
        }

        public override void OnCreate()
        {
            //适配
            InitGridContent();

            m_centerOnChild.Init(m_scrollView, m_checkThesHold);
            m_centerOnChild.SetElasticity(0.1f);

            //设置拖拽回调函数
            m_centerOnChild.SetOnDragCallBack(OnDrag);
            m_centerOnChild.SetCallback(OnEndDrag);
            m_centerOnChild.SetEndCallBack(OnEndMovingDrag);
            CheckHomeOpen();

            //设置按钮的状态
            // ShowBtnState();

            //设置按钮图标的初始状态
            // SetBtnImgScale();

            bool showGm = false;
// #if UNITY_EDITOR
//             showGm = true;
// #endif

            m_btnGm.gameObject.SetActive(showGm);
        }

        private void CheckHomeOpen()
        {
            GameMainButton curSelectType = GameMainButton.BtnFight;
            if (m_curIndex >= 0 && m_curIndex < m_listHomeChilds.Count)
            {
                curSelectType = m_listHomeChilds[m_curIndex].type;
            }

            bool chg = false;

            //商城
            if (!m_dictHomeChilds.ContainsKey(GameMainButton.BtnShop))
            {
                var child = CreateWidgetByType<ShopPage>(m_rectContent);
                child.type = GameMainButton.BtnShop;
                m_dictHomeChilds.Add(GameMainButton.BtnShop, child);
                m_listHomeChilds.Add(child);
                chg = true;
            }

            //英雄
            if (!m_dictHomeChilds.ContainsKey(GameMainButton.BtnHero))
            {
                var child = CreateWidgetByType<HeroPage>(m_rectContent);
                child.type = GameMainButton.BtnHero;
                m_dictHomeChilds.Add(GameMainButton.BtnHero, child);
                m_listHomeChilds.Add(child);
                chg = true;
            }

            //战斗
            if (!m_dictHomeChilds.ContainsKey(GameMainButton.BtnFight))
            {
                var child = CreateWidgetByType<MainLevelPage>(m_rectContent);
                child.type = GameMainButton.BtnFight;
                m_dictHomeChilds.Add(GameMainButton.BtnFight, child);
                m_listHomeChilds.Add(child);
                chg = true;
            }

            //主角
            if (!m_dictHomeChilds.ContainsKey(GameMainButton.BtnZhuJue))
            {
                var child = CreateWidgetByType<ZhuJuePage>(m_rectContent);
                child.type = GameMainButton.BtnZhuJue;
                m_dictHomeChilds.Add(GameMainButton.BtnZhuJue, child);
                m_listHomeChilds.Add(child);
                chg = true;
            }

            //活动(玩法)
            if (!m_dictHomeChilds.ContainsKey(GameMainButton.BtnActivity))
            {
                var child = CreateWidgetByType<ChallengePage>(m_rectContent);
                child.type = GameMainButton.BtnActivity;
                m_dictHomeChilds.Add(GameMainButton.BtnActivity, child);
                m_listHomeChilds.Add(child);
                chg = true;
            }


            if (chg)
            {
                m_listHomeChilds.Sort(delegate(HomeChildPage lhs, HomeChildPage rhs) { return lhs.type - rhs.type; });
                for (int i = 0; i < m_listHomeChilds.Count; i++)
                {
                    m_listHomeChilds[i].Index = i;
                    m_listHomeChilds[i].rectTransform.SetSiblingIndex(i);
                    m_listHomeChilds[i].OnActivePage(m_listHomeChilds[i].type == curSelectType);
                }

                int idx = GetGameMainIdx(curSelectType);
                m_curIndex = idx;
                m_lastIndex = idx;

                //默认显示进入游戏界面
                m_centerOnChild.CenterOnChild(m_curIndex, true);
            }
        }

        void InitGridContent()
        {
            var thisRect = this.rectTransform;
            m_tempV2.x = m_rectInfo.rect.width;
            m_tempV2.y = m_rectInfo.rect.height;
            m_gridContent.cellSize = m_tempV2;
            m_gridContent.SetLayoutHorizontal();
            m_gridContent.SetLayoutVertical();

            var rectbtns = m_rectbtns.transform as RectTransform;
            m_cellSize = rectbtns.rect.width / (int)GameMainButton.BtnMax;
            m_rectbtns.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            m_rectbtns.constraintCount = (int)GameMainButton.BtnMax;
            m_rectbtns.cellSize = new Vector2(m_cellSize, m_rectbtns.cellSize.y);
            m_rectbtns.SetLayoutHorizontal();
            m_rectbtns.SetLayoutVertical();
            var Checkrect = m_imgCheck.transform as RectTransform;
            Checkrect.sizeDelta = new Vector2(m_cellSize + 30f, Checkrect.rect.height);
        }


        private void ChangeBtnStyle()
        {
            // int[] systemIds = new[]
            // {
            //     (int)FuncType.FUNC_TYPE_MAIN_SHOP,
            //     (int)FuncType.FUNC_TYPE_HERO,
            //     (int)FuncType.FUNC_TYPE_FIGHT,
            //     (int)FuncType.FUNC_TYPE_ZHUJUE,
            //     (int)FuncType.FUNC_TYPE_MAIN_ACTIVITY,
            // };

            GameMainButton[] types = new[]
            {
                //商店
                GameMainButton.BtnShop,
                //卡牌
                GameMainButton.BtnHero,
                //主界面
                GameMainButton.BtnFight,
                //主角
                GameMainButton.BtnZhuJue,
                //挑战(玩法)
                GameMainButton.BtnActivity,
            };
            // FuncOpenConfig cfg;
            // Image img;
            // TextMeshProUGUI txt;
            // GameMainButton key;
            // int id;
            // var actTxt = GetActiveTextNameByType(key);
            // for (int i = 0; i < types.Length; i++)
            // {
            //     id = systemIds[i];
            //     key = types[i];
            //     cfg = FuncOpenCfgMgr.Instance.GetFuncOpenCfg((uint)id);
            //     if (cfg == null)
            //     {
            //         continue;
            //     }
            //     img = GetIconImageByType(key);
            //     if (img != null)
            //     {
            //         img.SetSprite( cfg.FuncIcon);
            //     }
            //
            //     txt = GetTextNameByType(key);
            //     if (txt != null)
            //     {
            //         txt.text = cfg.FuncName;
            //     }
            //
            //     if (actTxt != null)
            //     {
            //         actTxt.text = cfg.FuncName;
            //     }
            // }
        }

        #region 通用获取界面接口

        //根据按钮类型获取对应按钮的gameObject
        private GameObject GetBtnGoByType(GameMainButton buttonType)
        {
            return m_btnDic.TryGetValue(buttonType, out var targetGo) ? targetGo : null;
        }

        //根据按钮类型获取对应Icon
        private Image GetIconImageByType(GameMainButton buttonType)
        {
            return m_btnImgDic.TryGetValue(buttonType, out var targetImage) ? targetImage : null;
        }

        //根据按钮类型获取对应text
        private TextMeshProUGUI GetTextNameByType(GameMainButton buttonType)
        {
            return m_btnTextDic.TryGetValue(buttonType, out var targetText) ? targetText : null;
        }

        //根据按钮类型获取对应text
        private TextMeshProUGUI GetActiveTextNameByType(GameMainButton buttonType)
        {
            return m_btnActiveTextDic.TryGetValue(buttonType, out var targetText) ? targetText : null;
        }

        //获取按钮解锁状态
        private bool GetBtnLockStateByType(GameMainButton buttonType)
        {
            // switch (buttonType)
            // {
            //     case GameMainButton.BtnHero:
            //         return FuncOpenMgr.Instance.CheckFuncOpen(FuncType.FUNC_TYPE_HERO);
            //     case GameMainButton.BtnShop:
            //         return FuncOpenMgr.Instance.CheckFuncOpen(FuncType.FUNC_TYPE_MAIN_SHOP);
            //     case GameMainButton.BtnFight:
            //         return FuncOpenMgr.Instance.CheckFuncOpen(FuncType.FUNC_TYPE_FIGHT);
            //     case GameMainButton.BtnActivity:
            //         return FuncOpenMgr.Instance.CheckFuncOpen(FuncType.FUNC_TYPE_MAIN_ACTIVITY);
            //     case GameMainButton.BtnZhuJue:
            //         return FuncOpenMgr.Instance.CheckFuncOpen(FuncType.FUNC_TYPE_ZHUJUE);
            // }
            return true;

            return false;
        }

        public HomeChildPage GetChildPage(GameMainButton btnType)
        {
            if (m_dictHomeChilds.TryGetValue(btnType, out var ret))
            {
                return ret;
            }

            return null;
        }

        private int GetGameMainIdx(GameMainButton btnType)
        {
            var child = GetChildPage(btnType);
            if (child != null)
            {
                return child.Index;
            }

            return 0;
        }

        #endregion

        public bool IsOnMainLevelPage()
        {
            bool isBtnFight = m_curIndex != GetGameMainIdx(GameMainButton.BtnFight);
            OnClickFightBtn();
            return isBtnFight;
        }

        public override void OnUpdate()
        {
            m_centerOnChild.Update();
        }

        //商店
        private void OnClickShopBtn()
        {
            int idx = GetGameMainIdx(GameMainButton.BtnShop);
            if (m_curIndex != idx)
            {
                m_centerOnChild.CenterOnChild(idx, false);
            }
        }

        //英雄
        private void OnClickHeroBtn()
        {
            int idx = GetGameMainIdx(GameMainButton.BtnHero);
            if (m_curIndex != idx)
            {
                m_centerOnChild.CenterOnChild(idx, false);
            }

            // 调到英雄界面的时候，重新刷一遍出战英雄特效
            // GameEvent.Get<IHeroUI>().OnFightHeroEffectRefresh();
        }

        //战斗(主界面)
        private void OnClickFightBtn()
        {
            int idx = GetGameMainIdx(GameMainButton.BtnFight);
            if (m_curIndex != idx)
            {
                m_centerOnChild.CenterOnChild(idx, false);
            }
        }

        //活动(玩法)
        private void OnClickActivityBtn()
        {
            if (m_imgActivityTips != null)
            {
                m_imgActivityTips.gameObject.SetActive(false);
            }

            int idx = GetGameMainIdx(GameMainButton.BtnActivity);
            if (m_curIndex != idx)
            {
                m_centerOnChild.CenterOnChild(idx, false);
            }
        }

        //点击天赋
        private void OnClickZhuJueBtn()
        {
            int idx = GetGameMainIdx(GameMainButton.BtnZhuJue);
            if (m_curIndex != idx)
            {
                m_centerOnChild.CenterOnChild(idx, false);
            }
        }

        #region 拖拽事件

        private float _lastX = 0f;

        private const float DynamicsX = 0.3f;

        //拖拽中
        private void OnDrag(int curDragIndex)
        {
            if (curDragIndex >= m_listHomeChilds.Count)
            {
                Log.Error("DragIndex Error : OnDrag exceed Count / curDragIndex:{0} ,m_listHomeChilds.Count{1}", curDragIndex, m_listHomeChilds.Count);
                return;
            }

            var btnType = m_listHomeChilds[curDragIndex].type;
            float offset = m_rectContent.anchoredPosition.x + curDragIndex * m_tempV2.x;
            float x = m_cellSize * (int)btnType - (offset / m_cellSize) * m_cellSize;

            if (_lastX != 0)
            {
                //var Gap = (x - m_LastX)* (m_dynamicsX - (int)GameMainButton.BtnMax / 10);
                var Gap = (x - _lastX) * DynamicsX;
                var index = curDragIndex;
                if (Gap > 0)
                {
                    index = Mathf.Min(++curDragIndex, (int)GameMainButton.BtnZhuJue);
                }
                else
                {
                    index = Mathf.Max(--curDragIndex, (int)GameMainButton.BtnShop);
                }

                if (m_listHomeChilds.Count <= index)
                {
                    return;
                }

                m_listHomeChilds[index].OnActivePage(true);
                m_imgCheck.transform.localPosition = new Vector2(m_imgCheck.transform.localPosition.x + Gap, m_imgCheck.transform.localPosition.y);
            }

            _lastX = x;
        }

        private void OnEndMovingDrag(int curDragIndex)
        {
            var btnType = m_listHomeChilds[curDragIndex].type;
            GameObject btnGo = GetBtnGoByType(btnType);
            LeanTweenType tweenType = LeanTweenType.easeOutBack;
            if (curDragIndex == m_lastIndex)
            {
                tweenType = LeanTweenType.notUsed;
            }

            if (_lastX != 0 || curDragIndex != m_lastIndex)
            {
                LeanTween.moveX(m_imgCheck.gameObject, btnGo.transform.position.x, m_aniTime + 0.1f).setEase(tweenType).setOnComplete(
                    () =>
                    {
                        for (int i = 0; i < m_listHomeChilds.Count; i++)
                        {
                            m_listHomeChilds[i].OnActivePage(i == curDragIndex);
                        }
                    });
            }

            _lastX = 0f;

            // if (curDragIndex == (int)GameMainButton.BtnZhuJue)
            // {
            //     GameEvent.Get<ICommUI>().SetMainPageDisplayBlock(
            //         TopDisplayItemType.TOP_DIS_ITEM_TYPE_TALENT,
            //         TopDisplayItemType.TOP_DIS_ITEM_TYPE_GOLD,
            //         TopDisplayItemType.TOP_DIS_ITEM_TYPE_DIAMOND);
            // }
            // else
            // {
            //     GameEvent.Get<ICommUI>().SetMainPageDisplayBlock(
            //         TopDisplayItemType.TOP_DIS_ITEM_TYPE_POWER,
            //         TopDisplayItemType.TOP_DIS_ITEM_TYPE_GOLD,
            //         TopDisplayItemType.TOP_DIS_ITEM_TYPE_DIAMOND);
            // }

            // //播放下音效
            // if (!m_isInitEnter)
            // {
            //     AudioSys.GameMgr.PlayUISoundEffect(SysEffectSoundID.SysSoundHuaDongYinXiao);
            // }
            // else
            // {
            //     m_isInitEnter = false;
            // }
        }


        //拖拽结束,计算各个按钮的位置
        private void OnEndDrag(int curDragIndex)
        {
            if (curDragIndex >= m_listHomeChilds.Count)
            {
                Log.Error("DragIndex Error : OnEndDrag exceed Count / curDragIndex:{0} ,m_listHomeChilds.Count{1}", curDragIndex, m_listHomeChilds.Count);
                return;
            }

            m_lastIndex = m_curIndex;
            m_curIndex = curDragIndex;

            #region 回调下page界面

            for (int i = 0; i < m_listHomeChilds.Count; i++)
            {
                m_listHomeChilds[i].OnCenterOn(i == m_curIndex);
            }

            #endregion

            bool isLeftToRight = m_curIndex >= m_lastIndex;
            var btnType = m_listHomeChilds[m_curIndex].type;
            var oldBtnType = m_listHomeChilds[m_lastIndex].type;
            m_listHomeChilds[m_curIndex].OnActivePage(true);
            // m_homeChat.gameObject.SetActive(btnType == GameMainButton.BtnFight);

            Image curImg = GetIconImageByType(btnType);
            GameObject imgGo = curImg.gameObject;

            if (isLeftToRight)
            {
                for (var i = oldBtnType; i < btnType; i++)
                {
                    Image lastImg = GetIconImageByType(i);
                    GameObject lastImgGo = lastImg.gameObject;

                    //LeanTween.moveLocal(lastImgGo, new Vector2(imgGo.transform.localPosition.x, 0), m_aniTime);
                    if (GetBtnLockStateByType(i))
                    {
                        LeanTween.scale(lastImgGo, new Vector3(m_lastScaleRatio, m_lastScaleRatio, m_lastScaleRatio), m_aniTime);
                        GetTextNameByType(i).gameObject.SetActive(true);
                        GetActiveTextNameByType(i).gameObject.SetActive(false);
                    }

                    lastImg.color = new Color(255, 255, 255, 204);
                }
            }
            else
            {
                for (var i = oldBtnType; i > btnType; i--)
                {
                    Image lastImg = GetIconImageByType(i);
                    GameObject lastImgGo = lastImg.gameObject;

                    //LeanTween.moveLocal(lastImgGo, new Vector2(imgGo.transform.localPosition.x, 0), m_aniTime);
                    if (GetBtnLockStateByType(i))
                    {
                        LeanTween.scale(lastImgGo, new Vector3(m_lastScaleRatio, m_lastScaleRatio, m_lastScaleRatio), m_aniTime);

                        GetTextNameByType(i).gameObject.SetActive(true);
                        GetActiveTextNameByType(i).gameObject.SetActive(false);
                    }

                    lastImg.color = new Color(255, 255, 255, 204);
                }
            }

            //LeanTween.moveLocal(imgGo, new Vector2(imgGo.transform.localPosition.x, 30f), m_aniTime);
            LeanTween.scale(imgGo, new Vector3(m_curScaleRatio, m_curScaleRatio, m_curScaleRatio), m_aniTime).setOnComplete(
                () =>
                {
                    #region 回调下page界面

                    for (int i = 0; i < m_listHomeChilds.Count; i++)
                    {
                        m_listHomeChilds[i].OnCenterOnEnd(i == m_curIndex);
                    }

                    #endregion
                });
            curImg.color = new Color(255, 255, 255, 255);
            GetTextNameByType(btnType).gameObject.SetActive(false);
            GetActiveTextNameByType(btnType).gameObject.SetActive(true);
        }

        #endregion
    }
}