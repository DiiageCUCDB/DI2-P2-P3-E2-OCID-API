using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using OCID.Application.Interfaces;

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
/// Elle fournit aussi <see cref="LogAsync"/> et <see cref="CurrentUserId"/> aux contrôleurs dérivés.
/// </remarks>
[ApiController]
public abstract class BaseController : ControllerBase
{
    private readonly ILogService _logService;

    protected BaseController(ILogService logService)
    {
        _logService = logService;
    }

    /// <summary>Id de l'utilisateur authentifié, ou <c>null</c> si anonyme / claim absent.</summary>
    protected Guid? CurrentUserId =>
        Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var id) ? id : null;

    /// <summary>
    /// Crée rapidement un log. Si <paramref name="userId"/> est omis, l'utilisateur courant est utilisé.
    /// </summary>
    protected Task LogAsync(string eventName, Guid? userId = null, CancellationToken cancellationToken = default)
        => _logService.CreateAsync(eventName, userId ?? CurrentUserId, cancellationToken);
}