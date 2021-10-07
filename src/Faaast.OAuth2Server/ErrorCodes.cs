namespace Faaast.Authentication.OAuth2Server
{
    /// <summary>
    /// Standard errors
    /// See https://datatracker.ietf.org/doc/html/rfc6749#section-4.1.2.1
    /// </summary>
    public enum ErrorCodes
    {
        /// <summary>
        /// The request is missing a required parameter, includes an invalid parameter value, includes a parameter more than once, or is otherwise malformed 
        /// </summary>
        invalid_request,
        /// <summary>
        /// The client is not authorized to request an authorization code using this method
        /// </summary>
        unauthorized_client,
        /// <summary>
        /// The resource owner or authorization server denied the request.
        /// </summary>
        access_denied,
        /// <summary>
        /// The authorization server does not support obtaining an authorization code using this method
        /// </summary>
        unsupported_response_type,
        /// <summary>
        /// The requested scope is invalid, unknown, or malformed
        /// </summary>
        invalid_scope,
        /// <summary>
        /// The authorization server encountered an unexpected condition that prevented it from fulfilling the request. (This error code is needed because a 500 Internal Server Error HTTP status code cannot be returned to the client via an HTTP redirect.)
        /// </summary>
        server_error,
        /// <summary>
        /// The authorization server is currently unable to handle the request due to a temporary overloading or maintenance of the server.  (This error code is needed because a 503 Service Unavailable HTTP status code cannot be returned to the client via an HTTP redirect.)
        /// </summary>
        temporarily_unavailable
    }
}
