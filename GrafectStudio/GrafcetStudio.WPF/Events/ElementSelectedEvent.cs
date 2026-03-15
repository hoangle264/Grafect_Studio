using Prism.Events;

namespace GrafcetStudio.WPF.Events;

/// <summary>
/// Published whenever the user selects or deselects an element on the GRAFCET canvas.
/// Payload is the selected ViewModel (StepViewModel / TransitionViewModel) or null when deselected.
/// </summary>
public class ElementSelectedEvent : PubSubEvent<object?> { }
