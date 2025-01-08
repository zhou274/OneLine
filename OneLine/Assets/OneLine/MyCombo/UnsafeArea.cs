using UnityEngine;

public class UnsafeArea : MonoBehaviour
{
    public RectTransform safeArea;
    public RectTransform top, bottom;

    private void Start()
    {
#if UNITY_IOS
        float padding = safeArea.offsetMin.y;
        if (padding != 0)
        {
            top.sizeDelta = new Vector2(top.sizeDelta.x, padding);
            bottom.sizeDelta = new Vector2(bottom.sizeDelta.x, padding);
        }
#endif
    }
}