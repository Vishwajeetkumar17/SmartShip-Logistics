import { Component, inject, OnInit, OnDestroy, ViewChild, ElementRef, HostListener } from '@angular/core';
import { RouterLink, Router } from '@angular/router';
import { Location } from '@angular/common';

interface Sparkle {
  x: number;
  y: number;
  vx: number;
  vy: number;
  life: number;
  size: number;
  color: string;
}

@Component({
  selector: 'app-not-found',
  imports: [RouterLink],
  templateUrl: './not-found.html',
  styleUrl: './not-found.css',
})
export class NotFound implements OnInit, OnDestroy {
  @ViewChild('sparkleCanvas', { static: true }) canvasRef!: ElementRef<HTMLCanvasElement>;
  
  private location = inject(Location);
  private router = inject(Router);

  currentUrl: string = this.router.url;
  
  private ctx!: CanvasRenderingContext2D;
  private sparkles: Sparkle[] = [];
  private animationFrameId: number = 0;
  // Using primary branding colors for sparkles
  private colors = ['#6366F1', '#818CF8', '#A78BFA', '#38BDF8'];

  ngOnInit() {
    this.ctx = this.canvasRef.nativeElement.getContext('2d')!;
    this.resizeCanvas();
    this.animate();
  }

  ngOnDestroy() {
    if (this.animationFrameId) {
      cancelAnimationFrame(this.animationFrameId);
    }
  }

  @HostListener('window:resize')
  onResize() {
    this.resizeCanvas();
  }

  resizeCanvas() {
    this.canvasRef.nativeElement.width = window.innerWidth;
    this.canvasRef.nativeElement.height = window.innerHeight;
  }

  onMouseMove(e: MouseEvent) {
    // Generate sparkles on mouse move
    for (let i = 0; i < 4; i++) {
        this.sparkles.push({
            x: e.clientX,
            y: e.clientY,
            vx: (Math.random() - 0.5) * 3,
            vy: (Math.random() - 0.5) * 3 - 0.5, // Slight upward bias
            life: 1,
            size: Math.random() * 3 + 1.5,
            color: this.colors[Math.floor(Math.random() * this.colors.length)]
        });
    }
  }

  animate() {
    this.animationFrameId = requestAnimationFrame(() => this.animate());
    
    // Clear canvas completely to allow CSS background to show through
    this.ctx.clearRect(0, 0, window.innerWidth, window.innerHeight);
    
    for (let i = this.sparkles.length - 1; i >= 0; i--) {
        const p = this.sparkles[i];
        p.x += p.vx;
        p.y += p.vy;
        p.vy += 0.05; // slight gravity
        p.life -= 0.015; // fade out speed
        
        if (p.life <= 0) {
            this.sparkles.splice(i, 1);
            continue;
        }
        
        this.ctx.globalAlpha = p.life;
        this.ctx.fillStyle = p.color;
        
        // Draw star shape
        this.drawStar(this.ctx, p.x, p.y, 4, p.size, p.size / 2);
    }
  }

  drawStar(ctx: CanvasRenderingContext2D, cx: number, cy: number, spikes: number, outerRadius: number, innerRadius: number) {
    let rot = Math.PI / 2 * 3;
    let x = cx;
    let y = cy;
    let step = Math.PI / spikes;

    ctx.beginPath();
    ctx.moveTo(cx, cy - outerRadius);
    for (let i = 0; i < spikes; i++) {
        x = cx + Math.cos(rot) * outerRadius;
        y = cy + Math.sin(rot) * outerRadius;
        ctx.lineTo(x, y);
        rot += step;

        x = cx + Math.cos(rot) * innerRadius;
        y = cy + Math.sin(rot) * innerRadius;
        ctx.lineTo(x, y);
        rot += step;
    }
    ctx.lineTo(cx, cy - outerRadius);
    ctx.closePath();
    ctx.fill();
  }

  goBack() {
    this.location.back();
  }
}
