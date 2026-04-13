import { Component, inject, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { AdminService } from '../../../core/services/admin.service';
import { ShipmentResponse } from '../../../shared/models/shipment.model';
import { catchError, finalize } from 'rxjs/operators';
import { forkJoin, of } from 'rxjs';
import { BaseChartDirective } from 'ng2-charts';
import { ChartConfiguration } from 'chart.js';
import { timeout } from 'rxjs/operators';
import { provideCharts, withDefaultRegisterables } from 'ng2-charts';

@Component({
  selector: 'app-reports',
  standalone: true,
  imports: [CommonModule, FormsModule, BaseChartDirective],
  providers: [provideCharts(withDefaultRegisterables())],
  templateUrl: './reports.component.html',
  styleUrl: './reports.component.css'
})
/**
 * Admin reporting dashboard.
 * Aggregates multiple admin endpoints with timeouts/fallbacks, derives KPIs from live shipment data when available,
 * supports date-range filtering, drill-down details, and CSV export.
 */
export class ReportsComponent implements OnInit {
  private adminService = inject(AdminService);
  private readonly _isLoading = signal(false);
  private readonly _showDateRangePicker = signal(false);
  private readonly _dateRangeError = signal('');
  private readonly _detailsOpen = signal(false);
  private readonly _detailsTitle = signal('');
  private readonly _detailsRows = signal<Array<{ label: string; value: string }>>([]);
  private readonly _rawShipments = signal<ShipmentResponse[]>([]);
  private readonly _rawDailyShipments = signal<Array<{ date: string; shipments: number }>>([]);
  private readonly _rawDeliveryTrend = signal<Array<{ month: string; onTimePercent: number; delayedPercent: number }>>([]);

  readonly totalShipments = signal(0);
  readonly revenue = signal(0);
  readonly customerSatisfaction = signal(0);
  readonly baseTotalShipments = signal(0);
  readonly baseRevenue = signal(0);
  readonly baseCustomerSatisfaction = signal(0);
  readonly lineChartData = signal<ChartConfiguration<'line'>['data']>({
    labels: [],
    datasets: [{
      data: [],
      label: 'shipments',
      borderColor: '#3b82f6',
      backgroundColor: 'rgba(59, 130, 246, 0.18)',
      pointBackgroundColor: '#3b82f6',
      fill: false,
      tension: 0.35
    }]
  });
  readonly pieChartData = signal<ChartConfiguration<'pie'>['data']>({
    labels: [],
    datasets: [{
      data: [],
      backgroundColor: ['#3b82f6', '#10b981', '#f59e0b', '#ef4444'],
      borderWidth: 1,
      borderColor: '#ffffff'
    }]
  });
  readonly barChartData = signal<ChartConfiguration<'bar'>['data']>({
    labels: [],
    datasets: [
      { data: [], label: 'Delayed %', backgroundColor: '#f59e0b', borderRadius: 8 },
      { data: [], label: 'On Time %', backgroundColor: '#10b981', borderRadius: 8 }
    ]
  });
  readonly hubPerformanceRows = signal<Array<{ hubLocation: string; totalShipments: number; onTimePercent: number; avgProcessingTime: string; performance: 'Excellent' | 'Good' | 'Fair' | 'Poor'; }>>([]);

  fromDate = '';
  toDate = '';

  public readonly lineChartType: 'line' = 'line';
  public readonly pieChartType: 'pie' = 'pie';
  public readonly barChartType: 'bar' = 'bar';

  public readonly lineChartOptions: ChartConfiguration<'line'>['options'] = {
    responsive: true,
    maintainAspectRatio: false,
    plugins: { legend: { position: 'bottom' } },
    scales: {
      y: { beginAtZero: true, grid: { color: '#e2e8f0' } },
      x: { grid: { color: '#e2e8f0' } }
    }
  };

  public readonly pieChartOptions: ChartConfiguration<'pie'>['options'] = {
    responsive: true,
    maintainAspectRatio: false,
    plugins: { legend: { display: false } }
  };

  public readonly barChartOptions: ChartConfiguration<'bar'>['options'] = {
    responsive: true,
    maintainAspectRatio: false,
    plugins: { legend: { position: 'bottom' } },
    scales: {
      y: { beginAtZero: true, max: 100, grid: { color: '#e2e8f0' } },
      x: { grid: { display: false } }
    }
  };

  get isLoading(): boolean {
    return this._isLoading();
  }

  get showDateRangePicker(): boolean {
    return this._showDateRangePicker();
  }

  get dateRangeError(): string {
    return this._dateRangeError();
  }

  get detailsOpen(): boolean {
    return this._detailsOpen();
  }

  get detailsTitle(): string {
    return this._detailsTitle();
  }

  get detailsRows(): Array<{ label: string; value: string }> {
    return this._detailsRows();
  }

  ngOnInit(): void {
    this.loadReportDashboard();
  }

  loadReportDashboard(): void {
    this._isLoading.set(true);

    const withFallback = <T>(request$: import('rxjs').Observable<T>) =>
      request$.pipe(
        timeout(10000),
        catchError((err) => {
          console.error('Reports request fallback:', err);
          return of(null);
        })
      );

    forkJoin({
      overview: withFallback(this.adminService.getReportsOverview()),
      shipmentPerformance: withFallback(this.adminService.getShipmentPerformance()),
      deliverySla: withFallback(this.adminService.getDeliverySLA()),
      revenue: withFallback(this.adminService.getRevenue()),
      hubPerformance: withFallback(this.adminService.getHubPerformance()),
      statistics: withFallback(this.adminService.getDashboardMetrics()),
      shipments: withFallback(this.adminService.getShipmentsAll()),
    }).pipe(
      finalize(() => this._isLoading.set(false))
    ).subscribe((data) => {
      const liveShipments = Array.isArray(data?.shipments) ? data.shipments as ShipmentResponse[] : [];      this._rawShipments.set(liveShipments);      const liveStats = this.buildLiveStats(liveShipments);

      const total = this.toNumber(
        liveStats.totalShipments,
        data?.statistics?.totalShipments?.count,
        data?.overview?.totalShipmentsProcessed,
        data?.shipmentPerformance?.totalShipments
      ) ?? 0;

      const rev = liveShipments.length > 0
        ? liveStats.revenue
        : (this.toNumber(
          data?.revenue?.calculatedRevenueAmount,
          data?.overview?.totalRevenue,
          data?.revenue?.totalRevenue
        ) ?? 0);

      const csatCandidate = this.toNumber(
        liveStats.customerSatisfaction,
        data?.overview?.customerSatisfaction,
        data?.deliverySla?.customerSatisfaction,
        data?.statistics?.onTimeDeliveryPercent,
        data?.deliverySla?.currentSLA
      );
      const csat = csatCandidate === null
        ? 0
        : (csatCandidate > 5 ? this.deriveCsatFromOnTime(csatCandidate) : csatCandidate);

      this.baseTotalShipments.set(total);
      this.baseRevenue.set(rev);
      this.baseCustomerSatisfaction.set(csat);
      this.totalShipments.set(total);
      this.revenue.set(rev);
      this.customerSatisfaction.set(csat);

      const daily = (liveStats.dailyShipments.length > 0 ? liveStats.dailyShipments : data?.statistics?.dailyShipments);
      if (Array.isArray(daily) && daily.length > 0) {
        const mappedDaily = daily.map((d: any) => ({
          date: String(d.date ?? ''),
          shipments: Number(d.shipments ?? 0)
        }));
        this._rawDailyShipments.set(mappedDaily);
        this.lineChartData.set({
          labels: mappedDaily.map(d => d.date),
          datasets: [{
            data: mappedDaily.map(d => d.shipments),
            label: 'shipments',
            borderColor: '#3b82f6',
            backgroundColor: 'rgba(59, 130, 246, 0.18)',
            pointBackgroundColor: '#3b82f6',
            fill: false,
            tension: 0.35
          }]
        });
      }

      const serviceDist = (liveStats.serviceTypeDistribution.length > 0 ? liveStats.serviceTypeDistribution : data?.statistics?.serviceTypeDistribution);
      if (Array.isArray(serviceDist) && serviceDist.length > 0) {
        const statusLabels = serviceDist.map((s: any) => this.formatStatusLabel(String(s.serviceType ?? 'Unknown')));
        this.pieChartData.set({
          labels: serviceDist.map((s: any, idx: number) => `${statusLabels[idx]} ${Number(s.percentage ?? 0)}%`),
          datasets: [{
            data: serviceDist.map((s: any) => Number(s.percentage ?? 0)),
            backgroundColor: statusLabels.map(label => this.getServiceTypeColor(label)),
            borderWidth: 1,
            borderColor: '#ffffff'
          }]
        });
      }

      const trend = (liveStats.deliveryPerformanceTrend.length > 0 ? liveStats.deliveryPerformanceTrend : data?.statistics?.deliveryPerformanceTrend);
      if (Array.isArray(trend) && trend.length > 0) {
        this.barChartData.set({
          labels: trend.map((t: any) => t.month),
          datasets: [
            { data: trend.map((t: any) => Number(t.delayedPercent ?? 0)), label: 'Delayed %', backgroundColor: '#f59e0b', borderRadius: 8 },
            { data: trend.map((t: any) => Number(t.onTimePercent ?? 0)), label: 'On Time %', backgroundColor: '#10b981', borderRadius: 8 },
          ]
        });
        this._rawDeliveryTrend.set(trend.map((t: any) => ({
          month: String(t.month ?? ''),
          onTimePercent: Number(t.onTimePercent ?? 0),
          delayedPercent: Number(t.delayedPercent ?? 0),
        })));
      } else if (data?.deliverySla?.monthly && Array.isArray(data.deliverySla.monthly)) {
        const monthly = data.deliverySla.monthly.map((t: any) => ({
          month: String(t.month ?? ''),
          onTimePercent: Number(t.onTimePercent ?? 0),
          delayedPercent: Number(t.delayedPercent ?? 0),
        }));
        this.barChartData.set({
          labels: monthly.map((t: any) => t.month),
          datasets: [
            { data: monthly.map((t: any) => t.delayedPercent), label: 'Delayed %', backgroundColor: '#f59e0b', borderRadius: 8 },
            { data: monthly.map((t: any) => t.onTimePercent), label: 'On Time %', backgroundColor: '#10b981', borderRadius: 8 },
          ]
        });
        this._rawDeliveryTrend.set(monthly);
      }

      if (liveStats.hubPerformanceRows.length > 0) {
        this.hubPerformanceRows.set(liveStats.hubPerformanceRows);
      } else {
        const hubsWrapper = data?.hubPerformance;
        const hubsArray: any[] = Array.isArray(hubsWrapper)
          ? hubsWrapper
          : Array.isArray(hubsWrapper?.hubPerformance)
            ? hubsWrapper.hubPerformance
            : [];
        if (hubsArray.length > 0) {
          this.hubPerformanceRows.set(hubsArray.map((h: any) => ({
            hubLocation: h.hubLocation ?? h.hubName ?? h.name ?? 'Hub',
            totalShipments: Number(h.totalShipments ?? h.volumeHandled ?? 0),
            onTimePercent: Number(h.onTimePercent ?? 0),
            avgProcessingTime: h.avgProcessingTime ?? 'N/A',
            performance: (h.performance ?? 'Good') as 'Excellent' | 'Good' | 'Fair' | 'Poor'
          })));
        }
      }

      this.applyDateRange();
    });
  }

  private buildLiveStats(shipments: ShipmentResponse[]): {
    totalShipments: number;
    revenue: number;
    customerSatisfaction: number;
    dailyShipments: Array<{ date: string; shipments: number }>;
    serviceTypeDistribution: Array<{ serviceType: string; percentage: number }>;
    deliveryPerformanceTrend: Array<{ month: string; onTimePercent: number; delayedPercent: number }>;
    hubPerformanceRows: Array<{ hubLocation: string; totalShipments: number; onTimePercent: number; avgProcessingTime: string; performance: 'Excellent' | 'Good' | 'Fair' | 'Poor'; }>;
  } {
    if (!Array.isArray(shipments) || shipments.length === 0) {
      return {
        totalShipments: 0,
        revenue: 0,
        customerSatisfaction: 0,
        dailyShipments: [],
        serviceTypeDistribution: [],
        deliveryPerformanceTrend: [],
        hubPerformanceRows: []
      };
    }

    const now = new Date();
    const delivered = shipments.filter(s => String(s.status) === 'Delivered');
    const onTimePercent = shipments.length > 0 ? (delivered.length * 100) / shipments.length : 0;

    const dailyShipments = Array.from({ length: 7 }, (_, idx) => {
      const d = new Date(now);
      d.setDate(now.getDate() - (6 - idx));
      const key = `${d.getFullYear()}-${d.getMonth()}-${d.getDate()}`;

      const count = shipments.filter(s => {
        const createdAt = new Date(s.createdAt);
        if (Number.isNaN(createdAt.getTime())) return false;
        const shipmentKey = `${createdAt.getFullYear()}-${createdAt.getMonth()}-${createdAt.getDate()}`;
        return shipmentKey === key;
      }).length;

      return {
        date: d.toLocaleDateString('en-US', { month: 'short', day: 'numeric' }),
        shipments: count
      };
    });

    const statusMap = new Map<string, number>();
    for (const shipment of shipments) {
      const key = String(shipment.status || 'Draft');
      statusMap.set(key, (statusMap.get(key) ?? 0) + 1);
    }

    const serviceTypeDistribution = Array.from(statusMap.entries())
      .map(([serviceType, count]) => ({
        serviceType: this.formatStatusLabel(serviceType),
        percentage: Math.round((count * 10000) / shipments.length) / 100
      }))
      .sort((a, b) => b.percentage - a.percentage);

    const deliveryPerformanceTrend = Array.from({ length: 6 }, (_, idx) => {
      const monthDate = new Date(now.getFullYear(), now.getMonth() - (5 - idx), 1);
      const start = new Date(monthDate.getFullYear(), monthDate.getMonth(), 1);
      const end = new Date(monthDate.getFullYear(), monthDate.getMonth() + 1, 1);

      const rows = shipments.filter(s => {
        const createdAt = new Date(s.createdAt);
        return !Number.isNaN(createdAt.getTime()) && createdAt >= start && createdAt < end;
      });

      const total = rows.length;
      const deliveredCount = rows.filter(s => String(s.status) === 'Delivered').length;
      const monthlyOnTime = total > 0 ? Math.round((deliveredCount * 10000) / total) / 100 : 0;
      const monthlyDelayed = total > 0 ? Math.round((100 - monthlyOnTime) * 100) / 100 : 0;

      return {
        month: monthDate.toLocaleDateString('en-IN', { month: 'short' }),
        onTimePercent: monthlyOnTime,
        delayedPercent: monthlyDelayed
      };
    });

    const hubBuckets = new Map<string, ShipmentResponse[]>();
    for (const shipment of shipments) {
      const city = shipment.senderAddress?.city?.trim() || 'Unknown City';
      const state = shipment.senderAddress?.state?.trim() || '';
      const hubLocation = `${city}${state ? `, ${state}` : ''}`;
      const rows = hubBuckets.get(hubLocation) ?? [];
      rows.push(shipment);
      hubBuckets.set(hubLocation, rows);
    }

    const hubPerformanceRows = Array.from(hubBuckets.entries()).map(([hubLocation, rows]) => {
      const total = rows.length;
      const deliveredRows = rows.filter(item => String(item.status) === 'Delivered');
      const onTimePercentByDeliveredRatio = total > 0 ? Math.round((deliveredRows.length * 10000) / total) / 100 : 0;

      const avgAgeDays = total > 0
        ? rows.reduce((sum, item) => {
            const createdAt = new Date(item.createdAt).getTime();
            if (Number.isNaN(createdAt)) return sum;
            const ageDays = Math.max(0, (Date.now() - createdAt) / (1000 * 60 * 60 * 24));
            return sum + ageDays;
          }, 0) / total
        : 0;

      const avgProcessingTime = Number.isFinite(avgAgeDays)
        ? `${Math.max(0, Math.round(avgAgeDays * 10) / 10)} days`
        : 'N/A';

      const performance: 'Excellent' | 'Good' | 'Fair' | 'Poor' = onTimePercentByDeliveredRatio >= 90
        ? 'Excellent'
        : onTimePercentByDeliveredRatio >= 75
          ? 'Good'
          : onTimePercentByDeliveredRatio >= 50
            ? 'Fair'
            : 'Poor';

      return {
        hubLocation,
        totalShipments: total,
        onTimePercent: onTimePercentByDeliveredRatio,
        avgProcessingTime,
        performance,
      };
    }).sort((a, b) => b.totalShipments - a.totalShipments);

    return {
      totalShipments: shipments.length,
      revenue: Math.round(shipments.reduce((sum, shipment) => {
        const weight = Number(shipment.totalWeight ?? 0);
        const shipmentCharge = 100 + (Math.max(0, weight) * 20);
        return sum + shipmentCharge;
      }, 0)),
      customerSatisfaction: Math.round((onTimePercent / 20) * 10) / 10,
      dailyShipments,
      serviceTypeDistribution,
      deliveryPerformanceTrend,
      hubPerformanceRows
    };
  }

  private formatStatusLabel(value: string): string {
    const raw = String(value ?? '').trim();
    if (!raw) return 'Unknown';

    return raw
      .replace(/_/g, ' ')
      .replace(/([a-z])([A-Z])/g, '$1 $2')
      .split(' ')
      .filter(Boolean)
      .map(part => part.charAt(0).toUpperCase() + part.slice(1).toLowerCase())
      .join(' ');
  }

  private getServiceTypeColor(rawServiceType: string): string {
    const normalized = String(rawServiceType ?? '').trim().toLowerCase();
    const colorByStatus: Record<string, string> = {
      delivered: '#3b82f6',
      draft: '#14b8a6',
      booked: '#f59e0b',
      outfordelivery: '#f43f5e',
      'out for delivery': '#f43f5e',
      intransit: '#6366f1',
      'in transit': '#6366f1',
      pickup: '#8b5cf6',
      cancelled: '#ef4444',
      returned: '#0ea5e9'
    };

    if (colorByStatus[normalized]) {
      return colorByStatus[normalized];
    }

    // Fallback deterministic palette for any new/unexpected service types.
    const fallbackPalette = ['#3b82f6', '#14b8a6', '#f59e0b', '#f43f5e', '#6366f1', '#8b5cf6', '#0ea5e9', '#22c55e'];
    let hash = 0;
    for (let i = 0; i < normalized.length; i += 1) {
      hash = (hash * 31 + normalized.charCodeAt(i)) >>> 0;
    }
    return fallbackPalette[hash % fallbackPalette.length];
  }

  private toNumber(...values: unknown[]): number | null {
    for (const value of values) {
      const parsed = this.parseNumericValue(value);
      if (parsed !== null) {
        return parsed;
      }
    }
    return null;
  }

  private parseNumericValue(value: unknown): number | null {
    if (typeof value === 'number' && Number.isFinite(value)) {
      return value;
    }

    if (typeof value !== 'string') {
      return null;
    }

    const raw = value.trim();
    if (!raw) {
      return null;
    }

    const direct = Number(raw);
    if (!Number.isNaN(direct) && Number.isFinite(direct)) {
      return direct;
    }

    const firstNumericMatch = raw.match(/-?\d+(?:\.\d+)?/);
    if (!firstNumericMatch) {
      return null;
    }

    const extracted = Number(firstNumericMatch[0]);
    return Number.isNaN(extracted) || !Number.isFinite(extracted) ? null : extracted;
  }

  private deriveCsatFromOnTime(onTimePercent: unknown): number {
    const onTime = this.toNumber(onTimePercent);
    if (onTime === null) return 0;
    const csat = onTime / 20;
    return Math.round(csat * 10) / 10;
  }

  get performanceLegendItems(): string[] {
    return (this.pieChartData().labels as string[]) ?? [];
  }

  get hasLineData(): boolean {
    return !!this.lineChartData().datasets?.some(dataset => (dataset.data?.length ?? 0) > 0);
  }

  get hasPieData(): boolean {
    const data = this.pieChartData().datasets?.[0]?.data ?? [];
    return data.length > 0 && data.some(value => Number(value) > 0);
  }

  get hasBarData(): boolean {
    return !!this.barChartData().datasets?.some(dataset => (dataset.data?.length ?? 0) > 0);
  }

  get hasHubRows(): boolean {
    return this.hubPerformanceRows().length > 0;
  }

  get hasActiveRange(): boolean {
    return !!this.fromDate || !!this.toDate;
  }

  get activeRangeLabel(): string {
    if (!this.hasActiveRange) return 'All Time';
    const from = this.fromDate || 'Start';
    const to = this.toDate || 'Today';
    return `${from} → ${to}`;
  }

  get kpiTimeScope(): string {
    if (this.dateRangeError || !this.hasActiveRange) {
      return 'All months';
    }
    return `Selected range: ${this.activeRangeLabel}`;
  }

  getPerformanceClass(performance: 'Excellent' | 'Good' | 'Fair' | 'Poor'): string {
    return `performance-${performance.toLowerCase()}`;
  }

  getPercentageColorClass(percentage: number): string {
    if (percentage >= 90) return 'percentage-excellent';
    if (percentage >= 75) return 'percentage-good';
    if (percentage >= 50) return 'percentage-fair';
    return 'percentage-poor';
  }

  getLineChartLegendItems(): Array<{ label: string; color: string }> {
    const datasets = this.lineChartData().datasets;
    if (!datasets || datasets.length === 0) return [];
    return datasets.map(dataset => ({
      label: String(dataset.label ?? 'Series'),
      color: String(dataset.borderColor ?? '#3b82f6')
    }));
  }

  getBarChartLegendItems(): Array<{ label: string; color: string }> {
    const datasets = this.barChartData().datasets;
    if (!datasets || datasets.length === 0) return [];
    return datasets.map(dataset => ({
      label: String(dataset.label ?? 'Series'),
      color: String(dataset.backgroundColor ?? '#000000')
    }));
  }

  getServiceDistributionLegendItems(): Array<{ label: string; color: string }> {
    const labels = this.pieChartData().labels as string[] | undefined;
    const colors = (this.pieChartData().datasets?.[0]?.backgroundColor ?? []) as string[];
    if (!labels || !colors) return [];
    return labels.map((label, idx) => ({
      label,
      color: colors[idx] ?? '#000000'
    }));
  }

  toggleDateRangePicker(): void {
    this._showDateRangePicker.set(!this._showDateRangePicker());
  }

  onDateRangeInputChange(): void {
    if (this._dateRangeError()) {
      this._dateRangeError.set('');
    }
  }

  clearDateRange(): void {
    this.fromDate = '';
    this.toDate = '';
    this._dateRangeError.set('');
    this.applyDateRange();
  }

  applyDateRange(): void {
    if (!this.isDateRangeValid()) {
      return;
    }

    this._dateRangeError.set('');

    const hasRange = !!this.fromDate || !!this.toDate;
    const rawShipments = this._rawShipments();

    if (rawShipments.length > 0) {
      const scopedShipments = hasRange
        ? this.filterShipmentsByDateRange(rawShipments)
        : rawShipments;
      const scopedStats = this.buildLiveStats(scopedShipments);
      const scopedDaily = hasRange
        ? this.buildDailyShipmentsSeries(scopedShipments)
        : scopedStats.dailyShipments;
      const scopedTrend = hasRange
        ? this.buildMonthlyTrendSeries(scopedShipments)
        : scopedStats.deliveryPerformanceTrend;

      this.totalShipments.set(scopedStats.totalShipments);
      this.revenue.set(scopedStats.revenue);
      this.customerSatisfaction.set(scopedStats.customerSatisfaction);

      this.lineChartData.set({
        labels: scopedDaily.map(d => d.date),
        datasets: [{
          data: scopedDaily.map(d => d.shipments),
          label: 'shipments',
          borderColor: '#3b82f6',
          backgroundColor: 'rgba(59, 130, 246, 0.18)',
          pointBackgroundColor: '#3b82f6',
          fill: false,
          tension: 0.35
        }]
      });

      this.pieChartData.set({
        labels: scopedStats.serviceTypeDistribution.map(s => `${this.formatStatusLabel(String(s.serviceType ?? 'Unknown'))} ${Number(s.percentage ?? 0)}%`),
        datasets: [{
          data: scopedStats.serviceTypeDistribution.map(s => Number(s.percentage ?? 0)),
          backgroundColor: scopedStats.serviceTypeDistribution.map(s => this.getServiceTypeColor(String(s.serviceType ?? 'Unknown'))),
          borderWidth: 1,
          borderColor: '#ffffff'
        }]
      });

      this.barChartData.set({
        labels: scopedTrend.map(t => t.month),
        datasets: [
          { data: scopedTrend.map(t => t.delayedPercent), label: 'Delayed %', backgroundColor: '#f59e0b', borderRadius: 8 },
          { data: scopedTrend.map(t => t.onTimePercent), label: 'On Time %', backgroundColor: '#10b981', borderRadius: 8 },
        ]
      });

      this.hubPerformanceRows.set(scopedStats.hubPerformanceRows);
      return;
    }

    const filteredDaily = this.filterDailyByDateRange(this._rawDailyShipments());
    this.lineChartData.set({
      labels: filteredDaily.map(d => d.date),
      datasets: [{
        data: filteredDaily.map(d => d.shipments),
        label: 'shipments',
        borderColor: '#3b82f6',
        backgroundColor: 'rgba(59, 130, 246, 0.18)',
        pointBackgroundColor: '#3b82f6',
        fill: false,
        tension: 0.35
      }]
    });

    const filteredTrend = this.filterDeliveryTrendByRange(this._rawDeliveryTrend());
    this.barChartData.set({
      labels: filteredTrend.map(t => t.month),
      datasets: [
        { data: filteredTrend.map(t => t.delayedPercent), label: 'Delayed %', backgroundColor: '#f59e0b', borderRadius: 8 },
        { data: filteredTrend.map(t => t.onTimePercent), label: 'On Time %', backgroundColor: '#10b981', borderRadius: 8 },
      ]
    });

    if (!hasRange) {
      this.totalShipments.set(this.baseTotalShipments());
      this.revenue.set(this.baseRevenue());
      this.customerSatisfaction.set(this.baseCustomerSatisfaction());
      return;
    }

    const filteredShipments = filteredDaily.reduce((sum, item) => sum + item.shipments, 0);
    this.totalShipments.set(filteredShipments);

    const allShipments = this._rawDailyShipments().reduce((sum, item) => sum + item.shipments, 0);
    if (allShipments > 0) {
      const revenueRatio = filteredShipments / allShipments;
      this.revenue.set(Math.round(this.baseRevenue() * revenueRatio));
    } else {
      this.revenue.set(0);
    }

    if (filteredTrend.length > 0) {
      const avgOnTime = filteredTrend.reduce((sum, item) => sum + item.onTimePercent, 0) / filteredTrend.length;
      this.customerSatisfaction.set(this.deriveCsatFromOnTime(avgOnTime));
    } else {
      this.customerSatisfaction.set(this.baseCustomerSatisfaction());
    }
  }

  private isDateRangeValid(): boolean {
    if (!this.fromDate || !this.toDate) {
      return true;
    }

    const from = this.parseDateValue(this.fromDate);
    const to = this.parseDateValue(this.toDate);

    if (!from || !to) {
      this._dateRangeError.set('Please enter valid dates in both From and To fields.');
      return false;
    }

    if (from.getTime() > to.getTime()) {
      this._dateRangeError.set('From date must be earlier than or equal to To date.');
      return false;
    }

    return true;
  }

  openDetails(section: 'daily' | 'service' | 'delivery'): void {
    if (section === 'daily') {
      this._detailsTitle.set('Daily Shipments Trend Details');
      this._detailsRows.set((this.lineChartData().labels ?? []).map((label, index) => ({
        label: String(label),
        value: String(this.lineChartData().datasets?.[0]?.data?.[index] ?? 0)
      })));
    }

    if (section === 'service') {
      this._detailsTitle.set('Service Type Distribution Details');
      const labels = (this.pieChartData().labels ?? []).map(label => String(label));
      const values = this.pieChartData().datasets?.[0]?.data ?? [];
      this._detailsRows.set(labels.map((label, index) => ({
        label: this.formatStatusLabel(label),
        value: `${values[index] ?? 0}%`
      })));
    }

    if (section === 'delivery') {
      this._detailsTitle.set('Delivery Performance Trend Details');
      const labels = (this.barChartData().labels ?? []).map(label => String(label));
      const delayed = this.barChartData().datasets?.[0]?.data ?? [];
      const onTime = this.barChartData().datasets?.[1]?.data ?? [];
      this._detailsRows.set(labels.map((label, index) => ({
        label,
        value: `On Time: ${onTime[index] ?? 0}% | Delayed: ${delayed[index] ?? 0}%`
      })));
    }

    this._detailsOpen.set(true);
  }

  closeDetails(): void {
    this._detailsOpen.set(false);
  }

  exportAll(): void {
    const csvParts: string[] = [];
    csvParts.push('SmartShip Reports Export');
    csvParts.push(`Generated At,${new Date().toISOString()}`);
    csvParts.push('');
    csvParts.push('KPI,Value');
    csvParts.push(`Total Shipments,${this.totalShipments()}`);
    csvParts.push(`Revenue,${this.revenue()}`);
    csvParts.push(`Customer Satisfaction,${this.customerSatisfaction()}`);
    csvParts.push('');

    csvParts.push('Daily Shipments Trend');
    csvParts.push('Date,Shipments');
    const dailyLabels = (this.lineChartData().labels ?? []).map(label => String(label));
    const dailyData = this.lineChartData().datasets?.[0]?.data ?? [];
    for (let index = 0; index < dailyLabels.length; index++) {
      csvParts.push(`${dailyLabels[index]},${dailyData[index] ?? 0}`);
    }
    csvParts.push('');

    csvParts.push('Service Type Distribution');
    csvParts.push('Service,Percentage');
    const serviceLabels = (this.pieChartData().labels ?? []).map(label => String(label).replace(',', ' '));
    const serviceData = this.pieChartData().datasets?.[0]?.data ?? [];
    for (let index = 0; index < serviceLabels.length; index++) {
      csvParts.push(`${serviceLabels[index]},${serviceData[index] ?? 0}`);
    }
    csvParts.push('');

    csvParts.push('Delivery Performance Trend');
    csvParts.push('Month,On Time %,Delayed %');
    const perfLabels = (this.barChartData().labels ?? []).map(label => String(label));
    const delayedData = this.barChartData().datasets?.[0]?.data ?? [];
    const onTimeData = this.barChartData().datasets?.[1]?.data ?? [];
    for (let index = 0; index < perfLabels.length; index++) {
      csvParts.push(`${perfLabels[index]},${onTimeData[index] ?? 0},${delayedData[index] ?? 0}`);
    }
    csvParts.push('');

    csvParts.push('Hub Performance Summary');
    csvParts.push('Hub Location,Total Shipments,On-Time %,Avg Processing Time,Performance');
    for (const row of this.hubPerformanceRows()) {
      csvParts.push(`${row.hubLocation.replace(',', ' ')},${row.totalShipments},${row.onTimePercent},${row.avgProcessingTime.replace(',', ' ')},${this.formatStatusLabel(row.performance)}`);
    }

    const blob = new Blob([csvParts.join('\n')], { type: 'text/csv;charset=utf-8;' });
    const link = document.createElement('a');
    link.href = URL.createObjectURL(blob);
    link.download = `smartship-reports-${new Date().toISOString().slice(0, 10)}.csv`;
    link.click();
    URL.revokeObjectURL(link.href);
  }

  private filterDailyByDateRange(source: Array<{ date: string; shipments: number }>): Array<{ date: string; shipments: number }> {
    if (!this.fromDate && !this.toDate) return source;

    const from = this.fromDate ? this.parseDateValue(this.fromDate) : null;
    const to = this.toDate ? this.parseDateValue(this.toDate) : null;
    const toEnd = to ? new Date(to.getFullYear(), to.getMonth(), to.getDate(), 23, 59, 59, 999) : null;

    return source.filter(item => {
      const date = this.parseDateValue(item.date);
      if (!date) return false;
      if (from && date < from) return false;
      if (toEnd && date > toEnd) return false;
      return true;
    });
  }

  private filterDeliveryTrendByRange(source: Array<{ month: string; onTimePercent: number; delayedPercent: number }>): Array<{ month: string; onTimePercent: number; delayedPercent: number }> {
    if (!this.fromDate && !this.toDate) return source;

    const from = this.fromDate ? this.parseDateValue(this.fromDate) : null;
    const to = this.toDate ? this.parseDateValue(this.toDate) : null;
    const toEnd = to ? new Date(to.getFullYear(), to.getMonth(), to.getDate(), 23, 59, 59, 999) : null;

    return source.filter(item => {
      const monthStart = this.monthLabelToDate(item.month);
      if (!monthStart) return true;
      const monthEnd = new Date(monthStart.getFullYear(), monthStart.getMonth() + 1, 0, 23, 59, 59, 999);
      if (from && monthEnd < from) return false;
      if (toEnd && monthStart > toEnd) return false;
      return true;
    });
  }

  private filterShipmentsByDateRange(source: ShipmentResponse[]): ShipmentResponse[] {
    if (!this.fromDate && !this.toDate) return source;

    const from = this.fromDate ? this.parseDateValue(this.fromDate) : null;
    const to = this.toDate ? this.parseDateValue(this.toDate) : null;
    const toEnd = to ? new Date(to.getFullYear(), to.getMonth(), to.getDate(), 23, 59, 59, 999) : null;

    return source.filter(shipment => {
      const createdAt = this.parseDateValue(String(shipment.createdAt ?? ''));
      if (!createdAt) return false;
      if (from && createdAt < from) return false;
      if (toEnd && createdAt > toEnd) return false;
      return true;
    });
  }

  private buildDailyShipmentsSeries(shipments: ShipmentResponse[]): Array<{ date: string; shipments: number }> {
    if (shipments.length === 0) {
      return [];
    }

    const from = this.fromDate ? this.parseDateValue(this.fromDate) : null;
    const to = this.toDate ? this.parseDateValue(this.toDate) : null;

    const validCreatedDates = shipments
      .map(shipment => this.parseDateValue(String(shipment.createdAt ?? '')))
      .filter((date): date is Date => date !== null)
      .sort((a, b) => a.getTime() - b.getTime());

    if (validCreatedDates.length === 0) {
      return [];
    }

    const start = from ?? new Date(validCreatedDates[0].getFullYear(), validCreatedDates[0].getMonth(), validCreatedDates[0].getDate());
    const endRaw = to ?? validCreatedDates[validCreatedDates.length - 1];
    const end = new Date(endRaw.getFullYear(), endRaw.getMonth(), endRaw.getDate());

    const buckets = new Map<string, number>();
    for (const shipment of shipments) {
      const createdAt = this.parseDateValue(String(shipment.createdAt ?? ''));
      if (!createdAt) {
        continue;
      }

      const key = `${createdAt.getFullYear()}-${createdAt.getMonth()}-${createdAt.getDate()}`;
      buckets.set(key, (buckets.get(key) ?? 0) + 1);
    }

    const series: Array<{ date: string; shipments: number }> = [];
    for (let cursor = new Date(start); cursor <= end; cursor.setDate(cursor.getDate() + 1)) {
      const key = `${cursor.getFullYear()}-${cursor.getMonth()}-${cursor.getDate()}`;
      series.push({
        date: cursor.toLocaleDateString('en-US', { month: 'short', day: 'numeric' }),
        shipments: buckets.get(key) ?? 0,
      });
    }

    return series;
  }

  private buildMonthlyTrendSeries(shipments: ShipmentResponse[]): Array<{ month: string; onTimePercent: number; delayedPercent: number }> {
    if (shipments.length === 0) {
      return [];
    }

    const from = this.fromDate ? this.parseDateValue(this.fromDate) : null;
    const to = this.toDate ? this.parseDateValue(this.toDate) : null;

    const validCreatedDates = shipments
      .map(shipment => this.parseDateValue(String(shipment.createdAt ?? '')))
      .filter((date): date is Date => date !== null)
      .sort((a, b) => a.getTime() - b.getTime());

    if (validCreatedDates.length === 0) {
      return [];
    }

    const startDate = from ?? validCreatedDates[0];
    const endDate = to ?? validCreatedDates[validCreatedDates.length - 1];
    const startMonth = new Date(startDate.getFullYear(), startDate.getMonth(), 1);
    const endMonth = new Date(endDate.getFullYear(), endDate.getMonth(), 1);

    const trend: Array<{ month: string; onTimePercent: number; delayedPercent: number }> = [];
    for (let cursor = new Date(startMonth); cursor <= endMonth; cursor.setMonth(cursor.getMonth() + 1)) {
      const monthStart = new Date(cursor.getFullYear(), cursor.getMonth(), 1);
      const monthEnd = new Date(cursor.getFullYear(), cursor.getMonth() + 1, 1);
      const monthlyRows = shipments.filter(shipment => {
        const createdAt = this.parseDateValue(String(shipment.createdAt ?? ''));
        return !!createdAt && createdAt >= monthStart && createdAt < monthEnd;
      });

      const monthlyTotal = monthlyRows.length;
      const monthlyDelivered = monthlyRows.filter(shipment => String(shipment.status) === 'Delivered').length;
      const onTimePercent = monthlyTotal > 0 ? Math.round((monthlyDelivered * 10000) / monthlyTotal) / 100 : 0;

      trend.push({
        month: monthStart.toLocaleDateString('en-IN', { month: 'short' }),
        onTimePercent,
        delayedPercent: monthlyTotal > 0 ? Math.round((100 - onTimePercent) * 100) / 100 : 0,
      });
    }

    return trend;
  }

  private monthLabelToDate(monthLabel: string): Date | null {
    const cleaned = monthLabel?.trim();
    if (!cleaned) return null;
    const parsed = new Date(`${cleaned} 1, ${new Date().getFullYear()}`);
    if (Number.isNaN(parsed.getTime())) return null;
    return parsed;
  }

  private parseDateValue(value: string): Date | null {
    const raw = String(value ?? '').trim();
    if (!raw) return null;

    // "dd-MM-yyyy" or "dd/MM/yyyy"
    const ddmmyyyy = raw.match(/^(\d{1,2})[-\/](\d{1,2})[-\/](\d{4})$/);
    if (ddmmyyyy) {
      const day = Number(ddmmyyyy[1]);
      const month = Number(ddmmyyyy[2]) - 1;
      const year = Number(ddmmyyyy[3]);
      const parsed = new Date(year, month, day);
      if (!Number.isNaN(parsed.getTime())) return parsed;
    }

    // "yyyy-MM-dd" or "yyyy/MM/dd"
    const yyyymmdd = raw.match(/^(\d{4})[-\/](\d{1,2})[-\/](\d{1,2})$/);
    if (yyyymmdd) {
      const year = Number(yyyymmdd[1]);
      const month = Number(yyyymmdd[2]) - 1;
      const day = Number(yyyymmdd[3]);
      const parsed = new Date(year, month, day);
      if (!Number.isNaN(parsed.getTime())) return parsed;
    }

    // "MMM d" or "MMM dd" — e.g. "Mar 12", "Oct 5" (from C# "MMM d" format)
    const mmmD = raw.match(/^([A-Za-z]{3})\s+(\d{1,2})$/);
    if (mmmD) {
      const parsed = new Date(`${mmmD[1]} ${mmmD[2]}, ${new Date().getFullYear()}`);
      if (!Number.isNaN(parsed.getTime())) return parsed;
    }

    // ISO or any other browser-parseable string
    const isoParsed = new Date(raw);
    if (!Number.isNaN(isoParsed.getTime())) {
      return isoParsed;
    }

    return null;
  }
}
