using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectResetAct : MonoBehaviour
{
    [SerializeField] private AudioSource source;
    [SerializeField] private AudioClip clip;
    [SerializeField] private Vector3 position;
    [SerializeField] private Quaternion rotation;
    [SerializeField] private Vector3 scale;

    [ContextMenu("CaptureData")]
    public void CaptureData()
    {
        position = transform.localPosition;
        rotation = transform.localRotation;
        scale = transform.localScale;
    }

    private void OnEnable()
    {
        source.clip = clip;
        source.Play();

        transform.localPosition = position;
        transform.localRotation = rotation;
        transform.localScale = scale;
    }
}