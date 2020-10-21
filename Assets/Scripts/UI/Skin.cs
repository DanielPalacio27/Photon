using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class Skin 
{
    public Mesh mesh;
    public Shader shader;
    public Material material;
    public Image thumbnail;
    public float value;

    public Skin(Mesh _mesh, Shader _shader, Material _material, Image _thumbnail, float _value)
    {
        mesh = _mesh;
        shader = _shader;
        material = _material;
        thumbnail = _thumbnail;
        value = _value;
    }

}
