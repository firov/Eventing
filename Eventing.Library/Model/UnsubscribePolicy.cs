namespace Eventing.Library {
    public enum UnsubscribePolicy {
        /// <summary>
        ///     Do not remove subscription automatically
        /// </summary>
        Manual,

        /// <summary>
        ///     Remove subscription from bus after first accepted message
        /// </summary>
        Auto
    }
}