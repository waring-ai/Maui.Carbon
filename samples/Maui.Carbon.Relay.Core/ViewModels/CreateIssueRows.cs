namespace Maui.Carbon.Relay.ViewModels;

/// <summary>A section of the create-issue review: what was entered, and the wizard step that edits it.</summary>
public sealed record ReviewRow(string Section, string Details, int Step);
