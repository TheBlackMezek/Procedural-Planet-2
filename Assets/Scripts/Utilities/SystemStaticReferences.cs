using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SystemStaticReferences : MonoBehaviour {
    
    public static TMPro.TextMeshProUGUI SpeedText { get; private set; }
    [SerializeField]
    private TMPro.TextMeshProUGUI speedText;



    private void Awake()
    {
        SpeedText = speedText;
    }

}
