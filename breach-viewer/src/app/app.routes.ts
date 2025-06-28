import { Routes } from '@angular/router';
import { BreachViewerComponent } from './components/breach-viewer/breach-viewer.component';

export const routes: Routes = [
  { path: '', component: BreachViewerComponent },
  { path: '**', redirectTo: '' }
];
