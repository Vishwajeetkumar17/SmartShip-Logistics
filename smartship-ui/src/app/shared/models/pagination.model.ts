/**
 * Generic pagination envelope used across API responses.
 * Normalized by services when backend responses vary by shape.
 */
export interface PaginatedResponse<T> {
  data: T[];
  pageNumber: number;
  pageSize: number;
  totalItems: number;
  totalPages: number;
  hasNextPage: boolean;
  hasPreviousPage: boolean;
}
