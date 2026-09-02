using System.Diagnostics.CodeAnalysis;
using System.Linq;
using UnityEngine;

namespace HackedDesign
{
    public static class UnityExtensions
    {
        public const string CloneSuffix = "(Clone)";
        public static void ClearCloneSuffix(this GameObject gameObject) => gameObject.ClearCloneSuffix("");

        public static void ClearCloneSuffix(this GameObject gameObject, string with) => gameObject.name = gameObject.name.Replace(CloneSuffix, with);

        /// <summary>
        /// Automatically binds a component in a monobehaviour to a reference if the reference is null. 
        /// Then logs a reminder warning 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="monoBehaviour"></param>
        /// <param name="reference"></param>
        public static void AutoBind<T>(this MonoBehaviour monoBehaviour, ref T reference)
        {
            if (reference == null)
            {
                reference ??= monoBehaviour.GetComponent<T>();
                Debug.LogWarning(nameof(reference) + " was not bound in the editor", monoBehaviour);
            }
        }

        /// https://github.com/adammyhre/Unity-GOAP/blob/master/Assets/_Project/Scripts/Utilities/GameObjectExtensions.cs
        /// <summary>
        /// Returns the object itself if it exists, null otherwise.
        /// </summary>
        /// <remarks>
        /// This method helps differentiate between a null reference and a destroyed Unity object. Unity's "== null" check
        /// can incorrectly return true for destroyed objects, leading to misleading behaviour. The OrNull method use
        /// Unity's "null check", and if the object has been marked for destruction, it ensures an actual null reference is returned,
        /// aiding in correctly chaining operations and preventing NullReferenceExceptions.
        /// </remarks>
        /// <typeparam name="T">The type of the object.</typeparam>
        /// <param name="obj">The object being checked.</param>
        /// <returns>The object itself if it exists and not destroyed, null otherwise.</returns>
        public static T OrNull<T>(this T obj) where T : Object => obj ? obj : null;

        public static bool EnsureNotNull<T>([NotNullWhen(true)] this T obj, Object context, string name) where T : Object
        {
            if (obj.OrNull() == null)
            {
                Debug.LogWarning($"{name} is null", context);
                return false;
            }
            return true;
        }

        public static bool EnsureNotNull<T>([NotNullWhen(true)] this T obj, string name) where T : Object
        {
            if (obj.OrNull() == null)
            {
                Debug.LogWarning($"{name} is null");
                return false;
            }
            return true;
        }

        public static void Require<T>(this T obj, Object context, string name) where T : Object
        {
            if (obj.OrNull() == null)
            {
                throw new MissingReferenceException($"{name} not assigned in {context.name}");
            }
        }

        public static void Require<T>(this T obj, string name) where T : Object
        {
            if (obj.OrNull() == null)
            {
                throw new MissingReferenceException($"{name} not assigned");
            }
        }

        public static Bounds GetWorldBounds(this GameObject obj)
        {
            // Exclude ParticleSystemRenderer - it reports zero-size bounds at world origin
            // whenever it has no live particles, which would otherwise drag the combined
            // bounds down to include (0,0,0).
            Renderer[] renderers = obj.GetComponentsInChildren<Renderer>()
                .Where(r => r is not ParticleSystemRenderer)
                .ToArray();

            if (renderers.Length == 0)
            {
                return new Bounds(obj.transform.position, Vector3.zero);
            }

            Bounds combinedBounds = renderers[0].bounds;
            for (int i = 1; i < renderers.Length; i++)
            {
                combinedBounds.Encapsulate(renderers[i].bounds);
            }

            return combinedBounds;
        }

    }
}
