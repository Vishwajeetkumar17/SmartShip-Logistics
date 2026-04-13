import { Pipe, PipeTransform } from '@angular/core';

@Pipe({
  name: 'statusBadge',
  standalone: true
})
export class StatusBadgePipe implements PipeTransform {
  private readonly statusMap: Record<string, { label: string; cssClass: string }> = {
    Draft: { label: 'Draft', cssClass: 'badge-secondary' },
    Booked: { label: 'Booked', cssClass: 'badge-info' },
    PickedUp: { label: 'Picked Up', cssClass: 'badge-info' },
    InTransit: { label: 'In Transit', cssClass: 'badge-primary' },
    OutForDelivery: { label: 'Out for Delivery', cssClass: 'badge-accent' },
    Delivered: { label: 'Delivered', cssClass: 'badge-success' }
  };

  transform(status: string, type: 'label' | 'class' = 'label'): string {
    const mapped = this.statusMap[status];
    if (!mapped) return status;
    return type === 'label' ? mapped.label : mapped.cssClass;
  }
}
