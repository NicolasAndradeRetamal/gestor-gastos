namespace GestorGastos.Domain.Common;

/// <summary>Requested resource does not exist or does not belong to the current user.</summary>
public class NotFoundException(string message) : Exception(message);

/// <summary>The request conflicts with the current state of the resource (e.g. duplicate name).</summary>
public class ConflictException(string message) : Exception(message);

/// <summary>Credentials supplied by the client are not valid.</summary>
public class InvalidCredentialsException(string message) : Exception(message);
