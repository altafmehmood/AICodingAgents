# Breach Viewer

A modern Angular application for viewing and filtering data breaches by date range. The application allows users to retrieve breaches from the API and download them as PDF files.

## Features

- **Date Range Filtering**: Filter breaches by from and to dates
- **Data Table Display**: View breaches in a responsive data table
- **PDF Download**: Download filtered breaches as PDF files
- **Modern UI**: Built with Angular Material for a professional look
- **Responsive Design**: Works on desktop and mobile devices
- **Real-time Loading**: Loading indicators and error handling

## Prerequisites

- Node.js (v18 or higher)
- Angular CLI (v20 or higher)
- .NET 8 SDK (for the API)

## Installation

1. Install dependencies:
```bash
npm install
```

2. Install Angular Material (if not already installed):
```bash
npm install @angular/material @angular/cdk @angular/animations
```

## Development

1. Start the Angular development server:
```bash
ng serve
```

2. The application will be available at `http://localhost:4200`

3. Make sure the BreachApi is running on `http://localhost:5089` (or update the API URL in the service)

## Usage

1. **Load All Breaches**: Click "Load Breaches" without selecting dates to view all breaches
2. **Filter by Date Range**: 
   - Select a "From Date" to filter breaches from that date onwards
   - Select a "To Date" to filter breaches up to that date
   - Select both dates to filter breaches within the range
3. **Download PDF**: Click "Download PDF" to download the filtered breaches as a PDF file
4. **Clear Filters**: Click "Clear Filters" to reset the date inputs and load all breaches

## API Endpoints

The application communicates with the following API endpoints:

- `GET /api/breaches` - Get breaches (supports `from` and `to` query parameters)
- `GET /api/breaches/pdf` - Download breaches as PDF (supports `from` and `to` query parameters)

## Mock Data

For development purposes, the API uses mock data instead of the real HaveIBeenPwned API. This includes:

- **Adobe** (2013-10-04): 153 million records
- **LinkedIn** (2012-06-05): 164 million records  
- **MySpace** (2013-01-01): 360 million records
- **Dropbox** (2012-07-01): 68.7 million records
- **Tumblr** (2013-02-28): 65.1 million records

To use real data, you'll need to:
1. Get an API key from https://haveibeenpwned.com/API/Key
2. Uncomment the `GetRealBreaches()` method in both query handlers
3. Add your API key to the configuration

## Project Structure

```
src/
├── app/
│   ├── components/
│   │   └── breach-viewer/
│   │       ├── breach-viewer.component.ts
│   │       ├── breach-viewer.component.html
│   │       └── breach-viewer.component.css
│   ├── models/
│   │   └── breach.model.ts
│   ├── services/
│   │   └── breach.service.ts
│   ├── app.ts
│   ├── app.config.ts
│   └── app.routes.ts
├── styles.css
└── index.html
```

## Configuration

### API URL

Update the API URL in `src/app/services/breach.service.ts` if your API is running on a different port:

```typescript
private readonly apiUrl = 'http://localhost:5089/api/breaches';
```

### CORS

The API needs to be configured to allow CORS requests from the Angular app. The API should include:

```csharp
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngularApp", policy =>
    {
        policy.WithOrigins("http://localhost:4200", "http://localhost:4201")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});
```

## Building for Production

1. Build the application:
```bash
ng build --configuration production
```

2. The built files will be in the `dist/` directory

## Technologies Used

- **Angular 20**: Frontend framework
- **Angular Material**: UI component library
- **TypeScript**: Programming language
- **RxJS**: Reactive programming library
- **Angular Forms**: Form handling and validation

## Browser Support

- Chrome (latest)
- Firefox (latest)
- Safari (latest)
- Edge (latest)

## Troubleshooting

### CORS Issues
If you encounter CORS errors, ensure:
1. The API is running and accessible
2. CORS is properly configured in the API
3. The API URL in the service is correct

### Date Format Issues
The application uses ISO date strings for API communication. Ensure your API accepts ISO date format.

### PDF Download Issues
If PDF downloads don't work:
1. Check browser console for errors
2. Ensure the API endpoint returns proper PDF content
3. Verify the API is running and accessible

### API Connection Issues
If you get "Failed to load breaches" errors:
1. Check that the API is running on the correct port (5089)
2. Verify the API URL in the service matches your API port
3. Check browser console for detailed error messages
4. Ensure CORS is properly configured in the API

### Mock Data Testing
To test with mock data:
1. The API handlers are configured to use mock data by default
2. You should see 5 sample breaches when you load the application
3. Date filtering should work with the mock data dates (2012-2013)

## Contributing

1. Follow the Angular style guide
2. Use TypeScript strict mode
3. Write unit tests for new features
4. Follow the existing code structure and naming conventions
