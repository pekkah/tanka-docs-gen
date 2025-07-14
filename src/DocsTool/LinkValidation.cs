namespace Tanka.DocsTool;

/// <summary>
/// Defines the validation mode for broken xref links
/// </summary>
public enum LinkValidation
{
    /// <summary>
    /// Broken xref links cause build errors and build failure
    /// </summary>
    Strict,

    /// <summary>
    /// Broken xref links generate warnings but build continues with placeholder links
    /// </summary>
    Relaxed
}