using UnityEngine;

[AddComponentMenu("OG Fun Monke Horror/Map Hitsounds")]
public class MapHitsounds : MonoBehaviour
{
    [System.Serializable]
    public struct MaterialSound
    {
        [Tooltip("Materials in your map that should use these custom hit sounds.")]
        public Material[] materials;

        [Tooltip("One is picked at random each time a listed material is hit.")]
        public AudioClip[] sounds;
    }

    [Tooltip("Give surfaces in your map their own hit sounds. Hitting a material listed " +
             "here plays one of its sounds. A material you don't list uses the game's sound " +
             "for a material with the same name, or makes no sound if the game has none.")]
    public MaterialSound[] materialSounds;
}
