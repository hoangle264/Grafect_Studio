using Prism.Events;

namespace GrafcetStudio.WPF.Events;

/// <summary>
/// Published whenever the document data changes from any table view,
/// signalling all subscribers to reload from GrafcetDocument.
/// </summary>
public class DocumentChangedEvent : PubSubEvent { }
