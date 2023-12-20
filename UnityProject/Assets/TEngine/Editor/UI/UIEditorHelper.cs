using UnityEditor;
using UnityEngine;

namespace TEngine.Editor.UI
{
    public class UIEditorHelper
    {
        public static GameObject CreateUIElementRoot(string name, Vector2 size)
        {
            GameObject child = new GameObject(name);
            RectTransform rectTransform = child.AddComponent<RectTransform>();
            rectTransform.sizeDelta = size;
            return child;
        }

        public static void PlaceUIElementRoot(GameObject element, MenuCommand menuCommand)
        {
            GameObject parent = menuCommand.context as GameObject;
            if (parent == null || parent.GetComponentInParent<Canvas>() == null)
            {
                parent = GetOrCreateCanvasGameObject();
            }

            string uniqueName = GameObjectUtility.GetUniqueNameForSibling(parent.transform, element.name);
            element.name = uniqueName;
            Undo.RegisterCreatedObjectUndo(element, "Create " + element.name);
            Undo.SetTransformParent(element.transform, parent.transform, "Parent " + element.name);
            GameObjectUtility.SetParentAndAlign(element, parent);
            if (parent != menuCommand.context) // not a context click, so center in sceneview
            {
                SetPositionVisibleInSceneView(parent.GetComponent<RectTransform>(), element.GetComponent<RectTransform>());
            }
            Selection.activeGameObject = element;
        }

        /// <summary>
        /// Helper function that returns a Canvas GameObject; preferably a parent of the selection, or other existing Canvas.
        /// </summary>
        /// <returns></returns>
        public static GameObject GetOrCreateCanvasGameObject()
        {
            GameObject selectedGo = Selection.activeGameObject;

            // Try to find a gameobject that is the selected GO or one if its parents.
            Canvas canvas = (selectedGo != null) ? selectedGo.GetComponentInParent<Canvas>() : null;
            if (canvas != null && canvas.gameObject.activeInHierarchy)
                return canvas.gameObject;

            // No canvas in selection or its parents? Then use just any canvas..
            canvas = Object.FindObjectOfType(typeof(Canvas)) as Canvas;
            if (canvas != null && canvas.gameObject.activeInHierarchy)
                return canvas.gameObject;

            // No canvas in the scene at all? Then create a new one.
            return null;
        }

        private static void SetPositionVisibleInSceneView(RectTransform canvasRTransform, RectTransform itemTransform)
        {
            // Find the best scene view
            SceneView sceneView = SceneView.lastActiveSceneView;
            if (sceneView == null && SceneView.sceneViews.Count > 0)
                sceneView = SceneView.sceneViews[0] as SceneView;

            // Couldn't find a SceneView. Don't set position.
            if (sceneView == null || sceneView.camera == null)
                return;

            // Create world space Plane from canvas position.
            Vector2 localPlanePosition;
            Camera camera = sceneView.camera;
            Vector3 position = Vector3.zero;
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRTransform, new Vector2(camera.pixelWidth / 2, camera.pixelHeight / 2), camera,
                    out localPlanePosition))
            {
                // Adjust for canvas pivot
                localPlanePosition.x = localPlanePosition.x + canvasRTransform.sizeDelta.x * canvasRTransform.pivot.x;
                localPlanePosition.y = localPlanePosition.y + canvasRTransform.sizeDelta.y * canvasRTransform.pivot.y;

                localPlanePosition.x = Mathf.Clamp(localPlanePosition.x, 0, canvasRTransform.sizeDelta.x);
                localPlanePosition.y = Mathf.Clamp(localPlanePosition.y, 0, canvasRTransform.sizeDelta.y);

                // Adjust for anchoring
                position.x = localPlanePosition.x - canvasRTransform.sizeDelta.x * itemTransform.anchorMin.x;
                position.y = localPlanePosition.y - canvasRTransform.sizeDelta.y * itemTransform.anchorMin.y;

                Vector3 minLocalPosition;
                minLocalPosition.x = canvasRTransform.sizeDelta.x * (0 - canvasRTransform.pivot.x) + itemTransform.sizeDelta.x * itemTransform.pivot.x;
                minLocalPosition.y = canvasRTransform.sizeDelta.y * (0 - canvasRTransform.pivot.y) + itemTransform.sizeDelta.y * itemTransform.pivot.y;

                Vector3 maxLocalPosition;
                maxLocalPosition.x = canvasRTransform.sizeDelta.x * (1 - canvasRTransform.pivot.x) - itemTransform.sizeDelta.x * itemTransform.pivot.x;
                maxLocalPosition.y = canvasRTransform.sizeDelta.y * (1 - canvasRTransform.pivot.y) - itemTransform.sizeDelta.y * itemTransform.pivot.y;

                position.x = Mathf.Clamp(position.x, minLocalPosition.x, maxLocalPosition.x);
                position.y = Mathf.Clamp(position.y, minLocalPosition.y, maxLocalPosition.y);
            }

            itemTransform.anchoredPosition = position;
            itemTransform.localRotation = Quaternion.identity;
            itemTransform.localScale = Vector3.one;
        }
    }
}