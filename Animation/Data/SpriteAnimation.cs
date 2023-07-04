using Unity.Mathematics;
using UnityEngine;

[CreateAssetMenu(fileName = "NewNSpriteAnimation", menuName = "NSprites/Animation (frame sequence)")]
public class SpriteAnimation : ScriptableObject
{
    // Sprite here required because whe want to know UV of animation frame sequence on atlas
    public Sprite  SpriteSheet;
    public int2    FramesPerSprite = new(1);
    public int     FrameOffset;
    public int     FrameCount;
    public float[] FrameDurations       = new float[1] { 0.1f };
    public float   DefaultFrameDuration = .1f;

    #region Editor
#if UNITY_EDITOR
    private void OnValidate()
    {
        if (FrameDurations.Length != FrameCount)
        {
            var correctedFrameDurations = new float[FrameCount];
            var minLength               = math.min(FrameDurations.Length, correctedFrameDurations.Length);
            for (int i = 0; i < minLength; i++)
                correctedFrameDurations[i] = FrameDurations[i];
            for (int i = minLength; i < correctedFrameDurations.Length; i++)
                correctedFrameDurations[i] = DefaultFrameDuration;
            FrameDurations = correctedFrameDurations;
        }
    }
#endif

    #endregion
}