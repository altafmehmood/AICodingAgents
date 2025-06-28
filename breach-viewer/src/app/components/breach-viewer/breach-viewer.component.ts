import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatTableModule } from '@angular/material/table';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { MatIconModule } from '@angular/material/icon';
import { MatDatepickerModule } from '@angular/material/datepicker';
import { MatNativeDateModule } from '@angular/material/core';
import { BreachService } from '../../services/breach.service';
import { Breach } from '../../models/breach.model';

@Component({
  selector: 'app-breach-viewer',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    ReactiveFormsModule,
    MatFormFieldModule,
    MatInputModule,
    MatButtonModule,
    MatCardModule,
    MatTableModule,
    MatProgressSpinnerModule,
    MatSnackBarModule,
    MatIconModule,
    MatDatepickerModule,
    MatNativeDateModule
  ],
  templateUrl: './breach-viewer.component.html',
  styleUrls: ['./breach-viewer.component.css']
})
export class BreachViewerComponent implements OnInit {
  breachForm: FormGroup;
  breaches: Breach[] = [];
  displayedColumns: string[] = ['title', 'name', 'domain', 'breachDate', 'pwnCount', 'description'];
  loading = false;
  errorMessage = '';

  constructor(
    private formBuilder: FormBuilder,
    private breachService: BreachService,
    private snackBar: MatSnackBar
  ) {
    this.breachForm = this.formBuilder.group({
      fromDate: [null],
      toDate: [null]
    });
  }

  ngOnInit(): void {
    // Load all breaches on component initialization
    this.loadBreaches();
  }

  /**
   * Load breaches based on form data
   */
  loadBreaches(): void {
    this.loading = true;
    this.errorMessage = '';

    const fromDate = this.breachForm.get('fromDate')?.value;
    const toDate = this.breachForm.get('toDate')?.value;

    console.log('Loading breaches with dates:', { fromDate, toDate });

    this.breachService.getBreaches(fromDate, toDate).subscribe({
      next: (data) => {
        this.breaches = data;
        this.loading = false;
        console.log('Breaches loaded successfully:', data.length, 'items');
        this.snackBar.open(`Loaded ${data.length} breaches`, 'Close', {
          duration: 3000
        });
      },
      error: (error) => {
        this.loading = false;
        this.errorMessage = `Failed to load breaches: ${error.message}`;
        console.error('Error loading breaches:', error);
        this.snackBar.open(`Error: ${error.message}`, 'Close', {
          duration: 5000
        });
      }
    });
  }

  /**
   * Download breaches as PDF
   */
  downloadPdf(): void {
    this.loading = true;
    this.errorMessage = '';

    const fromDate = this.breachForm.get('fromDate')?.value;
    const toDate = this.breachForm.get('toDate')?.value;

    console.log('Downloading PDF with dates:', { fromDate, toDate });

    this.breachService.downloadBreachesPdf(fromDate, toDate).subscribe({
      next: (blob) => {
        this.loading = false;
        
        // Create download link
        const url = window.URL.createObjectURL(blob);
        const link = document.createElement('a');
        link.href = url;
        
        // Generate filename
        let filename = 'breaches';
        if (fromDate && toDate) {
          filename += `-${fromDate.toISOString().split('T')[0]}-${toDate.toISOString().split('T')[0]}`;
        } else if (fromDate) {
          filename += `-from-${fromDate.toISOString().split('T')[0]}`;
        } else if (toDate) {
          filename += `-to-${toDate.toISOString().split('T')[0]}`;
        } else {
          filename += `-${new Date().toISOString().split('T')[0]}`;
        }
        filename += '.pdf';
        
        link.download = filename;
        link.click();
        
        // Cleanup
        window.URL.revokeObjectURL(url);
        
        console.log('PDF downloaded successfully:', filename);
        this.snackBar.open('PDF downloaded successfully', 'Close', {
          duration: 3000
        });
      },
      error: (error) => {
        this.loading = false;
        this.errorMessage = `Failed to download PDF: ${error.message}`;
        console.error('Error downloading PDF:', error);
        this.snackBar.open(`Error: ${error.message}`, 'Close', {
          duration: 5000
        });
      }
    });
  }

  /**
   * Clear form and reload all breaches
   */
  clearFilters(): void {
    this.breachForm.reset();
    this.loadBreaches();
  }

  /**
   * Format date for display
   */
  formatDate(dateString: string): string {
    return new Date(dateString).toLocaleDateString();
  }

  /**
   * Format number with commas
   */
  formatNumber(num: number): string {
    return num.toLocaleString();
  }
} 