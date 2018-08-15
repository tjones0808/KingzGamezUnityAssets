using UnityEditor;
using UnityEngine;

namespace EasyBuildSystem.Codes.Editor
{
    public class SceneHelper
    {
        #region Public Methods

        public static void Focus(Object target, DrawCameraMode mode = DrawCameraMode.Wireframe, bool autoSelect = true)
        {
            EditorWindow.GetWindow<SceneView>("", typeof(SceneView));

            if (autoSelect)
                Selection.activeObject = target;

            if (SceneView.lastActiveSceneView != null)
            {
                SceneView.lastActiveSceneView.FrameSelected();

#if UNITY_2018
                SceneView.CameraMode Mode = new SceneView.CameraMode();

                Mode.drawMode = mode;

                SceneView.lastActiveSceneView.cameraMode = Mode;

                SceneView.lastActiveSceneView.Repaint();
#else
                SceneView.lastActiveSceneView.renderMode = mode;

                SceneView.lastActiveSceneView.Repaint();

#endif
            }
        }

        public static void UnFocus()
        {
            if (SceneView.lastActiveSceneView != null)
            {
#if UNITY_2018
                SceneView.CameraMode Mode = new SceneView.CameraMode();

                Mode.drawMode = DrawCameraMode.Textured;

                SceneView.lastActiveSceneView.cameraMode = Mode;

                SceneView.lastActiveSceneView.Repaint();
#else
                SceneView.lastActiveSceneView.renderMode = DrawCameraMode.Textured;

                SceneView.lastActiveSceneView.Repaint();
#endif
            }
        }

        #endregion Public Methods
    }
}