using Microsoft.AspNetCore.Mvc;

namespace OCID.Api.Controllers;

/// <summary>
/// Contrôleur de base de l'API OCID : porte le comportement commun à tous les contrôleurs qui en héritent.
/// </summary>
/// <remarks>
/// L'attribut <c>[ApiController]</c> active pour toutes les classes dérivées :
/// <list type="bullet">
/// <item><description>la validation automatique du corps des requêtes (annotations <c>[Required]</c>, <c>[StringLength]</c>...) : un modèle invalide renvoie directement un <c>400 Bad Request</c> au format <c>ProblemDetails</c>, sans que l'action soit appelée ;</description></item>
/// <item><description>l'inférence des sources de binding (<c>[FromBody]</c> pour un type complexe, <c>[FromRoute]</c> pour un paramètre présent dans la route) ;</description></item>
/// </list>
/// Les contrôleurs d'équipements, d'alertes, d'incidents, de notes et d'accueil en dérivent pour partager <see cref="ErreurZabbix"/>.
/// </remarks>
[ApiController]
public abstract class BaseController : ControllerBase
{
    // protected Guid CurrentUserId =>
    //     Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)
    //         ?? throw new InvalidOperationException("Authenticated user has no NameIdentifier claim."));
}
