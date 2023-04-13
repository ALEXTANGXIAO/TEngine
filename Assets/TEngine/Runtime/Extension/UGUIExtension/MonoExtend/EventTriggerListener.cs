using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;

namespace TEngine
{
	public class EventTriggerListener : EventTrigger
	{
		public delegate void VoidDelegate(GameObject go);
		public delegate void EventDelegate(GameObject go, PointerEventData ev);
		[FormerlySerializedAs("onClick")]
		public VoidDelegate OnClick;
		[FormerlySerializedAs("onDown")]
		public EventDelegate OnDown;
		[FormerlySerializedAs("v")]
		public EventDelegate OnExit;
		[FormerlySerializedAs("onUp")]
		public EventDelegate OnUp;
		[FormerlySerializedAs("onSelect")]
		public VoidDelegate OnSelectEvent;
		[FormerlySerializedAs("onUpdateSelect")]
		public VoidDelegate OnUpdateSelect;
		[FormerlySerializedAs("onDragBegin")]
		public EventDelegate OnDragBegin;
		[FormerlySerializedAs("onDrag")]
		public EventDelegate OnDragEvent;
		[FormerlySerializedAs("onDragEnd")]
		public EventDelegate OnDragEnd;
		[FormerlySerializedAs("onEnter")]
		public EventDelegate OnEnter;
		[FormerlySerializedAs("onDrop")]
		public EventDelegate OnDropEvent;

		public delegate void ClickEffectDelegate();
		public static EventTriggerListener Get(GameObject go, float time = -1, bool play_ani = true, float scale = 1)
		{
			if (!go)
			{
				Log.Warning("EventTriggerListener.Get, GameObject is null!!");
			}
			EventTriggerListener listener = go.GetComponent<EventTriggerListener>();

			if (listener == null)
				listener = go.AddComponent<EventTriggerListener>();
			return listener;
		}

		private bool _IsValidTrigger()
		{
			return true;
		}

		public override void OnPointerClick(PointerEventData event_data)
		{
			if (!_IsValidTrigger())
			{
				return;
			}

			if (Input.touchCount > 1)
			{
				return;
			}
			OnClick?.Invoke(gameObject);
		}

		public override void OnPointerDown(PointerEventData event_data)
		{
			if (!this._IsValidTrigger())
			{
				return;
			}
			OnDown?.Invoke(gameObject, event_data);
		}

		public override void OnPointerEnter(PointerEventData event_data)
		{
			if (!_IsValidTrigger())
			{
				return;
			}
			OnEnter?.Invoke(gameObject, event_data);
		}

		public override void OnPointerExit(PointerEventData event_data)
		{
			if (!_IsValidTrigger())
			{
				return;
			}

			OnExit?.Invoke(gameObject, event_data);
		}

		public override void OnPointerUp(PointerEventData event_data)
		{
			if (!_IsValidTrigger())
			{
				return;
			}

			OnUp?.Invoke(gameObject, event_data);
		}

		public override void OnSelect(BaseEventData event_data)
		{
			if (!_IsValidTrigger())
			{
				return;
			}

			OnSelectEvent?.Invoke(gameObject);
		}

		public override void OnUpdateSelected(BaseEventData event_data)
		{
			if (!_IsValidTrigger())
			{
				return;
			}

			OnUpdateSelect?.Invoke(gameObject);
		}

		public override void OnBeginDrag(PointerEventData event_data)
		{
			if (!_IsValidTrigger())
			{
				return;
			}

			OnDragBegin?.Invoke(gameObject, event_data);
		}

		public override void OnDrag(PointerEventData event_data)
		{
			if (!_IsValidTrigger())
			{
				return;
			}

			OnDragEvent?.Invoke(gameObject, event_data);
		}

		public override void OnEndDrag(PointerEventData event_data)
		{
			if (!_IsValidTrigger())
			{
				return;
			}

			OnDragEnd?.Invoke(gameObject, event_data);
		}

		public override void OnDrop(PointerEventData event_data)
		{
			if (!_IsValidTrigger())
			{
				return;
			}

			OnDropEvent?.Invoke(gameObject, event_data);
		}

		private void OnDestroy()
		{
			OnClick = null;
			OnDown = null;
			OnExit = null;
			OnUp = null;
			OnSelectEvent = null;
			OnDragBegin = null;
			OnDragEvent = null;
			OnDragEnd = null;
			OnEnter = null;
			OnDropEvent = null;
			OnDragEnd = null;
		}
	}
}
