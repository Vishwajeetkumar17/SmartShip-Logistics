import { Component, Input } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-loader',
  standalone: true,
  imports: [CommonModule],
  template: `
    @if (visible) {
      <div class="loader-overlay" [class.inline]="inline">
        <div class="spinner"></div>
        @if (message) {
          <p class="loader-message">{{ message }}</p>
        }
      </div>
    }
  `,
  styles: [`
    .loader-overlay {
      display: flex;
      flex-direction: column;
      align-items: center;
      justify-content: center;
      padding: 3rem 2rem;
      gap: 1rem;
    }

    .loader-overlay.inline {
      padding: 1.5rem;
    }

    .spinner {
      width: 40px;
      height: 40px;
      border: 3px solid #e2e8f0;
      border-top-color: #3b82f6;
      border-radius: 50%;
      animation: spin 0.75s linear infinite;
    }

    .loader-overlay.inline .spinner {
      width: 24px;
      height: 24px;
      border-width: 2px;
    }

    .loader-message {
      color: #64748b;
      font-size: 0.9rem;
      margin: 0;
    }

    @keyframes spin {
      to { transform: rotate(360deg); }
    }
  `]
})
export class LoaderComponent {
  @Input() visible = true;
  @Input() message = '';
  @Input() inline = false;
}
