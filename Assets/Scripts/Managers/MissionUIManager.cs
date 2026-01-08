using UnityEngine;
using System.Collections.Generic;

public class MissionUIManager : MonoBehaviour
{
    [Header("UI Kutuları")]
    public Transform mainMissionContainer; // Ana Görevlerin dizileceği kutu
    public Transform sideMissionContainer; // Yan Görevlerin dizileceği kutu
    
    [Header("Prefab")]
    public GameObject missionPrefab;  // Satır tasarımı (MissionItem)

    void Start()
    {
        if (LevelManager.instance != null)
        {
            LevelManager.instance.OnMissionsUpdated += UpdateUI;
            UpdateUI(); 
        }
    }

    void OnDestroy()
    {
        if (LevelManager.instance != null)
        {
            LevelManager.instance.OnMissionsUpdated -= UpdateUI;
        }
    }

    void UpdateUI()
    {
        // 1. Önce iki kutuyu da temizle (Eskileri sil)
        ClearContainer(mainMissionContainer);
        ClearContainer(sideMissionContainer);

        if (LevelManager.instance == null) return;

        // 2. Tüm görevleri tek tek kontrol et
        foreach (MissionData mission in LevelManager.instance.activeMissions)
        {
            // Hangi kutuya koyacağız?
            Transform targetContainer = mission.isMainMission ? mainMissionContainer : sideMissionContainer;

            // İlgili kutunun içine oluştur
            GameObject itemObj = Instantiate(missionPrefab, targetContainer);
            
            // Veriyi script'e gönder
            MissionItem itemScript = itemObj.GetComponent<MissionItem>();
            if (itemScript != null)
            {
                itemScript.Setup(mission);
            }
        }
    }

    // Yardımcı fonksiyon: Kutunun içini temizler
    void ClearContainer(Transform container)
    {
        foreach (Transform child in container)
        {
            Destroy(child.gameObject);
        }
    }
}