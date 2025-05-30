//                                                                                          //
//>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>//
//                                                                                          //
//                                                                                          //
//                This file is for compatibility with Kuska Helper's nail.                  //
//            if you're reading this for some reason, check here for more info.             //
//                https://github.com/kuksattu/Kelper/wiki/Kelper-ModInterop                 //
//                                                                                          //
//                                                                                          //
//<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<//
//                                                                                          //
using Microsoft.Xna.Framework;
using MonoMod.ModInterop;
using Monocle;
using System;
using System.Diagnostics.CodeAnalysis;
#pragma warning disable CS0618 // Type or member is obsolete

namespace FlaglinesAndSuch;

[ModImportName("KelperAPI")]
[SuppressMessage("ReSharper", "UnassignedField.Global")]
public static class KelperImports
{
    /// <summary>
    ///     If there is some massive change that breaks the modinterop this version number will change.  <br/>
    ///     It should never happen hopefully.
    /// </summary>
    public static int? GetVersion()
        => GetVersionDelegate?.Invoke();


    /// <summary>
    ///     Returns a KelperNailCollider as a Component.                                                <br/>
    ///     For example the following collider calls the following method:
    /// </summary>
    /// <code>
    ///     Add(CreateNailCollider(OnNailHit));
    ///     public bool OnNailHit(Player player, Vector2 nailDir) { return true; }
    /// </code>
    /// <param name="callback">A callback function that gets run when the entity is hit with the nail.</param>
    /// Even if they are not specified anywhere.</param>
    /// <returns>The component or null if Kelper is not enabled.</returns>
    public static Component CreateNailCollider(Delegate callback)
        => CreateNailColliderDelegate?.Invoke(callback, null);


    /// <summary>
    ///     Returns a KelperNailCollider as a Component with a custom hitbox that interacts with the nail.
    /// </summary>
    /// <code>
    ///     Add(CreateNailCollider(new Hitbox(1,2,3,4), OnNailHit);
    ///     public bool OnNailHit(Player player, Vector2 nailDir) { return true; }
    /// </code>
    /// <param name="hitbox">The custom hitbox that interacts with the nail.</param>
    /// <param name="callback">A callback function that gets run when the entity is hit with the nail.</param>
    /// <returns>The component or null if Kelper is not enabled.</returns>
    public static Component CreateNailColliderWithCustomHitbox(Hitbox hitbox, Delegate callback)
        => CreateNailColliderWithCustomHitboxDelegate?.Invoke(callback, hitbox, null);

    /// <summary>
    /// Returns a nailDir that is flipped vertically if the player has upside down gravity. <br/>
    /// Vector2(0, 1) means that the player is swinging towards their feet.
    /// </summary>
    /// <returns>Vector2 or null if Kelper is not enabled.</returns>>
    public static Vector2? GetGravityRelativeNailDir()
        => GetGravityRelativeNailDirDelegate?.Invoke();


    /// <summary>
    ///     Bounces the player backwards relative to the nail swing direction. <br/>
    ///     If multiple entities are hit on the same frame, the highest multiplier will be applied.
    ///     Has logic to handle adding speed, doing nail wallbounces and backboosting etc.
    /// </summary>
    /// <param name="multiplier">Rebound strength multiplier. Minimum value is 1f</param>
    public static void ApplyNailRebound(float multiplier = 1f)
        => ApplyNailReboundDelegate?.Invoke(multiplier);


    /// <summary>
    ///     Plays a kind of metallic tink sound
    /// </summary>
    public static void PlayNailTinkSound()
        => PlayNailTinkSoundDelegate?.Invoke();


    /// <summary>
    /// Disables the nail's hitbox for the current swing. All entities hit on this frame will still be hit.
    /// </summary>
    public static void ConsumeNailSwing()
        => ConsumeNailSwingDelegate?.Invoke();

    /// <summary>
    ///     Creates a HitCounter and returns it as a Component.
    /// </summary>
    /// <code>
    ///     Add(CreateHitCounter(3, -10, true, Color.Purple, ReachedZero));
    ///     public void ReachedZero() { }
    /// </code>
    /// <param name="count">The amount of nail hits.</param>
    /// <param name="displayOffset">How high the count display should be.</param>
    /// <param name="disableKNCWhen0">Automatically removes kelper nail colliders from the entity.</param>
    /// <param name="color">The color of the count display.</param>
    /// <param name="callBack">A method that gets run when the counter reaches 0.</param>
    /// <returns>The component or null if Kelper is not enabled.</returns>
    public static Component CreateHitCounter(int count, int displayOffset, bool disableKNCWhen0, Color color, Delegate callBack = null)
        => CreateHitCounterDelegate?.Invoke(count, displayOffset, disableKNCWhen0, color, callBack, null);





    // Delegates used by the ModInterop.
    [Obsolete("Use the provided method instead of the delegate.")]
    public static Func<int, int, bool, Color, Delegate, object[], Component> CreateHitCounterDelegate;
    [Obsolete("Use the provided method instead of the delegate.")]
    public static Action ConsumeNailSwingDelegate;
    [Obsolete("Use the provided method instead of the delegate.")]
    public static Action PlayNailTinkSoundDelegate;
    [Obsolete("Use the provided method instead of the delegate.")]
    public static Action<float> ApplyNailReboundDelegate;
    [Obsolete("Use the provided method instead of the delegate.")]
    public static Func<Vector2> GetGravityRelativeNailDirDelegate;
    [Obsolete("Use the provided method instead of the delegate.")]
    public static Func<Delegate, Hitbox, object[], Component> CreateNailColliderWithCustomHitboxDelegate;
    [Obsolete("Use the provided method instead of the delegate.")]
    public static Func<Delegate, object[], Component> CreateNailColliderDelegate;
    [Obsolete("Use the provided method instead of the delegate.")]
    public static Func<int> GetVersionDelegate;

    public static void Load()
    {
        typeof(KelperImports).ModInterop();
    }

}

