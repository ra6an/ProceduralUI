using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using ProceduralUI;

public static class CreateProceduralImageMenu
{
    [MenuItem("GameObject/UI/Procedural Image", false, 2000)]
    public static void CreateProceduralImage(MenuCommand menuCommand)
    {
        // Find or create canvas
        Canvas canvas = Object.FindObjectOfType<Canvas>();
        if (canvas == null)
            canvas = CreateCanvas();

        // Determine parent: if right-clicked on a UI object use that, otherwise canvas
        GameObject parent = menuCommand.context as GameObject ?? canvas.gameObject;

        GameObject go = new GameObject("Procedural Image");
        GameObjectUtility.SetParentAndAlign(go, parent);

        RectTransform rect = go.AddComponent<RectTransform>();
        rect.sizeDelta = new Vector2(200, 200);

        ProceduralImage img = go.AddComponent<ProceduralImage>();
        img.color = Color.white;

        // Register undo
        Undo.RegisterCreatedObjectUndo(go, "Create Procedural Image");
        Selection.activeGameObject = go;
    }

    static Canvas CreateCanvas()
    {
        GameObject canvasGO = new GameObject("Canvas");
        Canvas canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;

        CanvasScaler cs = canvasGO.AddComponent<CanvasScaler>();
        cs.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        cs.referenceResolution = new Vector2(1920, 1080);

        canvasGO.AddComponent<GraphicRaycaster>();

        if (Object.FindObjectOfType<EventSystem>() == null)
        {
            GameObject eventSystemGO = new GameObject("EventSystem");
            eventSystemGO.AddComponent<EventSystem>();
            eventSystemGO.AddComponent<StandaloneInputModule>();
            Undo.RegisterCreatedObjectUndo(eventSystemGO, "Create EventSystem");
        }

        Undo.RegisterCreatedObjectUndo(canvasGO, "Create Canvas");
        return canvas;
    }
}
