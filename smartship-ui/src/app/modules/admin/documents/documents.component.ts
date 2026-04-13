import { ChangeDetectorRef, Component, ElementRef, HostListener, inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { DocumentService } from '../../../core/services/document.service';
import { DocumentResponse, DeliveryProofResponse } from '../../../shared/models/document.model';
import { ShipmentService } from '../../../core/services/shipment.service';
import { ShipmentResponse } from '../../../shared/models/shipment.model';
import { catchError, finalize } from 'rxjs/operators';
import { of } from 'rxjs';

@Component({
  selector: 'app-admin-documents',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './documents.component.html',
  styleUrl: './documents.component.css'
})
/**
 * Admin document management screen.
 * Provides search by shipment/customer, paginated results (shipment mode), document uploads by type, delivery proof uploads,
 * and autocomplete helpers backed by shipment lists.
 */
export class DocumentsComponent implements OnInit {
  private documentService = inject(DocumentService);
  private shipmentService = inject(ShipmentService);
  private cdr = inject(ChangeDetectorRef);
  private elementRef = inject(ElementRef<HTMLElement>);
  private readonly defaultPageSize = 5;

  documents: DocumentResponse[] = [];
  deliveryProof: DeliveryProofResponse | null = null;
  isLoading = false;
  error = '';
  success = '';
  pageNumber = 1;
  pageSize = this.defaultPageSize;
  totalItems = 0;
  totalPages = 0;

  // Search
  searchShipmentId: number | null = null;
  searchMode: 'shipment' | 'customer' = 'shipment';
  searchCustomerId: number | null = null;
  searchShipmentQuery = '';
  searchCustomerQuery = '';
  showSearchShipmentSuggestions = false;
  showSearchCustomerSuggestions = false;

  shipmentOptions: ShipmentResponse[] = [];
  filteredShipmentOptions: ShipmentResponse[] = [];
  customerOptions: Array<{ id: number; name: string }> = [];
  filteredCustomerOptions: Array<{ id: number; name: string }> = [];

  // Upload
  showUploadForm = false;
  uploadShipmentId: number | null = null;
  uploadType: 'general' | 'invoice' | 'label' | 'customs' = 'general';
  selectedFile: File | null = null;
  uploadShipmentQuery = '';
  filteredUploadShipmentOptions: ShipmentResponse[] = [];
  showUploadShipmentSuggestions = false;
  uploadFieldErrors: Record<'shipment' | 'file', string> = {
    shipment: '',
    file: ''
  };

  // Delivery Proof
  showProofForm = false;
  proofShipmentId: number | null = null;
  proofSignerName = '';
  proofNotes = '';
  proofFile: File | null = null;
  isUploading = false;
  proofShipmentQuery = '';
  filteredProofShipmentOptions: ShipmentResponse[] = [];
  showProofShipmentSuggestions = false;
  proofFieldErrors: Record<'shipment' | 'signerName' | 'proofFile', string> = {
    shipment: '',
    signerName: '',
    proofFile: ''
  };

  ngOnInit(): void {
    this.loadSelectionOptions();
  }

  @HostListener('document:click', ['$event'])
  onDocumentClick(event: MouseEvent): void {
    const target = event.target as Node | null;
    if (!target) {
      return;
    }

    const clickedInside = this.elementRef.nativeElement.contains(target) &&
      ((target as HTMLElement).closest('.autocomplete-wrap') !== null);

    if (!clickedInside) {
      this.showSearchShipmentSuggestions = false;
      this.showSearchCustomerSuggestions = false;
      this.showUploadShipmentSuggestions = false;
      this.showProofShipmentSuggestions = false;
    }
  }

  private loadSelectionOptions(): void {
    this.shipmentService.getAll().pipe(
      catchError(() => of([] as ShipmentResponse[]))
    ).subscribe((shipments) => {
      this.shipmentOptions = shipments;
      this.filteredShipmentOptions = shipments;
      this.filteredUploadShipmentOptions = shipments;

      const customerMap = new Map<number, string>();
      shipments.forEach((s) => {
        if (!customerMap.has(s.customerId)) {
          customerMap.set(s.customerId, s.customerName || `Customer ${s.customerId}`);
        }
      });
      this.customerOptions = Array.from(customerMap.entries())
        .map(([id, name]) => ({ id, name }))
        .sort((a, b) => a.id - b.id);
      this.filteredCustomerOptions = this.customerOptions;

      this.cdr.detectChanges();
    });
  }

  private normalizeSearchText(value: string | number | null | undefined): string {
    return String(value ?? '')
      .toLocaleLowerCase()
      .replace(/[^a-z0-9]/gi, '');
  }

  private matchesShipmentSearch(shipment: ShipmentResponse, query: string): boolean {
    const normalizedQuery = this.normalizeSearchText(query);
    if (!normalizedQuery) {
      return true;
    }

    const haystack = [
      shipment.shipmentId,
      shipment.trackingNumber,
      shipment.customerId,
      `SH${shipment.shipmentId}`,
      `Customer ${shipment.customerId}`,
    ]
      .map((item) => this.normalizeSearchText(item))
      .join(' ');

    return haystack.includes(normalizedQuery);
  }

  private matchesCustomerSearch(customer: { id: number; name: string }, query: string): boolean {
    const normalizedQuery = this.normalizeSearchText(query);
    if (!normalizedQuery) {
      return true;
    }

    const haystack = [customer.id, customer.name, `Customer ${customer.id}`]
      .map((item) => this.normalizeSearchText(item))
      .join(' ');

    return haystack.includes(normalizedQuery);
  }

  onSearchShipmentQueryChange(value: string): void {
    const term = value.trim();
    this.filteredShipmentOptions = !term
      ? this.shipmentOptions
      : this.shipmentOptions.filter((shipment) => this.matchesShipmentSearch(shipment, term));

    this.showSearchShipmentSuggestions = term.length > 0 && this.filteredShipmentOptions.length > 0;
    this.searchShipmentId = null;

    if (!term) {
      this.showSearchShipmentSuggestions = false;
    }

    if (this.searchShipmentId != null && !this.filteredShipmentOptions.some((s) => s.shipmentId === this.searchShipmentId)) {
      this.searchShipmentId = null;
    }
  }

  selectSearchShipment(shipment: ShipmentResponse): void {
    this.searchShipmentId = shipment.shipmentId;
    this.searchShipmentQuery = `SH${shipment.shipmentId} • ${shipment.trackingNumber}`;
    this.showSearchShipmentSuggestions = false;
  }

  hideSearchShipmentSuggestions(): void {
    setTimeout(() => this.showSearchShipmentSuggestions = false, 120);
  }

  showSearchShipmentSuggestionsOnFocus(): void {
    const term = this.searchShipmentQuery.trim();
    this.filteredShipmentOptions = !term
      ? this.shipmentOptions
      : this.shipmentOptions.filter((shipment) => this.matchesShipmentSearch(shipment, term));
    this.showSearchShipmentSuggestions = this.filteredShipmentOptions.length > 0;
  }

  toggleSearchShipmentSuggestions(): void {
    if (this.showSearchShipmentSuggestions) {
      this.showSearchShipmentSuggestions = false;
      return;
    }
    this.showSearchShipmentSuggestionsOnFocus();
  }

  onSearchCustomerQueryChange(value: string): void {
    const term = value.trim();
    this.filteredCustomerOptions = !term
      ? this.customerOptions
      : this.customerOptions.filter((customer) => this.matchesCustomerSearch(customer, term));

    this.showSearchCustomerSuggestions = term.length > 0 && this.filteredCustomerOptions.length > 0;
    this.searchCustomerId = null;

    if (!term) {
      this.showSearchCustomerSuggestions = false;
    }

    if (this.searchCustomerId != null && !this.filteredCustomerOptions.some((c) => c.id === this.searchCustomerId)) {
      this.searchCustomerId = null;
    }
  }

  selectSearchCustomer(customer: { id: number; name: string }): void {
    this.searchCustomerId = customer.id;
    this.searchCustomerQuery = `${customer.id} • ${customer.name}`;
    this.showSearchCustomerSuggestions = false;
  }

  hideSearchCustomerSuggestions(): void {
    setTimeout(() => this.showSearchCustomerSuggestions = false, 120);
  }

  showSearchCustomerSuggestionsOnFocus(): void {
    const term = this.searchCustomerQuery.trim();
    this.filteredCustomerOptions = !term
      ? this.customerOptions
      : this.customerOptions.filter((customer) => this.matchesCustomerSearch(customer, term));
    this.showSearchCustomerSuggestions = this.filteredCustomerOptions.length > 0;
  }

  toggleSearchCustomerSuggestions(): void {
    if (this.showSearchCustomerSuggestions) {
      this.showSearchCustomerSuggestions = false;
      return;
    }
    this.showSearchCustomerSuggestionsOnFocus();
  }

  onUploadShipmentQueryChange(value: string): void {
    this.uploadFieldErrors.shipment = '';
    this.error = '';
    this.uploadShipmentQuery = value;
    this.applyUploadShipmentSearch();

    const term = value.trim();
    this.showUploadShipmentSuggestions = term.length > 0 && this.filteredUploadShipmentOptions.length > 0;
    this.uploadShipmentId = null;

    if (!term) {
      this.showUploadShipmentSuggestions = false;
    }

    if (this.uploadShipmentId != null && !this.filteredUploadShipmentOptions.some((s) => s.shipmentId === this.uploadShipmentId)) {
      this.uploadShipmentId = null;
    }
  }

  selectUploadShipment(shipment: ShipmentResponse): void {
    this.uploadShipmentId = shipment.shipmentId;
    this.uploadShipmentQuery = `SH${shipment.shipmentId} • ${shipment.trackingNumber} • Customer ${shipment.customerId}`;
    this.uploadFieldErrors.shipment = '';
    this.showUploadShipmentSuggestions = false;
  }

  hideUploadShipmentSuggestions(): void {
    setTimeout(() => this.showUploadShipmentSuggestions = false, 120);
  }

  showUploadShipmentSuggestionsOnFocus(): void {
    const term = this.uploadShipmentQuery.trim().toLowerCase();
    this.filteredUploadShipmentOptions = !term
      ? this.shipmentOptions
      : this.shipmentOptions.filter((shipment) => {
          const shipmentId = String(shipment.shipmentId);
          const tracking = (shipment.trackingNumber || '').toLowerCase();
          const customerId = String(shipment.customerId);
          return shipmentId.includes(term) || tracking.includes(term) || customerId.includes(term);
        });
    this.showUploadShipmentSuggestions = this.filteredUploadShipmentOptions.length > 0;
  }

  toggleUploadShipmentSuggestions(): void {
    if (this.showUploadShipmentSuggestions) {
      this.showUploadShipmentSuggestions = false;
      return;
    }
    this.showUploadShipmentSuggestionsOnFocus();
  }

  applyUploadShipmentSearch(): void {
    const term = this.uploadShipmentQuery.trim();
    if (!term) {
      this.filteredUploadShipmentOptions = this.shipmentOptions;
      return;
    }

    this.filteredUploadShipmentOptions = this.shipmentOptions.filter((shipment) => this.matchesShipmentSearch(shipment, term));
  }

  searchDocuments(): void {
    if (this.searchMode === 'shipment' && this.searchShipmentId == null) return;
    if (this.searchMode === 'customer' && this.searchCustomerId == null) return;

    this.searchDocumentsPage(1);
  }

  searchDocumentsPage(pageNumber: number): void {
    if (this.searchMode === 'shipment' && this.searchShipmentId == null) return;
    if (this.searchMode === 'customer' && this.searchCustomerId == null) return;

    this.isLoading = true;
    this.error = '';
    this.documents = [];
    this.deliveryProof = null;

    const selectedShipmentId = this.searchShipmentId;
    const selectedCustomerId = this.searchCustomerId;

    if (this.searchMode === 'shipment') {
      this.documentService.getByShipmentPage(selectedShipmentId as number, pageNumber, this.pageSize).pipe(
        catchError(err => {
          this.error = err.error?.message || 'Failed to load documents.';
          return of({
            data: [] as DocumentResponse[],
            pageNumber,
            pageSize: this.pageSize,
            totalItems: 0,
            totalPages: 0,
            hasNextPage: false,
            hasPreviousPage: false
          });
        }),
        finalize(() => { this.isLoading = false; this.cdr.detectChanges(); })
      ).subscribe(response => {
        this.documents = response.data;
        this.pageNumber = response.pageNumber;
        this.totalItems = response.totalItems;
        this.totalPages = response.totalPages;

        if (selectedShipmentId != null) {
          this.documentService.getDeliveryProof(selectedShipmentId).pipe(
            catchError(() => of(null))
          ).subscribe(proof => { this.deliveryProof = proof; this.cdr.detectChanges(); });
        }
      });
      return;
    }

    this.documentService.getByCustomer(selectedCustomerId as number).pipe(
      catchError(err => {
        this.error = err.error?.message || 'Failed to load documents.';
        return of([] as DocumentResponse[]);
      }),
      finalize(() => { this.isLoading = false; this.cdr.detectChanges(); })
    ).subscribe(data => {
      this.documents = data;
      this.pageNumber = 1;
      this.totalItems = data.length;
      this.totalPages = data.length > 0 ? 1 : 0;
    });
  }

  get hasPreviousPage(): boolean {
    return this.pageNumber > 1;
  }

  get hasNextPage(): boolean {
    return this.pageNumber < this.totalPages;
  }

  goToPage(pageNumber: number): void {
    if (this.searchMode !== 'shipment') {
      return;
    }

    if (pageNumber < 1 || pageNumber > this.totalPages || pageNumber === this.pageNumber) {
      return;
    }

    this.searchDocumentsPage(pageNumber);
  }

  onFileSelected(event: Event): void {
    const input = event.target as HTMLInputElement;
    if (input.files && input.files[0]) {
      this.selectedFile = input.files[0];
      this.uploadFieldErrors.file = '';
      this.error = '';
    }
  }

  onProofFileSelected(event: Event): void {
    const input = event.target as HTMLInputElement;
    if (input.files && input.files[0]) {
      this.proofFile = input.files[0];
      this.proofFieldErrors.proofFile = '';
      this.error = '';
    }
  }

  toggleUploadForm(): void {
    this.showUploadForm = !this.showUploadForm;
    if (!this.showUploadForm) {
      this.selectedFile = null;
      this.uploadShipmentId = null;
      this.uploadShipmentQuery = '';
      this.filteredUploadShipmentOptions = this.shipmentOptions;
      this.showUploadShipmentSuggestions = false;
    }
  }

  submitUpload(): void {
    this.clearUploadFieldErrors();
    this.error = '';

    if (!this.uploadShipmentId) {
      this.uploadFieldErrors.shipment = 'Shipment is required.';
    }

    if (!this.selectedFile) {
      this.uploadFieldErrors.file = 'File is required.';
    }

    if (this.uploadFieldErrors.shipment || this.uploadFieldErrors.file) {
      return;
    }

    const uploadShipmentId = this.uploadShipmentId;
    const selectedFile = this.selectedFile;
    if (uploadShipmentId == null || !selectedFile) {
      return;
    }

    this.isUploading = true;
    this.error = '';

    let obs$;
    switch (this.uploadType) {
      case 'invoice':
        obs$ = this.documentService.uploadInvoice(uploadShipmentId, selectedFile);
        break;
      case 'label':
        obs$ = this.documentService.uploadLabel(uploadShipmentId, selectedFile);
        break;
      case 'customs':
        obs$ = this.documentService.uploadCustoms(uploadShipmentId, selectedFile);
        break;
      default:
        obs$ = this.documentService.upload(uploadShipmentId, selectedFile);
    }

    obs$.pipe(
      catchError(err => {
        this.error = err.error?.message || 'Upload failed.';
        return of(null);
      }),
      finalize(() => this.isUploading = false)
    ).subscribe(result => {
      if (result) {
        this.success = 'Document uploaded successfully!';
        this.showUploadForm = false;
        this.selectedFile = null;
        if (this.searchShipmentId === this.uploadShipmentId) {
          this.searchDocuments();
        }
        setTimeout(() => this.success = '', 3000);
      }
    });
  }

  toggleProofForm(): void {
    this.showProofForm = !this.showProofForm;
    if (!this.showProofForm) {
      this.proofFile = null;
      this.proofSignerName = '';
      this.proofNotes = '';
      this.proofShipmentId = null;
      this.proofShipmentQuery = '';
      this.filteredProofShipmentOptions = this.shipmentOptions;
      this.showProofShipmentSuggestions = false;
    }
  }

  onProofShipmentQueryChange(value: string): void {
    this.proofFieldErrors.shipment = '';
    this.error = '';
    this.proofShipmentQuery = value;
    this.applyProofShipmentSearch();

    const term = value.trim();
    this.showProofShipmentSuggestions = term.length > 0 && this.filteredProofShipmentOptions.length > 0;
    this.proofShipmentId = null;

    if (!term) {
      this.showProofShipmentSuggestions = false;
    }

    if (this.proofShipmentId != null && !this.filteredProofShipmentOptions.some((s) => s.shipmentId === this.proofShipmentId)) {
      this.proofShipmentId = null;
    }
  }

  selectProofShipment(shipment: ShipmentResponse): void {
    this.proofShipmentId = shipment.shipmentId;
    this.proofShipmentQuery = `SH${shipment.shipmentId} • ${shipment.trackingNumber} • Customer ${shipment.customerId}`;
    this.proofFieldErrors.shipment = '';
    this.showProofShipmentSuggestions = false;
  }

  hideProofShipmentSuggestions(): void {
    setTimeout(() => this.showProofShipmentSuggestions = false, 120);
  }

  showProofShipmentSuggestionsOnFocus(): void {
    const term = this.proofShipmentQuery.trim().toLowerCase();
    this.filteredProofShipmentOptions = !term
      ? this.shipmentOptions
      : this.shipmentOptions.filter((shipment) => {
          const shipmentId = String(shipment.shipmentId);
          const tracking = (shipment.trackingNumber || '').toLowerCase();
          const customerId = String(shipment.customerId);
          return shipmentId.includes(term) || tracking.includes(term) || customerId.includes(term);
        });
    this.showProofShipmentSuggestions = this.filteredProofShipmentOptions.length > 0;
  }

  toggleProofShipmentSuggestions(): void {
    if (this.showProofShipmentSuggestions) {
      this.showProofShipmentSuggestions = false;
      return;
    }
    this.showProofShipmentSuggestionsOnFocus();
  }

  applyProofShipmentSearch(): void {
    const term = this.proofShipmentQuery.trim();
    if (!term) {
      this.filteredProofShipmentOptions = this.shipmentOptions;
      return;
    }

    this.filteredProofShipmentOptions = this.shipmentOptions.filter((shipment) => this.matchesShipmentSearch(shipment, term));
  }

  submitDeliveryProof(): void {
    this.clearProofFieldErrors();
    this.error = '';

    if (!this.proofShipmentId) {
      this.proofFieldErrors.shipment = 'Shipment is required.';
    }

    if (!this.proofSignerName.trim()) {
      this.proofFieldErrors.signerName = 'Signer name is required.';
    }

    if (!this.proofFile) {
      this.proofFieldErrors.proofFile = 'Proof file is required.';
    }

    if (this.proofFieldErrors.shipment || this.proofFieldErrors.signerName || this.proofFieldErrors.proofFile) {
      return;
    }

    const proofShipmentId = this.proofShipmentId;
    const proofFile = this.proofFile;
    if (proofShipmentId == null || !proofFile) {
      return;
    }

    this.isUploading = true;
    this.error = '';

    this.documentService.uploadDeliveryProof(proofShipmentId, proofFile, this.proofSignerName, this.proofNotes).pipe(
      catchError(err => {
        this.error = err.error?.message || 'Failed to upload delivery proof.';
        return of(null);
      }),
      finalize(() => this.isUploading = false)
    ).subscribe(result => {
      if (result) {
        this.success = 'Delivery proof uploaded!';
        this.showProofForm = false;
        this.proofFile = null;
        this.proofSignerName = '';
        this.proofNotes = '';
        setTimeout(() => this.success = '', 3000);
      }
    });
  }

  deleteDocument(id: number): void {
    if (!confirm('Are you sure you want to delete this document?')) return;
    this.documentService.delete(id).pipe(
      catchError(err => {
        this.error = err.error?.message || 'Failed to delete document.';
        return of(null);
      })
    ).subscribe(result => {
      if (result !== null) {
        this.success = 'Document deleted.';
        this.documents = this.documents.filter(d => d.documentId !== id);
        setTimeout(() => this.success = '', 3000);
      }
    });
  }

  onProofSignerNameChange(value: string): void {
    this.proofSignerName = value;
    this.proofFieldErrors.signerName = '';
    this.error = '';
  }

  private clearUploadFieldErrors(): void {
    this.uploadFieldErrors = {
      shipment: '',
      file: ''
    };
  }

  private clearProofFieldErrors(): void {
    this.proofFieldErrors = {
      shipment: '',
      signerName: '',
      proofFile: ''
    };
  }

  getTypeIcon(type: string): string {
    const map: Record<string, string> = {
      'General': '📄',
      'Invoice': '🧾',
      'Label': '🏷️',
      'Customs': '📋'
    };
    return map[type] || '📄';
  }
}
