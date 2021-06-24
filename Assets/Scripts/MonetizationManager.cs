using System.Collections;
using UnityEngine;
using UnityEngine.Advertisements;
public class MonetizationManager : MonoBehaviour
{
    string m_GameId = "4183125";
    bool m_TestMode = true;
    string m_SurfacingId = "bannerPlacement";
    void Start()
    {
        Advertisement.Initialize(m_GameId, m_TestMode);
        StartCoroutine(ShowBannerWhenInitialized());
    }
    IEnumerator ShowBannerWhenInitialized()
    {
        while (!Advertisement.isInitialized)
        {
            yield return new WaitForSeconds(0.5f);
        }
        Advertisement.Banner.SetPosition(BannerPosition.TOP_CENTER);
        Advertisement.Banner.Show(m_SurfacingId);
    }
}
