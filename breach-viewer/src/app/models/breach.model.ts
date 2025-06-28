export interface Breach {
  title: string;
  name: string;
  domain: string;
  breachDate: string; // ISO date string
  addedDate: string; // ISO date string
  modifiedDate: string; // ISO date string
  description: string;
  pwnCount: number;
  logoPath: string;
} 