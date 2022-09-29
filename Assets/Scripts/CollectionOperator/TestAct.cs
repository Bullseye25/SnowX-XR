using System.Collections;
using System.Collections.Generic;
using UniGLTF;
using UnityEngine;
using UnityEngine.Networking;

public class TestAct : MonoBehaviour
{
    [SerializeField] private string uri;
    [SerializeField] private GameObject glb;
    [SerializeField] private List<glTFMesh> meshes = new List<glTFMesh>();

    IEnumerator Start()
    {
        UnityWebRequest processor = UnityWebRequest.Get(uri);

        yield return processor.SendWebRequest();

        GlbBinaryParser s = new GlbBinaryParser(processor.downloadHandler.data, "asd");

        var act = s.Parse();

        UniGLTF.ImporterContext importer = new ImporterContext(act);

        var context = importer.Load();

        context.ShowMeshes();

        glb = context.Root;
    }
}
