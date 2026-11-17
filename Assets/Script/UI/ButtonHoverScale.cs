using UnityEngine;
using UnityEngine.EventSystems;

// 마우스를 올리면 버튼이 부드럽게 커지고, 벗어나면 원래 크기로 돌아옵니다.
// hoverDecor(예: 양옆 월계수 잎)를 넣으면 호버할 때 서서히 나타납니다.
public class ButtonHoverScale : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Tooltip("마우스를 올렸을 때 크기 배율 (1.15 = 15% 커짐)")]
    public float hoverScale = 1.15f;

    [Tooltip("커지고 작아지는 속도 (클수록 빠름)")]
    public float speed = 12f;

    [Header("Hover Decoration (선택)")]
    [Tooltip("호버할 때 나타날 장식 (월계수 잎 묶음). 비워두면 크기만 바뀝니다.")]
    public CanvasGroup hoverDecor;

    [Tooltip("장식이 나타날 때 바깥쪽에서 안쪽으로 미끄러져 들어오는 거리")]
    public float decorSlide = 12f;

    private Vector3 originalScale;
    private Vector3 targetScale;
    private bool hovered;
    private RectTransform[] decorChildren;
    private Vector2[] decorBasePos;

    private void Awake()
    {
        originalScale = transform.localScale;
        targetScale = originalScale;

        if (hoverDecor != null)
        {
            int n = hoverDecor.transform.childCount;
            decorChildren = new RectTransform[n];
            decorBasePos = new Vector2[n];
            for (int i = 0; i < n; i++)
            {
                decorChildren[i] = hoverDecor.transform.GetChild(i) as RectTransform;
                if (decorChildren[i] != null) decorBasePos[i] = decorChildren[i].anchoredPosition;
            }
            hoverDecor.alpha = 0f;
            hoverDecor.blocksRaycasts = false;
            hoverDecor.interactable = false;
        }
    }

    private void OnDisable()
    {
        // 패널 전환 등으로 꺼질 때 커진 상태로 남지 않도록 원래 크기로 되돌림
        hovered = false;
        targetScale = originalScale;
        transform.localScale = originalScale;
        if (hoverDecor != null)
        {
            hoverDecor.alpha = 0f;
            ApplyDecorSlide(0f);
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        hovered = true;
        targetScale = originalScale * hoverScale;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        hovered = false;
        targetScale = originalScale;
    }

    private void Update()
    {
        // unscaledDeltaTime: 게임이 일시정지(Time.timeScale = 0)여도 동작
        float k = 1f - Mathf.Exp(-speed * Time.unscaledDeltaTime);
        transform.localScale = Vector3.Lerp(transform.localScale, targetScale, k);

        if (hoverDecor != null)
        {
            hoverDecor.alpha = Mathf.Lerp(hoverDecor.alpha, hovered ? 1f : 0f, k);
            ApplyDecorSlide(hoverDecor.alpha);
        }
    }

    // 장식들을 버튼 중심 기준 바깥쪽에서 원래 자리로 모아줌 (t: 0 = 바깥, 1 = 원래 자리)
    private void ApplyDecorSlide(float t)
    {
        if (decorChildren == null) return;
        for (int i = 0; i < decorChildren.Length; i++)
        {
            if (decorChildren[i] == null) continue;
            Vector2 basePos = decorBasePos[i];
            float dir = Mathf.Sign(basePos.x == 0f ? 1f : basePos.x);
            decorChildren[i].anchoredPosition = basePos + new Vector2(dir * decorSlide * (1f - t), 0f);
        }
    }
}
