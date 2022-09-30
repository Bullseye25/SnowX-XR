using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using EGOR = Microsoft.MixedReality.Toolkit.Utilities;
using Microsoft.MixedReality.Toolkit.Utilities.Gltf.Serialization;
using System;
using UnityEngine.UI;
using DanielLochner.Assets.SimpleScrollSnap;
using Microsoft.MixedReality.Toolkit.Input;
using Microsoft.MixedReality.Toolkit.UI;
using Microsoft.MixedReality.Toolkit.Examples.Demos;
using UnityEngine.Events;
using TMPro;
using UniGLTF;

public class CollectionOperator : MonoBehaviour
{
    [SerializeField] private int collection;

    [Space]
    [SerializeField] private string url;
    [SerializeField] private ManagerAct managerAct;
    [SerializeField] private Material mat;
    [SerializeField] private Texture2D defaultTexture;

    [Space]
    [SerializeField] private GameObject slider;
    [SerializeField] private SimpleScrollSnap snap;
    [SerializeField] private GameObject selectionSlot;
    [SerializeField] private Transform selectionParent;

    [Space]
    [SerializeField] private Transform artHolder;
    [SerializeField] private Vector3 position;

    [Space]
    [Header("Loading Area")]
    [SerializeField] private GameObject loadingSlotPrefab;
    [SerializeField] private Transform loadingParent;

    [Space]
    [SerializeField] private List<GameObject> selections = new List<GameObject>();
    [SerializeField] private List<GameObject> artAct = new List<GameObject>();

    [SerializeField] private GameObject current = null;
    private UnityEvent onGenerate = new UnityEvent();


    IEnumerator Start()
    {
        UnityWebRequest processor = UnityWebRequest.Get(url);

        yield return processor.SendWebRequest();

        managerAct = JsonUtility.FromJson<ManagerAct>(processor.downloadHandler.text);

        List<Artwork> artworks = managerAct.collections[collection].artworks;

        //for(int index = 0; index < artworks.Count; index++)
        //{
        //    var newSelection = Instantiate(selectionSlot, selectionParent);
        //    selections.Add(newSelection);
        //    newSelection.SetActive(false);
        //}

        for (int index = 0; index < artworks.Count; index++)
        {
            Artwork art = artworks[index];
            var title = art.title;
            var glbPath = art.urls.glb;

            onGenerate.AddListener(() => { GenerateArt(glbPath, title, art); });
        }

        slider.SetActive(true);
        onGenerate?.Invoke();
    }

    public void GenerateArt(string glbPath, string title, Artwork art)
    {
        var loadingbar = Instantiate(loadingSlotPrefab, loadingParent).GetComponentInChildren<TextMeshProUGUI>();

        StartCoroutine(GetGLB(glbPath, title, loadingbar, glb =>
        {
            if (glb != null)
            {
                glb.transform.SetParent(artHolder);

                var defaultScale = glb.transform.localScale;
                glb.transform.localScale = new Vector3(((defaultScale.x / 2) / 2) / 2, ((defaultScale.y / 2) / 2) / 2, ((defaultScale.z / 2) / 2) / 2);

                glb.transform.localPosition = position;
                glb.name = title;

                artAct.Add(glb);

                var rb = glb.AddComponent<Rigidbody>();
                rb.useGravity = false;
                rb.constraints = RigidbodyConstraints.FreezePosition;

                var mc = glb.AddComponent<BoxCollider>();
                mc.size = new Vector3(2.5f, 2.5f, 2.5f);
                mc.center = Vector3.zero;

                //glb.AddComponent<NearInteractionGrabbable>();

                //glb.AddComponent<CursorContextObjectManipulator>();

                //var tp = glb.AddComponent<TetheredPlacement>();
                //tp.DistanceThreshold = 20;

                //glb.AddComponent<ObjectManipulator>();
                //glb.AddComponent<ConstraintManager>();

                glb.SetActive(false);

                var iconPath = art.urls.thumbnail;

                StartCoroutine(GetTexture(iconPath, title, loadingbar, sprite =>
                {
                    var selection = GetAvailableSlot();
                    selection.name = art.title;

                    selection.GetComponent<Image>().sprite = sprite;

                    selection.GetComponent<ArtActivator>().OnActivate.RemoveAllListeners();
                    selection.GetComponent<ArtActivator>().OnActivate.AddListener(() =>
                    {
                        if (current != null)
                            current.SetActive(false);

                        glb.SetActive(true);
                        current = glb;
                    });

                    selection.SetActive(true);
                }));
            }
        }));
    }

    private IEnumerator GetGLB(string uri, string title, TextMeshProUGUI loading, Action<GameObject> glb)
    {
        UnityWebRequest www = UnityWebRequest.Get(uri);

        //yield return www.SendWebRequest();

        var processor = www.SendWebRequest();

        while (!processor.isDone)
        {
            var downloadDataProgress = www.downloadProgress * 100;
            var loader = $"{title}: {(int)downloadDataProgress}";
            loading.text = loader;

            yield return null;
        }

        if (processor.isDone)
        {
            yield return new WaitForEndOfFrame();

            try
            {
                var data = www.downloadHandler.data;

                var binaryParser = new GlbBinaryParser(data, title);

                var parse = binaryParser.Parse();

                ImporterContext importer = new ImporterContext(parse);

                var context = importer.Load();

                context.ShowMeshes();

                glb(context.Root);
            }
            catch
            {
                var gltfObject = Microsoft.MixedReality.Toolkit.Utilities.Gltf.Serialization.GltfUtility.GetGltfObjectFromGlb(www.downloadHandler.data);

                if (gltfObject == null)
                {
                    glb(null);
                }
                else
                {
                    CreateArt(gltfObject, newArt =>
                    {
                        newArt.name = title;

                        var meshes = newArt.GetComponentsInChildren<MeshRenderer>();

                        foreach (MeshRenderer mesh in meshes)
                        {
                            mesh.materials = new Material[0];

                            mesh.materials = new Material[]
                            {
                                mat
                            };
                        }

                        glb(newArt);
                    });
                }
            }
        }
    }

    private async void CreateArt(EGOR.Gltf.Schema.GltfObject gltfObject, Action<GameObject> action)
    {
        await gltfObject.ConstructAsync();

        action(gltfObject.GameObjectReference);
    }

    private IEnumerator GetTexture(string uri, string name, TextMeshProUGUI loadingbar, Action<Sprite> action)
    {
        UnityWebRequest www = UnityWebRequestTexture.GetTexture(uri);

        var processor = www.SendWebRequest();

        while (!processor.isDone)
        {
            var downloadDataProgress = www.downloadProgress * 100;
            var loader = $"{name} icon: {(int)downloadDataProgress}";
            loadingbar.text = loader;

            yield return null;
        }

#if UNITY_2019_4
        if(www.isNetworkError == true || www.isHttpError == true)
#else
        if (www.result != UnityWebRequest.Result.Success)
#endif
        {
            Debug.Log($"{name} Image not Found, replacing with default image");

            var width = defaultTexture.width;
            var height = defaultTexture.height;

            var sprite = Sprite.Create(defaultTexture, new Rect(0, 0, width, height), new Vector2(width / 2, height / 2));

            sprite.name = name;
            loadingbar.text = $"{name} Downloaded!";
            action(sprite);
        }

        if(processor.isDone)
        {
            Texture2D tex = ((DownloadHandlerTexture)www.downloadHandler).texture;

            var width = tex.width;
            var height = tex.height;

            var sprite = Sprite.Create(tex, new Rect(0, 0, width, height), new Vector2(width / 2, height / 2));

            sprite.name = name;
            loadingbar.text = $"{name} Downloaded!";
            action(sprite);
        }
    }

    private GameObject GetAvailableSlot()
    {
        foreach(GameObject selection in selections)
        {
            if (!selection.activeSelf)
            {
                return selection;
            }
        }

        var newSelection = Instantiate(selectionSlot, selectionParent);
        selections.Add(newSelection);
        snap.Setup();
        return newSelection;
    }
}