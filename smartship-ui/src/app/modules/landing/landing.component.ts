import { Component, OnInit, OnDestroy, AfterViewInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule, Router } from '@angular/router';
import { FormsModule } from '@angular/forms';

@Component({
  selector: 'app-landing',
  standalone: true,
  imports: [CommonModule, RouterModule, FormsModule],
  templateUrl: './landing.component.html',
  styleUrl: './landing.component.css'
})
export class LandingComponent implements OnInit, AfterViewInit, OnDestroy {
  currentYear = new Date().getFullYear();
  trackingId = '';
  mobileMenuOpen = false;
  navScrolled = false;
  private router = inject(Router);

  // Counter animation targets
  counters = [
    { current: 0, target: 8400, label: 'Shipments processed this quarter', suffix: '', icon: 'inventory_2', color: 'indigo' },
    { current: 0, target: 1.6, label: 'Average delivery time', suffix: ' days', icon: 'schedule', color: 'blue', isDecimal: true },
    { current: 0, target: 34, label: 'Active routes nationwide', suffix: '', icon: 'route', color: 'purple' }
  ];

  // Parcel animation state
  parcelStep = 0;
  private parcelInterval: any;
  private scrollListener: any;
  private counterAnimated = false;
  private observers: IntersectionObserver[] = [];

  ngOnInit() {
    this.scrollListener = () => {
      this.navScrolled = window.scrollY > 20;
    };
    window.addEventListener('scroll', this.scrollListener, { passive: true });
    this.startParcelAnimation();
  }

  ngAfterViewInit() {
    this.setupScrollAnimations();
    this.setupCounterAnimation();
  }

  ngOnDestroy() {
    if (this.scrollListener) {
      window.removeEventListener('scroll', this.scrollListener);
    }
    if (this.parcelInterval) {
      clearInterval(this.parcelInterval);
    }
    this.observers.forEach(obs => obs.disconnect());
  }

  toggleMobileMenu() {
    this.mobileMenuOpen = !this.mobileMenuOpen;
  }

  closeMobileMenu() {
    this.mobileMenuOpen = false;
  }

  startParcelAnimation() {
    this.parcelInterval = setInterval(() => {
      this.parcelStep = (this.parcelStep + 1) % 5;
    }, 2200);
  }

  onTrack() {
    this.router.navigate(['/auth/login']);
  }

  scrollTo(fragment: string, event: Event) {
    event.preventDefault();
    this.closeMobileMenu();
    const el = document.getElementById(fragment);
    if (el) {
      const navHeight = 64;
      const pos = el.getBoundingClientRect().top + window.scrollY - navHeight - 20;
      window.scrollTo({ top: pos, behavior: 'smooth' });
    }
  }

  private setupScrollAnimations() {
    const observer = new IntersectionObserver((entries) => {
      entries.forEach(entry => {
        if (entry.isIntersecting) {
          entry.target.classList.add('visible');
          observer.unobserve(entry.target);
        }
      });
    }, { threshold: 0.1, rootMargin: '0px 0px -40px 0px' });

    document.querySelectorAll('.animate-on-scroll').forEach(el => observer.observe(el));
    this.observers.push(observer);
  }

  private setupCounterAnimation() {
    const statsSection = document.getElementById('stats');
    if (!statsSection) return;

    const observer = new IntersectionObserver((entries) => {
      entries.forEach(entry => {
        if (entry.isIntersecting && !this.counterAnimated) {
          this.counterAnimated = true;
          this.animateCounters();
          observer.unobserve(entry.target);
        }
      });
    }, { threshold: 0.3 });

    observer.observe(statsSection);
    this.observers.push(observer);
  }

  private animateCounters() {
    const duration = 2000;
    const startTime = performance.now();

    const animate = (currentTime: number) => {
      const elapsed = currentTime - startTime;
      const progress = Math.min(elapsed / duration, 1);
      const eased = 1 - Math.pow(1 - progress, 3);

      this.counters.forEach(c => {
        if (c.isDecimal) {
          c.current = Math.round(eased * c.target * 10) / 10;
        } else {
          c.current = Math.round(eased * c.target);
        }
      });

      if (progress < 1) {
        requestAnimationFrame(animate);
      }
    };

    requestAnimationFrame(animate);
  }

  formatCounter(counter: any): string {
    if (counter.isDecimal) {
      return counter.current.toFixed(1) + counter.suffix;
    }
    return counter.current.toLocaleString('en-IN') + counter.suffix;
  }
}
