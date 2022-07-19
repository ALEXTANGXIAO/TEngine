using UnityEngine;
using UnityEngine.UI;

namespace TEngine
{
    public class LocalizeText:Text
    {
        [SerializeField] public int Key;

        public string SetText(int key)
        {
            return LocalizeMgr.Instance.GetLocalizeStr(key);
        }

        protected override void Start()
        {
            base.Start();
            if (Key > 0 && string.IsNullOrEmpty(this.text))
            {
                text = SetText(Key);
            }
            else if (!string.IsNullOrEmpty(this.text))
            {
                Key = LocalizeMgr.Instance.GetLocalId(this.text);
                if (Key <= 0)
                {
                    return;
                }
                text = SetText(Key);
            }
        }
    }
}
