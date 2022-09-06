using System;
using UnityEngine;

/**
* Internal Representation of a Tween<br>
* <br>
* This class represents all of the optional parameters you can pass to a method (it also represents the internal representation of the tween).<br><br>
* <strong id='optional'>Optional Parameters</strong> are passed at the end of every method:<br> 
* <br>
* &nbsp;&nbsp;<i>Example:</i><br>
* &nbsp;&nbsp;LeanTween.moveX( gameObject, 1f, 1f).setEase( <a href="LeanTweenType.html">LeanTweenType</a>.easeInQuad ).setDelay(1f);<br>
* <br>
* You can pass the optional parameters in any order, and chain on as many as you wish.<br>
* You can also <strong>pass parameters at a later time</strong> by saving a reference to what is returned.<br>
* <br>
* Retrieve a <strong>unique id</strong> for the tween by using the "id" property. You can pass this to LeanTween.pause, LeanTween.resume, LeanTween.cancel, LeanTween.isTweening methods<br>
* <br>
* &nbsp;&nbsp;<h4>Example:</h4>
* &nbsp;&nbsp;int id = LeanTween.moveX(gameObject, 1f, 3f).id;<br>
* <div style="color:gray">&nbsp;&nbsp;// pause a specific tween</div>
* &nbsp;&nbsp;LeanTween.pause(id);<br>
* <div style="color:gray">&nbsp;&nbsp;// resume later</div>
* &nbsp;&nbsp;LeanTween.resume(id);<br>
* <div style="color:gray">&nbsp;&nbsp;// check if it is tweening before kicking of a new tween</div>
* &nbsp;&nbsp;if( LeanTween.isTweening( id ) ){<br>
* &nbsp;&nbsp; &nbsp;&nbsp;	LeanTween.cancel( id );<br>
* &nbsp;&nbsp; &nbsp;&nbsp;	LeanTween.moveZ(gameObject, 10f, 3f);<br>
* &nbsp;&nbsp;}<br>
* @class LTDescr
* @constructor
*/

namespace TEngine.Runtime
{
    public class LTDescr
    {
        public bool toggle;
        public bool useEstimatedTime;
        public bool useFrames;
        public bool useManualTime;
        public bool usesNormalDt;
        public bool hasInitiliazed;
        public bool hasExtraOnCompletes;
        public bool hasPhysics;
        public bool onCompleteOnRepeat;
        public bool onCompleteOnStart;
        public bool useRecursion;
        public float ratioPassed;
        public float passed;
        public float delay;
        public float time;
        public float speed;
        public float lastVal;
        private uint _id;
        public int loopCount;
        public uint counter = uint.MaxValue;
        public float direction;
        public float directionLast;
        public float overshoot;
        public float period;
        public float scale;
        public bool destroyOnComplete;
        public Transform trans;
        internal Vector3 fromInternal;

        public Vector3 from
        {
            get { return this.fromInternal; }
            set { this.fromInternal = value; }
        }

        internal Vector3 toInternal;

        public Vector3 to
        {
            get { return this.toInternal; }
            set { this.toInternal = value; }
        }

        internal Vector3 diff;
        internal Vector3 diffDiv2;
        public TweenAction type;
        private LeanTweenType easeType;
        public LeanTweenType loopType;

        public bool hasUpdateCallback;

        public EaseTypeDelegate easeMethod;
        public ActionMethodDelegate easeInternal { get; set; }
        public ActionMethodDelegate initInternal { get; set; }

        public delegate Vector3 EaseTypeDelegate();

        public delegate void ActionMethodDelegate();

#if !UNITY_3_5 && !UNITY_4_0 && !UNITY_4_0_1 && !UNITY_4_1 && !UNITY_4_2
        public SpriteRenderer spriteRen;
#endif

#if !UNITY_3_5 && !UNITY_4_0 && !UNITY_4_0_1 && !UNITY_4_1 && !UNITY_4_2 && !UNITY_4_3 && !UNITY_4_5
        public RectTransform rectTransform;
        public UnityEngine.UI.Text uiText;
        public UnityEngine.UI.Image uiImage;
        public UnityEngine.UI.RawImage rawImage;
        public UnityEngine.Sprite[] sprites;
#endif

        public LTDescrOptional _optional = new LTDescrOptional();

        public override string ToString()
        {
            return (trans != null ? "name:" + trans.gameObject.name : "gameObject:null") + " toggle:" + toggle +
                   " passed:" + passed + " time:" + time + " delay:" + delay + " direction:" + direction + " from:" +
                   from + " to:" + to + " diff:" + diff + " type:" + type + " ease:" + easeType + " useEstimatedTime:" +
                   useEstimatedTime + " id:" + id + " hasInitiliazed:" + hasInitiliazed;
        }

        public LTDescr()
        {
        }

        [System.Obsolete("Use 'LeanTween.cancel( id )' instead")]
        public LTDescr cancel(GameObject gameObject)
        {
            // Debug.Log("canceling id:"+this._id+" this.uniqueId:"+this.uniqueId+" go:"+this.trans.gameObject);
            if (gameObject == this.trans.gameObject)
                LeanTween.removeTween((int)this._id, this.uniqueId);
            return this;
        }

        public int uniqueId
        {
            get
            {
                uint toId = _id | counter << 16;

                /*uint backId = toId & 0xFFFF;
			uint backCounter = toId >> 16;
			if(_id!=backId || backCounter!=counter){
				Debug.LogError("BAD CONVERSION toId:"+_id);
			}*/

                return (int)toId;
            }
        }

        public int id
        {
            get { return uniqueId; }
        }

        public LTDescrOptional optional
        {
            get { return _optional; }
            set { this._optional = value; }
        }

        public void reset()
        {
            this.toggle = this.useRecursion = this.usesNormalDt = true;
            this.trans = null;
            this.spriteRen = null;
            this.passed = this.delay = this.lastVal = 0.0f;
            this.hasUpdateCallback =
                this.useEstimatedTime =
                    this.useFrames =
                        this.hasInitiliazed =
                            this.onCompleteOnRepeat =
                                this.destroyOnComplete =
                                    this.onCompleteOnStart = this.useManualTime = this.hasExtraOnCompletes = false;
            this.easeType = LeanTweenType.linear;
            this.loopType = LeanTweenType.once;
            this.loopCount = 0;
            this.direction = this.directionLast = this.overshoot = this.scale = 1.0f;
            this.period = 0.3f;
            this.speed = -1f;
            this.easeMethod = this.easeLinear;
            this.from = this.to = Vector3.zero;
            this._optional.reset();
        }

        // Initialize and Internal Methods

        public LTDescr setMoveX()
        {
            this.type = TweenAction.MOVE_X;
            this.initInternal = () => { this.fromInternal.x = trans.position.x; };
            this.easeInternal =
                () => { trans.position = new Vector3(easeMethod().x, trans.position.y, trans.position.z); };
            return this;
        }

        public LTDescr setMoveY()
        {
            this.type = TweenAction.MOVE_Y;
            this.initInternal = () => { this.fromInternal.x = trans.position.y; };
            this.easeInternal =
                () => { trans.position = new Vector3(trans.position.x, easeMethod().x, trans.position.z); };
            return this;
        }

        public LTDescr setMoveZ()
        {
            this.type = TweenAction.MOVE_Z;
            this.initInternal = () => { this.fromInternal.x = trans.position.z; };
            ;
            this.easeInternal =
                () => { trans.position = new Vector3(trans.position.x, trans.position.y, easeMethod().x); };
            return this;
        }

        public LTDescr setMoveLocalX()
        {
            this.type = TweenAction.MOVE_LOCAL_X;
            this.initInternal = () => { this.fromInternal.x = trans.localPosition.x; };
            this.easeInternal =
                () =>
                {
                    trans.localPosition = new Vector3(easeMethod().x, trans.localPosition.y, trans.localPosition.z);
                };
            return this;
        }

        public LTDescr setMoveLocalY()
        {
            this.type = TweenAction.MOVE_LOCAL_Y;
            this.initInternal = () => { this.fromInternal.x = trans.localPosition.y; };
            this.easeInternal =
                () =>
                {
                    trans.localPosition = new Vector3(trans.localPosition.x, easeMethod().x, trans.localPosition.z);
                };
            return this;
        }

        public LTDescr setMoveLocalZ()
        {
            this.type = TweenAction.MOVE_LOCAL_Z;
            this.initInternal = () => { this.fromInternal.x = trans.localPosition.z; };
            this.easeInternal =
                () =>
                {
                    trans.localPosition = new Vector3(trans.localPosition.x, trans.localPosition.y, easeMethod().x);
                };
            return this;
        }

        private void initFromInternal()
        {
            this.fromInternal.x = 0;
        }

        public LTDescr setMoveCurved()
        {
            this.type = TweenAction.MOVE_CURVED;
            this.initInternal = this.initFromInternal;
            this.easeInternal = () =>
            {
                newVect = easeMethod();
                val = newVect.x;
                if (this._optional.path.orientToPath)
                {
                    if (this._optional.path.orientToPath2d)
                    {
                        this._optional.path.place2d(trans, val);
                    }
                    else
                    {
                        this._optional.path.place(trans, val);
                    }
                }
                else
                {
                    trans.position = this._optional.path.point(val);
                }
            };
            return this;
        }

        public LTDescr setMoveCurvedLocal()
        {
            this.type = TweenAction.MOVE_CURVED_LOCAL;
            this.initInternal = this.initFromInternal;
            this.easeInternal = () =>
            {
                newVect = easeMethod();
                val = newVect.x;
                if (this._optional.path.orientToPath)
                {
                    if (this._optional.path.orientToPath2d)
                    {
                        this._optional.path.placeLocal2d(trans, val);
                    }
                    else
                    {
                        this._optional.path.placeLocal(trans, val);
                    }
                }
                else
                {
                    trans.localPosition = this._optional.path.point(val);
                }
            };
            return this;
        }

        public LTDescr setMoveSpline()
        {
            this.type = TweenAction.MOVE_SPLINE;
            this.initInternal = this.initFromInternal;
            this.easeInternal = () =>
            {
                newVect = easeMethod();
                val = newVect.x;
                if (this._optional.spline.orientToPath)
                {
                    if (this._optional.spline.orientToPath2d)
                    {
                        this._optional.spline.place2d(trans, val);
                    }
                    else
                    {
                        this._optional.spline.place(trans, val);
                    }
                }
                else
                {
                    trans.position = this._optional.spline.point(val);
                }
            };
            return this;
        }

        public LTDescr setMoveSplineLocal()
        {
            this.type = TweenAction.MOVE_SPLINE_LOCAL;
            this.initInternal = this.initFromInternal;
            this.easeInternal = () =>
            {
                newVect = easeMethod();
                val = newVect.x;
                if (this._optional.spline.orientToPath)
                {
                    if (this._optional.spline.orientToPath2d)
                    {
                        this._optional.spline.placeLocal2d(trans, val);
                    }
                    else
                    {
                        this._optional.spline.placeLocal(trans, val);
                    }
                }
                else
                {
                    trans.localPosition = this._optional.spline.point(val);
                }
            };
            return this;
        }

        public LTDescr setScaleX()
        {
            this.type = TweenAction.SCALE_X;
            this.initInternal = () => { this.fromInternal.x = trans.localScale.x; };
            this.easeInternal =
                () => { trans.localScale = new Vector3(easeMethod().x, trans.localScale.y, trans.localScale.z); };
            return this;
        }

        public LTDescr setScaleY()
        {
            this.type = TweenAction.SCALE_Y;
            this.initInternal = () => { this.fromInternal.x = trans.localScale.y; };
            this.easeInternal =
                () => { trans.localScale = new Vector3(trans.localScale.x, easeMethod().x, trans.localScale.z); };
            return this;
        }

        public LTDescr setScaleZ()
        {
            this.type = TweenAction.SCALE_Z;
            this.initInternal = () => { this.fromInternal.x = trans.localScale.z; };
            this.easeInternal =
                () => { trans.localScale = new Vector3(trans.localScale.x, trans.localScale.y, easeMethod().x); };
            return this;
        }

        public LTDescr setRotateX()
        {
            this.type = TweenAction.ROTATE_X;
            this.initInternal = () =>
            {
                this.fromInternal.x = trans.eulerAngles.x;
                this.toInternal.x = LeanTween.closestRot(this.fromInternal.x, this.toInternal.x);
            };
            this.easeInternal =
                () => { trans.eulerAngles = new Vector3(easeMethod().x, trans.eulerAngles.y, trans.eulerAngles.z); };
            return this;
        }

        public LTDescr setRotateY()
        {
            this.type = TweenAction.ROTATE_Y;
            this.initInternal = () =>
            {
                this.fromInternal.x = trans.eulerAngles.y;
                this.toInternal.x = LeanTween.closestRot(this.fromInternal.x, this.toInternal.x);
            };
            this.easeInternal =
                () => { trans.eulerAngles = new Vector3(trans.eulerAngles.x, easeMethod().x, trans.eulerAngles.z); };
            return this;
        }

        public LTDescr setRotateZ()
        {
            this.type = TweenAction.ROTATE_Z;
            this.initInternal = () =>
            {
                this.fromInternal.x = trans.eulerAngles.z;
                this.toInternal.x = LeanTween.closestRot(this.fromInternal.x, this.toInternal.x);
            };
            this.easeInternal =
                () => { trans.eulerAngles = new Vector3(trans.eulerAngles.x, trans.eulerAngles.y, easeMethod().x); };
            return this;
        }

        public LTDescr setRotateAround()
        {
            this.type = TweenAction.ROTATE_AROUND;
            this.initInternal = () =>
            {
                this.fromInternal.x = 0f;
                this._optional.origRotation = trans.rotation;
            };
            this.easeInternal = () =>
            {
                newVect = easeMethod();
                val = newVect.x;
                Vector3 origPos = trans.localPosition;
                Vector3 rotateAroundPt = (Vector3)trans.TransformPoint(this._optional.point);
                // Debug.Log("this._optional.point:"+this._optional.point);
                trans.RotateAround(rotateAroundPt, this._optional.axis, -this._optional.lastVal);
                Vector3 diff = origPos - trans.localPosition;

                trans.localPosition = origPos - diff;
                // Subtract the amount the object has been shifted over by the rotate, to get it back to it's orginal position
                trans.rotation = this._optional.origRotation;

                rotateAroundPt = (Vector3)trans.TransformPoint(this._optional.point);
                trans.RotateAround(rotateAroundPt, this._optional.axis, val);

                this._optional.lastVal = val;
            };
            return this;
        }

        public LTDescr setRotateAroundLocal()
        {
            this.type = TweenAction.ROTATE_AROUND_LOCAL;
            this.initInternal = () =>
            {
                this.fromInternal.x = 0f;
                this._optional.origRotation = trans.localRotation;
            };
            this.easeInternal = () =>
            {
                newVect = easeMethod();
                val = newVect.x;
                Vector3 origPos = trans.localPosition;
                trans.RotateAround((Vector3)trans.TransformPoint(this._optional.point),
                    trans.TransformDirection(this._optional.axis), -this._optional.lastVal);
                Vector3 diff = origPos - trans.localPosition;

                trans.localPosition = origPos - diff;
                // Subtract the amount the object has been shifted over by the rotate, to get it back to it's orginal position
                trans.localRotation = this._optional.origRotation;
                Vector3 rotateAroundPt = (Vector3)trans.TransformPoint(this._optional.point);
                trans.RotateAround(rotateAroundPt, trans.TransformDirection(this._optional.axis), val);

                this._optional.lastVal = val;
            };
            return this;
        }

        public LTDescr setAlpha()
        {
            this.type = TweenAction.ALPHA;
            this.initInternal = () =>
            {
#if UNITY_3_5 || UNITY_4_0 || UNITY_4_0_1 || UNITY_4_1 || UNITY_4_2
			if(trans.gameObject.renderer){ this.fromInternal.x =
 trans.gameObject.renderer.material.color.a; }else if(trans.childCount>0){ foreach (Transform child in trans) { if(child.gameObject.renderer!=null){ Color col
 = child.gameObject.renderer.material.color; this.fromInternal.x = col.a; break; }}}
			this.easeInternal = this.alpha;
			break;
#else
                SpriteRenderer ren = trans.GetComponent<SpriteRenderer>();
                if (ren != null)
                {
                    this.fromInternal.x = ren.color.a;
                }
                else
                {
                    if (trans.GetComponent<Renderer>() != null &&
                        trans.GetComponent<Renderer>().material.HasProperty("_Color"))
                    {
                        this.fromInternal.x = trans.GetComponent<Renderer>().material.color.a;
                    }
                    else if (trans.GetComponent<Renderer>() != null &&
                             trans.GetComponent<Renderer>().material.HasProperty("_TintColor"))
                    {
                        Color col = trans.GetComponent<Renderer>().material.GetColor("_TintColor");
                        this.fromInternal.x = col.a;
                    }
                    else if (trans.childCount > 0)
                    {
                        foreach (Transform child in trans)
                        {
                            if (child.gameObject.GetComponent<Renderer>() != null)
                            {
                                Color col = child.gameObject.GetComponent<Renderer>().material.color;
                                this.fromInternal.x = col.a;
                                break;
                            }
                        }
                    }
                }
#endif

                this.easeInternal = () =>
                {
                    val = easeMethod().x;
#if UNITY_3_5 || UNITY_4_0 || UNITY_4_0_1 || UNITY_4_1 || UNITY_4_2
				alphaRecursive(this.trans, val, this.useRecursion);
#else
                    if (this.spriteRen != null)
                    {
                        this.spriteRen.color = new Color(this.spriteRen.color.r, this.spriteRen.color.g,
                            this.spriteRen.color.b, val);
                        alphaRecursiveSprite(this.trans, val);
                    }
                    else
                    {
                        alphaRecursive(this.trans, val, this.useRecursion);
                    }
#endif
                };
            };
            this.easeInternal = () =>
            {
                newVect = easeMethod();
                val = newVect.x;
#if UNITY_3_5 || UNITY_4_0 || UNITY_4_0_1 || UNITY_4_1 || UNITY_4_2
			alphaRecursive(this.trans, val, this.useRecursion);
#else
                if (this.spriteRen != null)
                {
                    this.spriteRen.color = new Color(this.spriteRen.color.r, this.spriteRen.color.g,
                        this.spriteRen.color.b, val);
                    alphaRecursiveSprite(this.trans, val);
                }
                else
                {
                    alphaRecursive(this.trans, val, this.useRecursion);
                }
#endif
            };
            return this;
        }

        public LTDescr setTextAlpha()
        {
            this.type = TweenAction.TEXT_ALPHA;
            this.initInternal = () =>
            {
                this.uiText = trans.GetComponent<UnityEngine.UI.Text>();
                this.fromInternal.x = this.uiText != null ? this.uiText.color.a : 1f;
            };
            this.easeInternal = () => { textAlphaRecursive(trans, easeMethod().x, this.useRecursion); };
            return this;
        }

        public LTDescr setAlphaVertex()
        {
            this.type = TweenAction.ALPHA_VERTEX;
            this.initInternal = () => { this.fromInternal.x = trans.GetComponent<MeshFilter>().mesh.colors32[0].a; };
            this.easeInternal = () =>
            {
                newVect = easeMethod();
                val = newVect.x;
                Mesh mesh = trans.GetComponent<MeshFilter>().mesh;
                Vector3[] vertices = mesh.vertices;
                Color32[] colors = new Color32[vertices.Length];
                if (colors.Length == 0)
                {
                    //MaxFW fix: add vertex colors if the mesh doesn't have any             
                    Color32 transparentWhiteColor32 = new Color32(0xff, 0xff, 0xff, 0x00);
                    colors = new Color32[mesh.vertices.Length];
                    for (int k = 0; k < colors.Length; k++)
                        colors[k] = transparentWhiteColor32;
                    mesh.colors32 = colors;
                } // fix end

                Color32 c = mesh.colors32[0];
                c = new Color(c.r, c.g, c.b, val);
                for (int k = 0; k < vertices.Length; k++)
                    colors[k] = c;
                mesh.colors32 = colors;
            };
            return this;
        }

        public LTDescr setColor()
        {
            this.type = TweenAction.COLOR;
            this.initInternal = () =>
            {
#if UNITY_3_5 || UNITY_4_0 || UNITY_4_0_1 || UNITY_4_1 || UNITY_4_2
			if(trans.gameObject.renderer){
			this.setFromColor( trans.gameObject.renderer.material.color );
			}else if(trans.childCount>0){
			foreach (Transform child in trans) {
			if(child.gameObject.renderer!=null){
			this.setFromColor( child.gameObject.renderer.material.color );
			break;
			}
			}
			}
#else
                SpriteRenderer renColor = trans.GetComponent<SpriteRenderer>();
                if (renColor != null)
                {
                    this.setFromColor(renColor.color);
                }
                else
                {
                    if (trans.GetComponent<Renderer>() != null &&
                        trans.GetComponent<Renderer>().material.HasProperty("_Color"))
                    {
                        Color col = trans.GetComponent<Renderer>().material.color;
                        this.setFromColor(col);
                    }
                    else if (trans.GetComponent<Renderer>() != null &&
                             trans.GetComponent<Renderer>().material.HasProperty("_TintColor"))
                    {
                        Color col = trans.GetComponent<Renderer>().material.GetColor("_TintColor");
                        this.setFromColor(col);
                    }
                    else if (trans.childCount > 0)
                    {
                        foreach (Transform child in trans)
                        {
                            if (child.gameObject.GetComponent<Renderer>() != null)
                            {
                                Color col = child.gameObject.GetComponent<Renderer>().material.color;
                                this.setFromColor(col);
                                break;
                            }
                        }
                    }
                }
#endif
            };
            this.easeInternal = () =>
            {
                newVect = easeMethod();
                val = newVect.x;
                Color toColor = tweenColor(this, val);

#if !UNITY_3_5 && !UNITY_4_0 && !UNITY_4_0_1 && !UNITY_4_1 && !UNITY_4_2

                if (this.spriteRen != null)
                {
                    this.spriteRen.color = toColor;
                    colorRecursiveSprite(trans, toColor);
                }
                else
                {
#endif
                    // Debug.Log("val:"+val+" tween:"+tween+" tween.diff:"+tween.diff);
                    if (this.type == TweenAction.COLOR)
                        colorRecursive(trans, toColor, this.useRecursion);

#if !UNITY_3_5 && !UNITY_4_0 && !UNITY_4_0_1 && !UNITY_4_1 && !UNITY_4_2
                }
#endif
                if (dt != 0f && this._optional.onUpdateColor != null)
                {
                    this._optional.onUpdateColor(toColor);
                }
                else if (dt != 0f && this._optional.onUpdateColorObject != null)
                {
                    this._optional.onUpdateColorObject(toColor, this._optional.onUpdateParam);
                }
            };
            return this;
        }

        public LTDescr setCallbackColor()
        {
            this.type = TweenAction.CALLBACK_COLOR;
            this.initInternal = () => { this.diff = new Vector3(1.0f, 0.0f, 0.0f); };
            this.easeInternal = () =>
            {
                newVect = easeMethod();
                val = newVect.x;
                Color toColor = tweenColor(this, val);

#if !UNITY_3_5 && !UNITY_4_0 && !UNITY_4_0_1 && !UNITY_4_1 && !UNITY_4_2
                if (this.spriteRen != null)
                {
                    this.spriteRen.color = toColor;
                    colorRecursiveSprite(trans, toColor);
                }
                else
                {
#endif
                    // Debug.Log("val:"+val+" tween:"+tween+" tween.diff:"+tween.diff);
                    if (this.type == TweenAction.COLOR)
                        colorRecursive(trans, toColor, this.useRecursion);

#if !UNITY_3_5 && !UNITY_4_0 && !UNITY_4_0_1 && !UNITY_4_1 && !UNITY_4_2
                }
#endif
                if (dt != 0f && this._optional.onUpdateColor != null)
                {
                    this._optional.onUpdateColor(toColor);
                }
                else if (dt != 0f && this._optional.onUpdateColorObject != null)
                {
                    this._optional.onUpdateColorObject(toColor, this._optional.onUpdateParam);
                }
            };
            return this;
        }


#if !UNITY_3_5 && !UNITY_4_0 && !UNITY_4_0_1 && !UNITY_4_1 && !UNITY_4_2 && !UNITY_4_3 && !UNITY_4_5

        public LTDescr setTextColor()
        {
            this.type = TweenAction.TEXT_COLOR;
            this.initInternal = () =>
            {
                this.uiText = trans.GetComponent<UnityEngine.UI.Text>();
                this.setFromColor(this.uiText != null ? this.uiText.color : Color.white);
            };
            this.easeInternal = () =>
            {
                newVect = easeMethod();
                val = newVect.x;
                Color toColor = tweenColor(this, val);
                this.uiText.color = toColor;
                if (dt != 0f && this._optional.onUpdateColor != null)
                    this._optional.onUpdateColor(toColor);

                if (this.useRecursion && trans.childCount > 0)
                    textColorRecursive(this.trans, toColor);
            };
            return this;
        }

        public LTDescr setCanvasAlpha()
        {
            this.type = TweenAction.CANVAS_ALPHA;
            this.initInternal = () =>
            {
                this.uiImage = trans.GetComponent<UnityEngine.UI.Image>();
                if (this.uiImage != null)
                {
                    this.fromInternal.x = this.uiImage.color.a;
                }
                else
                {
                    this.rawImage = trans.GetComponent<UnityEngine.UI.RawImage>();
                    if (this.rawImage != null)
                    {
                        this.fromInternal.x = this.rawImage.color.a;
                    }
                    else
                    {
                        this.fromInternal.x = 1f;
                    }
                }
            };
            this.easeInternal = () =>
            {
                newVect = easeMethod();
                val = newVect.x;
                if (this.uiImage != null)
                {
                    Color c = this.uiImage.color;
                    c.a = val;
                    this.uiImage.color = c;
                }
                else if (this.rawImage != null)
                {
                    Color c = this.rawImage.color;
                    c.a = val;
                    this.rawImage.color = c;
                }

                if (this.useRecursion)
                {
                    alphaRecursive(this.rectTransform, val, 0);
                    textAlphaChildrenRecursive(this.rectTransform, val);
                }
            };
            return this;
        }

        public LTDescr setCanvasGroupAlpha()
        {
            this.type = TweenAction.CANVASGROUP_ALPHA;
            this.initInternal = () => { this.fromInternal.x = trans.GetComponent<CanvasGroup>().alpha; };
            this.easeInternal = () => { this.trans.GetComponent<CanvasGroup>().alpha = easeMethod().x; };
            return this;
        }

        public LTDescr setCanvasColor()
        {
            this.type = TweenAction.CANVAS_COLOR;
            this.initInternal = () =>
            {
                this.uiImage = trans.GetComponent<UnityEngine.UI.Image>();
                if (this.uiImage == null)
                {
                    this.rawImage = trans.GetComponent<UnityEngine.UI.RawImage>();
                    this.setFromColor(this.rawImage != null ? this.rawImage.color : Color.white);
                }
                else
                {
                    this.setFromColor(this.uiImage.color);
                }
            };
            this.easeInternal = () =>
            {
                newVect = easeMethod();
                val = newVect.x;
                Color toColor = tweenColor(this, val);
                if (this.uiImage != null)
                {
                    this.uiImage.color = toColor;
                }
                else if (this.rawImage != null)
                {
                    this.rawImage.color = toColor;
                }

                if (dt != 0f && this._optional.onUpdateColor != null)
                    this._optional.onUpdateColor(toColor);

                if (this.useRecursion)
                    colorRecursive(this.rectTransform, toColor);
            };
            return this;
        }

        public LTDescr setCanvasMoveX()
        {
            this.type = TweenAction.CANVAS_MOVE_X;
            this.initInternal = () => { this.fromInternal.x = this.rectTransform.anchoredPosition3D.x; };
            this.easeInternal = () =>
            {
                Vector3 c = this.rectTransform.anchoredPosition3D;
                this.rectTransform.anchoredPosition3D = new Vector3(easeMethod().x, c.y, c.z);
            };
            return this;
        }

        public LTDescr setCanvasMoveY()
        {
            this.type = TweenAction.CANVAS_MOVE_Y;
            this.initInternal = () => { this.fromInternal.x = this.rectTransform.anchoredPosition3D.y; };
            this.easeInternal = () =>
            {
                Vector3 c = this.rectTransform.anchoredPosition3D;
                this.rectTransform.anchoredPosition3D = new Vector3(c.x, easeMethod().x, c.z);
            };
            return this;
        }

        public LTDescr setCanvasMoveZ()
        {
            this.type = TweenAction.CANVAS_MOVE_Z;
            this.initInternal = () => { this.fromInternal.x = this.rectTransform.anchoredPosition3D.z; };
            this.easeInternal = () =>
            {
                Vector3 c = this.rectTransform.anchoredPosition3D;
                this.rectTransform.anchoredPosition3D = new Vector3(c.x, c.y, easeMethod().x);
            };
            return this;
        }

        private void initCanvasRotateAround()
        {
            this.lastVal = 0.0f;
            this.fromInternal.x = 0.0f;
            this._optional.origRotation = this.rectTransform.rotation;
        }

        public LTDescr setCanvasRotateAround()
        {
            this.type = TweenAction.CANVAS_ROTATEAROUND;
            this.initInternal = this.initCanvasRotateAround;
            this.easeInternal = () =>
            {
                newVect = easeMethod();
                val = newVect.x;
                RectTransform rect = this.rectTransform;
                Vector3 origPos = rect.localPosition;
                rect.RotateAround((Vector3)rect.TransformPoint(this._optional.point), this._optional.axis, -val);
                Vector3 diff = origPos - rect.localPosition;

                rect.localPosition = origPos - diff;
                // Subtract the amount the object has been shifted over by the rotate, to get it back to it's orginal position
                rect.rotation = this._optional.origRotation;
                rect.RotateAround((Vector3)rect.TransformPoint(this._optional.point), this._optional.axis, val);
            };
            return this;
        }

        public LTDescr setCanvasRotateAroundLocal()
        {
            this.type = TweenAction.CANVAS_ROTATEAROUND_LOCAL;
            this.initInternal = this.initCanvasRotateAround;
            this.easeInternal = () =>
            {
                newVect = easeMethod();
                val = newVect.x;
                RectTransform rect = this.rectTransform;
                Vector3 origPos = rect.localPosition;
                rect.RotateAround((Vector3)rect.TransformPoint(this._optional.point),
                    rect.TransformDirection(this._optional.axis), -val);
                Vector3 diff = origPos - rect.localPosition;

                rect.localPosition = origPos - diff;
                // Subtract the amount the object has been shifted over by the rotate, to get it back to it's orginal position
                rect.rotation = this._optional.origRotation;
                rect.RotateAround((Vector3)rect.TransformPoint(this._optional.point),
                    rect.TransformDirection(this._optional.axis), val);
            };
            return this;
        }

        public LTDescr setCanvasPlaySprite()
        {
            this.type = TweenAction.CANVAS_PLAYSPRITE;
            this.initInternal = () =>
            {
                this.uiImage = trans.GetComponent<UnityEngine.UI.Image>();
                this.fromInternal.x = 0f;
            };
            this.easeInternal = () =>
            {
                newVect = easeMethod();
                val = newVect.x;
                int frame = (int)Mathf.Round(val);
                this.uiImage.sprite = this.sprites[frame];
            };
            return this;
        }

        public LTDescr setCanvasMove()
        {
            this.type = TweenAction.CANVAS_MOVE;
            this.initInternal = () => { this.fromInternal = this.rectTransform.anchoredPosition3D; };
            this.easeInternal = () => { this.rectTransform.anchoredPosition3D = easeMethod(); };
            return this;
        }

        public LTDescr setCanvasScale()
        {
            this.type = TweenAction.CANVAS_SCALE;
            this.initInternal = () => { this.from = this.rectTransform.localScale; };
            this.easeInternal = () => { this.rectTransform.localScale = easeMethod(); };
            return this;
        }

        public LTDescr setCanvasSizeDelta()
        {
            this.type = TweenAction.CANVAS_SIZEDELTA;
            this.initInternal = () => { this.from = this.rectTransform.sizeDelta; };
            this.easeInternal = () => { this.rectTransform.sizeDelta = easeMethod(); };
            return this;
        }
#endif

        private void callback()
        {
            newVect = easeMethod();
            val = newVect.x;
        }

        public LTDescr setCallback()
        {
            this.type = TweenAction.CALLBACK;
            this.initInternal = () => { };
            this.easeInternal = this.callback;
            return this;
        }

        public LTDescr setValue3()
        {
            this.type = TweenAction.VALUE3;
            this.initInternal = () => { };
            this.easeInternal = this.callback;
            return this;
        }

        public LTDescr setMove()
        {
            this.type = TweenAction.MOVE;
            this.initInternal = () => { this.from = trans.position; };
            this.easeInternal = () =>
            {
                newVect = easeMethod();
                trans.position = newVect;
            };
            return this;
        }

        public LTDescr setMoveLocal()
        {
            this.type = TweenAction.MOVE_LOCAL;
            this.initInternal = () => { this.from = trans.localPosition; };
            this.easeInternal = () =>
            {
                newVect = easeMethod();
                trans.localPosition = newVect;
            };
            return this;
        }

        public LTDescr setMoveToTransform()
        {
            this.type = TweenAction.MOVE_TO_TRANSFORM;
            this.initInternal = () => { this.from = trans.position; };
            this.easeInternal = () =>
            {
                this.to = this._optional.toTrans.position;
                this.diff = this.to - this.from;
                this.diffDiv2 = this.diff * 0.5f;

                newVect = easeMethod();
                this.trans.position = newVect;
            };
            return this;
        }

        public LTDescr setRotate()
        {
            this.type = TweenAction.ROTATE;
            this.initInternal = () =>
            {
                this.from = trans.eulerAngles;
                this.to = new Vector3(LeanTween.closestRot(this.fromInternal.x, this.toInternal.x),
                    LeanTween.closestRot(this.from.y, this.to.y), LeanTween.closestRot(this.from.z, this.to.z));
            };
            this.easeInternal = () =>
            {
                newVect = easeMethod();
                trans.eulerAngles = newVect;
            };
            return this;
        }

        public LTDescr setRotateLocal()
        {
            this.type = TweenAction.ROTATE_LOCAL;
            this.initInternal = () =>
            {
                this.from = trans.localEulerAngles;
                this.to = new Vector3(LeanTween.closestRot(this.fromInternal.x, this.toInternal.x),
                    LeanTween.closestRot(this.from.y, this.to.y), LeanTween.closestRot(this.from.z, this.to.z));
            };
            this.easeInternal = () =>
            {
                newVect = easeMethod();
                trans.localEulerAngles = newVect;
            };
            return this;
        }

        public LTDescr setScale()
        {
            this.type = TweenAction.SCALE;
            this.initInternal = () => { this.from = trans.localScale; };
            this.easeInternal = () =>
            {
                newVect = easeMethod();
                trans.localScale = newVect;
            };
            return this;
        }

        public LTDescr setGUIMove()
        {
            this.type = TweenAction.GUI_MOVE;
            this.initInternal =
                () => { this.from = new Vector3(this._optional.ltRect.rect.x, this._optional.ltRect.rect.y, 0); };
            this.easeInternal = () =>
            {
                Vector3 v = easeMethod();
                this._optional.ltRect.rect = new Rect(v.x, v.y, this._optional.ltRect.rect.width,
                    this._optional.ltRect.rect.height);
            };
            return this;
        }

        public LTDescr setGUIMoveMargin()
        {
            this.type = TweenAction.GUI_MOVE_MARGIN;
            this.initInternal =
                () => { this.from = new Vector2(this._optional.ltRect.margin.x, this._optional.ltRect.margin.y); };
            this.easeInternal = () =>
            {
                Vector3 v = easeMethod();
                this._optional.ltRect.margin = new Vector2(v.x, v.y);
            };
            return this;
        }

        public LTDescr setGUIScale()
        {
            this.type = TweenAction.GUI_SCALE;
            this.initInternal =
                () =>
                {
                    this.from = new Vector3(this._optional.ltRect.rect.width, this._optional.ltRect.rect.height, 0);
                };
            this.easeInternal = () =>
            {
                Vector3 v = easeMethod();
                this._optional.ltRect.rect = new Rect(this._optional.ltRect.rect.x, this._optional.ltRect.rect.y, v.x,
                    v.y);
            };
            return this;
        }

        public LTDescr setGUIAlpha()
        {
            this.type = TweenAction.GUI_ALPHA;
            this.initInternal = () => { this.fromInternal.x = this._optional.ltRect.alpha; };
            this.easeInternal = () => { this._optional.ltRect.alpha = easeMethod().x; };
            return this;
        }

        public LTDescr setGUIRotate()
        {
            this.type = TweenAction.GUI_ROTATE;
            this.initInternal = () =>
            {
                if (this._optional.ltRect.rotateEnabled == false)
                {
                    this._optional.ltRect.rotateEnabled = true;
                    this._optional.ltRect.resetForRotation();
                }

                this.fromInternal.x = this._optional.ltRect.rotation;
            };
            this.easeInternal = () => { this._optional.ltRect.rotation = easeMethod().x; };
            return this;
        }

        public LTDescr setDelayedSound()
        {
            this.type = TweenAction.DELAYED_SOUND;
            this.initInternal = () => { this.hasExtraOnCompletes = true; };
            this.easeInternal = this.callback;
            return this;
        }

        private void init()
        {
            this.hasInitiliazed = true;

            usesNormalDt = !(useEstimatedTime || useManualTime || useFrames);
            // only set this to true if it uses non of the other timing modes

            if (useFrames)
                this.optional.initFrameCount = Time.frameCount;

            if (this.time <= 0f) // avoid dividing by zero
                this.time = Mathf.Epsilon;

            this.initInternal();

            this.diff = this.to - this.from;
            this.diffDiv2 = this.diff * 0.5f;

            if (this._optional.onStart != null)
                this._optional.onStart();

            if (this.onCompleteOnStart)
                callOnCompletes();

            if (this.speed >= 0)
            {
                initSpeed();
            }
        }

        private void initSpeed()
        {
            if (this.type == TweenAction.MOVE_CURVED || this.type == TweenAction.MOVE_CURVED_LOCAL)
            {
                this.time = this._optional.path.distance / this.speed;
            }
            else if (this.type == TweenAction.MOVE_SPLINE || this.type == TweenAction.MOVE_SPLINE_LOCAL)
            {
                this.time = this._optional.spline.distance / this.speed;
            }
            else
            {
                this.time = (this.to - this.from).magnitude / this.speed;
            }
        }

        public static float val;
        public static float dt;
        public static Vector3 newVect;

        /**
	* If you need a tween to happen immediately instead of waiting for the next Update call, you can force it with this method
	* 
	* @method updateNow
	* @return {LTDescr} LTDescr an object that distinguishes the tween
	* @example
	* LeanTween.moveX(gameObject, 5f, 0f ).updateNow();
	*/
        public LTDescr updateNow()
        {
            updateInternal();
            return this;
        }

        public bool updateInternal()
        {
            float directionLocal = this.direction;
            if (this.usesNormalDt)
            {
                dt = LeanTween.dtActual;
            }
            else if (this.useEstimatedTime)
            {
                dt = LeanTween.dtEstimated;
            }
            else if (this.useFrames)
            {
                dt = this.optional.initFrameCount == 0 ? 0 : 1;
                this.optional.initFrameCount = Time.frameCount;
            }
            else if (this.useManualTime)
            {
                dt = LeanTween.dtManual;
            }

//		Debug.Log ("tween:" + this+ " dt:"+dt);
            if (this.delay <= 0f && directionLocal != 0f)
            {
                if (trans == null)
                    return true;

                // initialize if has not done so yet
                if (!this.hasInitiliazed)
                    this.init();

                dt = dt * directionLocal;
                this.passed += dt;

                this.passed = Mathf.Clamp(this.passed, 0f, this.time);

                this.ratioPassed = (this.passed / this.time);
                // need to clamp when finished so it will finish at the exact spot and not overshoot

                this.easeInternal();

                if (this.hasUpdateCallback)
                    this._optional.callOnUpdate(val, this.ratioPassed);

                bool isTweenFinished = directionLocal > 0f ? this.passed >= this.time : this.passed <= 0f;
                //			Debug.Log("lt "+this+" dt:"+dt+" fin:"+isTweenFinished);
                if (isTweenFinished)
                {
                    // increment or flip tween
                    this.loopCount--;
                    if (this.loopType == LeanTweenType.pingPong)
                    {
                        this.direction = 0.0f - directionLocal;
                    }
                    else
                    {
                        this.passed = Mathf.Epsilon;
                    }

                    isTweenFinished = this.loopCount == 0 || this.loopType == LeanTweenType.once;
                    // only return true if it is fully complete

                    if (isTweenFinished == false && this.onCompleteOnRepeat && this.hasExtraOnCompletes)
                        callOnCompletes();
                    // this only gets called if onCompleteOnRepeat is set to true, otherwise LeanTween class takes care of calling it

                    return isTweenFinished;
                }
            }
            else
            {
                this.delay -= dt;
            }

            return false;
        }

        public void callOnCompletes()
        {
            if (this.type == TweenAction.GUI_ROTATE)
                this._optional.ltRect.rotateFinished = true;

            if (this.type == TweenAction.DELAYED_SOUND)
            {
                AudioSource.PlayClipAtPoint((AudioClip)this._optional.onCompleteParam, this.to, this.from.x);
            }

            if (this._optional.onComplete != null)
            {
                this._optional.onComplete();
            }
            else if (this._optional.onCompleteObject != null)
            {
                this._optional.onCompleteObject(this._optional.onCompleteParam);
            }
        }

        // Helper Methods

        public LTDescr setFromColor(Color col)
        {
            this.from = new Vector3(0.0f, col.a, 0.0f);
            this.diff = new Vector3(1.0f, 0.0f, 0.0f);
            this._optional.axis = new Vector3(col.r, col.g, col.b);
            return this;
        }

        private static void alphaRecursive(Transform transform, float val, bool useRecursion = true)
        {
            Renderer renderer = transform.gameObject.GetComponent<Renderer>();
            if (renderer != null)
            {
                foreach (Material mat in renderer.materials)
                {
                    if (mat.HasProperty("_Color"))
                    {
                        mat.color = new Color(mat.color.r, mat.color.g, mat.color.b, val);
                    }
                    else if (mat.HasProperty("_TintColor"))
                    {
                        Color col = mat.GetColor("_TintColor");
                        mat.SetColor("_TintColor", new Color(col.r, col.g, col.b, val));
                    }
                }
            }

            if (useRecursion && transform.childCount > 0)
            {
                foreach (Transform child in transform)
                {
                    alphaRecursive(child, val);
                }
            }
        }

        private static void colorRecursive(Transform transform, Color toColor, bool useRecursion = true)
        {
            Renderer ren = transform.gameObject.GetComponent<Renderer>();
            if (ren != null)
            {
                foreach (Material mat in ren.materials)
                {
                    mat.color = toColor;
                }
            }

            if (useRecursion && transform.childCount > 0)
            {
                foreach (Transform child in transform)
                {
                    colorRecursive(child, toColor);
                }
            }
        }

#if !UNITY_3_5 && !UNITY_4_0 && !UNITY_4_0_1 && !UNITY_4_1 && !UNITY_4_2 && !UNITY_4_3 && !UNITY_4_5

        private static void alphaRecursive(RectTransform rectTransform, float val, int recursiveLevel = 0)
        {
            if (rectTransform.childCount > 0)
            {
                foreach (RectTransform child in rectTransform)
                {
                    UnityEngine.UI.MaskableGraphic uiImage = child.GetComponent<UnityEngine.UI.Image>();
                    if (uiImage != null)
                    {
                        Color c = uiImage.color;
                        c.a = val;
                        uiImage.color = c;
                    }
                    else
                    {
                        uiImage = child.GetComponent<UnityEngine.UI.RawImage>();
                        if (uiImage != null)
                        {
                            Color c = uiImage.color;
                            c.a = val;
                            uiImage.color = c;
                        }
                    }

                    alphaRecursive(child, val, recursiveLevel + 1);
                }
            }
        }

        private static void alphaRecursiveSprite(Transform transform, float val)
        {
            if (transform.childCount > 0)
            {
                foreach (Transform child in transform)
                {
                    SpriteRenderer ren = child.GetComponent<SpriteRenderer>();
                    if (ren != null)
                        ren.color = new Color(ren.color.r, ren.color.g, ren.color.b, val);
                    alphaRecursiveSprite(child, val);
                }
            }
        }

        private static void colorRecursiveSprite(Transform transform, Color toColor)
        {
            if (transform.childCount > 0)
            {
                foreach (Transform child in transform)
                {
                    SpriteRenderer ren = transform.gameObject.GetComponent<SpriteRenderer>();
                    if (ren != null)
                        ren.color = toColor;
                    colorRecursiveSprite(child, toColor);
                }
            }
        }

        private static void colorRecursive(RectTransform rectTransform, Color toColor)
        {
            if (rectTransform.childCount > 0)
            {
                foreach (RectTransform child in rectTransform)
                {
                    UnityEngine.UI.MaskableGraphic uiImage = child.GetComponent<UnityEngine.UI.Image>();
                    if (uiImage != null)
                    {
                        uiImage.color = toColor;
                    }
                    else
                    {
                        uiImage = child.GetComponent<UnityEngine.UI.RawImage>();
                        if (uiImage != null)
                            uiImage.color = toColor;
                    }

                    colorRecursive(child, toColor);
                }
            }
        }

        private static void textAlphaChildrenRecursive(Transform trans, float val, bool useRecursion = true)
        {
            if (useRecursion && trans.childCount > 0)
            {
                foreach (Transform child in trans)
                {
                    UnityEngine.UI.Text uiText = child.GetComponent<UnityEngine.UI.Text>();
                    if (uiText != null)
                    {
                        Color c = uiText.color;
                        c.a = val;
                        uiText.color = c;
                    }

                    textAlphaChildrenRecursive(child, val);
                }
            }
        }

        private static void textAlphaRecursive(Transform trans, float val, bool useRecursion = true)
        {
            UnityEngine.UI.Text uiText = trans.GetComponent<UnityEngine.UI.Text>();
            if (uiText != null)
            {
                Color c = uiText.color;
                c.a = val;
                uiText.color = c;
            }

            if (useRecursion && trans.childCount > 0)
            {
                foreach (Transform child in trans)
                {
                    textAlphaRecursive(child, val);
                }
            }
        }

        private static void textColorRecursive(Transform trans, Color toColor)
        {
            if (trans.childCount > 0)
            {
                foreach (Transform child in trans)
                {
                    UnityEngine.UI.Text uiText = child.GetComponent<UnityEngine.UI.Text>();
                    if (uiText != null)
                    {
                        uiText.color = toColor;
                    }

                    textColorRecursive(child, toColor);
                }
            }
        }
#endif

        private static Color tweenColor(LTDescr tween, float val)
        {
            Vector3 diff3 = tween._optional.point - tween._optional.axis;
            float diffAlpha = tween.to.y - tween.from.y;
            return new Color(tween._optional.axis.x + diff3.x * val, tween._optional.axis.y + diff3.y * val,
                tween._optional.axis.z + diff3.z * val, tween.from.y + diffAlpha * val);
        }

        /**
	* Pause a tween
	* 
	* @method pause
	* @return {LTDescr} LTDescr an object that distinguishes the tween
	*/
        public LTDescr pause()
        {
            if (this.direction != 0.0f)
            {
                // check if tween is already paused
                this.directionLast = this.direction;
                this.direction = 0.0f;
            }

            return this;
        }

        /**
	* Resume a paused tween
	* 
	* @method resume
	* @return {LTDescr} LTDescr an object that distinguishes the tween
	*/
        public LTDescr resume()
        {
            this.direction = this.directionLast;

            return this;
        }

        /**
	* Set Axis optional axis for tweens where it is relevant
	* 
	* @method setAxis
	* @param {Vector3} axis either the tween rotates around, or the direction it faces in the case of setOrientToPath
	* @return {LTDescr} LTDescr an object that distinguishes the tween
	* @example
	* LeanTween.move( ltLogo, path, 1.0f ).setEase(LeanTweenType.easeOutQuad).setOrientToPath(true).setAxis(Vector3.forward);
	*/
        public LTDescr setAxis(Vector3 axis)
        {
            this._optional.axis = axis;
            return this;
        }

        /**
	* Delay the start of a tween
	* 
	* @method setDelay
	* @param {float} float time The time to complete the tween in
	* @return {LTDescr} LTDescr an object that distinguishes the tween
	* @example
	* LeanTween.moveX(gameObject, 5f, 2.0f ).setDelay( 1.5f );
	*/
        public LTDescr setDelay(float delay)
        {
            this.delay = delay;

            return this;
        }

        /**
	* Set the type of easing used for the tween. <br>
	* <ul><li><a href="LeanTweenType.html">List of all the ease types</a>.</li>
	* <li><a href="http://www.robertpenner.com/easing/easing_demo.html">This page helps visualize the different easing equations</a></li>
	* </ul>
	* 
	* @method setEase
	* @param {LeanTweenType} easeType:LeanTweenType the easing type to use
	* @return {LTDescr} LTDescr an object that distinguishes the tween
	* @example
	* LeanTween.moveX(gameObject, 5f, 2.0f ).setEase( LeanTweenType.easeInBounce );
	*/
        public LTDescr setEase(LeanTweenType easeType)
        {
            switch (easeType)
            {
                case LeanTweenType.linear:
                    setEaseLinear();
                    break;
                case LeanTweenType.easeOutQuad:
                    setEaseOutQuad();
                    break;
                case LeanTweenType.easeInQuad:
                    setEaseInQuad();
                    break;
                case LeanTweenType.easeInOutQuad:
                    setEaseInOutQuad();
                    break;
                case LeanTweenType.easeInCubic:
                    setEaseInCubic();
                    break;
                case LeanTweenType.easeOutCubic:
                    setEaseOutCubic();
                    break;
                case LeanTweenType.easeInOutCubic:
                    setEaseInOutCubic();
                    break;
                case LeanTweenType.easeInQuart:
                    setEaseInQuart();
                    break;
                case LeanTweenType.easeOutQuart:
                    setEaseOutQuart();
                    break;
                case LeanTweenType.easeInOutQuart:
                    setEaseInOutQuart();
                    break;
                case LeanTweenType.easeInQuint:
                    setEaseInQuint();
                    break;
                case LeanTweenType.easeOutQuint:
                    setEaseOutQuint();
                    break;
                case LeanTweenType.easeInOutQuint:
                    setEaseInOutQuint();
                    break;
                case LeanTweenType.easeInSine:
                    setEaseInSine();
                    break;
                case LeanTweenType.easeOutSine:
                    setEaseOutSine();
                    break;
                case LeanTweenType.easeInOutSine:
                    setEaseInOutSine();
                    break;
                case LeanTweenType.easeInExpo:
                    setEaseInExpo();
                    break;
                case LeanTweenType.easeOutExpo:
                    setEaseOutExpo();
                    break;
                case LeanTweenType.easeInOutExpo:
                    setEaseInOutExpo();
                    break;
                case LeanTweenType.easeInCirc:
                    setEaseInCirc();
                    break;
                case LeanTweenType.easeOutCirc:
                    setEaseOutCirc();
                    break;
                case LeanTweenType.easeInOutCirc:
                    setEaseInOutCirc();
                    break;
                case LeanTweenType.easeInBounce:
                    setEaseInBounce();
                    break;
                case LeanTweenType.easeOutBounce:
                    setEaseOutBounce();
                    break;
                case LeanTweenType.easeInOutBounce:
                    setEaseInOutBounce();
                    break;
                case LeanTweenType.easeInBack:
                    setEaseInBack();
                    break;
                case LeanTweenType.easeOutBack:
                    setEaseOutBack();
                    break;
                case LeanTweenType.easeInOutBack:
                    setEaseInOutBack();
                    break;
                case LeanTweenType.easeInElastic:
                    setEaseInElastic();
                    break;
                case LeanTweenType.easeOutElastic:
                    setEaseOutElastic();
                    break;
                case LeanTweenType.easeInOutElastic:
                    setEaseInOutElastic();
                    break;
                case LeanTweenType.punch:
                    setEasePunch();
                    break;
                case LeanTweenType.easeShake:
                    setEaseShake();
                    break;
                case LeanTweenType.easeSpring:
                    setEaseSpring();
                    break;
                default:
                    setEaseLinear();
                    break;
            }

            return this;
        }

        public LTDescr setEaseLinear()
        {
            this.easeType = LeanTweenType.linear;
            this.easeMethod = this.easeLinear;
            return this;
        }

        public LTDescr setEaseSpring()
        {
            this.easeType = LeanTweenType.easeSpring;
            this.easeMethod = this.easeSpring;
            return this;
        }

        public LTDescr setEaseInQuad()
        {
            this.easeType = LeanTweenType.easeInQuad;
            this.easeMethod = this.easeInQuad;
            return this;
        }

        public LTDescr setEaseOutQuad()
        {
            this.easeType = LeanTweenType.easeOutQuad;
            this.easeMethod = this.easeOutQuad;
            return this;
        }

        public LTDescr setEaseInOutQuad()
        {
            this.easeType = LeanTweenType.easeInOutQuad;
            this.easeMethod = this.easeInOutQuad;
            return this;
        }

        public LTDescr setEaseInCubic()
        {
            this.easeType = LeanTweenType.easeInCubic;
            this.easeMethod = this.easeInCubic;
            return this;
        }

        public LTDescr setEaseOutCubic()
        {
            this.easeType = LeanTweenType.easeOutCubic;
            this.easeMethod = this.easeOutCubic;
            return this;
        }

        public LTDescr setEaseInOutCubic()
        {
            this.easeType = LeanTweenType.easeInOutCubic;
            this.easeMethod = this.easeInOutCubic;
            return this;
        }

        public LTDescr setEaseInQuart()
        {
            this.easeType = LeanTweenType.easeInQuart;
            this.easeMethod = this.easeInQuart;
            return this;
        }

        public LTDescr setEaseOutQuart()
        {
            this.easeType = LeanTweenType.easeOutQuart;
            this.easeMethod = this.easeOutQuart;
            return this;
        }

        public LTDescr setEaseInOutQuart()
        {
            this.easeType = LeanTweenType.easeInOutQuart;
            this.easeMethod = this.easeInOutQuart;
            return this;
        }

        public LTDescr setEaseInQuint()
        {
            this.easeType = LeanTweenType.easeInQuint;
            this.easeMethod = this.easeInQuint;
            return this;
        }

        public LTDescr setEaseOutQuint()
        {
            this.easeType = LeanTweenType.easeOutQuint;
            this.easeMethod = this.easeOutQuint;
            return this;
        }

        public LTDescr setEaseInOutQuint()
        {
            this.easeType = LeanTweenType.easeInOutQuint;
            this.easeMethod = this.easeInOutQuint;
            return this;
        }

        public LTDescr setEaseInSine()
        {
            this.easeType = LeanTweenType.easeInSine;
            this.easeMethod = this.easeInSine;
            return this;
        }

        public LTDescr setEaseOutSine()
        {
            this.easeType = LeanTweenType.easeOutSine;
            this.easeMethod = this.easeOutSine;
            return this;
        }

        public LTDescr setEaseInOutSine()
        {
            this.easeType = LeanTweenType.easeInOutSine;
            this.easeMethod = this.easeInOutSine;
            return this;
        }

        public LTDescr setEaseInExpo()
        {
            this.easeType = LeanTweenType.easeInExpo;
            this.easeMethod = this.easeInExpo;
            return this;
        }

        public LTDescr setEaseOutExpo()
        {
            this.easeType = LeanTweenType.easeOutExpo;
            this.easeMethod = this.easeOutExpo;
            return this;
        }

        public LTDescr setEaseInOutExpo()
        {
            this.easeType = LeanTweenType.easeInOutExpo;
            this.easeMethod = this.easeInOutExpo;
            return this;
        }

        public LTDescr setEaseInCirc()
        {
            this.easeType = LeanTweenType.easeInCirc;
            this.easeMethod = this.easeInCirc;
            return this;
        }

        public LTDescr setEaseOutCirc()
        {
            this.easeType = LeanTweenType.easeOutCirc;
            this.easeMethod = this.easeOutCirc;
            return this;
        }

        public LTDescr setEaseInOutCirc()
        {
            this.easeType = LeanTweenType.easeInOutCirc;
            this.easeMethod = this.easeInOutCirc;
            return this;
        }

        public LTDescr setEaseInBounce()
        {
            this.easeType = LeanTweenType.easeInBounce;
            this.easeMethod = this.easeInBounce;
            return this;
        }

        public LTDescr setEaseOutBounce()
        {
            this.easeType = LeanTweenType.easeOutBounce;
            this.easeMethod = this.easeOutBounce;
            return this;
        }

        public LTDescr setEaseInOutBounce()
        {
            this.easeType = LeanTweenType.easeInOutBounce;
            this.easeMethod = this.easeInOutBounce;
            return this;
        }

        public LTDescr setEaseInBack()
        {
            this.easeType = LeanTweenType.easeInBack;
            this.easeMethod = this.easeInBack;
            return this;
        }

        public LTDescr setEaseOutBack()
        {
            this.easeType = LeanTweenType.easeOutBack;
            this.easeMethod = this.easeOutBack;
            return this;
        }

        public LTDescr setEaseInOutBack()
        {
            this.easeType = LeanTweenType.easeInOutBack;
            this.easeMethod = this.easeInOutBack;
            return this;
        }

        public LTDescr setEaseInElastic()
        {
            this.easeType = LeanTweenType.easeInElastic;
            this.easeMethod = this.easeInElastic;
            return this;
        }

        public LTDescr setEaseOutElastic()
        {
            this.easeType = LeanTweenType.easeOutElastic;
            this.easeMethod = this.easeOutElastic;
            return this;
        }

        public LTDescr setEaseInOutElastic()
        {
            this.easeType = LeanTweenType.easeInOutElastic;
            this.easeMethod = this.easeInOutElastic;
            return this;
        }

        public LTDescr setEasePunch()
        {
            this._optional.animationCurve = LeanTween.punch;
            this.toInternal.x = this.from.x + this.to.x;
            this.easeMethod = this.tweenOnCurve;
            return this;
        }

        public LTDescr setEaseShake()
        {
            this._optional.animationCurve = LeanTween.shake;
            this.toInternal.x = this.from.x + this.to.x;
            this.easeMethod = this.tweenOnCurve;
            return this;
        }

        private Vector3 tweenOnCurve()
        {
            return new Vector3(this.from.x + (this.diff.x) * this._optional.animationCurve.Evaluate(ratioPassed),
                this.from.y + (this.diff.y) * this._optional.animationCurve.Evaluate(ratioPassed),
                this.from.z + (this.diff.z) * this._optional.animationCurve.Evaluate(ratioPassed));
        }

        // Vector3 Ease Methods

        private Vector3 easeInOutQuad()
        {
            val = this.ratioPassed * 2f;

            if (val < 1f)
            {
                val = val * val;
                return new Vector3(this.diffDiv2.x * val + this.from.x, this.diffDiv2.y * val + this.from.y,
                    this.diffDiv2.z * val + this.from.z);
            }

            val = (1f - val) * (val - 3f) + 1f;
            return new Vector3(this.diffDiv2.x * val + this.from.x, this.diffDiv2.y * val + this.from.y,
                this.diffDiv2.z * val + this.from.z);
        }

        private Vector3 easeInQuad()
        {
            val = ratioPassed * ratioPassed;
            return new Vector3(this.diff.x * val + this.from.x, this.diff.y * val + this.from.y,
                this.diff.z * val + this.from.z);
        }

        private Vector3 easeOutQuad()
        {
            val = this.ratioPassed;
            val = -val * (val - 2f);
            return (this.diff * val + this.from);
        }

        private Vector3 easeLinear()
        {
            val = this.ratioPassed;
            return new Vector3(this.from.x + this.diff.x * val, this.from.y + this.diff.y * val,
                this.from.z + this.diff.z * val);
        }

        private Vector3 easeSpring()
        {
            val = Mathf.Clamp01(this.ratioPassed);
            val = (Mathf.Sin(val * Mathf.PI * (0.2f + 2.5f * val * val * val)) * Mathf.Pow(1f - val, 2.2f) + val) *
                  (1f + (1.2f * (1f - val)));
            return this.from + this.diff * val;
        }

        private Vector3 easeInCubic()
        {
            val = this.ratioPassed * this.ratioPassed * this.ratioPassed;
            return new Vector3(this.diff.x * val + this.from.x, this.diff.y * val + this.from.y,
                this.diff.z * val + this.from.z);
        }

        private Vector3 easeOutCubic()
        {
            val = this.ratioPassed - 1f;
            val = (val * val * val + 1);
            return new Vector3(this.diff.x * val + this.from.x, this.diff.y * val + this.from.y,
                this.diff.z * val + this.from.z);
        }

        private Vector3 easeInOutCubic()
        {
            val = this.ratioPassed * 2f;
            if (val < 1f)
            {
                val = val * val * val;
                return new Vector3(this.diffDiv2.x * val + this.from.x, this.diffDiv2.y * val + this.from.y,
                    this.diffDiv2.z * val + this.from.z);
            }

            val -= 2f;
            val = val * val * val + 2f;
            return new Vector3(this.diffDiv2.x * val + this.from.x, this.diffDiv2.y * val + this.from.y,
                this.diffDiv2.z * val + this.from.z);
        }

        private Vector3 easeInQuart()
        {
            val = this.ratioPassed * this.ratioPassed * this.ratioPassed * this.ratioPassed;
            return diff * val + this.from;
        }

        private Vector3 easeOutQuart()
        {
            val = this.ratioPassed - 1f;
            val = -(val * val * val * val - 1);
            return new Vector3(this.diff.x * val + this.from.x, this.diff.y * val + this.from.y,
                this.diff.z * val + this.from.z);
        }

        private Vector3 easeInOutQuart()
        {
            val = this.ratioPassed * 2f;
            if (val < 1f)
            {
                val = val * val * val * val;
                return new Vector3(this.diffDiv2.x * val + this.from.x, this.diffDiv2.y * val + this.from.y,
                    this.diffDiv2.z * val + this.from.z);
            }

            val -= 2f;
//		val = (val * val * val * val - 2f);
            return -this.diffDiv2 * (val * val * val * val - 2f) + this.from;
        }

        private Vector3 easeInQuint()
        {
            val = this.ratioPassed;
            val = val * val * val * val * val;
            return new Vector3(this.diff.x * val + this.from.x, this.diff.y * val + this.from.y,
                this.diff.z * val + this.from.z);
        }

        private Vector3 easeOutQuint()
        {
            val = this.ratioPassed - 1f;
            val = (val * val * val * val * val + 1f);
            return new Vector3(this.diff.x * val + this.from.x, this.diff.y * val + this.from.y,
                this.diff.z * val + this.from.z);
        }

        private Vector3 easeInOutQuint()
        {
            val = this.ratioPassed * 2f;
            if (val < 1f)
            {
                val = val * val * val * val * val;
                return new Vector3(this.diffDiv2.x * val + this.from.x, this.diffDiv2.y * val + this.from.y,
                    this.diffDiv2.z * val + this.from.z);
            }

            val -= 2f;
            val = (val * val * val * val * val + 2f);
            return new Vector3(this.diffDiv2.x * val + this.from.x, this.diffDiv2.y * val + this.from.y,
                this.diffDiv2.z * val + this.from.z);
        }

        private Vector3 easeInSine()
        {
            val = -Mathf.Cos(this.ratioPassed * LeanTween.PI_DIV2);
            return new Vector3(this.diff.x * val + this.diff.x + this.from.x,
                this.diff.y * val + this.diff.y + this.from.y,
                this.diff.z * val + this.diff.z + this.from.z);
        }

        private Vector3 easeOutSine()
        {
            val = Mathf.Sin(this.ratioPassed * LeanTween.PI_DIV2);
            return new Vector3(this.diff.x * val + this.from.x, this.diff.y * val + this.from.y,
                this.diff.z * val + this.from.z);
        }

        private Vector3 easeInOutSine()
        {
            val = -(Mathf.Cos(Mathf.PI * this.ratioPassed) - 1f);
            return new Vector3(this.diffDiv2.x * val + this.from.x, this.diffDiv2.y * val + this.from.y,
                this.diffDiv2.z * val + this.from.z);
        }

        private Vector3 easeInExpo()
        {
            val = Mathf.Pow(2f, 10f * (this.ratioPassed - 1f));
            return new Vector3(this.diff.x * val + this.from.x, this.diff.y * val + this.from.y,
                this.diff.z * val + this.from.z);
        }

        private Vector3 easeOutExpo()
        {
            val = (-Mathf.Pow(2f, -10f * this.ratioPassed) + 1f);
            return new Vector3(this.diff.x * val + this.from.x, this.diff.y * val + this.from.y,
                this.diff.z * val + this.from.z);
        }

        private Vector3 easeInOutExpo()
        {
            val = this.ratioPassed * 2f;
            if (val < 1) return this.diffDiv2 * Mathf.Pow(2, 10 * (val - 1)) + this.from;
            val--;
            return this.diffDiv2 * (-Mathf.Pow(2, -10 * val) + 2) + this.from;
        }

        private Vector3 easeInCirc()
        {
            val = -(Mathf.Sqrt(1f - this.ratioPassed * this.ratioPassed) - 1f);
            return new Vector3(this.diff.x * val + this.from.x, this.diff.y * val + this.from.y,
                this.diff.z * val + this.from.z);
        }

        private Vector3 easeOutCirc()
        {
            val = this.ratioPassed - 1f;
            val = Mathf.Sqrt(1f - val * val);

            return new Vector3(this.diff.x * val + this.from.x, this.diff.y * val + this.from.y,
                this.diff.z * val + this.from.z);
        }

        private Vector3 easeInOutCirc()
        {
            val = this.ratioPassed * 2f;
            if (val < 1f)
            {
                val = -(Mathf.Sqrt(1f - val * val) - 1f);
                return new Vector3(this.diffDiv2.x * val + this.from.x, this.diffDiv2.y * val + this.from.y,
                    this.diffDiv2.z * val + this.from.z);
            }

            val -= 2f;
            val = (Mathf.Sqrt(1f - val * val) + 1f);
            return new Vector3(this.diffDiv2.x * val + this.from.x, this.diffDiv2.y * val + this.from.y,
                this.diffDiv2.z * val + this.from.z);
        }

        private Vector3 easeInBounce()
        {
            val = this.ratioPassed;
            val = 1f - val;
            return new Vector3(this.diff.x - LeanTween.easeOutBounce(0, this.diff.x, val) + this.from.x,
                this.diff.y - LeanTween.easeOutBounce(0, this.diff.y, val) + this.from.y,
                this.diff.z - LeanTween.easeOutBounce(0, this.diff.z, val) + this.from.z);
        }

        private Vector3 easeOutBounce()
        {
            val = ratioPassed;
            float valM, valN; // bounce values
            if (val < (valM = 1 - 1.75f * this.overshoot / 2.75f))
            {
                val = 1 / valM / valM * val * val;
            }
            else if (val < (valN = 1 - .75f * this.overshoot / 2.75f))
            {
                val -= (valM + valN) / 2;
                // first bounce, height: 1/4
                val = 7.5625f * val * val + 1 - .25f * this.overshoot * this.overshoot;
            }
            else if (val < (valM = 1 - .25f * this.overshoot / 2.75f))
            {
                val -= (valM + valN) / 2;
                // second bounce, height: 1/16
                val = 7.5625f * val * val + 1 - .0625f * this.overshoot * this.overshoot;
            }
            else
            {
                // valN = 1
                val -= (valM + 1) / 2;
                // third bounce, height: 1/64
                val = 7.5625f * val * val + 1 - .015625f * this.overshoot * this.overshoot;
            }

            return this.diff * val + this.from;
        }

        private Vector3 easeInOutBounce()
        {
            val = this.ratioPassed * 2f;
            if (val < 1f)
            {
                return new Vector3(LeanTween.easeInBounce(0, this.diff.x, val) * 0.5f + this.from.x,
                    LeanTween.easeInBounce(0, this.diff.y, val) * 0.5f + this.from.y,
                    LeanTween.easeInBounce(0, this.diff.z, val) * 0.5f + this.from.z);
            }
            else
            {
                val = val - 1f;
                return new Vector3(LeanTween.easeOutBounce(0, this.diff.x, val) * 0.5f + this.diffDiv2.x + this.from.x,
                    LeanTween.easeOutBounce(0, this.diff.y, val) * 0.5f + this.diffDiv2.y + this.from.y,
                    LeanTween.easeOutBounce(0, this.diff.z, val) * 0.5f + this.diffDiv2.z + this.from.z);
            }
        }

        private Vector3 easeInBack()
        {
            val = this.ratioPassed;
            val /= 1;
            float s = 1.70158f * this.overshoot;
            return this.diff * (val) * val * ((s + 1) * val - s) + this.from;
        }

        private Vector3 easeOutBack()
        {
            float s = 1.70158f * this.overshoot;
            val = (this.ratioPassed / 1) - 1;
            val = ((val) * val * ((s + 1) * val + s) + 1);
            return this.diff * val + this.from;
        }

        private Vector3 easeInOutBack()
        {
            float s = 1.70158f * this.overshoot;
            val = this.ratioPassed * 2f;
            if ((val) < 1)
            {
                s *= (1.525f) * overshoot;
                return this.diffDiv2 * (val * val * (((s) + 1) * val - s)) + this.from;
            }

            val -= 2;
            s *= (1.525f) * overshoot;
            val = ((val) * val * (((s) + 1) * val + s) + 2);
            return this.diffDiv2 * val + this.from;
        }

        private Vector3 easeInElastic()
        {
            return
                new Vector3(
                    LeanTween.easeInElastic(this.from.x, this.to.x, this.ratioPassed, this.overshoot, this.period),
                    LeanTween.easeInElastic(this.from.y, this.to.y, this.ratioPassed, this.overshoot, this.period),
                    LeanTween.easeInElastic(this.from.z, this.to.z, this.ratioPassed, this.overshoot, this.period));
        }

        private Vector3 easeOutElastic()
        {
            return
                new Vector3(
                    LeanTween.easeOutElastic(this.from.x, this.to.x, this.ratioPassed, this.overshoot, this.period),
                    LeanTween.easeOutElastic(this.from.y, this.to.y, this.ratioPassed, this.overshoot, this.period),
                    LeanTween.easeOutElastic(this.from.z, this.to.z, this.ratioPassed, this.overshoot, this.period));
        }

        private Vector3 easeInOutElastic()
        {
            return
                new Vector3(
                    LeanTween.easeInOutElastic(this.from.x, this.to.x, this.ratioPassed, this.overshoot, this.period),
                    LeanTween.easeInOutElastic(this.from.y, this.to.y, this.ratioPassed, this.overshoot, this.period),
                    LeanTween.easeInOutElastic(this.from.z, this.to.z, this.ratioPassed, this.overshoot, this.period));
        }

        /**
	* Set how far past a tween will overshoot  for certain ease types (compatible:  easeInBack, easeInOutBack, easeOutBack, easeOutElastic, easeInElastic, easeInOutElastic). <br>
	* @method setOvershoot
	* @param {float} overshoot:float how far past the destination it will go before settling in
	* @return {LTDescr} LTDescr an object that distinguishes the tween
	* @example
	* LeanTween.moveX(gameObject, 5f, 2.0f ).setEase( LeanTweenType.easeOutBack ).setOvershoot(2f);
	*/
        public LTDescr setOvershoot(float overshoot)
        {
            this.overshoot = overshoot;
            return this;
        }

        /**
	* Set how short the iterations are for certain ease types (compatible: easeOutElastic, easeInElastic, easeInOutElastic). <br>
	* @method setPeriod
	* @param {float} period:float how short the iterations are that the tween will animate at (default 0.3f)
	* @return {LTDescr} LTDescr an object that distinguishes the tween
	* @example
	* LeanTween.moveX(gameObject, 5f, 2.0f ).setEase( LeanTweenType.easeOutElastic ).setPeriod(0.3f);
	*/
        public LTDescr setPeriod(float period)
        {
            this.period = period;
            return this;
        }

        /**
    * Set how large the effect is for certain ease types (compatible: punch, shake, animation curves). <br>
    * @method setScale
    * @param {float} scale:float how much the ease will be multiplied by (default 1f)
    * @return {LTDescr} LTDescr an object that distinguishes the tween
    * @example
    * LeanTween.moveX(gameObject, 5f, 2.0f ).setEase( LeanTweenType.punch ).setScale(2f);
    */
        public LTDescr setScale(float scale)
        {
            this.scale = scale;
            return this;
        }

        /**
	* Set the type of easing used for the tween with a custom curve. <br>
	* @method setEase (AnimationCurve)
	* @param {AnimationCurve} easeDefinition:AnimationCurve an <a href="http://docs.unity3d.com/Documentation/ScriptReference/AnimationCurve.html" target="_blank">AnimationCure</a> that describes the type of easing you want, this is great for when you want a unique type of movement
	* @return {LTDescr} LTDescr an object that distinguishes the tween
	* @example
	* LeanTween.moveX(gameObject, 5f, 2.0f ).setEase( LeanTweenType.easeInBounce );
	*/
        public LTDescr setEase(AnimationCurve easeCurve)
        {
            this._optional.animationCurve = easeCurve;
            this.easeMethod = this.tweenOnCurve;
            this.easeType = LeanTweenType.animationCurve;
            return this;
        }

        /**
	* Set the end that the GameObject is tweening towards
	* @method setTo
	* @param {Vector3} to:Vector3 point at which you want the tween to reach
	* @return {LTDescr} LTDescr an object that distinguishes the tween
	* @example
	* LTDescr descr = LeanTween.move( cube, Vector3.up, new Vector3(1f,3f,0f), 1.0f ).setEase( LeanTweenType.easeInOutBounce );<br>
	* // Later your want to change your destination or your destiation is constantly moving<br>
	* descr.setTo( new Vector3(5f,10f,3f) );<br>
	*/
        public LTDescr setTo(Vector3 to)
        {
            if (this.hasInitiliazed)
            {
                this.to = to;
                this.diff = to - this.from;
            }
            else
            {
                this.to = to;
            }

            return this;
        }

        public LTDescr setTo(Transform to)
        {
            this._optional.toTrans = to;
            return this;
        }

        /**
	* Set the beginning of the tween
	* @method setFrom
	* @param {Vector3} from:Vector3 the point you would like the tween to start at
	* @return {LTDescr} LTDescr an object that distinguishes the tween
	* @example
	* LTDescr descr = LeanTween.move( cube, Vector3.up, new Vector3(1f,3f,0f), 1.0f ).setFrom( new Vector3(5f,10f,3f) );<br>
	*/
        public LTDescr setFrom(Vector3 from)
        {
            if (this.trans)
            {
                this.init();
            }

            this.from = from;
            // this.hasInitiliazed = true; // this is set, so that the "from" value isn't overwritten later on when the tween starts
            this.diff = this.to - this.from;
            this.diffDiv2 = this.diff * 0.5f;
            return this;
        }

        public LTDescr setFrom(float from)
        {
            return setFrom(new Vector3(from, 0f, 0f));
        }

        public LTDescr setDiff(Vector3 diff)
        {
            this.diff = diff;
            return this;
        }

        public LTDescr setHasInitialized(bool has)
        {
            this.hasInitiliazed = has;
            return this;
        }

        public LTDescr setId(uint id, uint global_counter)
        {
            this._id = id;
            this.counter = global_counter;
            // Debug.Log("Global counter:"+global_counter);
            return this;
        }

        /**
	* Set the point of time the tween will start in
	* @method setPassed
	* @param {float} passedTime:float the length of time in seconds the tween will start in
	* @return {LTDescr} LTDescr an object that distinguishes the tween
	* @example
	* int tweenId = LeanTween.moveX(gameObject, 5f, 2.0f ).id;<br>
	* // Later<br>
	* LTDescr descr = description( tweenId );<br>
	* descr.setPassed( 1f );<br>
	*/
        public LTDescr setPassed(float passed)
        {
            this.passed = passed;
            return this;
        }

        /**
	* Set the finish time of the tween
	* @method setTime
	* @param {float} finishTime:float the length of time in seconds you wish the tween to complete in
	* @return {LTDescr} LTDescr an object that distinguishes the tween
	* @example
	* int tweenId = LeanTween.moveX(gameObject, 5f, 2.0f ).id;<br>
	* // Later<br>
	* LTDescr descr = description( tweenId );<br>
	* descr.setTime( 1f );<br>
	*/
        public LTDescr setTime(float time)
        {
            float passedTimeRatio = this.passed / this.time;
            this.passed = time * passedTimeRatio;
            this.time = time;
            return this;
        }

        /**
	* Set the finish time of the tween
	* @method setSpeed
	* @param {float} speed:float the speed in unity units per second you wish the object to travel (overrides the given time)
	* @return {LTDescr} LTDescr an object that distinguishes the tween
	* @example
	* LeanTween.moveLocalZ( gameObject, 10f, 1f).setSpeed(0.2f) // the given time is ignored when speed is set<br>
	*/
        public LTDescr setSpeed(float speed)
        {
            this.speed = speed;
            if (this.hasInitiliazed)
                initSpeed();
            return this;
        }

        /**
	* Set the tween to repeat a number of times.
	* @method setRepeat
	* @param {int} repeatNum:int the number of times to repeat the tween. -1 to repeat infinite times
	* @return {LTDescr} LTDescr an object that distinguishes the tween
	* @example
	* LeanTween.moveX(gameObject, 5f, 2.0f ).setRepeat( 10 ).setLoopPingPong();
	*/
        public LTDescr setRepeat(int repeat)
        {
            this.loopCount = repeat;
            if ((repeat > 1 && this.loopType == LeanTweenType.once) ||
                (repeat < 0 && this.loopType == LeanTweenType.once))
            {
                this.loopType = LeanTweenType.clamp;
            }

            if (this.type == TweenAction.CALLBACK || this.type == TweenAction.CALLBACK_COLOR)
            {
                this.setOnCompleteOnRepeat(true);
            }

            return this;
        }

        public LTDescr setLoopType(LeanTweenType loopType)
        {
            this.loopType = loopType;
            return this;
        }

        public LTDescr setUseEstimatedTime(bool useEstimatedTime)
        {
            this.useEstimatedTime = useEstimatedTime;
            this.usesNormalDt = false;
            return this;
        }

        /**
	* Set ignore time scale when tweening an object when you want the animation to be time-scale independent (ignores the Time.timeScale value). Great for pause screens, when you want all other action to be stopped (or slowed down)
	* @method setIgnoreTimeScale
	* @param {bool} useUnScaledTime:bool whether to use the unscaled time or not
	* @return {LTDescr} LTDescr an object that distinguishes the tween
	* @example
	* LeanTween.moveX(gameObject, 5f, 2.0f ).setRepeat( 2 ).setIgnoreTimeScale( true );
	*/
        public LTDescr setIgnoreTimeScale(bool useUnScaledTime)
        {
            this.useEstimatedTime = useUnScaledTime;
            this.usesNormalDt = false;
            return this;
        }

        /**
	* Use frames when tweening an object, when you don't want the animation to be time-frame independent...
	* @method setUseFrames
	* @param {bool} useFrames:bool whether to use estimated time or not
	* @return {LTDescr} LTDescr an object that distinguishes the tween
	* @example
	* LeanTween.moveX(gameObject, 5f, 2.0f ).setRepeat( 2 ).setUseFrames( true );
	*/
        public LTDescr setUseFrames(bool useFrames)
        {
            this.useFrames = useFrames;
            this.usesNormalDt = false;
            return this;
        }

        public LTDescr setUseManualTime(bool useManualTime)
        {
            this.useManualTime = useManualTime;
            this.usesNormalDt = false;
            return this;
        }

        public LTDescr setLoopCount(int loopCount)
        {
            this.loopType = LeanTweenType.clamp;
            this.loopCount = loopCount;
            return this;
        }

        /**
	* No looping involved, just run once (the default)
	* @method setLoopOnce
	* @return {LTDescr} LTDescr an object that distinguishes the tween
	* @example
	* LeanTween.moveX(gameObject, 5f, 2.0f ).setLoopOnce();
	*/
        public LTDescr setLoopOnce()
        {
            this.loopType = LeanTweenType.once;
            return this;
        }

        /**
	* When the animation gets to the end it starts back at where it began
	* @method setLoopClamp
	* @param {int} loops:int (defaults to -1) how many times you want the loop to happen (-1 for an infinite number of times)
	* @return {LTDescr} LTDescr an object that distinguishes the tween
	* @example
	* LeanTween.moveX(gameObject, 5f, 2.0f ).setLoopClamp( 2 );
	*/
        public LTDescr setLoopClamp()
        {
            this.loopType = LeanTweenType.clamp;
            if (this.loopCount == 0)
                this.loopCount = -1;
            return this;
        }

        public LTDescr setLoopClamp(int loops)
        {
            this.loopCount = loops;
            return this;
        }

        /**
	* When the animation gets to the end it then tweens back to where it started (and on, and on)
	* @method setLoopPingPong
	* @param {int} loops:int (defaults to -1) how many times you want the loop to happen in both directions (-1 for an infinite number of times). Passing a value of 1 will cause the object to go towards and back from it's destination once.
	* @return {LTDescr} LTDescr an object that distinguishes the tween
	* @example
	* LeanTween.moveX(gameObject, 5f, 2.0f ).setLoopPingPong( 2 );
	*/
        public LTDescr setLoopPingPong()
        {
            this.loopType = LeanTweenType.pingPong;
            if (this.loopCount == 0)
                this.loopCount = -1;
            return this;
        }

        public LTDescr setLoopPingPong(int loops)
        {
            this.loopType = LeanTweenType.pingPong;
            this.loopCount = loops == -1 ? loops : loops * 2;
            return this;
        }

        /**
	* Have a method called when the tween finishes
	* @method setOnComplete
	* @param {Action} onComplete:Action the method that should be called when the tween is finished ex: tweenFinished(){ }
	* @return {LTDescr} LTDescr an object that distinguishes the tween
	* @example
	* LeanTween.moveX(gameObject, 5f, 2.0f ).setOnComplete( tweenFinished );
	*/
        public LTDescr setOnComplete(Action onComplete)
        {
            this._optional.onComplete = onComplete;
            this.hasExtraOnCompletes = true;
            return this;
        }

        /**
	* Have a method called when the tween finishes
	* @method setOnComplete (object)
	* @param {Action<object>} onComplete:Action<object> the method that should be called when the tween is finished ex: tweenFinished( object myObj ){ }
	* @return {LTDescr} LTDescr an object that distinguishes the tween
	* @example
	* object tweenFinishedObj = "hi" as object;
	* LeanTween.moveX(gameObject, 5f, 2.0f ).setOnComplete( tweenFinished, tweenFinishedObj );
	*/
        public LTDescr setOnComplete(Action<object> onComplete)
        {
            this._optional.onCompleteObject = onComplete;
            this.hasExtraOnCompletes = true;
            return this;
        }

        public LTDescr setOnComplete(Action<object> onComplete, object onCompleteParam)
        {
            this._optional.onCompleteObject = onComplete;
            this.hasExtraOnCompletes = true;
            if (onCompleteParam != null)
                this._optional.onCompleteParam = onCompleteParam;
            return this;
        }

        /**
	* Pass an object to along with the onComplete Function
	* @method setOnCompleteParam
	* @param {object} onComplete:object an object that 
	* @return {LTDescr} LTDescr an object that distinguishes the tween
	* @example
	* LeanTween.delayedCall(1.5f, enterMiniGameStart).setOnCompleteParam( new object[]{""+5} );<br><br>
	* void enterMiniGameStart( object val ){<br>
    * &nbsp;object[] arr = (object [])val;<br>
    * &nbsp;int lvl = int.Parse((string)arr[0]);<br>
    * }<br>
	*/
        public LTDescr setOnCompleteParam(object onCompleteParam)
        {
            this._optional.onCompleteParam = onCompleteParam;
            this.hasExtraOnCompletes = true;
            return this;
        }


        /**
	* Have a method called on each frame that the tween is being animated (passes a float value)
	* @method setOnUpdate
	* @param {Action<float>} onUpdate:Action<float> a method that will be called on every frame with the float value of the tweened object
	* @return {LTDescr} LTDescr an object that distinguishes the tween
	* @example
	* LeanTween.moveX(gameObject, 5f, 2.0f ).setOnUpdate( tweenMoved );<br>
	* <br>
	* void tweenMoved( float val ){ }<br>
	*/
        public LTDescr setOnUpdate(Action<float> onUpdate)
        {
            this._optional.onUpdateFloat = onUpdate;
            this.hasUpdateCallback = true;
            return this;
        }

        public LTDescr setOnUpdateRatio(Action<float, float> onUpdate)
        {
            this._optional.onUpdateFloatRatio = onUpdate;
            this.hasUpdateCallback = true;
            return this;
        }

        public LTDescr setOnUpdateObject(Action<float, object> onUpdate)
        {
            this._optional.onUpdateFloatObject = onUpdate;
            this.hasUpdateCallback = true;
            return this;
        }

        public LTDescr setOnUpdateVector2(Action<Vector2> onUpdate)
        {
            this._optional.onUpdateVector2 = onUpdate;
            this.hasUpdateCallback = true;
            return this;
        }

        public LTDescr setOnUpdateVector3(Action<Vector3> onUpdate)
        {
            this._optional.onUpdateVector3 = onUpdate;
            this.hasUpdateCallback = true;
            return this;
        }

        public LTDescr setOnUpdateColor(Action<Color> onUpdate)
        {
            this._optional.onUpdateColor = onUpdate;
            this.hasUpdateCallback = true;
            return this;
        }

        public LTDescr setOnUpdateColor(Action<Color, object> onUpdate)
        {
            this._optional.onUpdateColorObject = onUpdate;
            this.hasUpdateCallback = true;
            return this;
        }

#if !UNITY_FLASH

        public LTDescr setOnUpdate(Action<Color> onUpdate)
        {
            this._optional.onUpdateColor = onUpdate;
            this.hasUpdateCallback = true;
            return this;
        }

        public LTDescr setOnUpdate(Action<Color, object> onUpdate)
        {
            this._optional.onUpdateColorObject = onUpdate;
            this.hasUpdateCallback = true;
            return this;
        }

        /**
	* Have a method called on each frame that the tween is being animated (passes a float value and a object)
	* @method setOnUpdate (object)
	* @param {Action<float,object>} onUpdate:Action<float,object> a method that will be called on every frame with the float value of the tweened object, and an object of the person's choosing
	* @return {LTDescr} LTDescr an object that distinguishes the tween
	* @example
	* LeanTween.moveX(gameObject, 5f, 2.0f ).setOnUpdate( tweenMoved ).setOnUpdateParam( myObject );<br>
	* <br>
	* void tweenMoved( float val, object obj ){ }<br>
	*/
        public LTDescr setOnUpdate(Action<float, object> onUpdate, object onUpdateParam = null)
        {
            this._optional.onUpdateFloatObject = onUpdate;
            this.hasUpdateCallback = true;
            if (onUpdateParam != null)
                this._optional.onUpdateParam = onUpdateParam;
            return this;
        }

        public LTDescr setOnUpdate(Action<Vector3, object> onUpdate, object onUpdateParam = null)
        {
            this._optional.onUpdateVector3Object = onUpdate;
            this.hasUpdateCallback = true;
            if (onUpdateParam != null)
                this._optional.onUpdateParam = onUpdateParam;
            return this;
        }

        public LTDescr setOnUpdate(Action<Vector2> onUpdate, object onUpdateParam = null)
        {
            this._optional.onUpdateVector2 = onUpdate;
            this.hasUpdateCallback = true;
            if (onUpdateParam != null)
                this._optional.onUpdateParam = onUpdateParam;
            return this;
        }

        /**
	* Have a method called on each frame that the tween is being animated (passes a float value)
	* @method setOnUpdate (Vector3)
	* @param {Action<Vector3>} onUpdate:Action<Vector3> a method that will be called on every frame with the float value of the tweened object
	* @return {LTDescr} LTDescr an object that distinguishes the tween
	* @example
	* LeanTween.moveX(gameObject, 5f, 2.0f ).setOnUpdate( tweenMoved );<br>
	* <br>
	* void tweenMoved( Vector3 val ){ }<br>
	*/
        public LTDescr setOnUpdate(Action<Vector3> onUpdate, object onUpdateParam = null)
        {
            this._optional.onUpdateVector3 = onUpdate;
            this.hasUpdateCallback = true;
            if (onUpdateParam != null)
                this._optional.onUpdateParam = onUpdateParam;
            return this;
        }
#endif


        /**
	* Have an object passed along with the onUpdate method
	* @method setOnUpdateParam
	* @param {object} onUpdateParam:object an object that will be passed along with the onUpdate method
	* @return {LTDescr} LTDescr an object that distinguishes the tween
	* @example
	* LeanTween.moveX(gameObject, 5f, 2.0f ).setOnUpdate( tweenMoved ).setOnUpdateParam( myObject );<br>
	* <br>
	* void tweenMoved( float val, object obj ){ }<br>
	*/
        public LTDescr setOnUpdateParam(object onUpdateParam)
        {
            this._optional.onUpdateParam = onUpdateParam;
            return this;
        }

        /**
	* While tweening along a curve, set this property to true, to be perpendicalur to the path it is moving upon
	* @method setOrientToPath
	* @param {bool} doesOrient:bool whether the gameobject will orient to the path it is animating along
	* @return {LTDescr} LTDescr an object that distinguishes the tween
	* @example
	* LeanTween.move( ltLogo, path, 1.0f ).setEase(LeanTweenType.easeOutQuad).setOrientToPath(true).setAxis(Vector3.forward);<br>
	*/
        public LTDescr setOrientToPath(bool doesOrient)
        {
            if (this.type == TweenAction.MOVE_CURVED || this.type == TweenAction.MOVE_CURVED_LOCAL)
            {
                if (this._optional.path == null)
                    this._optional.path = new LTBezierPath();
                this._optional.path.orientToPath = doesOrient;
            }
            else
            {
                this._optional.spline.orientToPath = doesOrient;
            }

            return this;
        }

        /**
	* While tweening along a curve, set this property to true, to be perpendicalur to the path it is moving upon
	* @method setOrientToPath2d
	* @param {bool} doesOrient:bool whether the gameobject will orient to the path it is animating along
	* @return {LTDescr} LTDescr an object that distinguishes the tween
	* @example
	* LeanTween.move( ltLogo, path, 1.0f ).setEase(LeanTweenType.easeOutQuad).setOrientToPath2d(true).setAxis(Vector3.forward);<br>
	*/
        public LTDescr setOrientToPath2d(bool doesOrient2d)
        {
            setOrientToPath(doesOrient2d);
            if (this.type == TweenAction.MOVE_CURVED || this.type == TweenAction.MOVE_CURVED_LOCAL)
            {
                this._optional.path.orientToPath2d = doesOrient2d;
            }
            else
            {
                this._optional.spline.orientToPath2d = doesOrient2d;
            }

            return this;
        }

        public LTDescr setRect(LTRect rect)
        {
            this._optional.ltRect = rect;
            return this;
        }

        public LTDescr setRect(Rect rect)
        {
            this._optional.ltRect = new LTRect(rect);
            return this;
        }

        public LTDescr setPath(LTBezierPath path)
        {
            this._optional.path = path;
            return this;
        }

        /**
	* Set the point at which the GameObject will be rotated around
	* @method setPoint
	* @param {Vector3} point:Vector3 point at which you want the object to rotate around (local space)
	* @return {LTDescr} LTDescr an object that distinguishes the tween
	* @example
	* LeanTween.rotateAround( cube, Vector3.up, 360.0f, 1.0f ) .setPoint( new Vector3(1f,0f,0f) ) .setEase( LeanTweenType.easeInOutBounce );<br>
	*/
        public LTDescr setPoint(Vector3 point)
        {
            this._optional.point = point;
            return this;
        }

        public LTDescr setDestroyOnComplete(bool doesDestroy)
        {
            this.destroyOnComplete = doesDestroy;
            return this;
        }

        public LTDescr setAudio(object audio)
        {
            this._optional.onCompleteParam = audio;
            return this;
        }

        /**
	* Set the onComplete method to be called at the end of every loop cycle (also applies to the delayedCall method)
	* @method setOnCompleteOnRepeat
	* @param {bool} isOn:bool does call onComplete on every loop cycle
	* @return {LTDescr} LTDescr an object that distinguishes the tween
	* @example
	* LeanTween.delayedCall(gameObject,0.3f, delayedMethod).setRepeat(4).setOnCompleteOnRepeat(true);
	*/
        public LTDescr setOnCompleteOnRepeat(bool isOn)
        {
            this.onCompleteOnRepeat = isOn;
            return this;
        }

        /**
	* Set the onComplete method to be called at the beginning of the tween (it will still be called when it is completed as well)
	* @method setOnCompleteOnStart
	* @param {bool} isOn:bool does call onComplete at the start of the tween
	* @return {LTDescr} LTDescr an object that distinguishes the tween
	* @example
	* LeanTween.delayedCall(gameObject, 2f, ()=>{<br> // Flash an object 5 times
	* &nbsp;LeanTween.alpha(gameObject, 0f, 1f);<br>
	* &nbsp;LeanTween.alpha(gameObject, 1f, 0f).setDelay(1f);<br>
	* }).setOnCompleteOnStart(true).setRepeat(5);<br>
	*/
        public LTDescr setOnCompleteOnStart(bool isOn)
        {
            this.onCompleteOnStart = isOn;
            return this;
        }

#if !UNITY_3_5 && !UNITY_4_0 && !UNITY_4_0_1 && !UNITY_4_1 && !UNITY_4_2 && !UNITY_4_3 && !UNITY_4_5
        public LTDescr setRect(RectTransform rect)
        {
            this.rectTransform = rect;
            return this;
        }

        public LTDescr setSprites(UnityEngine.Sprite[] sprites)
        {
            this.sprites = sprites;
            return this;
        }

        public LTDescr setFrameRate(float frameRate)
        {
            this.time = this.sprites.Length / frameRate;
            return this;
        }
#endif

        /**
	* Have a method called when the tween starts
	* @method setOnStart
	* @param {Action<>} onStart:Action<> the method that should be called when the tween is starting ex: tweenStarted( ){ }
	* @return {LTDescr} LTDescr an object that distinguishes the tween
	* @example
	* <i>C#:</i><br>
	* LeanTween.moveX(gameObject, 5f, 2.0f ).setOnStart( ()=>{ Debug.Log("I started!"); });
	* <i>Javascript:</i><br>
	* LeanTween.moveX(gameObject, 5f, 2.0f ).setOnStart( function(){ Debug.Log("I started!"); } );
	*/
        public LTDescr setOnStart(Action onStart)
        {
            this._optional.onStart = onStart;
            return this;
        }

        /**
	* Set the direction of a tween -1f for backwards 1f for forwards (currently only bezier and spline paths are supported)
	* @method setDirection
	* @param {float} direction:float the direction that the tween should run, -1f for backwards 1f for forwards
	* @return {LTDescr} LTDescr an object that distinguishes the tween
	* @example
    * LeanTween.moveSpline(gameObject, new Vector3[]{new Vector3(0f,0f,0f),new Vector3(1f,0f,0f),new Vector3(1f,0f,0f),new Vector3(1f,0f,1f)}, 1.5f).setDirection(-1f);<br>
	*/
        public LTDescr setDirection(float direction)
        {
            if (this.direction != -1f && this.direction != 1f)
            {
                Log.Warning("You have passed an incorrect direction of '" + direction +
                                "', direction must be -1f or 1f");
                return this;
            }

            if (this.direction != direction)
            {
                // Debug.Log("reverse path:"+this.path+" spline:"+this._optional.spline+" hasInitiliazed:"+this.hasInitiliazed);
                if (this.hasInitiliazed)
                {
                    this.direction = direction;
                }
                else
                {
                    if (this._optional.path != null)
                    {
                        this._optional.path = new LTBezierPath(LTUtility.reverse(this._optional.path.pts));
                    }
                    else if (this._optional.spline != null)
                    {
                        this._optional.spline = new LTSpline(LTUtility.reverse(this._optional.spline.pts));
                    }
                    // this.passed = this.time - this.passed;
                }
            }

            return this;
        }

        /**
	* Set whether or not the tween will recursively effect an objects children in the hierarchy
	* @method setRecursive
	* @param {bool} useRecursion:bool whether the tween will recursively effect an objects children in the hierarchy
	* @return {LTDescr} LTDescr an object that distinguishes the tween
	* @example
    * LeanTween.alpha(gameObject, 0f, 1f).setRecursive(true);<br>
	*/
        public LTDescr setRecursive(bool useRecursion)
        {
            this.useRecursion = useRecursion;

            return this;
        }
    }
}