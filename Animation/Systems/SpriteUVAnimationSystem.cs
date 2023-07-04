using Unity.Mathematics;
using Unity.Entities;
using Unity.Burst;

namespace NSprites
{
    // TODO: check animation system can work with different frame size animations 
    
    /// Compare <see cref="AnimationTimer"/> with global time and switch <see cref="FrameIndex"/> when timer expired.
    /// Perform only not-culled entities. Restore <see cref="FrameIndex"/> and duration time for entities which be culled for some time.
    /// Somehow calculations goes a bit wrong and unculled entities gets synchronized, don't know how to fix
    public partial struct SpriteUVAnimationSystem : ISystem
    {
        [BurstCompile]
        [WithNone(typeof(CullSpriteTag))]
        private partial struct AnimationJob : IJobEntity
        {
            public double Time;

            private void Execute(ref AnimationTimer animationTimer,
                                    ref FrameIndex frameIndex,
                                    ref UVAtlas uvAtlas,
                                    in AnimationSetLink animationSet,
                                    in AnimationIndex animationIndex)
            {
                var timerDelta = Time - animationTimer.value;

                if (timerDelta >= 0f)
                {
                    ref var animData = ref animationSet.value.Value[animationIndex.value];
                    frameIndex.value = (frameIndex.value + 1) % animData.FrameCount;
                    var nextFrameDuration = animData.FrameDurations[frameIndex.value];

                    if (timerDelta >= animData.AnimationDuration)
                    {
                        var extraTime = (float)(timerDelta % animData.AnimationDuration);
                        while (extraTime > nextFrameDuration)
                        {
                            extraTime         -= nextFrameDuration;
                            frameIndex.value  =  (frameIndex.value + 1) % animData.FrameCount;
                            nextFrameDuration =  animData.FrameDurations[frameIndex.value];
                        }
                        nextFrameDuration -= extraTime;
                    }

                    animationTimer.value = Time + nextFrameDuration;

                    var frameSize = new float2(animData.UVAtlas.xy / animData.GridSize);
                    var frameIndexOffseted = frameIndex.value + animData.FrameOffset;
                    var framePosition = new int2(frameIndexOffseted % animData.GridSize.x, frameIndexOffseted / animData.GridSize.x);
                    float2 framePositionF = frameSize * framePosition;
                    framePositionF.y = 1.0f - frameSize.y - framePositionF.y;
                    uvAtlas = new UVAtlas { value = new float4(frameSize, animData.UVAtlas.zw + framePositionF) };
                }
            }
        }
        
        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var animationJob = new AnimationJob { Time = SystemAPI.Time.ElapsedTime };
            state.Dependency = animationJob.ScheduleParallelByRef(state.Dependency);
        }
    }
}