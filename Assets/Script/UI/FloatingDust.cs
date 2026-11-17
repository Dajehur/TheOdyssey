using UnityEngine;
using UnityEngine.UI;

// 화면 위로 금가루 같은 작은 입자가 천천히 떠다니며 반짝입니다.
// 꽉 찬(stretch) RectTransform에 붙이면, 그 영역 안에서 입자를 만들어 움직입니다.
// UI 이미지로 그려서 Screen Space - Overlay 캔버스에서도 배경 위·버튼 아래에 정확히 보입니다.
[RequireComponent(typeof(RectTransform))]
public class FloatingDust : MonoBehaviour
{
    [Tooltip("입자 모양 (둥근 스프라이트)")]
    public Sprite particleSprite;

    [Tooltip("입자 개수")]
    public int count = 60;

    [Tooltip("입자 크기 범위 (픽셀)")]
    public Vector2 sizeRange = new Vector2(8f, 20f);

    [Tooltip("위로 떠오르는 속도 범위 (픽셀/초)")]
    public Vector2 riseSpeedRange = new Vector2(6f, 22f);

    [Tooltip("좌우로 흔들리는 폭 (픽셀)")]
    public float swayAmount = 18f;

    [Tooltip("입자 색 (금색)")]
    public Color color = new Color(0.85f, 0.55f, 0.12f, 1f);

    [Tooltip("투명도 범위 (반짝임 최저~최고)")]
    public Vector2 alphaRange = new Vector2(0.45f, 1f);

    private struct Mote
    {
        public RectTransform rt;
        public Image img;
        public float baseX, y, rise, swayFreq, swayPhase, twinkleFreq, twinklePhase, maxAlpha;
    }

    private Mote[] motes;
    private RectTransform area;

    private void Start()
    {
        area = (RectTransform)transform;
        motes = new Mote[count];
        for (int i = 0; i < count; i++)
        {
            var go = new GameObject("Mote", typeof(RectTransform), typeof(Image));
            go.transform.SetParent(transform, false);
            var img = go.GetComponent<Image>();
            img.sprite = particleSprite;
            img.raycastTarget = false; // 버튼 클릭을 막지 않도록
            var rt = (RectTransform)go.transform;
            float s = Random.Range(sizeRange.x, sizeRange.y);
            rt.sizeDelta = new Vector2(s, s);

            motes[i] = new Mote
            {
                rt = rt,
                img = img,
                rise = Random.Range(riseSpeedRange.x, riseSpeedRange.y),
                swayFreq = Random.Range(0.15f, 0.45f),
                swayPhase = Random.Range(0f, Mathf.PI * 2f),
                twinkleFreq = Random.Range(0.6f, 1.8f),
                twinklePhase = Random.Range(0f, Mathf.PI * 2f),
                maxAlpha = Random.Range(alphaRange.x, alphaRange.y),
            };
            Respawn(ref motes[i], true);
        }
    }

    private void Respawn(ref Mote m, bool anywhere)
    {
        Rect r = area.rect;
        m.baseX = Random.Range(r.xMin, r.xMax);
        m.y = anywhere ? Random.Range(r.yMin, r.yMax) : r.yMin - 10f;
    }

    private void Update()
    {
        if (motes == null) return;
        float dt = Time.unscaledDeltaTime;
        float t = Time.unscaledTime;
        Rect r = area.rect;

        for (int i = 0; i < motes.Length; i++)
        {
            ref Mote m = ref motes[i];
            m.y += m.rise * dt;
            if (m.y > r.yMax + 10f) Respawn(ref m, false);

            float x = m.baseX + Mathf.Sin(t * m.swayFreq * Mathf.PI * 2f + m.swayPhase) * swayAmount;
            m.rt.anchoredPosition = new Vector2(x, m.y);

            // 반짝임 + 위/아래 가장자리에서 서서히 사라짐
            float twinkle = 0.55f + 0.45f * Mathf.Sin(t * m.twinkleFreq * Mathf.PI * 2f + m.twinklePhase);
            float edge = Mathf.Clamp01((m.y - r.yMin) / 80f) * Mathf.Clamp01((r.yMax - m.y) / 80f);
            Color c = color;
            c.a = m.maxAlpha * twinkle * edge;
            m.img.color = c;
        }
    }
}
