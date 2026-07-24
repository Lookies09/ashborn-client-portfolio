using UnityEngine;
using TMPro;

public class TitleAnimation : MonoBehaviour
{
    private Material _titleMaterial;
    private static readonly int AnimationTime = Shader.PropertyToID("_AnimationTime");

    void Start()
    {
        // TMP의 머티리얼을 가져옵니다 (SharedMaterial을 쓰면 프로젝트 원본이 수정되니 주의)
        _titleMaterial = GetComponent<TextMeshProUGUI>().fontMaterial;

        Debug.Log(_titleMaterial);
    }

    void Update()
    {
        // 매 프레임 시간을 전달하여 불꽃 일렁임과 노이즈 이동을 만듭니다.
        _titleMaterial.SetFloat(AnimationTime, Time.time);
    }
}