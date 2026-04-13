import { Component, inject, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { AdminService, DashboardMetrics } from '../../../core/services/admin.service';
import { ShipmentResponse } from '../../../shared/models/shipment.model';
import { BaseChartDirective } from 'ng2-charts';
import { ChartConfiguration, ChartType } from 'chart.js';
import { provideCharts, withDefaultRegisterables } from 'ng2-charts';
import { catchError, finalize, timeout } from 'rxjs/operators';
import { forkJoin, of } from 'rxjs';

@Component({
  selector: 'app-admin-dashboard',
  standalone: true,
  imports: [CommonModule, RouterModule, BaseChartDirective],
  providers: [provideCharts(withDefaultRegisterables())],
  templateUrl: './dashboard.component.html',
  styleUrl: './dashboard.component.css'
})
/**
 * Admin dashboard overview.
 * Combines API-provided metrics with live shipment-derived stats (when available) and renders chart summaries.
 */
export class DashboardComponent implements OnInit {
  private adminService = inject(AdminService);
  private readonly _metrics = signal<DashboardMetrics | null>(null);
  private readonly _isLoading = signal(true);
  private readonly _error = signal('');

  get metrics(): DashboardMetrics | null {
    return this._metrics();
  }

  get isLoading(): boolean {
    return this._isLoading();
  }

  get error(): string {
    return this._error();
  }

  // Chart Properties
  public lineChartData: ChartConfiguration['data'] = {
    datasets: [],
    labels: []
  };
  public lineChartOptions: ChartConfiguration['options'] = {
    responsive: true,
    maintainAspectRatio: false,
    elements: {
      line: {
        tension: 0.4
      }
    },
    scales: {
      y: {
        beginAtZero: true
      }
    }
  };
  public lineChartType: ChartType = 'line';

  public pieChartData: ChartConfiguration<'pie'>['data'] = {
    labels: [],
    datasets: [{ data: [], backgroundColor: [] }]
  };
  public pieChartOptions: ChartConfiguration['options'] = {
    responsive: true,
    maintainAspectRatio: false,
    plugins: {
      legend: { position: 'right' }
    }
  };
  public pieChartType: ChartType = 'pie';

  public barChartData: ChartConfiguration<'bar'>['data'] = {
    labels: [],
    datasets: []
  };
  public barChartOptions: ChartConfiguration['options'] = {
    responsive: true,
    maintainAspectRatio: false,
    scales: {
      y: { beginAtZero: true, max: 100 }
    }
  };
  public barChartType: ChartType = 'bar';

  ngOnInit(): void {
    this._isLoading.set(true);
    this._error.set('');

    forkJoin({
      statistics: this.adminService.getDashboardMetrics().pipe(timeout(10000), catchError(() => of(null))),
      shipments: this.adminService.getShipmentsAll().pipe(timeout(10000), catchError(() => of([] as ShipmentResponse[])))
    }).pipe(
      finalize(() => this._isLoading.set(false))
    ).subscribe({
      next: ({ statistics, shipments }) => {
        const metrics = this.buildMetrics(statistics, shipments);
        this._metrics.set(metrics);
        this.setupCharts(metrics);
      },
      error: (err) => {
        console.error('Admin Dashboard Error:', err);
        this._error.set('Failed to load dashboard metrics.');
      }
    });
  }

  private buildMetrics(statistics: DashboardMetrics | null, shipments: ShipmentResponse[]): DashboardMetrics {
    const liveShipments = Array.isArray(shipments) ? shipments : [];
    const totalCount = liveShipments.length;
    const inTransitCount = liveShipments.filter(s => ['InTransit', 'OutForDelivery', 'PickedUp'].includes(String(s.status))).length;
    const deliveredRows = liveShipments.filter(s => String(s.status) === 'Delivered');
    const deliveredCount = deliveredRows.length;

    // Use delivery success ratio for dashboard KPI to avoid stale-date bias.
    const liveOnTimeDeliveryPercent = totalCount > 0
      ? Math.round((deliveredCount * 10000) / totalCount) / 100
      : 0;

    const now = new Date();

    const liveAvgDeliveryTimeDays = deliveredCount > 0
      ? Math.round((deliveredRows.reduce((sum, s) => {
          const createdAt = new Date(s.createdAt);
          if (Number.isNaN(createdAt.getTime())) return sum;
          return sum + ((now.getTime() - createdAt.getTime()) / (24 * 60 * 60 * 1000));
        }, 0) / deliveredCount) * 100) / 100
      : 0;

    const statsTotalCount = Number(statistics?.totalShipments?.count ?? 0);
    const statsInTransitCount = Number(statistics?.inTransit?.count ?? 0);
    const statsDeliveredCount = Number(statistics?.delivered?.count ?? 0);

    const dailyShipments = liveShipments.length > 0
      ? this.buildDailyShipments(liveShipments)
      : (statistics?.dailyShipments ?? []);

    const serviceTypeDistribution = liveShipments.length > 0
      ? this.buildStatusDistribution(liveShipments)
      : (statistics?.serviceTypeDistribution ?? []);

    const deliveryPerformanceTrend = liveShipments.length > 0
      ? this.buildMonthlyTrend(liveShipments)
      : (statistics?.deliveryPerformanceTrend ?? []);

    const totalShipmentsCount = liveShipments.length > 0 ? totalCount : statsTotalCount;
    const inTransitShipmentsCount = liveShipments.length > 0 ? inTransitCount : statsInTransitCount;
    const deliveredShipmentsCount = liveShipments.length > 0 ? deliveredCount : statsDeliveredCount;
    const issuesCount = statistics?.exceptions?.count ?? 0;
    const onTimeDeliveryPercent = liveShipments.length > 0 ? liveOnTimeDeliveryPercent : Number(statistics?.onTimeDeliveryPercent ?? 0);
    const avgDeliveryTimeDays = liveShipments.length > 0 ? liveAvgDeliveryTimeDays : Number(statistics?.avgDeliveryTimeDays ?? 0);
    
    // Auto-calculate percentage changes dynamically based on current vs total counts
    const calculateTrend = (current: number, total: number) => {
        if (total === 0) return 0;
        // Mock a percentage change logic since there is no 'previous' historical data returned from API for shipments yet
        // So we compare current vs total to show a dynamic percentage, or stick to backend if provided.
        // E.g. In Transit is X% of Total Shipments
        return Math.round((current / total) * 100);
    };

    return {
      totalShipments: {
        count: totalShipmentsCount,
        percentageChange: statistics?.totalShipments?.percentageChange || calculateTrend(totalShipmentsCount, totalShipmentsCount) // Usually 100% or provided by backend
      },
      inTransit: {
        count: inTransitShipmentsCount,
        percentageChange: calculateTrend(inTransitShipmentsCount, totalShipmentsCount) || statistics?.inTransit?.percentageChange || 0
      },
      delivered: {
        count: deliveredShipmentsCount,
        percentageChange: calculateTrend(deliveredShipmentsCount, totalShipmentsCount) || statistics?.delivered?.percentageChange || 0
      },
      exceptions: {
        count: issuesCount,
        percentageChange: calculateTrend(issuesCount, totalShipmentsCount) || statistics?.exceptions?.percentageChange || 0
      },
      onTimeDeliveryPercent,
      avgDeliveryTimeDays,
      dailyShipments,
      serviceTypeDistribution,
      deliveryPerformanceTrend
    };
  }

  getTrendClass(value: number): 'positive' | 'negative' {
    return value >= 0 ? 'positive' : 'negative';
  }

  formatTrend(value: number): string {
    return `${value > 0 ? '+' : ''}${value}%`;
  }

  private buildDailyShipments(shipments: ShipmentResponse[]): DashboardMetrics['dailyShipments'] {
    const now = new Date();
    const dateKeys = Array.from({ length: 7 }, (_, idx) => {
      const d = new Date(now);
      d.setDate(now.getDate() - (6 - idx));
      return d;
    });

    return dateKeys.map(d => {
      const key = `${d.getFullYear()}-${d.getMonth()}-${d.getDate()}`;
      const count = shipments.filter(s => {
        const createdAt = new Date(s.createdAt);
        if (Number.isNaN(createdAt.getTime())) return false;
        const shipmentKey = `${createdAt.getFullYear()}-${createdAt.getMonth()}-${createdAt.getDate()}`;
        return shipmentKey === key;
      }).length;

      return {
        date: d.toLocaleDateString('en-IN', { month: 'short', day: 'numeric' }),
        shipments: count
      };
    });
  }

  private buildStatusDistribution(shipments: ShipmentResponse[]): DashboardMetrics['serviceTypeDistribution'] {
    if (shipments.length === 0) {
      return [];
    }

    const grouped = new Map<string, number>();
    for (const shipment of shipments) {
      const key = String(shipment.status || 'Draft');
      grouped.set(key, (grouped.get(key) ?? 0) + 1);
    }

    return Array.from(grouped.entries())
      .map(([serviceType, count]) => ({
        serviceType,
        percentage: Math.round((count * 10000) / shipments.length) / 100
      }))
      .sort((a, b) => b.percentage - a.percentage);
  }

  private buildMonthlyTrend(shipments: ShipmentResponse[]): DashboardMetrics['deliveryPerformanceTrend'] {
    const now = new Date();

    return Array.from({ length: 6 }, (_, idx) => {
      const date = new Date(now.getFullYear(), now.getMonth() - (5 - idx), 1);
      const start = new Date(date.getFullYear(), date.getMonth(), 1);
      const end = new Date(date.getFullYear(), date.getMonth() + 1, 1);
      const monthRows = shipments.filter(s => {
        const createdAt = new Date(s.createdAt);
        return !Number.isNaN(createdAt.getTime()) && createdAt >= start && createdAt < end;
      });

      const total = monthRows.length;
      const delivered = monthRows.filter(s => String(s.status) === 'Delivered').length;
      const onTimePercent = total > 0 ? Math.round((delivered * 10000) / total) / 100 : 0;
      const delayedPercent = total > 0 ? Math.round((100 - onTimePercent) * 100) / 100 : 0;

      return {
        month: date.toLocaleDateString('en-IN', { month: 'short' }),
        onTimePercent,
        delayedPercent
      };
    });
  }

  private setupCharts(data: DashboardMetrics) {
    // 1. Line Chart: Daily Shipments
    const dailyShipments = data.dailyShipments ?? [];
    const dates = dailyShipments.map(ds => ds.date);
    const shipments = dailyShipments.map(ds => ds.shipments);
    this.lineChartData = {
      labels: dates,
      datasets: [
        {
          data: shipments,
          label: 'Shipments',
          backgroundColor: 'rgba(54, 162, 235, 0.2)',
          borderColor: 'rgba(54, 162, 235, 1)',
          pointBackgroundColor: 'rgba(54, 162, 235, 1)',
          fill: true,
        }
      ]
    };

    // 2. Pie Chart: Service Distribution
    const serviceDist = data.serviceTypeDistribution ?? [];
    const serviceLabels = serviceDist.map(s => s.serviceType);
    const serviceData = serviceDist.map(s => s.percentage);
    this.pieChartData = {
      labels: serviceLabels,
      datasets: [{
        data: serviceData,
        backgroundColor: ['#36A2EB', '#4BC0C0', '#FFCD56', '#FF6384']
      }]
    };

    // 3. Bar Chart: Delivery Performance Trend
    const perfTrend = data.deliveryPerformanceTrend ?? [];
    const months = perfTrend.map(pt => pt.month);
    const delayed = perfTrend.map(pt => pt.delayedPercent);
    const onTime = perfTrend.map(pt => pt.onTimePercent);
    this.barChartData = {
      labels: months,
      datasets: [
        { data: delayed, label: 'Delayed %', backgroundColor: '#F59E0B' },
        { data: onTime, label: 'On Time %', backgroundColor: '#10B981' }
      ]
    };
  }
}
