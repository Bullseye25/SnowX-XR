using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class ArtActivator : MonoBehaviour
{
    [SerializeField] private UnityEvent onActivate = new UnityEvent();

    public UnityEvent OnActivate { get => onActivate; private set => onActivate = value; }

    [SerializeField] private string targetTag;
    [SerializeField] private GameObject[] targets;

    [ContextMenu("GetAllTargets")]
    public void GetAllTargets()
    {
        targets = GameObject.FindGameObjectsWithTag(targetTag);
    }

    public void Activate()
    {
        foreach (GameObject target in targets)
        {
            if (target.name == gameObject.name)
            {
                target.SetActive(true);
            }
            else
            {
                target.SetActive(false);
            }
        }

        //OnActivate?.Invoke();
    }
}
