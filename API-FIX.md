# API Connection Fix for EgzamApp

This document explains the fixes made to resolve the API connection issue resulting in the error: `Request failed with status code 404`.

## Issue Summary

The API requests were returning 404 errors because the frontend was trying to access the API endpoints at `https://localhost:5173/api/exams/upload` (the frontend port) instead of correctly proxying the requests to the backend at `http://localhost:5080/api/exams/upload`.

## Fixes Applied

### 1. Vite Proxy Configuration

Updated the proxy configuration in `vite.config.js` to properly route API requests:

```javascript
proxy: {
  '/api': {
    target: 'http://localhost:5080',
    changeOrigin: true,
    secure: false
  },
  '^/weatherforecast': {
    target: 'http://localhost:5080',
    secure: false
  }
}
```

This ensures that any request to `/api/*` from the frontend gets correctly routed to the backend API running on port 5080.

### 2. Enhanced Error Handling

Added improved error handling and debugging capabilities:

- Added Axios interceptors to log all requests and responses
- Improved error handling in the ExamUpload component
- Added detailed error information display for better debugging
- Added more file type validation and error messaging

### 3. Improved API Service

- Added request/response logging for easier debugging
- Added better error handling with detailed console logging
- Added file information logging for upload tracking

## How to Verify the Fix

1. Make sure both the backend and frontend are running:
   - Backend on http://localhost:5080
   - Frontend on https://localhost:5173

2. Open the browser's developer console (F12)

3. Try uploading a JSON exam file - you should see detailed logs in the console for:
   - The request being made to `/api/exams/upload`
   - The request being proxied to the backend
   - The response from the backend

4. If there are still issues, the detailed error information displayed in the UI will help diagnose the problem.

## Common Issues

If you still encounter problems:

1. **CORS Issues**: Check if there are any CORS-related errors in the console. The proxy should handle these, but the backend might need additional configuration.

2. **File Type Issues**: Make sure you're uploading valid JSON files.

3. **Backend Not Running**: Ensure the backend API is actually running on port 5080.

4. **Certificate Issues**: If you see certificate warnings, you may need to accept the certificate or regenerate the development certificates.
