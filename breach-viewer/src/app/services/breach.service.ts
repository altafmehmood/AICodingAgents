import { Injectable } from '@angular/core';
import { HttpClient, HttpParams, HttpErrorResponse } from '@angular/common/http';
import { Observable, throwError } from 'rxjs';
import { catchError, tap } from 'rxjs/operators';
import { Breach } from '../models/breach.model';

@Injectable({
  providedIn: 'root'
})
export class BreachService {
  private readonly apiUrl = 'http://localhost:5089/api/breaches'; // Updated to match API port

  constructor(private http: HttpClient) { }

  /**
   * Get breaches filtered by date range
   * @param from Start date (optional)
   * @param to End date (optional)
   * @returns Observable of breach array
   */
  getBreaches(from?: Date, to?: Date): Observable<Breach[]> {
    let params = new HttpParams();
    
    if (from) {
      params = params.set('from', from.toISOString());
    }
    
    if (to) {
      params = params.set('to', to.toISOString());
    }

    console.log('Making API request to:', this.apiUrl, 'with params:', params.toString());

    return this.http.get<Breach[]>(this.apiUrl, { params }).pipe(
      tap(data => console.log('API response received:', data)),
      catchError(this.handleError)
    );
  }

  /**
   * Download breaches as PDF filtered by date range
   * @param from Start date (optional)
   * @param to End date (optional)
   * @returns Observable of blob for PDF download
   */
  downloadBreachesPdf(from?: Date, to?: Date): Observable<Blob> {
    let params = new HttpParams();
    
    if (from) {
      params = params.set('from', from.toISOString());
    }
    
    if (to) {
      params = params.set('to', to.toISOString());
    }

    console.log('Making PDF API request to:', `${this.apiUrl}/pdf`, 'with params:', params.toString());

    return this.http.get(`${this.apiUrl}/pdf`, { 
      params, 
      responseType: 'blob' 
    }).pipe(
      tap(data => console.log('PDF API response received, size:', data.size)),
      catchError(this.handleError)
    );
  }

  /**
   * Handle HTTP errors
   */
  private handleError(error: HttpErrorResponse) {
    console.error('API Error:', error);
    
    let errorMessage = 'An error occurred';
    if (error.error instanceof ErrorEvent) {
      // Client-side error
      errorMessage = `Client Error: ${error.error.message}`;
    } else {
      // Server-side error
      errorMessage = `Server Error: ${error.status} - ${error.message}`;
    }
    
    return throwError(() => new Error(errorMessage));
  }
} 