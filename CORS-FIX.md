# CORS Configuration Fix for EgzamApp

This document explains the fixes made to resolve the CORS (Cross-Origin Resource Sharing) issue: `Access to XMLHttpRequest at 'https://localhost:7068/api/exams/upload' from origin 'https://localhost:5173' has been blocked by CORS policy`.

## Issue Summary

The CORS error occurred because the frontend (running on https://localhost:5173) was trying to make an XMLHttpRequest to the backend (running on https://localhost:7068), but the backend wasn't configured to allow cross-origin requests from the frontend origin. By default, browsers implement a "same-origin policy" that prevents web pages from making requests to a different domain/port than the one the page was served from.

## Fixes Applied

### 1. Added CORS Configuration to Backend

Updated the `Program.cs` file to add CORS support:

```csharp
// Add CORS policy
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins("https://localhost:5173")
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
    });
});

// ... later in the request pipeline ...
// Enable CORS
app.UseCors();
```

Key components:
- `WithOrigins("https://localhost:5173")` - Specifically allows requests from the frontend origin
- `AllowAnyMethod()` - Allows all HTTP methods (GET, POST, PUT, DELETE, etc.)
- `AllowAnyHeader()` - Allows all HTTP headers to be sent
- `AllowCredentials()` - Allows credentials (cookies, authorization headers) to be sent

### 2. Updated Frontend Proxy Configuration

To ensure consistent communication, we also updated the Vite proxy configuration:

```javascript
// Define the backend URL - important to keep consistent
const BACKEND_URL = 'https://localhost:7068';

// ... in the defineConfig ...
server: {
    proxy: {
        // Updated proxy configuration to handle all API requests consistently
        '/api': {
            target: BACKEND_URL,
            changeOrigin: true,
            secure: false
        },
        '^/weatherforecast': {
            target: BACKEND_URL,
            secure: false
        }
    },
    // ...
}
```

This ensures all API requests from the frontend are consistently proxied to the same backend URL.

## How to Verify the Fix

1. Restart both the frontend and backend applications
2. Try uploading a JSON exam file through the frontend
3. Open the browser's developer tools (F12) to check the Network tab - you should see successful requests with status 200 OK

## Common CORS Issues

If you still encounter CORS issues:

1. **Check URLs**: Make sure the frontend origin URL in the CORS policy matches exactly what's shown in the error message
2. **CORS Middleware Order**: Ensure `app.UseCors()` is placed before other middleware that might handle requests
3. **Credentials**: If you're sending cookies or authorization headers, ensure the frontend is configured with `{ credentials: 'include' }` in fetch/axios calls
4. **HTTP vs HTTPS**: Make sure you're consistent with HTTP or HTTPS

## Additional Resources

- [Microsoft Docs: Enable CORS in ASP.NET Core](https://docs.microsoft.com/en-us/aspnet/core/security/cors)
- [MDN Web Docs: CORS](https://developer.mozilla.org/en-US/docs/Web/HTTP/CORS)
