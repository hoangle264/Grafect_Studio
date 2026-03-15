using Prism.Events;

namespace GrafcetStudio.WPF.Events;

/// <summary>
/// Published by MainWindow just before the application closes.
/// The payload is the AvalonDock layout serialized as an XML string.
/// </summary>
public class WindowClosingEvent : PubSubEvent<string> { }
