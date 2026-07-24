public class AdEnums
{
    public enum AdState
    {
        None,        // 아직 생성 안 됨
        Loading,     // LoadAd 호출 후
        Ready,       // Show 가능
        Showing,     // 화면에 표시 중
        Failed       // 로드 실패
    }

}
