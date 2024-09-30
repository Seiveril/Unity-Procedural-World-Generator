using System;

/// <summary>
/// Struct encapsulating information for a threaded operation callback.
/// </summary>
struct ThreadInfo
{
    /// <summary>
    /// Callback action to be executed with the parameter.
    /// </summary>
    public readonly Action<object> callback;

    /// <summary>
    /// Parameter passed to the callback action.
    /// </summary>
    public readonly object parameter;

    /// <summary>
    /// Constructor for MapThreadInfo struct.
    /// </summary>
    /// <param name="callback">Callback action to be executed.</param>
    /// <param name="parameter">Parameter passed to the callback action.</param>
    public ThreadInfo(Action<object> callback, object parameter)
    {
        this.callback = callback;
        this.parameter = parameter;
    }
}