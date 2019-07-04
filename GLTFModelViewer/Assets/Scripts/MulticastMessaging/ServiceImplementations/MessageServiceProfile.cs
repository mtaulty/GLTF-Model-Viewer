using Microsoft.MixedReality.Toolkit;
using UnityEngine;

namespace MulticastMessaging
{

    [CreateAssetMenu(
        menuName = "Mixed Reality Toolkit/Message Service Profile",
        fileName = "MessageServiceProfile")]
    [MixedRealityServiceProfile(typeof(MessageService))]
    public class MessageServiceProfile : BaseMixedRealityProfile
    {
        [SerializeField]
        [Tooltip("The address to use for multicast messaging")]
        public string multicastAddress = "239.0.0.0";

        [SerializeField]
        [Tooltip("The port to use for multicast messaging")]
        public int multicastPort = 49152;
    }
}
