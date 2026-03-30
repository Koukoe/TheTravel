// using UnityEngine;
// using System.Collections;

// public class UIMainMenuAnim : UIListener
// {
//     public EaseParam entranceEase;
//     public float duration = 0.5f;
//     public float delay = 0f;
//     public Vector2 startOffset = new Vector2(-250f, 0f);


//     private RectTransform rectTransform;
//     private Vector2 originalAnchoredPos;
//     private Coroutine currentRoutine;

//     void Awake()
//     {
//         rectTransform = GetComponent<RectTransform>();
//         originalAnchoredPos = rectTransform.anchoredPosition;
//     }

//     void OnEnable()
//     {
//         rectTransform.anchoredPosition = originalAnchoredPos + startOffset;

//         if (currentRoutine != null) StopCoroutine(currentRoutine);
//         currentRoutine = StartCoroutine(DoEntrance());
//     }

//     void OnDisable()
//     {
//         if (currentRoutine != null) StopCoroutine(currentRoutine);
//     }

//     private IEnumerator DoEntrance()
//     {
//         if (delay > 0)
//             yield return new WaitForSecondsRealtime(delay);

//         float elapsed = 0;
//         Vector2 startPos = originalAnchoredPos + startOffset;

//         while (elapsed < duration)
//         {
//             elapsed += Time.deltaTime;
//             float t = Mathf.Clamp01(elapsed / duration);

//             rectTransform.anchoredPosition = entranceEase.Lerp(startPos, originalAnchoredPos, t);

//             yield return null;
//         }

//         rectTransform.anchoredPosition = originalAnchoredPos;
//     }
// }