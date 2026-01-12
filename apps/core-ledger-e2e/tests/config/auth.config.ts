/**
 * Authentication configuration for API tests
 */

export const authConfig = {
    adminToken: 'eyJhbGciOiJub25lIiwidHlwIjoiSldUIn0.eyJpc3MiOiJodHRwczovL2xvY2FsZGV2LmNvbS8iLCJzdWIiOiJtb2NrfGFkbWluLTAwMSIsImF1ZCI6WyJodHRwczovL2NvcmUtbGVkZ2VyLWFwaSIsImh0dHBzOi8vbG9jYWxkZXovdXNlcmluZm8iXSwiaWF0IjoxNzY3MjExMDc3LCJleHAiOjE3NjcyOTc0NzcsInNjb3BlIjoib3BlbmlkIHByb2ZpbGUgZW1haWwiLCJhenAiOiJQRFBuVDE5ZnREQkF2NVZMamczT1I0VTZONXdxa0dvaFQiLCJwZXJtaXNzaW9ucyI6WyJhZG1pbjpzZWN1cml0aWVzOm1hbmFnZSIsImFkbWluOnNlY3VyaXRpZXM6cmVhZCIsImZ1bmQ6bWFuYWdlIiwiZnVuZDpyZWFkIiwibGVkZ2VyOmFjY291bnQ6bWFuYWdlIiwibGVkZ2VyOmFjY291bnQ6cmVhZCIsImxlZGdlcjp0cmFuc2FjdGlvbjptYW5hZ2UiLCJsZWRnZXI6dHJhbnNhY3Rpb246cmVhZCIsInBvcnRmb2xpbzptYW5hZ2UiLCJwb3J0Zm9saW86cmVhZCIsIm5hdjptYW5hZ2UiLCJuYXY6cmVhZCIsInJlcG9ydHM6cmVhZCJdfQ.',

    /**
     * Get authorization header
     * @param token - The JWT token (defaults to adminToken)
     * @returns Authorization header object
     */
    getAuthHeader: (token: string = authConfig.adminToken) => ({
        'Authorization': `Bearer ${token}`
    })
};
