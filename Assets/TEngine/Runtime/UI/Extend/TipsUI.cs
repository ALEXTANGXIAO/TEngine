using System.Collections;
using System.Collections.Generic;
using TEngine;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class MsgUI : UIWindow
    {
        protected override void RegisterEvent()
        {
            base.RegisterEvent();
            //GameEventMgr.Instance.AddEventListener<string>(TipsEvent.Log, TipsUI.Instance.Show);
        }
    }

    public class TipsEvent
    {
        public static int Log = StringId.StringToHash("TipsEvent.Log");
    }

    public class TipsUI : MonoBehaviour
    {
        //单例
        public static TipsUI Instance;
        private void Awake()
        {
            Instance = this;

            AvailablePool = new List<GameObject>();
            UsedPool = new List<GameObject>();
            for (int i = 0; i < 5; i++)
            {
                AvailablePool.Add(Instantiate(Prefab, Mask.transform));
                AvailablePool[i].SetActive(false);
            }
            GameEventMgr.Instance.AddEventListener<string>(TipsEvent.Log, TipsUI.Instance.Show);
        }
        //Prefab
        public GameObject Prefab;
        public GameObject Mask;
        //对象池
        public List<GameObject> AvailablePool;
        public List<GameObject> UsedPool;
        //自定义内容
        [SerializeField] private float Speed;
        //定时器
        private float timer1;
        private float timer2;
        void Start()
        {
            //GameEventMgr.Instance.AddEventListener<string>(TipsEvent.Log, Show);
        }

        void Update()
        {
            //如果一定时间内没有新消息，则挨个消失
            timer1 += Time.deltaTime;
            if (timer1 >= 2)
            {
                timer2 += Time.deltaTime;
                if (timer2 > 1)
                {
                    timer2 = 0;
                    if (UsedPool.Count > 0)
                    {
                        Text Tname = UsedPool[0].transform.Find("Name").GetComponent<Text>();
                        Text Tmessage = UsedPool[0].transform.Find("Message").GetComponent<Text>();
                        Image BG = UsedPool[0].GetComponent<Image>();
                        StartCoroutine(AlphaDown(Tname));
                        StartCoroutine(AlphaDown(Tmessage));
                        StartCoroutine(ImageAlphaDown(BG));
                    }
                }
            }
        }
        public void Show(string message)
        {
            GameObject go;
            //使用对象池
            if (AvailablePool.Count > 0)
            {
                go = AvailablePool[0];
                AvailablePool.Remove(go);
                UsedPool.Add(go);
            }
            else
            {
                go = UsedPool[0];
                UsedPool.Remove(go);
                UsedPool.Add(go);
            }
            //进行一些初始化设定
            go.transform.localPosition = new Vector3(0, -75, 0);
            go.SetActive(true);
            go.GetComponent<Image>().color = new Color(0, 0, 0, 150f / 255f);
            timer1 = timer2 = 0;
            Text Tname = go.transform.Find("Name").GetComponent<Text>();
            Text Tmessage = go.transform.Find("Message").GetComponent<Text>();
            Tname.text = "  " + "SYSTEM";
            Tmessage.text = "  " + message;
            float TnameWidth = Tname.preferredWidth;
            float TmessageWidth = Tmessage.preferredWidth;
            float goSizey = go.transform.GetComponent<RectTransform>().sizeDelta.y;
            go.transform.GetComponent<RectTransform>().sizeDelta = new Vector2(TnameWidth + TmessageWidth, goSizey);
            Tname.rectTransform.sizeDelta = new Vector2(TnameWidth, Tname.rectTransform.sizeDelta.y);
            Tname.rectTransform.anchoredPosition = new Vector2(TnameWidth / 2, Tname.rectTransform.anchoredPosition.y);
            Tmessage.rectTransform.sizeDelta = new Vector2(TmessageWidth, Tmessage.rectTransform.sizeDelta.y);
            Tmessage.rectTransform.anchoredPosition = new Vector2(-TmessageWidth / 2, Tmessage.rectTransform.anchoredPosition.y);
            StartCoroutine(AlphaUP(Tname));
            StartCoroutine(AlphaUP(Tmessage));
            foreach (GameObject go1 in UsedPool)
            {
                StartCoroutine(MoveUP(go1));
            }
            if (UsedPool.Count >= 4)
            {
                Text Tname2 = UsedPool[UsedPool.Count - 4].transform.Find("Name").GetComponent<Text>();
                Text Tmessage2 = UsedPool[UsedPool.Count - 4].transform.Find("Message").GetComponent<Text>();
                Image BG = UsedPool[UsedPool.Count - 4].GetComponent<Image>();
                StartCoroutine(AlphaDown(Tname2));
                StartCoroutine(AlphaDown(Tmessage2));
                StartCoroutine(ImageAlphaDown(BG));
            }
        }


        public void Show(string name, string message)
        {
            GameObject go;
            //使用对象池
            if (AvailablePool.Count > 0)
            {
                go = AvailablePool[0];
                AvailablePool.Remove(go);
                UsedPool.Add(go);
            }
            else
            {
                go = UsedPool[0];
                UsedPool.Remove(go);
                UsedPool.Add(go);
            }
            //进行一些初始化设定
            go.transform.localPosition = new Vector3(0, -75, 0);
            go.SetActive(true);
            go.GetComponent<Image>().color = new Color(0, 0, 0, 150f / 255f);
            timer1 = timer2 = 0;
            Text Tname = go.transform.Find("Name").GetComponent<Text>();
            Text Tmessage = go.transform.Find("Message").GetComponent<Text>();
            Tname.text = "  " + name;
            Tmessage.text = "  " + message;
            float TnameWidth = Tname.preferredWidth;
            float TmessageWidth = Tmessage.preferredWidth;
            float goSizey = go.transform.GetComponent<RectTransform>().sizeDelta.y;
            go.transform.GetComponent<RectTransform>().sizeDelta = new Vector2(TnameWidth + TmessageWidth, goSizey);
            Tname.rectTransform.sizeDelta = new Vector2(TnameWidth, Tname.rectTransform.sizeDelta.y);
            Tname.rectTransform.anchoredPosition = new Vector2(TnameWidth / 2, Tname.rectTransform.anchoredPosition.y);
            Tmessage.rectTransform.sizeDelta = new Vector2(TmessageWidth, Tmessage.rectTransform.sizeDelta.y);
            Tmessage.rectTransform.anchoredPosition = new Vector2(-TmessageWidth / 2, Tmessage.rectTransform.anchoredPosition.y);
            StartCoroutine(AlphaUP(Tname));
            StartCoroutine(AlphaUP(Tmessage));
            foreach (GameObject go1 in UsedPool)
            {
                StartCoroutine(MoveUP(go1));
            }
            if (UsedPool.Count >= 4)
            {
                Text Tname2 = UsedPool[UsedPool.Count - 4].transform.Find("Name").GetComponent<Text>();
                Text Tmessage2 = UsedPool[UsedPool.Count - 4].transform.Find("Message").GetComponent<Text>();
                Image BG = UsedPool[UsedPool.Count - 4].GetComponent<Image>();
                StartCoroutine(AlphaDown(Tname2));
                StartCoroutine(AlphaDown(Tmessage2));
                StartCoroutine(ImageAlphaDown(BG));
            }
        }
        //文字透明度提高
        public IEnumerator AlphaUP(Text text)
        {
            text.color += new Color(0, 0, 0, -1);
            while (true)
            {
                yield return new WaitForSeconds(0.01f);
                text.color += new Color(0, 0, 0, 0.08f);
                if (text.color.a >= 1)
                {
                    yield break;
                }
            }
        }
        //向上移动
        public IEnumerator MoveUP(GameObject go)
        {
            float i = 0;
            while (true)
            {
                yield return new WaitForSeconds(0.01f);
                if (i + Speed >= 35)
                {
                    go.transform.localPosition += new Vector3(0, 35 - i, 0);
                    yield break;
                }
                else
                {
                    go.transform.localPosition += new Vector3(0, Speed, 0);
                    i += Speed;
                }
            }
        }
        //文字透明度下降
        public IEnumerator AlphaDown(Text text)
        {
            while (true)
            {
                yield return new WaitForSeconds(0.01f);
                text.color -= new Color(0, 0, 0, 0.08f);
                if (text.color.a <= 0)
                {
                    yield break;
                }
            }
        }
        //背景透明度下降
        public IEnumerator ImageAlphaDown(Image image)
        {
            while (true)
            {
                yield return new WaitForSeconds(0.01f);
                image.color -= new Color(0, 0, 0, 0.08f);
                if (image.color.a <= 0)
                {
                    image.gameObject.SetActive(false);
                    UsedPool.Remove(image.gameObject);
                    AvailablePool.Add(image.gameObject);
                    yield break;
                }
            }
        }
        //三个按钮的测试函数
        public void test()
        {
            Show("[没关系丶是爱情啊(安娜)]", "该睡觉咯。");
        }
        public void test2()
        {
            Show("[没关系丶是爱情啊(安娜)]", "妈妈永远是对的。");
        }
        public void test3()
        {
            Show("[没关系丶是爱情啊(安娜)]", "天降正义。");
        }
    }

}