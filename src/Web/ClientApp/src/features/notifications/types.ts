export interface NotificationDto {
  id: number;
  userId: number;
  notificationType: string;
  title: string;
  message: string;
  documentType?: string | null;
  documentId?: number | null;
  priority: string;
  isRead: boolean;
  readAt?: string | null;
  created: string;
}

export interface PaginatedResult<T> {
  items: T[];
  total: number;
  page: number;
  pageSize: number;
}

export interface UnreadCountDto {
  count: number;
}
