import { useState, useCallback } from 'react';

interface UsePaginationResult {
  page: number;
  pageSize: number;
  total: number;
  setPage: (p: number) => void;
  setPageSize: (s: number) => void;
  setTotal: (t: number) => void;
}

export function usePagination(initialPage = 1, initialPageSize = 20, initialTotal = 0): UsePaginationResult {
  const [page, setPage] = useState(initialPage);
  const [pageSize, setPageSize] = useState(initialPageSize);
  const [total, setTotal] = useState(initialTotal);

  const handlePageChange = useCallback((p: number) => {
    setPage(Math.max(1, p));
  }, []);

  return { page, pageSize, total, setPage: handlePageChange, setPageSize, setTotal };
}
