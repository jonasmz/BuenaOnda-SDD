namespace BuenaOnda.Domain.Common;

/// <summary>Base de las excepciones que expresan el incumplimiento de una regla de dominio.</summary>
public abstract class DomainException(string message) : Exception(message);

/// <summary>Datos ausentes o inválidos (se traduce a 400).</summary>
public sealed class ValidationException(string message) : DomainException(message);

/// <summary>El recurso solicitado no existe (se traduce a 404).</summary>
public sealed class NotFoundException(string message) : DomainException(message);

/// <summary>La operación contradice el estado o las reglas vigentes (se traduce a 409).</summary>
public sealed class ConflictException(string message) : DomainException(message);
